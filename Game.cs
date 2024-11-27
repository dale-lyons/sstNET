using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Serialization;
using System.IO;

namespace sstNET
{
    public class Game
    {
        /// <summary>
        /// The console where we output all the text and get input from user
        /// </summary>
        public static SSTConsole Console;

        /// <summary>
        /// The game data associated with this game.
        /// </summary>
        private GameData mGameData;

        public Game()
        {
            mGameData = new GameData();
        }

        private bool setup()
        {
            mGameData.Random = new Random();
            if (choose())
                return false;

            //compute a damage factor based on the skill level selected
            mGameData.DamageFactor = 0.5 * (int)mGameData.GameSkill;

            mGameData.Galaxy = new Galaxy.Galaxy(mGameData);
            mGameData.Galaxy.Generate(mGameData);
            Console.Skip(3);

            if (mGameData.GameSkill == GameData.GameSkillEnum.Novice)
            {
                Console.WriteLine("It is stardate {0}. The Federation is being attacked by", (int)mGameData.Date);
                Console.WriteLine("a deadly Klingon invasion force. As captain of the United");
                Console.WriteLine("Starship U.S.S. Enterprise, it is your mission to seek out");
                Console.WriteLine("and destroy this invasion force of {0} battle cruisers.", mGameData.Galaxy._inkling);
                Console.WriteLine("You have an initial allotment of {0} stardates to complete", (int)mGameData.Galaxy._intime);
                Console.WriteLine("your mission.  As you proceed you may be given more time.\n");
                Console.WriteLine("You will have {0} supporting starbases.", mGameData.Galaxy._inbase);
                Console.Write("Starbase locations-  ");
            }//if
            else
            {
                Console.Write("Stardate {0}.\n\n{1} Klingons.\nAn unknown number of Romulans\n", (int)mGameData.Date, mGameData.Galaxy._inkling);
                if (mGameData.Galaxy._nscrem > 0)
                    Console.WriteLine("and one (GULP) Super-Commander.");
                Console.Write("{0} stardates\n{1} starbases in  ", (int)mGameData.Galaxy._intime, mGameData.Galaxy._inbase);
            }//else

            bool first = true;
            foreach (sstNET.Galaxy.QuadrantObjects.QuadrantStarBase sb in mGameData.Galaxy.Bases)
            {
                if (!first)
                    Console.Write("  ");
                Console.Write(sb.QuadrantCoordinate.ToString(false));
                first = false;
            }

            Console.WriteLine("\n\nThe Enterprise is currently in{0} {1}\n", mGameData.Galaxy.Ship.QuadrantCoordinate.ToString(true), mGameData.Galaxy.Ship.Sector.ToString(true));
            Console.WriteLine("Good Luck!");

            if (mGameData.Galaxy._nscrem > 0)
                Console.Write("  YOU'LL NEED IT.");

            Console.Skip(1);
            mGameData.Galaxy.newquad(mGameData, false);

            // If starting in a quadrant with baddies, start with shields up
            if (mGameData.Galaxy.CurrentQuadrant.Enemies.Count > 0)
                mGameData.Galaxy.Ship.ShieldsUp = true;

            // bad luck to start in a Romulan Neutral Zone
            if (mGameData.Galaxy.CurrentQuadrant.NeutralZone)
                Battle.Attack(mGameData, false);

            return false;
        }//setup

        private void makemoves(GameData.TurnInfo turn)
        {
            while (true)//command loop
            {
                //mGameData.Galaxy.Commanders.dump();
                //mGameData.Galaxy.CurrentQuadrant.dumpkl(mGameData);
                //mGameData.Galaxy.Bases.Dump();

                turn.justin = false;
                //hitme = false;

                int cmd = -1;
                while (true)//get a command
                {
                    turn.Time = 0.0;

                    Console.chew();
                    Console.Write("\nCOMMAND> ");
                    object tok = Console.scan();
                    if (tok == null)
                        continue;

                    cmd = -1;
                    if (tok is string)
                        cmd = Commands.Parse(tok as string);

                    if (cmd >= 0)
                        break;

                    Console.Write("UNRECOGNIZED COMMAND.");
                    if (mGameData.GameSkill <= GameData.GameSkillEnum.Fair)
                    {
                        Console.WriteLine(" LEGAL COMMANDS ARE:");
                        Commands.ListCommands(true);
                    }//if
                    else
                        Console.Skip(1);

                }//while

                bool hitme = false;
                switch (cmd)//command switch
                {
                    case 0:			// srscan
                        Reports.ShortRangeScan(mGameData);
                        break;
                    case 1:			// lrscan
                        Reports.LongRangeScan(mGameData.Galaxy);
                        break;
                    case 2:			// phasers
                        Battle.Phasers(mGameData);
                        if (turn.ididit) hitme = true;
                        break;
                    case 3:			// photons
                        Battle.Photon(mGameData);
                        if (turn.ididit) hitme = true;
                        break;
                    case 4:			// move
                        Moving.Warp(mGameData);
                        break;
                    case 5:			// shields
                        Battle.Shield(mGameData, false);
                        if (turn.ididit)
                        {
                            Battle.Attack(mGameData, false);
                            mGameData.Galaxy.Ship.ShieldChange = false;
                        }//if
                        break;
                    case 6:			// dock
                        Moving.Dock(mGameData);
                        break;
                    case 7:			// damages
                        Reports.DamageReport(mGameData);
                        break;
                    case 8:         //chart
                        Reports.Chart(true, mGameData);
                        break;
                    case 10:        //rest
                        Events.Wait(mGameData);
                        if (mGameData.Turn.ididit)
                            hitme = true;
                        break;
                    case 11:		// warp
                        Moving.SetWarp(mGameData.Galaxy);
                        break;
                    case 12:		// status
                        Reports.Status(mGameData);
                        break;
                    case 13:			// sensors
                        Planets.Sensor(mGameData.Galaxy);
                        break;
                    case 14:			// orbit
                        Planets.Orbit(mGameData);
                        if (turn.ididit) hitme = true;
                        break;
                    case 15:			// transport "beam"
                        Planets.Beam(mGameData);
                        break;
                    case 16:			// mine
                        Planets.Mine(mGameData);
                        if (turn.ididit) hitme = true;
                        break;
                    case 17:            //use crystals
                        Planets.UseCrystals(mGameData);
                        break;
                    case 18:			// shuttle
                        Planets.Shuttle(mGameData);
                        if (turn.ididit) hitme = true;
                        break;
                    case 19:			// Planet list
                        Planets.PlanetReport(mGameData);
                        break;
                    case 20:			// Request information
                        Reports.Request(mGameData);
                        break;
                    case 21:            //Game report
                        Reports.Report(mGameData, false);
                        break;
                    case 23:
                        Commands.ListCommands(true);
                        break;
                    case 24: //Boss mode, emergency save game
                        this.Freeze(true);
                        break;
                    case 25:
                        Moving.LaunchProbe(mGameData);		// Launch probe
                        break;
                    case 26:			// Abandon Ship
                        Moving.AbandonShip(mGameData);
                        break;
                    case 27:			// Self Destruct
                        Finish.SelfDestruct(mGameData);
                        break;
                    case 28:            //Freeze
                        this.Freeze(false);

                        if (mGameData.GameSkill > GameData.GameSkillEnum.Good)
                            Game.Console.WriteLine("WARNING--Frozen games produce no plaques!");

                        break;
                    case 29:			// Try a desparation measure
                        Battle.DeathRay(mGameData);
                        if (turn.ididit) hitme = true;
                        break;

                    case 30:
                        GameData.DEBUGME = true;
                        break;

                    case 31:		// Call for help
                        Moving.Help(mGameData);
                        break;
                    case 32:
                        turn.alldone = true;
                        break;
                    case 33:
                        Commands.Helpme();      // get help
                        break;
                    case 34:
                        Console.WriteLine("{0,12:F6}", mGameData.Random.Peek());
                        break;

                    default: break;
                }//switch
                for (; ; )
                {
                    if (turn.alldone) break;		// Game has ended
                    if (turn.Time != 0.0)
                    {
                        Events.ProcessEvents(mGameData);
                        if (turn.alldone) break;		// Events did us in
                    }//if
                    if (mGameData.Galaxy[mGameData.Galaxy.Ship.QuadrantCoordinate].SuperNova)
                    {// Galaxy went Nova!
                        Moving.atover(mGameData, false);
                        continue;
                    }//if

                    //todo - hmmmm, is thoian an enemy?
                    if (mGameData.Galaxy.CurrentQuadrant.Enemies.Count == 0)
                        AI.MoveTholian(mGameData);

                    if (hitme && !turn.justin)
                    {
                        Battle.Attack(mGameData, false);
                        if (turn.alldone) break;
                        if (mGameData.Galaxy[mGameData.Galaxy.Ship.QuadrantCoordinate].SuperNova)
                        {// went NOVA! 
                            Moving.atover(mGameData, false);
                            hitme = true;
                            continue;
                        }//if
                    }//if
                    break;
                }//for
                if (turn.alldone)
                    break;

            }//while
        }//makemoves

        public bool Play()
        {
            mGameData.Turn = new GameData.TurnInfo();
            if (this.setup())
            {
                Finish.Score(mGameData, 0);
            }
            else
            {
                makemoves(mGameData.Turn);
            }

            Console.Skip(2);
            Console.stars();
            Console.Skip(1);

            if (mGameData.GameType == GameData.GameTypeEnum.Tournament && mGameData.Turn.alldone)
            {
                Console.Write("Do you want your score recorded?");
                if (Console.ja())
                {
                    Console.chew2();
                    //freeze(false);
                }
            }
            Console.Write("Do you want to play again?");
            return Console.ja();

        }//Play

        private bool choose()
        {
            mGameData.GameType = GameData.GameTypeEnum.None;
            mGameData.GameLength = GameData.GameLengthEnum.None;
            mGameData.GameSkill = GameData.GameSkillEnum.None;

            while (true)
            {
                //Can start with command line options
                if (Console.EOL)
                    Console.Write("Would you like a {0}, {1}, or {2} game?",
                        GameData.GameTypeEnum.Regular.ToString().ToLower(),
                        GameData.GameTypeEnum.Tournament.ToString().ToLower(),
                        GameData.GameTypeEnum.Frozen.ToString().ToLower());

                object tok = Console.scan();
                if (!(tok is string))
                    continue; // Try again

                if (SSTConsole.isit(tok, GameData.GameTypeEnum.Tournament.ToString()))
                {
                    mGameData.GameType = GameData.GameTypeEnum.Tournament;
                    tok = Console.scan();
                    while (tok == null)
                    {
                        Console.Write("Type in {0} number-", GameData.GameTypeEnum.Tournament.ToString().ToLower());
                        tok = Console.scan();
                    }//while
                    if (!(tok is double))
                    {
                        Console.chew();
                        continue; // We don't want a blank entry
                    }//if
                    mGameData.GameTourn = (int)(double)tok;
                    break;
                }//if

                if (SSTConsole.isit(tok, GameData.GameTypeEnum.Frozen.ToString()))
                {
                    mGameData.GameType = GameData.GameTypeEnum.Frozen;
                    this.Thaw();
                    Console.chew();
                    //if (sst.passwd.Length == 0) continue;
                    return true;
                }//if

                if (SSTConsole.isit(tok, GameData.GameTypeEnum.Regular.ToString()))
                {
                    mGameData.GameType = GameData.GameTypeEnum.Regular;
                    Console.Skip(2);
                    break;
                }//if
                Console.WriteLine("What is \"{0}\"?", (tok as string));
                Console.chew();
            }//while

            while (mGameData.GameLength == GameData.GameLengthEnum.None || mGameData.GameSkill == GameData.GameSkillEnum.None)
            {
                object tok = Console.scan();
                if (tok is string)
                {
                    if (SSTConsole.isit(tok, GameData.GameLengthEnum.Short.ToString()))
                        mGameData.GameLength = GameData.GameLengthEnum.Short;
                    else if (SSTConsole.isit(tok, GameData.GameLengthEnum.Medium.ToString()))
                        mGameData.GameLength = GameData.GameLengthEnum.Medium;
                    else if (SSTConsole.isit(tok, GameData.GameLengthEnum.Long.ToString()))
                        mGameData.GameLength = GameData.GameLengthEnum.Long;
                    else if (SSTConsole.isit(tok, GameData.GameSkillEnum.Novice.ToString()))
                        mGameData.GameSkill = GameData.GameSkillEnum.Novice;
                    else if (SSTConsole.isit(tok, GameData.GameSkillEnum.Fair.ToString()))
                        mGameData.GameSkill = GameData.GameSkillEnum.Fair;
                    else if (SSTConsole.isit(tok, GameData.GameSkillEnum.Good.ToString()))
                        mGameData.GameSkill = GameData.GameSkillEnum.Good;
                    else if (SSTConsole.isit(tok, GameData.GameSkillEnum.Expert.ToString()))
                        mGameData.GameSkill = GameData.GameSkillEnum.Expert;
                    else if (SSTConsole.isit(tok, GameData.GameSkillEnum.Emeritus.ToString()))
                        mGameData.GameSkill = GameData.GameSkillEnum.Emeritus;
                    else
                        Console.WriteLine("What is \"{0}\"?", (tok as string));
                }//if
                else
                {
                    Console.chew();
                    if (mGameData.GameLength == GameData.GameLengthEnum.None)
                        //Console.Write("Would you like a Short, Medium, or Long game? ");
                        Console.Write("Would you like a {0}, {1}, or {2} game? ",
                            GameData.GameLengthEnum.Short.ToString(),
                            GameData.GameLengthEnum.Medium.ToString(),
                            GameData.GameLengthEnum.Long.ToString());

                    else if (mGameData.GameSkill == GameData.GameSkillEnum.None)
                        //Console.Write("Are you a Novice, Fair, Good, Expert, or Emeritus player?");
                        Console.Write("Are you a {0}, {1}, {2}, {3}, or {4} player?",
                            GameData.GameSkillEnum.Novice.ToString(),
                            GameData.GameSkillEnum.Fair.ToString(),
                            GameData.GameSkillEnum.Good.ToString(),
                            GameData.GameSkillEnum.Expert.ToString(),
                            GameData.GameSkillEnum.Emeritus.ToString());

                }//else
            }//while

            while (true)
            {
                object tok = Console.scan();
                Console.chew();
                if ((tok is string) && (tok as string).Length > 0)
                {
                    mGameData.GamePassword = (tok as string).Trim();
                    Console.chew();
                    break;
                }
                Console.Write("Please type in a secret password-");
            }//while

            if (mGameData.GameType == GameData.GameTypeEnum.Regular)
            {
                //Seed the random number generator
                mGameData.Random.RandomSeed = ((int)(DateTime.Now.Ticks / 1000));

                //burn off a few samples? Not sure why, was in original code.
                mGameData.Random.Rand(); mGameData.Random.Rand(); mGameData.Random.Rand(); mGameData.Random.Rand();
            }
            else if (mGameData.GameType == GameData.GameTypeEnum.Tournament)
            {
                //Tournament game, seed random number generator with input seed.
                mGameData.Random.RandomSeed = mGameData.GameTourn;
                //burn off a few samples? Not sure why, was in original code.
                mGameData.Random.Rand(); mGameData.Random.Rand(); mGameData.Random.Rand(); mGameData.Random.Rand();
            }
            return false;

        }//choose

        public void Freeze(bool bossMode)
        {
            string fileName = null;
            if (bossMode)
            {
                fileName = "emsave.xml";
            }
            else
            {
                Game.Console.chew();
                while (string.IsNullOrEmpty(fileName))
                {
                    Game.Console.Write("File name: ");
                    object key = Game.Console.scan();
                    if (!(key is string))
                    {
                        Game.Console.huh();
                        continue;
                    }

                    fileName = (key as string).Trim();
                    if (!Path.HasExtension(fileName))
                    {
                        fileName += ".xml";
                    }

                }
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GameData));
                TextWriter writer = new StreamWriter(fileName);
                serializer.Serialize(writer, mGameData);
                writer.Close();
            }
            catch (Exception)
            {
                Game.Console.WriteLine("Can't freeze game as file ");
            }
        }//Freeze

        public void Thaw()
        {
            string fileName = null;
            Game.Console.chew();
            while (string.IsNullOrEmpty(fileName))
            {
                Game.Console.Write("File name: ");
                object key = Game.Console.scan();
                if (!(key is string))
                {
                    Game.Console.huh();
                    continue;
                }

                fileName = (key as string).Trim();
                if (!Path.HasExtension(fileName))
                {
                    fileName += ".xml";

                    if (!File.Exists(fileName))
                    {
                        fileName = null;
                        Game.Console.WriteLine("Can't find game file ");
                        continue;
                    }
                }
            }

            try
            {
                using (TextReader reader = new StreamReader(fileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(GameData));
                    mGameData = (GameData)serializer.Deserialize(reader);
                    //reader.Close();
                }
                Galaxy.CurrentQuadrant cq = mGameData.Galaxy.CurrentQuadrant;
                sstNET.Galaxy.SectorObjects.Ships.FederationShip ship = mGameData.Galaxy.Ship;
                cq[ship.Sector] = ship;

                sstNET.Galaxy.QuadrantObjects.QuadrantPlanet qp = mGameData.Galaxy[ship.QuadrantCoordinate].Planet;
                if (qp != null)
                {
                    sstNET.Galaxy.SectorObjects.SectorPlanet sp = cq.SectorPlanet;
                    if (sp != null)
                    {
                        sp.QuadrantPlanet = qp;
                    }
                }//if
                mGameData.GameThawed = true;
                Reports.Report(mGameData, true);

            }
            catch (Exception ex)
            {
                Game.Console.WriteLine("Can't thaw game:" + ex.Message);
            }

        }//Thaw

    }//class Game
}