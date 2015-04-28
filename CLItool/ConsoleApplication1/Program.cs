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

        // where: fetch listing for this location
        private static void getListings( string where )
        {
            String startingList = work.GetListing( where );
            if (startingList.StartsWith("error") == false)
            {
                // this is reading the console input
                string wait = "";
                JavaScriptSerializer js = new JavaScriptSerializer();
                
                //this is the current directory listing contents
                var dict = js.Deserialize<Dictionary<string, dynamic>>(startingList);
                
                int counter = 0;
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
                Console.WriteLine("x to go back, or select item by number");
                Console.WriteLine("");

                while (String.Compare("x", wait, true) != 0)
                {
                    wait = Console.ReadLine();
                    if (String.Compare("x", wait, true) == 0)
                        return;

                    int res = 0;
                    if (Int32.TryParse(wait, out res) == true)
                    {
                        if (res < counter)
                        {
                            if (String.Compare("true", dict["changes"]["adds"][res]["isFile"], true) == 0)
                            {
                                string tempname = dict["changes"]["adds"][res]["displayName"];
                                if ( tempname.EndsWith(".pdf", true, null) == false) //i.e. it doesn't end in .pdf ignoring case
                                    tempname += ".pdf";
                                string FILE_PATH = "E:\\Mobile Helix\\Visual Studio Projects\\" + tempname;
                                using (var writeStream = File.OpenWrite(FILE_PATH))
                                {
                                    Stream pdf = work.GetDocId(
                                        dict["changes"]["adds"][res]["globalKey"],
                                        dict["changes"]["adds"][res]["displayName"],
                                        dict["changes"]["adds"][res]["parentDigest"]
                                    );
                                    if (pdf == null)
                                    {
                                        Console.WriteLine("Unable to fetch PDF");
                                    }
                                    else
                                    {
                                        pdf.CopyTo(writeStream);
                                        writeStream.Close();
                                    }
                                }
                            }
                            else
                            {
                                getListings(dict["changes"]["adds"][res]["globalKey"]);
                            }
                        }
                        // after returning (i.e. when you enter 'x' to go back) reprint the list:
                        counter = 0;
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
                        Console.WriteLine("x to go back, or select item by number");
                        Console.WriteLine("");
                    }
                }
            }

        }
        static void Main(string[] args)
        {
            Class1 f = new Class1();
            String v = f.getVersion();
            var options = new Options( v );
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                try
                {
                    byte[] cert = System.IO.File.ReadAllBytes(options.ActionCertificate);

                    work = new doWork(
                        cert,
                        options.ActionCertificatePassword,
                        options.ActionHost,
                        options.ActionPort,
                        options.ActionUsername,
                        options.ActionPassword
                    );

                    foreach (String element in args)
                    {
                        Console.WriteLine(element);
                    }
                    System.Console.WriteLine();

                    // consume Options instance properties
                    if (options.Verbose)
                    {
                        Console.WriteLine(options.ActionCommand);
                        Console.WriteLine(options.ActionHost);
                        Console.WriteLine(options.ActionPort);
                        Console.WriteLine(options.ActionCertificate);
                        Console.WriteLine(options.ActionUsername);
                        Console.WriteLine(options.ActionPassword);
                        Console.WriteLine(options.MaximumLength);
                    }
                    else if (options.ActionCommand == "nrl")
                    {
                        work.GetNrl(
                            options.ActionHost,
                            options.ActionPort
                        );
                    }
                    else if (options.ActionCommand == "docid")
                    {
                        /*
                         * work.GetDocId(
                            options.ActionHost, 
                            options.ActionPort, 
                            options.ActionUsername, 
                            options.ActionPassword, 
                            options.ActionDocid
                        );
                         * */
                    }
                    else if (options.ActionCommand == "list")
                    {
                        //start by fetching the root, while being at the root level. Meaning back doesn't go any further back..
                        getListings("ROOT");
                    }
                    else
                        Console.WriteLine("working ...");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                }
            }
            else
            {
                doTests d = new doTests();
                byte[] mycert = System.IO.File.ReadAllBytes( "e:\\mobile helix\\mycert.p12" );
                work = new doWork(mycert , "coverity", "192.168.1.113", "8082", "ilya", "helix,41");
                getListings( "ROOT" );

                //d.test1();
            }

        }

        
        
        
        
        
    }
}
