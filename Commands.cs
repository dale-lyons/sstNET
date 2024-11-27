using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace sstNET
{
    public static class Commands
    {
/* Compared to original version, I've changed the "help" command to
   "call" and the "terminate" command to "quit" to better match
   user expectations. The DECUS version apparently made those changes
   as well as changing "freeze" to "save". However I like "freeze".

   When I got a later version of Super Star Trek that I was converting
   from, I added the emexit command.

   That later version also mentions srscan and lrscan working when
   docked (using the starbase's scanners), so I made some changes here
   to do this (and indicating that fact to the player), and then realized
   the base would have a subspace radio as well -- doing a Chart when docked
   updates the star chart, and all radio reports will be heard. The Dock
   command will also give a report if a base is under attack.

   Movecom no longer reports movement if sensors are damaged so you wouldn't
   otherwise know it.

   Also added:

   1. Better base positioning at startup

   2. deathray improvement (but keeping original failure alternatives)

   3. Tholian Web

   4. Enemies can ram the Enterprise. Regular Klingons and Romulans can
      move in Expert and Emeritus games. This code could use improvement.

   5. The deep space probe looks interesting! DECUS version

   6. Perhaps cloaking to be added later? BSD version
   */
        static private string[] commands = new string[]
        {
	        "srscan",   //0
	        "lrscan",   //1
	        "phasers",  //2
	        "photons",  //3
	        "move",     //4
	        "shields",
	        "dock",
	        "damages",
	        "chart",
	        "impulse",
	        "rest",     //10
	        "warp",
	        "status",
	        "sensors",
	        "orbit",
	        "transport",
	        "mine",
	        "crystals",
	        "shuttle",
	        "planets",
	        "request",   //20
	        "report",
	        "computer",
	        "commands",
            "emexit",
            "probe",    //25
	        "abandon",
	        "destruct",
	        "freeze",
	        "deathray",
        	"debug",
	        "call",
	        "quit",
	        "help",
            "peek"      //34
        };

        public static int Parse(string cmd)
        {
            int ii;
            for (ii = 0; ii < 26; ii++)
            {
                if (SSTConsole.isit(cmd,commands[ii]))
                    return ii;
            }

            for (; ii < commands.Length; ii++)
            {
                if (string.Compare(commands[ii], cmd, true) == 0)
                    return ii;
            }
            return -1;
        }//Parse

        public static void ListCommands(bool showHelp)
        {
            Game.Console.WriteLine("   SRSCAN    MOVE      PHASERS   CALL");
            Game.Console.WriteLine("   STATUS    IMPULSE   PHOTONS   ABANDON");
            Game.Console.WriteLine("   LRSCAN    WARP      SHIELDS   DESTRUCT");
            Game.Console.WriteLine("   CHART     REST      DOCK      QUIT");
            Game.Console.WriteLine("   DAMAGES   REPORT    SENSORS   ORBIT");
            Game.Console.WriteLine("   TRANSPORT MINE      CRYSTALS  SHUTTLE");
            Game.Console.WriteLine("   PLANETS   REQUEST   DEATHRAY  FREEZE");
            Game.Console.WriteLine("   COMPUTER  EMEXIT    PROBE     COMMANDS");

            if (showHelp)
                Game.Console.WriteLine("   HELP");

        }//ListCommands

        public static void Helpme()
        {
            int cmdIndex = 0;
            string cmd = "";
            while (true)
            {
                if (Game.Console.EOL)
                {
                    Game.Console.Write("Help on what command?");
                    Game.Console.chew();
                }

                object tok = Game.Console.scan();
                //if (tok == null || !(tok is string)) continue; // Try again
                if (tok == null || !(tok is string))
                    return;

                for (cmdIndex = 0; cmdIndex < commands.Length; cmdIndex++)
                {
                    if (commands[cmdIndex].ToUpper() == (tok as string).ToUpper())
                        break;
                }
                if (cmdIndex != commands.Length)
                    break;

                Game.Console.WriteLine("\nValid commands:");
                ListCommands(false);
                Game.Console.chew();
                Game.Console.Skip(1);

            }//while

            if (cmdIndex == 23)
            {
                cmd = " ABBREV";
            }
            else
            {
                cmd = "  Mnemonic:  " + commands[cmdIndex].ToUpper();
            }

            using (Stream helpStream = Game.Console.GetType().Assembly.GetManifestResourceStream("sst.NET.Resources.sst.doc"))
            {
                using (StreamReader sr = new StreamReader(helpStream))
                {
                    string line;
                    bool found = false;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.IndexOf(cmd) >= 0)
                        {
                            found = true;
                            break;
                        }
                    }//while

                    if (!found)
                    {
                        Game.Console.WriteLine("Spock- \"Captain, there is no information on that command.\"");
                    }
                    else
                    {
                        Game.Console.WriteLine("\nSpock- \"Captain, I've found the following information:\"\n");
                        do
                        {
                            if (line.Length > 0 && line[0] != 12)
                                Game.Console.Write(line);

                            Game.Console.Skip(1);
                            line = sr.ReadLine();

                        } while (line.IndexOf("******") < 0);

                    }
                }//using StreamReader
            }//using Stream
        }//helpme

    }//class Commands
}