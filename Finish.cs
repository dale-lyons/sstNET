using System;
using System.Collections.Generic;
using System.Text;

using sstNET.Galaxy;
using sstNET.Galaxy.SectorObjects;
using sstNET.Galaxy.SectorObjects.Ships;

namespace sstNET
{
    public static class Finish
    {
        //These constants define positive point values for user actions
        private static int PointsPerKlingonKilled = 10;
        private static int PointsPerCommanderKilled = 50;
        private static int PointsPerSuperCommanderKilled = 200;
        private static int PointsPerRomulanKilled = 20;
        private static int PointsPerRomulanRemaining = 1;

        //These constants define negative point values for user actions
        private static int PointsPerBaseKilled = 100;
        private static int PointsPerShipKilled = 100;
        private static int PointsPerCallForHelp = 45;
        private static int PointsPerStarKilled = 5;
        private static int PointsPerCausalty = 1;
        private static int PointsPerPlanetKilled = 10;

        /// <summary>
        /// The various ways a game can end.
        /// </summary>
        public enum FINTYPE
        {
            FWON, FDEPLETE, FLIFESUP, FNRG, FBATTLE,
            FNEG3, FNOVA, FSNOVAED, FABANDN, FDILITHIUM,
            FMATERIALIZE, FPHASER, FLOST, FDPLANET,
            FPNOVA, FSTRACTOR, FDRAY, FTRIBBLE,
            FHOLE
        }

        /// <summary>
        /// Todo - implement this.
        /// </summary>
        public static void plaque() { }

        /// <summary>
        /// Ship has destroyed itself. (Deathray, Crystals or Self-Destruct)
        /// In either case the resulting explosion will inflict massive damage to
        /// enemies in current quadrant. Inflict that damage and destroy any not capable
        /// of absorbing it.
        /// </summary>
        /// <param name="game">Reference to Game</param>
        public static void Kaboom(GameData game)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = game.Galaxy.Ship;

            Game.Console.stars();

            //why only Enterprise?
            if (ship is Enterprise)
                Game.Console.WriteLine("***");

            Game.Console.WriteLine("********* Entropy of \n{0} maximized *********\n", ship.Name);
            Game.Console.stars();
            Game.Console.Skip(1);

            //compute damage inflicted on enemies in quadrant
            double whammo = 25.0 * ship.ShipEnergy;
            foreach (EnemyShip es in galaxy.CurrentQuadrant.Enemies)
            {
                //any enemy ship not capable of absorbing the damage is killed
                if (es.Power * es.Distance < whammo)
                {
                    es.killShip(game);
                }//if
            }//foreach
            Finish.finish(Finish.FINTYPE.FDILITHIUM, game);
        }//Kaboom

        /// <summary>
        /// Compute and print the final score.
        /// </summary>
        /// <param name="game"></param>
        public static void Score(GameData game, int shipsKilled)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;

            //compute time used since start of game
            double timused = game.Date - galaxy._indate;

            int iskill = (int)game.GameSkill;
            int romulansCaptured = game.Turn.gamewon ? galaxy.Romulans : 0;// None captured if no win

            if ((timused == 0 || galaxy.Klingons != 0) && timused < 5.0)
                timused = 5.0;

            double perdate = (game.CommandersKilled + game.KlingonsKilled + game.SuperCommandersKilled) / timused;
            int ithperd = (int)(500 * perdate + 0.5);

            int iwon = 0;
            if (game.Turn.gamewon)
                iwon = 100 * iskill;

            //First, add all the game positive values for score
            int positiveScore = (PointsPerKlingonKilled * game.KlingonsKilled) +
                                (PointsPerCommanderKilled * game.CommandersKilled) +
                                ithperd + iwon +
                                (PointsPerRomulanKilled * game.RomulansKilled) +
                                (PointsPerSuperCommanderKilled * game.SuperCommandersKilled) +
                                (PointsPerRomulanRemaining * romulansCaptured);

            int negativeScore = (PointsPerBaseKilled * game.BasesKilled) +
                                (PointsPerShipKilled * shipsKilled) +
                                (PointsPerCallForHelp * game.CallsForHelp) +
                                (PointsPerStarKilled * game.StarsKilled) +
                                (PointsPerCausalty * game.Casualties) +
                                (PointsPerPlanetKilled * game.PlanetsKilled);

            int iscore = positiveScore - negativeScore;

            if (!game.Turn.alive)
                iscore -= 200;

            Game.Console.WriteLine("\n\nYour score --");
            if (game.RomulansKilled != 0)
                Game.Console.WriteLine("{0,6} Romulans destroyed                 {1,5}", game.RomulansKilled, PointsPerRomulanKilled * game.RomulansKilled);
            if (romulansCaptured != 0)
                Game.Console.WriteLine("{0,6} Romulans captured                  {1,5}", romulansCaptured, PointsPerRomulanRemaining * romulansCaptured);
            if (game.KlingonsKilled != 0)
                Game.Console.WriteLine("{0,6} ordinary Klingons destroyed        {1,5}", game.KlingonsKilled, PointsPerKlingonKilled * game.KlingonsKilled);
            if (game.CommandersKilled != 0)
                Game.Console.WriteLine("{0,6} Klingon commanders destroyed       {1,5}", game.CommandersKilled, PointsPerCommanderKilled * game.CommandersKilled);
            if (game.SuperCommandersKilled != 0)
                Game.Console.WriteLine("{0,6} Super-Commander destroyed          {1,5}", game.SuperCommandersKilled, PointsPerSuperCommanderKilled * game.SuperCommandersKilled);
            if (ithperd != 0)
                Game.Console.WriteLine("{0,6:F2} Klingons per stardate              {1,5}", perdate, ithperd);
            if (game.StarsKilled != 0)
                Game.Console.WriteLine("{0,6} stars destroyed by your action     {1,5}", game.StarsKilled, -(PointsPerStarKilled * game.StarsKilled));
            if (game.PlanetsKilled != 0)
                Game.Console.WriteLine("{0,6} planets destroyed by your action   {1,5}", game.PlanetsKilled, -(PointsPerPlanetKilled * game.PlanetsKilled));
            if (game.BasesKilled != 0)
                Game.Console.WriteLine("{0,6} bases destroyed by your action     {1,5}", game.BasesKilled, -(PointsPerBaseKilled * game.BasesKilled));
            if (game.CallsForHelp != 0)
                Game.Console.WriteLine("{0,6} calls for help from starbase       {1,5}", game.CallsForHelp, -(PointsPerCallForHelp * game.CallsForHelp));
            if (game.Casualties != 0)
                Game.Console.WriteLine("{0,6} casualties incurred                {1,5}", game.Casualties, -(PointsPerCausalty * game.Casualties));
            if (shipsKilled != 0)
                Game.Console.WriteLine("{0,6} ship(s) lost or destroyed          {1,5}", shipsKilled, -(PointsPerShipKilled * shipsKilled));
            if (!game.Turn.alive)
                Game.Console.WriteLine("Penalty for getting yourself killed        -200");
            if (game.Turn.gamewon)
            {
                Game.Console.Write("\nBonus for winning ");

                StringBuilder str = new StringBuilder(game.GameSkill.ToString());
                str.Append(" game");
                str.Append(' ', 13 - str.Length);

                Game.Console.Write(str.ToString());
                Game.Console.WriteLine("           {0,5}", iwon);
            }//if
            Game.Console.WriteLine("\n\nTOTAL SCORE                               {0,5}", iscore);

        }//Score

        private static void PrintPromoteString(GameData.GameSkillEnum from, GameData.GameSkillEnum to)
        {
            Game.Console.WriteLine("promotes you one step in rank from \"{0}\" to \"{1}\".", from.ToString(), to.ToString());
        }

        public static void finish(Finish.FINTYPE ifin, GameData game)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = game.Galaxy.Ship;

            bool igotit = false;
            game.Turn.alldone = true;
            Game.Console.WriteLine("\n\n\nIt is stardate {0,0:F1} .\n", game.Date);

            //Number of ships killed during this game. Set to 0 for now, compute it later depending
            //on the finish type.
            int shipsKilled = 0;

            switch (ifin)
            {
                case FINTYPE.FWON: // Game has been won
                    if (game.Galaxy.Romulans != 0)
                        Game.Console.Write("The remaining {0} Romulans surrender to Starfleet Command.\n", game.Galaxy.Romulans);

                    Game.Console.WriteLine("You have smashed the Klingon invasion fleet and saved");
                    Game.Console.WriteLine("the Federation.");
                    game.Turn.gamewon = true;
                    if (game.Turn.alive)
                    {
                        double badpt = 5.0 * game.StarsKilled + game.Casualties + 10.0 * game.PlanetsKilled + 45.0 * game.CallsForHelp + 100.0 * game.BasesKilled;

                        //determine the ship we finished with
                        if (ship == null)
                        {//both enterprise and fq were destroyed
                            badpt += 200.0;
                            shipsKilled = 2;
                        }
                        else if (ship is FaerieQueene)
                        {//only enterprise destroyed
                            badpt += 100.0;
                            shipsKilled = 1;
                        }

                        if (badpt < 100.0)
                            badpt = 0.0;	// Close enough!

                        if (game.Date - galaxy._indate < 5.0 || //killsPerDate >= RateMax
                            (game.KlingonsKilled + game.CommandersKilled + game.SuperCommandersKilled) / (game.Date - galaxy._indate) >=
                            0.1 * (int)game.GameSkill * ((int)game.GameSkill + 1.0) + 0.1 + 0.008 * badpt)
                        {
                            Game.Console.WriteLine("\nIn fact, you have done so well that Starfleet Command");
                            switch (game.GameSkill)
                            {
                                case GameData.GameSkillEnum.Novice:
                                    PrintPromoteString(GameData.GameSkillEnum.Novice, GameData.GameSkillEnum.Fair);
                                    break;
                                case GameData.GameSkillEnum.Fair:
                                    PrintPromoteString(GameData.GameSkillEnum.Fair, GameData.GameSkillEnum.Good);
                                    break;
                                case GameData.GameSkillEnum.Good:
                                    PrintPromoteString(GameData.GameSkillEnum.Good, GameData.GameSkillEnum.Expert);
                                    break;
                                case GameData.GameSkillEnum.Expert:
                                    Game.Console.WriteLine("promotes you to Commodore {0}.\n", GameData.GameSkillEnum.Emeritus.ToString());
                                    Game.Console.WriteLine("Now that you think you're really good, try playing");
                                    Game.Console.WriteLine("the \"{0}\" game. It will splatter your ego.", GameData.GameSkillEnum.Emeritus.ToString());
                                    break;
                                case GameData.GameSkillEnum.Emeritus:
                                    Game.Console.WriteLine("\nComputer-  ERROR-ERROR-ERROR-ERROR");
                                    Game.Console.WriteLine("\n  YOUR-SKILL-HAS-EXCEEDED-THE-CAPACITY-OF-THIS-PROGRAM");
                                    Game.Console.WriteLine("  THIS-PROGRAM-MUST-SURVIVE");
                                    Game.Console.WriteLine("  THIS-PROGRAM-MUST-SURVIVE");
                                    Game.Console.WriteLine("  THIS-PROGRAM-MUST-SURVIVE");
                                    Game.Console.WriteLine("  THIS-PROGRAM-MUST?- MUST ? - SUR? ? -?  VI");
                                    Game.Console.WriteLine("\nNow you can retire and write your own Star Trek game!\n");
                                    break;
                            }//switch
                            if ((int)game.GameSkill > 3)
                            {
                                //todo - fix this
                                if (game.GameThawed)
                                    Game.Console.WriteLine("You cannot get a citation, so...");
                                else
                                {
                                    Game.Console.WriteLine("Do you want your Commodore {0} Citation printed?", GameData.GameSkillEnum.Emeritus.ToString());
                                    Game.Console.Write("(You need a 132 column printer.)");
                                    Game.Console.chew();
                                    if (Game.Console.ja())
                                    {
                                        igotit = true;
                                    }//if
                                }//else
                            }//if
                        }//if
                        //Only grant long life if alive (original didn't!)
                        Game.Console.WriteLine("\nLIVE LONG AND PROSPER.");
                    }//if
                    Score(game, shipsKilled);

                    if (igotit)
                        plaque();
                    return;

                    //All remaining cases involve the current ship being destroyed.
                case FINTYPE.FDEPLETE://Federation Resources Depleted
                    Game.Console.WriteLine("Your time has run out and the Federation has been");
                    Game.Console.WriteLine("conquered.  Your starship is now Klingon property,");
                    Game.Console.WriteLine("and you are put on trial as a war criminal.  On the");
                    Game.Console.Write("basis of your record, you are ");
                    if (galaxy.Klingons * 3.0 > galaxy._inkling)
                    {
                        Game.Console.WriteLine("aquitted.");
                        Game.Console.WriteLine("\nLIVE LONG AND PROSPER.");
                    }//if
                    else
                    {
                        Game.Console.WriteLine("found guilty and");
                        Game.Console.WriteLine("sentenced to death by slow torture.");
                        game.Turn.alive = false;
                    }//else
                    Score(game, 0);
                    return;
                case FINTYPE.FLIFESUP:
                    Game.Console.WriteLine("Your life support reserves have run out, and");
                    Game.Console.WriteLine("you die of thirst, starvation, and asphyxiation.");
                    Game.Console.WriteLine("Your starship is a derelict in space.");
                    break;
                case FINTYPE.FNRG:
                    Game.Console.WriteLine("Your energy supply is exhausted.");
                    Game.Console.WriteLine("\nYour starship is a derelict in space.");
                    break;
                case FINTYPE.FBATTLE:
                    Game.Console.WriteLine("The {0}has been destroyed in battle.", ship.Name);
                    Game.Console.WriteLine("\nDulce et decorum est pro patria mori.");
                    break;
                case FINTYPE.FNEG3:
                    Game.Console.WriteLine("You have made three attempts to cross the negative energy");
                    Game.Console.WriteLine("barrier which surrounds the galaxy.");
                    Game.Console.WriteLine("\nYour navigation is abominable.");
                    Score(game, 1);
                    return;
                case FINTYPE.FNOVA:
                    Game.Console.WriteLine("Your starship has been destroyed by a nova.");
                    Game.Console.WriteLine("That was a great shot.\n");
                    break;
                case FINTYPE.FSNOVAED:
                    Game.Console.WriteLine("The {0} has been fried by a supernova.", ship.Name);
                    Game.Console.WriteLine("...Not even cinders remain...");
                    break;
                case FINTYPE.FABANDN:
                    Game.Console.WriteLine("You have been captured by the Klingons. If you still");
                    Game.Console.WriteLine("had a starbase to be returned to, you would have been");
                    Game.Console.WriteLine("repatriated and given another chance. Since you have");
                    Game.Console.WriteLine("no starbases, you will be mercilessly tortured to death.");
                    break;
                case FINTYPE.FDILITHIUM:
                    Game.Console.WriteLine("Your starship is now an expanding cloud of subatomic particles");
                    break;
                case FINTYPE.FMATERIALIZE:
                    Game.Console.WriteLine("Starbase was unable to re-materialize your starship.");
                    Game.Console.WriteLine("Sic transit gloria muntdi");
                    break;
                case FINTYPE.FPHASER:
                    Game.Console.WriteLine("The {0} has been cremated by its own phasers.", ship.Name);
                    break;
                case FINTYPE.FLOST:
                    Game.Console.WriteLine("You and your landing party have been");
                    Game.Console.WriteLine("converted to energy, disipating through space.");
                    break;
                //case FINTYPE.FMINING:
                //    Game.Console.WriteLine("You are left with your landing party on");
                //    Game.Console.WriteLine("a wild jungle planet inhabited by primitive cannibals.\n");
                //    Game.Console.WriteLine("They are very fond of \"Captain Kirk\" soup.\n");
                //    Game.Console.WriteLine("Without your leadership, the {0} is destroyed.", ship.Name);
                //    break;
                case FINTYPE.FDPLANET:
                    Game.Console.WriteLine("You and your mining party perish.\n");
                    Game.Console.WriteLine("That was a great shot.\n");
                    break;
                //case FINTYPE.FSSC:
                //    Game.Console.WriteLine("The Galileo is instantly annihilated by the supernova.");
                ////no break;
                case FINTYPE.FPNOVA:
                    Game.Console.WriteLine("You and your mining party are atomized.\n");
                    Game.Console.WriteLine("Mr. Spock takes command of the {0} and", ship.Name);
                    Game.Console.WriteLine("joins the Romulans, reigning terror on the Federation.");
                    break;
                case FINTYPE.FSTRACTOR:
                    Game.Console.WriteLine("The shuttle craft Galileo is also caught,");
                    Game.Console.WriteLine("and breaks up under the strain.\n");
                    Game.Console.WriteLine("Your debris is scattered for millions of miles.");
                    Game.Console.WriteLine("Without your leadership, the {0} is destroyed.", ship.Name);
                    break;
                case FINTYPE.FDRAY:
                    Game.Console.WriteLine("The mutants attack and kill Spock.");
                    Game.Console.WriteLine("Your ship is captured by Klingons, and");
                    Game.Console.WriteLine("your crew is put on display in a Klingon zoo.");
                    break;
                case FINTYPE.FTRIBBLE:
                    Game.Console.WriteLine("Tribbles consume all remaining water,");
                    Game.Console.WriteLine("food, and oxygen on your ship.\n");
                    Game.Console.WriteLine("You die of thirst, starvation, and asphyxiation.");
                    Game.Console.WriteLine("Your starship is a derelict in space.");
                    break;
                case FINTYPE.FHOLE:
                    Game.Console.WriteLine("Your ship is drawn to the center of the black hole.");
                    Game.Console.WriteLine("You are crushed into extremely dense matter.");
                    break;
            }//switch

            //At this point we know that the current ship has been killed. Determine the total
            //number of ships killed so far. (either 1 or 2)
            if (game.Galaxy.Ship is Enterprise)
                shipsKilled = 1;
            else if (game.Galaxy.Ship is FaerieQueene)
                shipsKilled = 2;

            game.Turn.alive = false;
            if (galaxy.Klingons != 0)
            {
                double goodies = game.RemainingResources / galaxy._inresor;
                double baddies = (galaxy.Klingons + 2.0 * galaxy.Commanders.Count) / (galaxy._inkling + 2.0 * galaxy._incom);
                if (goodies / baddies >= 1.0 + 0.5 * game.Random.Rand())
                {
                    Game.Console.WriteLine("As a result of your actions, a treaty with the Klingon");
                    Game.Console.WriteLine("Empire has been signed. The terms of the treaty are");
                    if (goodies / baddies >= 3.0 + game.Random.Rand())
                    {
                        Game.Console.WriteLine("favorable to the Federation.\n");
                        Game.Console.WriteLine("Congratulations!");
                    }//if
                    else
                        Game.Console.WriteLine("highly unfavorable to the Federation.");
                }//if
                else
                    Game.Console.WriteLine("The Federation will be destroyed.");
            }//if
            else
            {
                Game.Console.WriteLine("Since you took the last Klingon with you, you are a");
                Game.Console.WriteLine("martyr and a hero. Someday maybe they'll erect a");
                Game.Console.WriteLine("statue in your memory. Rest in peace, and try not");
                Game.Console.WriteLine("to think about pigeons.");
                game.Turn.gamewon = true;
            }//else
            Score(game, shipsKilled);
        }//finish

        /// <summary>
        /// The self-destruct command has been given. If the computer is functioning then
        /// begin the self-destruct sequence. The user must supply the password given at
        /// game start to finish the sequence.
        /// </summary>
        /// <param name="game"></param>
        public static void SelfDestruct(GameData game)
        {//Finish with a BANG! */
            //Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = game.Galaxy.Ship;

            Game.Console.chew();

            //check that computer is not damaged
            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Computer))
            {
                Game.Console.WriteLine("Computer damaged; cannot execute destruct sequence.");
                return;
            }

            Game.Console.WriteLine("\n---WORKING---\n");
            Game.Console.WriteLine("SELF-DESTRUCT-SEQUENCE-ACTIVATED");
            Game.Console.WriteLine("   10\n");
            Game.Console.WriteLine("       9\n");
            Game.Console.WriteLine("          8\n");
            Game.Console.WriteLine("             7\n");
            Game.Console.WriteLine("                6\n");
            Game.Console.WriteLine("ENTER-CORRECT-PASSWORD-TO-CONTINUE-");
            Game.Console.WriteLine("SELF-DESTRUCT-SEQUENCE-OTHERWISE-");
            Game.Console.WriteLine("SELF-DESTRUCT-SEQUENCE-WILL-BE-ABORTED");

            //get password from the user
            object tok = Game.Console.scan();
            Game.Console.chew();

            //and check it is the same as given password at game start
            //Note, password is case sensitive
            if (!(tok is string) || (string.Compare((tok as string), game.GamePassword, StringComparison.CurrentCulture) != 0))
            {
                Game.Console.WriteLine("PASSWORD-REJECTED;\n");
                Game.Console.WriteLine("CONTINUITY-EFFECTED\n");
                return;
            }//if

            //password accepted ... blowup ship
            Game.Console.WriteLine("PASSWORD-ACCEPTED\n");
            Game.Console.WriteLine("                   5\n");
            Game.Console.WriteLine("                      4\n");
            Game.Console.WriteLine("                         3\n");
            Game.Console.WriteLine("                            2\n");
            Game.Console.WriteLine("                              1\n");

            //a little humor before dying ...
            if (game.Random.Rand() < 0.15)
                Game.Console.WriteLine("GOODBYE-CRUEL-WORLD\n");

            Game.Console.Skip(2);
            Kaboom(game);

        }//SelfDestruct
    }//class Finish
}