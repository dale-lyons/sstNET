using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET
{
    //this class defines the commandline arguments that are valid for the sst.NET application.
    //these attributes define the arguments and their behavoir.
    //The CommandLine parser and this class were all written by a chap named Peter Hallam at Microsoft.
    //See the CommandLineArguments class for full license agreements and documentation.
    public class Arguments
    {
        [ArgumentAttribute(ArgumentType.AtMostOnce, HelpText = "Initial commands.")]
        public string cmds = null;
        [ArgumentAttribute(ArgumentType.AtMostOnce, HelpText = "Keystroke input file.")]
        public string keys = null;
        //[ArgumentAttribute(ArgumentType.AtMostOnce, HelpText = "Random number file.")]
        //public string random = null;
        [ArgumentAttribute(ArgumentType.AtMostOnce, HelpText = "Save stdout text to file.")]
        public string output = null;
    }
}
