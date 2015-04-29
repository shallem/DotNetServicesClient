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
        private String version = "0.05";
        public Options(String v)
        {
            version = v;
        }
        [Option('c', "command", Required = true, DefaultValue = "nrl", HelpText = "Specify command: nrl | docid | list | test")]
        public string ActionCommand { get; set; }

        [Option('h', "host", Required = true, HelpText = "Server host/ip")]
        public string ActionHost { get; set; }

        [Option('p', "port", Required = true, HelpText = "port")]
        public string ActionPort { get; set; }

        [Option('r', "certificate", Required = true, HelpText = "certificate path")]
        public string ActionCertificate { get; set; }

        [Option('o', "certificatePassword", Required = true, HelpText = "certificate password")]
        public string ActionCertificatePassword { get; set; }
        
        [Option('n', "username", Required = true, HelpText = "username")]
        public string ActionUsername { get; set; }

        [Option('w', "password", Required = true, HelpText = "password")]
        public string ActionPassword { get; set; }

        [Option('d', "docid", HelpText = "Document ID for 'docid' command")]
        public string ActionDocid { get; set; }

        [Option('l', "nrlFile", HelpText = "NRL source for the 'nrl' command")]
        public string ActionNrlFile { get; set; }

        [Option('v', null, HelpText = "Print details during execution.")]
        public bool Verbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            // this without using CommandLine.Text
            //  or using HelpText.AutoBuild
            var usage = new StringBuilder();
            usage.AppendLine("Mobile Helix CLI Tool v" + version);
            usage.AppendLine("Must specify \n (c)ommand \n (h)ost \n (p)ort \n ce(r)tificate \n certificatePassw(o)rd \n user(n)ame \n pass(w)ord. \nParameters are specified like this:  -h http://host -p port etc.\n If specifying command=nrl you must also provide nr(l)File.\n If specifying command=docid you must also provide (d)ocid");
            return usage.ToString();
        }
    }
}
