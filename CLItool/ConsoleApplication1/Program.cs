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



namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Class1 f = new Class1();
            String v = f.getVersion();
            
            var options = new Options( v );
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                doWork work = new doWork( options.ActionCertificate );

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
                else if ( options.ActionCommand == "nrl" )
                {
                    work.GetNrl(
                        options.ActionHost, 
                        options.ActionPort
                    );
                }
                else if (options.ActionCommand == "docid")
                {
                    work.GetDocId(
                        options.ActionHost, 
                        options.ActionPort, 
                        options.ActionUsername, 
                        options.ActionPassword, 
                        options.ActionDocid
                    );
                }
                else if (options.ActionCommand == "list")
                {
                    work.GetListing(
                        options.ActionHost, 
                        options.ActionPort, 
                        options.ActionUsername, 
                        options.ActionPassword
                    );
                }
                else
                    Console.WriteLine("working ...");
            }
            else
            {
                doTests d = new doTests();
                d.getSession();
                //d.test1();
            }

        }

        
        
        
        
        
    }
}
