﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using System.Net.Http.Headers;
using System.Net.Http; // if you want text formatting helpers (recommended)
using MobileHelixUtility;
using System.Web.Script.Serialization;
using System.IO;
using System.Net.Mail;

namespace ConsoleApplication1
{
    class Program
    {
        private static LinkClient work;

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

        static private string writeInstructions()
        {
            Console.WriteLine("X to go back, xx to delete this session, or select item by number to browse");
            Console.WriteLine("");
            return Console.ReadLine();
        }

        // where: fetch listing for this location
        //return -1 = error
        //return 0 = all good, all done
        //return 1 = start over - re-list the roots again
        static private int getListings( string where, bool justTesting = false )
        {
            Console.WriteLine("getListings called for " + where);
            // this is reading the console input
            string wait = "";
            int counter = 0; 
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, dynamic> dict = null;
            if (where == "ROOT") { 
                List<String[]> theRoots = work.getRoots(null); 
                int i=1;
                foreach (String[] s in theRoots)
                {
                    Console.WriteLine(i + ": " + s[0] + " - " + s[1]);
                    i++;
                }
                
                while (String.Compare("x", wait, true) != 0)
                {
                    wait = writeInstructions();
                    if (String.Compare("X", wait, true) == 0)
                        return 0; //all done
                    if (String.Compare("xx", wait, true) == 0)
                    {
                        work.deleteSession();
                        return 0; // all done
                    }

                    int res = 0;
                    if (Int32.TryParse(wait, out res) == true)
                    {
                        if (res > 0 && res < i)
                        {
                            String tempRootList = work.GetRootListing( theRoots[res - 1] );
                            dict = js.Deserialize<Dictionary<string, dynamic>>(tempRootList);
                            break;
                        }
                    }
                }
            }
            else
            {
                String tempRootList = work.GetListing( "ROOT:GY83lj", where);
                if (tempRootList.Contains("error: ") )
                    return -1;
                dict = js.Deserialize<Dictionary<string, dynamic>>(tempRootList);
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
            Console.WriteLine("x to go back, xx to delete this session, or select item by number");
            Console.WriteLine("");
            if (justTesting)
            {
                // if we got here then we successfully fetched data from WS and the test passed.
                return 1;
            }
            while (String.Compare("x", wait, true) != 0)
            {
                wait = Console.ReadLine();
                if (String.Compare("x", wait, true) == 0)
                    return 1; //go back to the top folks!
                if (String.Compare("xx", wait, true) == 0)
                {
                    work.deleteSession();
                    return 0; // all done
                }

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
                                if (getListings(dict["changes"]["adds"]["globalKey"]) <= 0) return 0;
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
                                if (getListings(dict["changes"]["adds"][res]["globalKey"]) <= 0) return 0;
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
                    Console.WriteLine("x to go back, or select item by number");
                    Console.WriteLine("");
                } else if (wait.EndsWith("P") || wait.EndsWith("p"))
                {
                    wait = wait.Substring(0, wait.Length - 1);
                    if (Int32.TryParse(wait, out res) == true)
                    {
                        if (String.Compare("true", dict["changes"]["adds"][res]["isFile"], true) == 0)
                        {
                            string props = work.GetProperties(
                                dict["changes"]["adds"][res]["globalKey"],
                                dict["changes"]["adds"][res]["parentDigest"]
                            );
                            Console.WriteLine("File properties: " + props);
                        }
                    }
                }
            }            
            return -1;
        }

        static void sendmail()
        {
            using (System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient("em1smtp", 25))
            {
                client.UseDefaultCredentials = false;
                client.EnableSsl = false;

                try
                {
                    Console.WriteLine("Attempting to send an email through the W&C SMTP interface...");
                    MailMessage msg = new MailMessage();
                    msg.Body = "Mobile Helix WorkSite agent is no longer responding. Please restart it.";
                    msg.Subject = "MOBILE HELIX: WorkSite Agent not responding";
                    msg.From = new System.Net.Mail.MailAddress("search-noreply@whitecase.com");
                    msg.To.Add("dfreeman@whitecase.com");
                    msg.To.Add("ilya@mobilehelix.com");
                    client.Send(msg);
                    Console.WriteLine("Email sent!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The email was not sent.");
                    Console.WriteLine("Error message: " + ex.Message);
                }
            }
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

                    //test must create a new session on each test cycle!
                    if (options.ActionCommand != "test" && options.ActionCommand != "testemail")
                    {
                        work = new LinkClient(
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
                    }
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
                        while (getListings("ROOT") > 0) ;
                        Console.ReadLine();
                    }

                    else if (options.ActionCommand == "testemail")
                    {
                        sendmail();
                        
                    }
                    else if (options.ActionCommand == "test")
                    {
                        while (true)
                        {
                            work = new LinkClient(
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

                            Console.WriteLine(DateTime.Now + ": attempting to fetch AMERICAS_DMS:DOC_WORKLIST");
                            int test = getListings("AMERICAS_DMS:DOC_WORKLIST", true); //true = justTesting
                            Console.WriteLine(DateTime.Now + ": got " + test.ToString());
                            if (test == -1)
                            {
                                sendmail();
                            }
                            Thread.Sleep(600000);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                }
            }

            else
            {

                //byte[] mycert = System.IO.File.ReadAllBytes("e:\\mobilehelixpoc-ilya.p12");
                //work = new doWork("region", "client name (e.g. whiteandcase)", mycert , "cert password", "controller host", "controller port (e.g. 8082)", "appserver host", "appserver port", "username", "mypassword");
                
            }
        }
    }
}
