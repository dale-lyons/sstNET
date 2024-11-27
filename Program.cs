using System;
using System.Collections.Generic;
using System.Text;

//http://www.almy.us/sst.html

namespace sstNET
{
    class Program
    {
        //Examples of commandline params
        //
        //sst.NET.exe /cmds:"r l em password"
        //    passes the given string as commands to be executed at startup
        //
        //sst.NET.exe /keys:filename
        //    passes the given filename and passes text as commmands to be executed
        //
        //sst.NET.exe /output:filename
        //    writes all output text to both the console and the given file

        static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            Arguments parsedArgs = new Arguments();

            if (!Parser.ParseArgumentsWithUsage(args, parsedArgs, delegate(string str) { sb.Append(str); sb.Append("\n"); }))
            {
                System.Console.WriteLine(sb.ToString()+"Command line parsing Error!");
                return;
            }

            Game game = new Game();
            Game.Console = new SSTConsole(parsedArgs);

            Game.Console.WriteLine("\n\n-SUPER- STAR TREK\n");
            Game.Console.WriteLine("Latest update-21 Sept 78\n");

            while (true)
            {
                if (!game.Play())
                    break;
            }//while
            Game.Console.WriteLine("\nMay the Great Bird of the Galaxy roost upon your home planet.");
            Game.Console.Close();
        }//Main
    }//class Program
}
