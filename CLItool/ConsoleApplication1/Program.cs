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

    class Product
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Class1 f = new Class1();
            int x = f.getFoo();

            Console.WriteLine( x );
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {

                foreach (String element in args)
                {
                    Console.WriteLine(element);
                }
                System.Console.WriteLine();

                // consume Options instance properties
                if (options.Verbose)
                {
                    Console.WriteLine(options.ActionType);
                    Console.WriteLine(options.InputFile);
                    Console.WriteLine(options.MaximumLength);
                }
                else if ( options.ActionType == "nrl" )
                {

                    doNRL d = new doNRL();
                    d.go();
                }
                else
                    Console.WriteLine("working ...");
            }

        }

        
        
        
        
        
    }
}
