using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using System.Net.Http.Headers;
using System.Net.Http; // if you want text formatting helpers (recommended)
using MobileHelixUtility;
using System.Web.Script.Serialization;
using System.IO;

namespace ConsoleApplication1
{
    class Program
    {
        private static doWork work;

        // used to save the PDF locally
        private static bool savePDF(Stream pdf, String tempname)
        {
            // add .pdf if not already there
            // TODO: add additional logic for specific extensions, since some files come throug as raw files. Images for example..
            if (tempname.EndsWith(".pdf", true, null) == false)
            {
                tempname += ".pdf";
            }
            string FILE_PATH = Directory.GetCurrentDirectory() + "\\" + tempname;
           
            Console.WriteLine("Saving PDF " + FILE_PATH);
            try
            {
                using (var writeStream = File.OpenWrite(FILE_PATH))
                {
                    if (pdf == null)
                    {
                        Console.WriteLine("Unable to fetch PDF");
                        return false;
                    }
                    else
                    {
                        pdf.CopyTo(writeStream);
                        writeStream.Close();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
                return false;
            }
        }

        // where: fetch listing for this location
        //return -1 = error
        //return 0 = all good, all done
        //return 1 = start over - re-list the roots again
        static private int getListings( string where )
        {
            Console.WriteLine("getListings called for " + where);
            //clearn the currentRoot as we may be selecting a different root (in case there are multiple resoruces in one app)
            if ( where == "ROOT")
                work.clearCurrentRoot();

            String startingList = work.GetListing( where );
            if (startingList.StartsWith("error") == false)
            {
                // this is reading the console input
                string wait = "";
                JavaScriptSerializer js = new JavaScriptSerializer();
                
                //this is the current directory listing contents
                var dict = js.Deserialize<Dictionary<string, dynamic>>(startingList);
                if (dict == null)
                    return -1; //error!!

                int counter = 0;
                List<String[]> theRoots = new List<String[]>();

                //ok - are we getting back multiple roots? If yes, there will be a "roots" key
                if ( dict.ContainsKey("roots")){
                    //we need to print all roots - as long as they have a prop.containsKey["26"] - that's the unique resource ID we need
                    int i = 1;
                    foreach (Dictionary<string, dynamic> r in dict["roots"])
                    {
                        var props = js.Deserialize<Dictionary<string, dynamic>>(r["props"]);
                        if (props.ContainsKey("26"))
                        {
                            String[] s = new String[2];
                            s[0] = r["digest"];
                            s[1] = props["26"];
                            Console.WriteLine(i + ": " + s[0] + " - " + s[1]);
                            theRoots.Add(s);
                            i++;
                        }
                    }
                    while (String.Compare("x", wait, true) != 0)
                    {
                        Console.WriteLine("X to go back, or select item by number");
                        Console.WriteLine("");

                        wait = Console.ReadLine();
                        if (String.Compare("X", wait, true) == 0)
                            return 0; //all done

                        int res = 0;
                        if (Int32.TryParse(wait, out res) == true)
                        {
                            if (res > 0 && res < i)
                            {
                                String tempRootList = work.GetRootListing(theRoots[res - 1]);
                                dict = js.Deserialize<Dictionary<string, dynamic>>(tempRootList);
                                break;
                            }
                        }
                    }
                }
                
                if (dict["changes"] == null)
                {
                    Console.WriteLine(" empty ");
                }    
                    
                else
                {
                    try
                    {
                        // This is to cover the case where there is only 1 adds item (so it isn't an array of adds)
                        Dictionary<string, dynamic> item = dict["changes"]["adds"];
                        if (String.Compare("true", item["isFile"], true) == 0)
                        {
                            Console.WriteLine("item: " + counter + " filename: " + item["displayName"]);
                            Console.WriteLine("id" + item["globalKey"]);
                            Console.WriteLine("location" + item["parentDigest"]);
                        }
                        else
                        {
                            Console.WriteLine("item: " + counter + " folder: " + item["displayName"]);
                            Console.WriteLine("key: " + item["globalKey"]);
                        }
                        counter++;
                    }
                    catch ( Exception e)
                    {
                        //exception is likely because adds is an array, so let's try processing it as array
                        try
                        {
                            foreach (Dictionary<string, dynamic> item in dict["changes"]["adds"])
                            {
                                if (String.Compare("true", item["isFile"], true) == 0)
                                {
                                    Console.WriteLine("item: " + counter + " filename: " + item["displayName"]);
                                    Console.WriteLine("id" + item["globalKey"]);
                                    Console.WriteLine("location" + item["parentDigest"]);
                                }
                                else
                                {
                                    Console.WriteLine("item: " + counter + " folder: " + item["displayName"]);
                                    Console.WriteLine("key: " + item["globalKey"]);
                                }
                                counter++;
                            }
                        }
                        catch (Exception f)
                        {
                            //nope, there really is a problem here
                            Console.WriteLine("Sorry, we hit a snag: " + f);
                        }
                    }
                }
                Console.WriteLine("x to go back, or select item by number");
                Console.WriteLine("");

                while (String.Compare("x", wait, true) != 0)
                {
                    wait = Console.ReadLine();
                    if (String.Compare("x", wait, true) == 0)
                        return 1; //go back to the top folks!

                    int res = 0;
                    if (Int32.TryParse(wait, out res) == true)
                    {
                        if (res < counter)
                        {
                            if (counter == 1)
                            {
                                //special case - adds is not an array here
                                if (String.Compare("true", dict["changes"]["adds"]["isFile"], true) == 0)
                                {
                                    string tempname = dict["changes"]["adds"]["displayName"];
                                    if (tempname.EndsWith(".pdf", true, null) == false) //i.e. it doesn't end in .pdf ignoring case
                                        tempname += ".pdf";
                                    Stream pdf = work.GetDocId(
                                        dict["changes"]["adds"]["globalKey"],
                                        dict["changes"]["adds"]["displayName"],
                                        dict["changes"]["adds"]["parentDigest"]
                                    );
                                    savePDF(pdf, dict["changes"]["adds"]["displayName"]);
                                }
                                else
                                {
                                    getListings(dict["changes"]["adds"]["globalKey"]);
                                }

                            }
                            else
                            {
                                //here adds is an array
                                if (String.Compare("true", dict["changes"]["adds"][res]["isFile"], true) == 0)
                                {
                                    string tempname = dict["changes"]["adds"][res]["displayName"];
                                    if (tempname.EndsWith(".pdf", true, null) == false) //i.e. it doesn't end in .pdf ignoring case
                                        tempname += ".pdf";
                                    Stream pdf = work.GetDocId(
                                        dict["changes"]["adds"][res]["globalKey"],
                                        dict["changes"]["adds"][res]["displayName"],
                                        dict["changes"]["adds"][res]["parentDigest"]
                                    );
                                    savePDF(pdf, dict["changes"]["adds"][res]["displayName"]);
                                }
                                else
                                {
                                    getListings(dict["changes"]["adds"][res]["globalKey"]);
                                }
                            }
                        }
                        // after returning (i.e. when you enter 'x' to go back) reprint the list:
                        counter = 0;
                        try
                        {
                            Dictionary<string, dynamic> item = dict["changes"]["adds"];
                            if (String.Compare("true", item["isFile"], true) == 0)
                            {
                                Console.WriteLine("item: " + counter + " filename: " + item["displayName"]);
                                Console.WriteLine("id" + item["globalKey"]);
                                Console.WriteLine("location" + item["parentDigest"]);
                            }
                            else
                            {
                                Console.WriteLine("item: " + counter + " folder: " + item["displayName"]);
                                Console.WriteLine("key: " + item["globalKey"]);
                            }
                            counter++;
                        }
                        catch (Exception e)
                        {
                            try {
                                foreach (Dictionary<string, dynamic> item in dict["changes"]["adds"])
                                {
                                    if (String.Compare("true", item["isFile"], true) == 0)
                                    {
                                        Console.WriteLine("item: " + counter + " filename: " + item["displayName"]);
                                        Console.WriteLine("id" + item["globalKey"]);
                                        Console.WriteLine("location" + item["parentDigest"]);
                                    }
                                    else
                                    {
                                        Console.WriteLine("item: " + counter + " folder: " + item["displayName"]);
                                        Console.WriteLine("key: " + item["globalKey"]);
                                    }
                                    counter++;
                                }
                            }
                            catch (Exception f)
                            {
                                //nope, there really is a problem here
                                Console.WriteLine("Sorry, we hit a snag: " + f);
                            }
                        }
                        Console.WriteLine("x to go back, or select item by number");
                        Console.WriteLine("");
                    }
                }
            }
            return 1;
        }
        static void Main(string[] args)
        {
            Class1 f = new Class1();
            String v = f.getVersion();
            var options = new Options( v );
            options.GetUsage();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                try
                {
                    if (options.Verbose)
                    {
                        // TODO
                    }

                    byte[] cert = System.IO.File.ReadAllBytes(options.ActionCertificate);

                    work = new doWork(
                        options.ActionRegion,
                        options.ActionClient,
                        cert,
                        options.ActionCertificatePassword,
                        options.ActionHost,
                        options.ActionPort,
                        options.AppsHost,
                        options.AppsPort,
                        options.ActionUsername,
                        options.ActionPassword
                    );

                    foreach (String element in args)
                    {
                        Console.WriteLine(element);
                    }
                    System.Console.WriteLine();

                    if (options.ActionCommand == "nrl")
                    {
                        if (options.ActionNrlFile != null)
                        {
                            using (StreamReader inputStreamReader = new System.IO.StreamReader(options.ActionNrlFile))
                            {
                                // make sure bogus files don't get too far
                                int counter = 10;
                                String nrlContents;
                                while ((nrlContents = inputStreamReader.ReadLine()) != null && counter > 0)
                                {
                                    counter--;
                                    if (nrlContents.StartsWith("!nrtdms") == true)
                                    {
                                        // drop the .nrl suffix.

                                        String filename = work.getFilename(nrlContents, null);
                                        Stream pdf = work.GetDocId(nrlContents, filename, null);
                                        savePDF(pdf, filename);

                                    }
                                }
                            }

                        }
                        else
                        {
                            Console.WriteLine("You did not specify a docid");
                        }
                    }
                    else if (options.ActionCommand == "docid")
                    {
                        if (options.ActionDocid != null)
                        {
                            String filename = work.getFilename(options.ActionDocid, null);
                            Stream pdf = work.GetDocId(options.ActionDocid, filename, null);
                            savePDF(pdf, filename);
                        }
                        else
                        {
                            Console.WriteLine("You did not specify a docid");
                        }
                    }
                    else if (options.ActionCommand == "list")
                    {
                        //start by fetching the root, while being at the root level. Meaning back doesn't go any further back..
                        while (getListings("ROOT") != 0) ;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                }
            }
            else
            {

                /*
                byte[] mycert = System.IO.File.ReadAllBytes( "d:\\demo-il.ya.p12" );
                //work = new doWork("region", "client name (e.g. whiteandcase)", mycert , "cert password", "controller host", "controller port (e.g. 8082)", "appserver host", "appserver port", "username", "mypassword");
                while ( getListings("ROOT") != 0 );
                */

                /*
                options.ActionDocid = "!nrtdms:0:!session:DMSIDOL:!database:Active:!document:32967,1:";
                if (options.ActionDocid != null)
                {
                    String filename = work.getFilename(options.ActionDocid, null);
                    Stream pdf = work.GetDocId(options.ActionDocid, filename, null);
                    savePDF(pdf, filename);
                }
                else
                {
                    Console.WriteLine("You did not specify a docid");
                }
                */
                 
            }
        }
    }
}
