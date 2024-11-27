using System;
using System.Collections.Generic;
using System.Text;

using sstNET.Galaxy;
using sstNET.Galaxy.SectorObjects;
using sstNET.Galaxy.SectorObjects.Ships;

namespace sstNET
{
    /// <summary>
    /// Prints various reports to the console at user request.
    /// </summary>
    public static class Reports
    {
        /// <summary>
        /// Types of information requests that can be made via the "Request" command.
        /// Note:The enum names are used for parsing the command so don't change the enum names
        /// unless you intend to change the command itself.
        /// </summary>
        private enum RequestTypeEnum
        {
            None = 0,
            Date = 1,
            Condition = 2,
            Position = 3,
            LSupport = 4,
            Warpfactor = 5,
            Energy = 6,
            Torpedoes = 7,
            Shields = 8,
            Klingons = 9,
            Time = 10
        }//RequestTypeEnum

        /// <summary>
        /// Provides for a short range scan. Ship sensors sweep the current quadrant and report its findings.
        /// Note that if srsensors are damaged no report is done, unless the ship is docked at a starbase
        /// in which case the starbase sensors are used instead.
        /// The ship can always see the adjacent sectors regardless of sensor damage.
        /// The srsensor command can have 1 parameter, either "chart" or "no"
        /// The chart parameter causes the Chart command to execute right after.
        /// The no paramater means to not print status information to the right side of the scan info.
        /// </summary>
        /// <param name="game"></param>
        public static void ShortRangeScan(GameData game)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;
            QuadrantCoordinate shipQuadrant = ship.QuadrantCoordinate;

            bool goodScan = true;
            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SRSensors))
            {
                //Allow base's sensors if docked
                if (!ship.Docked)
                {
                    Game.Console.WriteLine("SHORT-RANGE SENSORS DAMAGED");
                    goodScan = false;
                }//if
                else
                    Game.Console.WriteLine("[Using starbase's sensors]");
            }//if

            //as long as srsensors are working, update the star chart for this quadrant.
            if (goodScan)
                galaxy[shipQuadrant].Starch = ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) ? galaxy[shipQuadrant].ToInt + 1000 : 1;

            bool printChart = false;
            bool rightSide = true;

            //check for optional paramater
            object tok = Game.Console.scan();
            if (SSTConsole.isit(tok, "chart"))
                printChart = true;
            else if (SSTConsole.isit(tok, "no"))
                rightSide = false;

            //print the report header
            Game.Console.chew();
            Game.Console.WriteLine("\n    1 2 3 4 5 6 7 8 9 10");

            //iterate over the 10 lines that makeup the y axis of the sector report
            for (int iy = 1; iy <= 10; iy++)
            {
                //print the sector y axis number
                Game.Console.Write("{0,2}  ", iy);

                //iterate over the 10 columns that makeup the x axis of the sector report
                for (int ix = 1; ix <= 10; ix++)
                {
                    //construct a sector coordinate from the x and y axis values
                    SectorCoordinate sc = new SectorCoordinate(iy, ix);

                    //for a good scan OR sector is adjacent to ship, print the contents of the sector
                    if (goodScan || ship.Sector.AdjacentTo(sc))
                        Game.Console.Write("{0} ", galaxy.CurrentQuadrant[sc].Symbol);
                    else
                        //otherwise sector is unknown.
                        Game.Console.Write("- ");
                }//for ix

                //if the "no" paramater was NOT speicified, print the data item on the right side.
                if (rightSide)
                    PrintStatusItem(game, (RequestTypeEnum)iy);
                else
                    Game.Console.Skip(1);

            }//for iy

            //if the optional "chart" paramater was specified, print a galactic chart
            if (printChart)
                Chart(false, game);

        }//ShortRangeScan

        /// <summary>
        /// Request a piece of information about the game. The 10 pieces of information are
        /// defined in the RequestTypeEnum defined above. The actual name of the enum is used for parsing
        /// the request.
        /// ie:  Request da
        ///      Request date
        /// Both these requests are the same
        /// </summary>
        /// <param name="game"></param>
        public static void Request(GameData game)
        {
            object tok;
            while ((tok = Game.Console.scan()) == null)
                Game.Console.Write("Information desired? ");

            Game.Console.chew();
            string[] strs = Enum.GetNames(typeof(RequestTypeEnum));

            RequestTypeEnum request = RequestTypeEnum.None;
            if (tok is string)
            {
                string str = (tok as string).Trim().ToLower();

                //check the enum names for a match. Note we start at index 1 to skip the None member.
                for (int mm = 1; mm < strs.Length; mm++)
                {
                    if (SSTConsole.isit(str, strs[mm].ToLower()))
                    {
                        request = (RequestTypeEnum)mm;
                        break;
                    }//if
                }//for mm
            }//if

            if (request == RequestTypeEnum.None)
            {
                string str = string.Format("UNRECOGNIZED REQUEST. Legal requests are:\n" +
                    "  {0}, {1}, {2}, {3}," +
                    " {4},\n  {5}, {6}, {7}," +
                    " {8}, {9}.", strs[1].ToLower(), strs[2].ToLower(), strs[3].ToLower(), strs[4].ToLower(),
                                  strs[5].ToLower(), strs[6].ToLower(), strs[7].ToLower(), strs[8].ToLower(),
                                  strs[9].ToLower(), strs[10].ToLower()
                );
                Game.Console.WriteLine(str);
                return;
            }//if

            Game.Console.chew();
            Game.Console.Skip(1);
            PrintStatusItem(game, request);

        }//Request

        /// <summary>
        /// Prints the status of the ship. The 10 types of information is defined in the RequestTypeEnum defined above.
        /// </summary>
        /// <param name="game"></param>
        public static void Status(GameData game)
        {
            Game.Console.chew();
            Game.Console.Skip(1);

            for (int iy = 1; iy <= 10; iy++)
                PrintStatusItem(game, (RequestTypeEnum)iy);

        }//Status

        /// <summary>
        /// Print a general report about the game so far.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="revealPassword"></param>
        public static void Report(GameData game, bool revealPassword)
        {
            Game.Console.chew();

            Game.Console.WriteLine("\nYou {0} playing a {1}{2} {3} game.", game.Turn.alldone ? "were" : "are now",
                game.GameThawed ? "thawed " : "",
                game.GameLength.ToString().ToLower(),
                game.GameSkill.ToString().ToLower());

            if (game.GameSkill > GameData.GameSkillEnum.Good && game.GameThawed && !game.Turn.alldone)
                Game.Console.WriteLine("No plaque is allowed.");

            if (game.GameTourn != 0)
                Game.Console.WriteLine("This is tournament game {0}.", game.GameTourn);

            if (revealPassword)
                Game.Console.WriteLine("Your secret password is \"{0}\"", game.GamePassword);

            Game.Console.Write("{0} of {1} Klingons have been killed", (game.KlingonsKilled + game.CommandersKilled + game.SuperCommandersKilled), game.Galaxy._inkling);

            if (game.CommandersKilled > 0)
                Game.Console.WriteLine(", including {0} Commander{1}.", game.CommandersKilled, game.CommandersKilled == 1 ? "" : "s");
            else if ((game.KlingonsKilled + game.SuperCommandersKilled) > 0)
                Game.Console.WriteLine(", but no Commanders.");
            else
                Game.Console.WriteLine(".");

            if ((int)game.GameSkill > 2)
                Game.Console.WriteLine("The Super Commander has {0}been destroyed.", (game.Galaxy.SuperCommander != null) ? "not " : "");

            int basesKilled = game.Galaxy._inbase - game.Galaxy.Bases.Count;
            if (basesKilled > 0)
            {
                Game.Console.Write("There ");
                if (basesKilled == 1)
                    Game.Console.Write("has been 1 base");
                else
                    Game.Console.Write("have been {0} bases", basesKilled);

                Game.Console.WriteLine(" destroyed, {0} remaining.", game.Galaxy.Bases.Count);

            }//if
            else
            {
                Game.Console.WriteLine("There are {0} bases.", game.Galaxy.Bases.Count);
            }

            FederationShip ship = game.Galaxy.Ship;
            if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) || ship.Docked || game.Turn.iseenit)
            {
                AttackReport(game);
                game.Turn.iseenit = true;
            }//if

            if (game.Casualties > 0)
                Game.Console.WriteLine("{0} casualt{1} suffered so far.", game.Casualties, (game.Casualties == 1) ? "y" : "ies");

            if (game.CallsForHelp > 0)
                Game.Console.WriteLine("There were {0} call{1} for help.", game.CallsForHelp, (game.CallsForHelp == 1) ? "" : "s");

            if (ship.HasProbes)
                Game.Console.WriteLine("You have {0} deep space probe{1}.", ship.Probes > 0 ? ship.Probes.ToString() : "no", ship.Probes != 1 ? "s" : "");

            if ((!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) || ship.Docked) && game.Galaxy.Probe != null)
                Game.Console.WriteLine("{0} deep space probe is in{1}.", game.Galaxy.Probe.Armed ? "An armed" : "A", game.Galaxy.Probe.QuadrantCoordinate.ToString(true));

            if (ship.Crystals)
            {
                int uses = ship.CrystalUses;
                if (uses == 0)
                    Game.Console.WriteLine("Dilithium crystals aboard ship...not yet used.");
                else
                    Game.Console.WriteLine("Dilithium crystals have been used {0} time{1}.", uses, uses == 1 ? "" : "s");
            }
            Game.Console.Skip(1);
        }//Report

        /// <summary>
        /// Print a damage report about the damaged ship devices.
        /// </summary>
        /// <param name="game"></param>
        public static void DamageReport(GameData game)
        {
            Game.Console.chew();
            game.Galaxy.Ship.ShipDevices.DamageReport();
        }//DamageReport

        /// <summary>
        /// Print a galactic chart of all the known quadrants of the game.
        /// </summary>
        /// <param name="starChart"></param>
        /// <param name="game"></param>
        public static void Chart(bool starChart, GameData game)
        {
            Game.Console.chew();
            FederationShip ship = game.Galaxy.Ship;

            Game.Console.Skip(1);
            if (ship.StarChartDamage != FutureEvents.NEVER && ship.StarChartDamage != game.Date && ship.Docked)
                Game.Console.WriteLine("Spock-  \"I revised the Star Chart from the\n  starbase's records.\"\n");

            if (starChart)
                Game.Console.WriteLine("STAR CHART FOR THE KNOWN GALAXY");

            if (ship.StarChartDamage != FutureEvents.NEVER)
            {
                if (ship.Docked)
                {
                    //We are docked, so restore chart from base information
                    ship.StarChartDamage = game.Date;
                    game.Galaxy.UpdateStarChart(false);
                }//if
                else
                {
                    Game.Console.WriteLine("(Last surveillance update {0,0:F1} stardates ago.)", game.Date - ship.StarChartDamage);
                }//else
            }//if

            if (starChart)
                Game.Console.Skip(1);

            Game.Console.WriteLine("      1    2    3    4    5    6    7    8");
            Game.Console.WriteLine("    ----------------------------------------");

            if (starChart)
                Game.Console.WriteLine("  -");

            for (int ix = 1; ix <= Galaxy.Galaxy.GALAXYWIDTH; ix++)
            {
                Game.Console.Write("{0} -", ix);
                for (int iy = 1; iy <= Galaxy.Galaxy.GALAXYHEIGHT; iy++)
                {
                    int starch = game.Galaxy[ix, iy].Starch;
                    if (starch < 0)
                        Game.Console.Write("  .1.");
                    else if (starch == 0)
                        Game.Console.Write("  ...");
                    else if (starch > 999)
                        Game.Console.Write("{0,5}", starch - 1000);
                    else
                        Game.Console.Write("{0,5}", game.Galaxy[ix, iy].ToInt);
                }//for iy
                Game.Console.WriteLine("  -");
            }//for ix

            if (starChart)
                Game.Console.WriteLine("\n{0} is currently in{1}", ship.Name, ship.QuadrantCoordinate.ToString(true));

        }//Chart

        /// <summary>
        /// Perform a long range scan of the quadrants around the ship.
        /// If the lrsensors are damaged no scan takes place, unless docked
        /// at a starbase in which case the starbase sensors are used.
        /// </summary>
        /// <param name="galaxy"></param>
        public static void LongRangeScan(Galaxy.Galaxy galaxy)
        {
            Game.Console.chew();
            FederationShip ship = galaxy.Ship;

            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.LRSensors))
            {
                //Now allow base's sensors if docked
                if (!ship.Docked)
                {
                    Game.Console.WriteLine("LONG-RANGE SENSORS DAMAGED.");
                    return;
                }//if
                Game.Console.Write("\nStarbase's long-range scan for");
            }//if
            else
            {
                Game.Console.Write("\nLong-range scan for");
            }//else

            Galaxy.QuadrantCoordinate quad = ship.QuadrantCoordinate;
            Game.Console.WriteLine(quad.ToString(true));
            for (int ix = quad.X - 1; ix <= quad.X + 1; ix++)
            {
                for (int iy = quad.Y - 1; iy <= quad.Y + 1; iy++)
                {
                    QuadrantCoordinate qc = new QuadrantCoordinate(ix, iy);
                    if (!qc.Valid)
                        Game.Console.Write("   -1");
                    else
                    {
                        Game.Console.Write("{0,5}", galaxy[qc].ToInt);
                        galaxy[qc].Starch = ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) ? galaxy[qc].ToInt + 1000 : 1;
                    }//else
                }//for iy
                Game.Console.Skip(1);
            }//for ix

        }//lrscan

        /// <summary>
        /// Print an attack report if a Commander or SuperCommander are currently
        /// attacking a starbase.
        /// </summary>
        /// <param name="game"></param>
        public static void AttackReport(GameData game)
        {
            if (game.Future[FutureEvents.EventTypesEnum.FCDBAS] < FutureEvents.NEVER)
            {
                Game.Console.WriteLine("Starbase in {0} is currently under attack.", game.Galaxy.CommanderAttack.ToString(true));
                Game.Console.WriteLine("It can hold out until Stardate {0,0:F1}.", game.Future[FutureEvents.EventTypesEnum.FCDBAS]);
            }

            if (game.Galaxy.SuperCommanderAttack != null)
            {
                Game.Console.WriteLine("Starbase in {0} is under Super-commander attack.", game.Galaxy.SuperCommanderAttack.ToString(true));
                Game.Console.WriteLine("It can hold out until Stardate {0,0:F1}.", game.Future[FutureEvents.EventTypesEnum.FSCDBAS]);
            }
        }//AttackReport

        /// <summary>
        /// Helper function to print an information item. The types of information are defined in 
        /// RequestTypeEnum defined above.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="item"></param>
        private static void PrintStatusItem(GameData game, RequestTypeEnum item)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;
            QuadrantCoordinate shipQuadrant = ship.QuadrantCoordinate;

            switch (item)
            {
                case RequestTypeEnum.Date:
                    Game.Console.Write(" Stardate      {0,0:F1}", game.Date);
                    break;
                case RequestTypeEnum.Condition:
                    Game.Console.Write(" Condition     {0}", ship.Condition(galaxy[shipQuadrant]));
                    break;
                case RequestTypeEnum.Position:
                    Game.Console.Write(" Position     {0},{1}", shipQuadrant.ToString(false), ship.Sector.ToString(false));
                    break;
                case RequestTypeEnum.LSupport:
                    Game.Console.Write(" Life Support  ");
                    if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.LifeSupport))
                    {
                        if (ship.Docked)
                            Game.Console.Write("DAMAGED, supported by starbase");
                        else
                            Game.Console.Write("DAMAGED, reserves={0,4:F2}", ship.LifeSupportReserves);
                    }//if
                    else
                        Game.Console.Write("ACTIVE");
                    break;
                case RequestTypeEnum.Warpfactor:
                    Game.Console.Write(" Warp Factor   {0,0:F1}", ship.Warp);
                    break;
                case RequestTypeEnum.Energy:
                    Game.Console.Write(" Energy        {0,0:F2}", ship.ShipEnergy);
                    break;
                case RequestTypeEnum.Torpedoes:
                    Game.Console.Write(" Torpedoes     {0}", ship.Torpedoes);
                    break;
                case RequestTypeEnum.Shields:
                    Game.Console.Write(" Shields       ");
                    if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Shields))
                        Game.Console.Write("DAMAGED,");
                    else if (ship.ShieldsUp)
                        Game.Console.Write("UP,");
                    else
                        Game.Console.Write("DOWN,");

                    Game.Console.Write(" {0}% {1,0:F1} units", (int)((100.0 * ship.ShieldEnergy) / ship.InitialShieldEnergy + 0.5), ship.ShieldEnergy);
                    break;
                case RequestTypeEnum.Klingons:
                    Game.Console.Write(" Klingons Left {0}", galaxy.Klingons);
                    break;
                case RequestTypeEnum.Time:
                    Game.Console.Write(" Time Left     {0,0:F2}", game.RemainingTime);
                    break;
            }//switch item
            Game.Console.Skip(1);
        }

    }//class Reports
}