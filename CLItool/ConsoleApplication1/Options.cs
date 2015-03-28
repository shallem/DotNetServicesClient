using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{

    class Options
    {
        [Option('i', "input", Required = false, HelpText = "Input file to read.")]
        public string InputFile { get; set; }

        [Option('t', "type", Required = true, DefaultValue = "nrl", HelpText = "Specify request type. ('nrl', 'docid', 'fetch')")]
        public string ActionType { get; set; }

        [Option("length", DefaultValue = -1, HelpText = "The maximum number of bytes to process.")]
        public int MaximumLength { get; set; }

        [Option('v', null, HelpText = "Print details during execution.")]
        public bool Verbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            // this without using CommandLine.Text
            //  or using HelpText.AutoBuild
            var usage = new StringBuilder();
            usage.AppendLine("Quickstart Application 1.0");
            usage.AppendLine("Read user manual for usage instructions...");
            return usage.ToString();
        }
    }
}
