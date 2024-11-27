using System;
using sstNET.Galaxy.SectorObjects;
using sstNET.Galaxy.SectorObjects.Ships;

using sstNET.Galaxy.QuadrantObjects;

namespace sstNET
{
    /// <summary>
    /// This class handles all matters related to Planets. This includes orbitting
    /// them, shuttling to/from a planet and using the transporter to travel to/from a planet.
    /// It also handles the mining and using of dilithium crystals,
    /// the sensor command to scan a planet and the planet report.
    /// </summary>
    internal static class Planets
    {
        /// <summary>
        /// Use the dilithium crystals that were previously mined.
        /// Adds a shitload of energy to enterprise. Small chance that
        /// something will go wrong ...
        /// The chance of failure increases each time crytals are used.
        /// Crystal use is reset to 0 when new ones are mined.
        /// </summary>
        /// <param name="game"></param>
        internal static void UseCrystals(GameData game)
        {
            Game.Console.Skip(1);
            Game.Console.chew();

            FederationShip ship = game.Galaxy.Ship;

            //check if we have any crystals on board.
            if (!ship.Crystals)
            {
                Game.Console.WriteLine("No dilithium crystals available.");
                return;
            }//if

            //can only use crystals if energy is low.
            if (ship.ShipEnergy >= FederationShip.YellowAlertEnergyLevel)
            {
                Game.Console.WriteLine("Spock-  \"Captain, Starfleet Regulations prohibit such an operation");
                Game.Console.WriteLine("  except when condition Yellow exists.");
                return;
            }//if

            //make sure Kirk is sure, this can be dangerous!
            Game.Console.WriteLine("Spock- \"Captain, I must warn you that loading");
            Game.Console.WriteLine("  raw dilithium crystals into the ship's power");
            Game.Console.WriteLine("  system may risk a severe explosion.");
            Game.Console.Write("  Are you sure this is wise?\" ");
            if (!Game.Console.ja())
            {
                Game.Console.chew();
                return;
            }//if

            //give it a go
            Game.Console.WriteLine("\nEngineering Officer Scott-  \"(GULP) Aye Sir.");
            Game.Console.WriteLine("  Mr. Spock and I will try it.\"");
            Game.Console.WriteLine("\nSpock-  \"Crystals in place, Sir.");
            Game.Console.WriteLine("  Ready to activate circuit.\"");
            Game.Console.WriteLine("\nScotty-  \"Keep your fingers crossed, Sir!\"\n");

            //check if failure will occur. Current probability of failure is property of ship
            //which is dependant on how many times the crystals have been used in the past
            if (game.Random.Rand() <= ship.CrystalProbability)
            {
                //bad news ....
                Game.Console.WriteLine("  \"Activating now! - - No good!  It's***");
                Game.Console.WriteLine("\n\n***RED ALERT!  RED A*L********************************\n");
                Game.Console.stars();
                Game.Console.WriteLine("******************   KA-BOOM!!!!   *******************\n");
                Finish.Kaboom(game);
                return;
            }//if

            //looks like success. Increment ship power by a random amount and increment number of uses
            ship.ShipEnergy += 5000.0 * (1.0 + 0.9 * game.Random.Rand());
            Game.Console.WriteLine("  \"Activating now! - - ");
            Game.Console.WriteLine("The instruments");
            Game.Console.WriteLine("   are going crazy, but I think it's");
            Game.Console.WriteLine("   going to work!!  Congratulations, Sir!\"");

            //increment crystal usage count. Future uses are more dangerous
            //(unless new crystals mined again).
            ship.CrystalUses++;

        }//UseCrystals

        /// <summary>
        /// Attempt to enter into orbit around a planet.
        /// The ship must have operational warp or impluse engines and be
        /// adjacent to a planet to succeed.
        /// </summary>
        /// <param name="game"></param>
        internal static void Orbit(GameData game)
        {
            FederationShip ship = game.Galaxy.Ship;

            Game.Console.Skip(1);
            Game.Console.chew();
            game.Turn.ididit = false;

            //if already in orbit, print info and return
            if (ship.Orbit != null)
            {
                Game.Console.WriteLine("Already in standard orbit.");
                return;
            }//if

            //If neither the warp or impluse engines are working, we can't enter orbit
            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.WarpEngines) && ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.ImpulseEngines))
            {
                Game.Console.WriteLine("Both warp and impulse engines damaged.");
                return;
            }//if

            //check if adjacent to a planet. If not, print error and return
            SectorPlanet planet = game.Galaxy.CurrentQuadrant.SectorPlanet;
            if (planet == null || !ship.Sector.AdjacentTo(planet.Sector))
            {
                Game.Console.WriteLine("{0} not adjacient to planet.\n", ship.Name);
                return;
            }//if

            //compute a random time amount, print info and make sure ship is undocked
            //in case we were docked already.
            game.Turn.Time = 0.02 + 0.03 * game.Random.Rand();
            Game.Console.WriteLine("Helmsman Sulu-  \"Entering standard orbit, Sir.\"");
            ship.Docked = false;

            //eat the time amount, may return if game ends or ship gets tractor beamed
            if (consumeTime(game))
                return;

            //ok, we are entering orbit around the planet
            ship.Orbit = planet;

            //print entering orbit message
            //note - height is actually an int but we report it as a double to 2 digits
            //this is to be compatible with original
            ship.OrbitHeight = (int)(1400.0 + 7200.0 * game.Random.Rand());
            Game.Console.WriteLine("Sulu-  \"Entered orbit at altitude {0,0:F2} kilometers.\"", (double)ship.OrbitHeight);

        }//Orbit


        /// <summary>
        /// Shuttle the crew from the ship to planet surface OR planet surface to ship.
        /// Note that the FQ has no shuttle bay.
        /// Its possible to leave shuttle on the planet and come back later to get it
        /// </summary>
        /// <param name="game"></param>
        internal static void Shuttle(GameData game)
        {
            FederationShip ship = game.Galaxy.Ship;

            Game.Console.chew();
            Game.Console.Skip(1);
            game.Turn.ididit = false;

            if (ship.Orbit != null && ship.Orbit.GalileoPresent && !ship.HasShuttleBay)
                Game.Console.WriteLine("Ye Faerie Queene has no shuttle craft bay to dock it at.");
            else if (!ship.HasShuttleBay)
                Game.Console.WriteLine("Ye Faerie Queene had no shuttle craft.");
            else if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.ShuttleCraft))
                Game.Console.WriteLine("The Galileo is damaged.");
            else if (ship.ShuttleLocation == FederationShip.ShuttleLocationEnum.Aliens)
                Game.Console.WriteLine("Shuttle craft is now serving Big Mac's.");
            else if (ship.Orbit == null)
                Game.Console.WriteLine(ship.Name + " not in standard orbit.");
            else if (ship.ShuttleLocation != FederationShip.ShuttleLocationEnum.ShuttleBay && !ship.Orbit.GalileoPresent)
                Game.Console.WriteLine("Shuttle craft not currently available.");
            else if (ship.CrewLocation == FederationShip.CrewLocationEnum.Ship && ship.ShuttleLocation == FederationShip.ShuttleLocationEnum.Planet)
                Game.Console.WriteLine("You will have to beam down to retrieve the shuttle craft.");
            else if (ship.ShieldsUp || ship.Docked)
                Game.Console.WriteLine("Shuttle craft cannot pass through shields.");
            else if (!ship.Orbit.Known)
            {
                Game.Console.WriteLine("Spock-  \"Captain, we have no information on this planet");
                Game.Console.WriteLine("  and Starfleet Regulations clearly state that in this situation");
                Game.Console.WriteLine("  you may not fly down.\"");
            }
            else
            {
                //Compute a time based on the orbit height
                game.Turn.Time = 3.0e-5 * ship.OrbitHeight;

                //and check if time required exceeds available time
                if (game.Turn.Time >= 0.8 * game.RemainingTime)
                {
                    Game.Console.WriteLine("First Officer Spock-  \"Captain, I compute that such");
                    Game.Console.WriteLine("  a maneuver would require aproximately ");
                    Game.Console.WriteLine("{0,0:F4}", 100 * game.Turn.Time / game.RemainingTime);
                    Game.Console.WriteLine("% of our");
                    Game.Console.WriteLine("remaining time.");
                    Game.Console.WriteLine("Are you sure this is wise?\" ");
                    if (!Game.Console.ja())
                    {
                        game.Turn.Time = 0.0;
                        return;
                    }//if
                }//if

                if (ship.CrewLocation == FederationShip.CrewLocationEnum.Planet)
                {//Kirk on planet
                    if (ship.ShuttleLocation == FederationShip.ShuttleLocationEnum.ShuttleBay)
                    {//Galileo on ship!
                        if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Transporter))
                        {
                            Game.Console.Write("Spock-  \"Would you rather use the transporter?\" ");
                            if (Game.Console.ja())
                            {
                                Beam(game);
                                return;
                            }
                            Game.Console.Write("Shuttle crew");
                        }
                        else
                            Game.Console.Write("Rescue party");

                        Game.Console.WriteLine(" boards Galileo and swoops toward planet surface.\n");

                        ship.ShuttleLocation = FederationShip.ShuttleLocationEnum.Planet;
                        ship.Orbit.GalileoPresent = true;
                        if (consumeTime(game))
                            return;

                        Game.Console.WriteLine("Trip complete.");
                        return;
                    }
                    else
                    {//Ready to go back to ship
                        Game.Console.WriteLine("You and your mining party board the");
                        Game.Console.WriteLine("shuttle craft for the trip back to the Enterprise.\n");
                        Game.Console.WriteLine("The short hop begins . . .");

                        ship.Orbit.Known = true;
                        ship.CrewLocation = FederationShip.CrewLocationEnum.Shuttle;
                        Game.Console.Skip(1);
                        if (consumeTime(game))
                            return;

                        ship.CrewLocation = FederationShip.CrewLocationEnum.Ship;
                        ship.ShuttleLocation = FederationShip.ShuttleLocationEnum.ShuttleBay;
                        ship.Orbit.GalileoPresent = false;

                        if (ship.CrystalsMined)
                        {
                            ship.Crystals = true;
                            ship.CrystalUses = 0;
                        }
                        ship.CrystalsMined = false;
                        Game.Console.WriteLine("Trip complete.");
                        return;
                    }
                }
                else
                {//Kirk on ship  and so is Galileo
                    Game.Console.WriteLine("Mining party assembles in the hangar deck,");
                    Game.Console.WriteLine("ready to board the shuttle craft \"Galileo\".\n");
                    Game.Console.WriteLine("The hangar doors open; the trip begins.\n");
                    if (consumeTime(game))
                        return;

                    ship.ShuttleLocation = FederationShip.ShuttleLocationEnum.Planet;
                    ship.CrewLocation = FederationShip.CrewLocationEnum.Planet;
                    ship.Orbit.GalileoPresent = true;
                    Game.Console.WriteLine("Trip complete");
                    return;
                }//else
            }//else
        }//Shuttle

        /// <summary>
        /// Kirk and crew are about to transport either to the ship (from the planet)
        /// or to the planet(from the ship).
        /// If we attempt to transport to the ship from the planet, but the crew got to the
        /// planet via the shuttle, then make sure that is the intent. If so the shuttle
        /// will remain on the planet where it can be retrieved later if desired.
        /// </summary>
        /// <param name="game"></param>
        internal static void Beam(GameData game)
        {
            FederationShip ship = game.Galaxy.Ship;

            Game.Console.chew();
            Game.Console.Skip(1);
            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Transporter))
            {
                Game.Console.WriteLine("Transporter damaged.");
                if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.ShuttleCraft) && (ship.CrewLocation == FederationShip.CrewLocationEnum.Planet || ship.ShuttleLocation == FederationShip.ShuttleLocationEnum.Planet))
                {
                    Game.Console.WriteLine("\nSpock-  \"May I suggest the shuttle craft, Sir?\" ");
                    if (Game.Console.ja())
                        Shuttle(game);
                }
                return;
            }//if

            SectorPlanet planet = ship.Orbit;
            if (planet == null)
            {
                Game.Console.WriteLine("{0} not in standard orbit.", ship.Name);
                return;
            }
            if (ship.ShieldsUp)
            {
                Game.Console.WriteLine("Impossible to transport through shields.");
                return;
            }
            if (!planet.Known)
            {
                Game.Console.WriteLine("Spock-  \"Captain, we have no information on this planet");
                Game.Console.WriteLine("  and Starfleet Regulations clearly state that in this situation");
                Game.Console.WriteLine("  you may not go down.\"");
                return;
            }

            if (ship.CrewLocation == FederationShip.CrewLocationEnum.Planet)
            {//Coming from planet to the ship
                if (ship.ShuttleLocation == FederationShip.ShuttleLocationEnum.Planet && planet.GalileoPresent)
                {
                    Game.Console.Write("Spock-  \"Wouldn't you rather take the Galileo?\" ");
                    if (Game.Console.ja())
                    {
                        Game.Console.chew();
                        return;
                    }//if
                    Game.Console.WriteLine("Your crew hides the Galileo to prevent capture by aliens.");
                }//if
                Game.Console.WriteLine("Landing party assembled, ready to beam up.\n");
                Game.Console.WriteLine("Kirk whips out communicator...");
                Game.Console.WriteLine("BEEP  BEEP  BEEP\n\n");
                Game.Console.WriteLine("\"Kirk to enterprise-  Lock on coordinates...energize.\"");
            }//if
            else
            {//Going to planet from the ship
                if (!planet.Crystals)
                {
                    Game.Console.WriteLine("Spock-  \"Captain, I fail to see the logic in");
                    Game.Console.WriteLine("  exploring a planet with no dilithium crystals.");
                    Game.Console.Write("  Are you sure this is wise?\" ");
                    if (!Game.Console.ja())
                    {
                        Game.Console.chew();
                        return;
                    }//if
                }//if
                Game.Console.WriteLine("Scotty-  \"Transporter room ready, Sir.\"\n");
                Game.Console.WriteLine("Kirk, and landing party prepare to beam down to planet surface.\n");
                Game.Console.WriteLine("Kirk-  \"Energize.\"");
            }//else

            Game.Console.WriteLine("\nWWHOOOIIIIIRRRRREEEE.E.E.  .  .  .  .   .    .\n\n");
            if (game.Random.Rand() > 0.98)
            {
                Game.Console.WriteLine("BOOOIIIOOOIIOOOOIIIOIING . . .\n\n");
                Game.Console.WriteLine("Scotty-  \"Oh my God!  I've lost them.\"");
                Finish.finish(Finish.FINTYPE.FLOST, game);
                return;
            }//if

            Game.Console.WriteLine(".    .   .  .  .  .  .E.E.EEEERRRRRIIIIIOOOHWW\n\n");
            Game.Console.WriteLine("Transport complete.");

            ship.CrewLocation = (ship.CrewLocation == FederationShip.CrewLocationEnum.Planet) ? FederationShip.CrewLocationEnum.Ship : FederationShip.CrewLocationEnum.Planet;

            if (ship.CrewLocation == FederationShip.CrewLocationEnum.Planet && planet.GalileoPresent)
                Game.Console.WriteLine("The shuttle craft Galileo is here!");

            if (ship.CrewLocation == FederationShip.CrewLocationEnum.Ship && ship.CrystalsMined)
            {
                ship.Crystals = true;
                ship.CrystalUses = 0;
            }
            ship.CrystalsMined = false;

        }//Beam

        /// <summary>
        /// Helper function to use up time.
        /// Process events.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static bool consumeTime(GameData game)
        {
            game.Turn.ididit = true;
            Events.ProcessEvents(game);
            return (game.Turn.alldone || game.Galaxy[game.Galaxy.Ship.GalacticCoordinate].SuperNova || game.Turn.justin);
        }

        /// <summary>
        /// Mine crystals on the planet surface.
        /// </summary>
        /// <param name="game"></param>
        internal static void Mine(GameData game)
        {
            FederationShip ship = game.Galaxy.Ship;

            Game.Console.Skip(1);
            Game.Console.chew();
            game.Turn.ididit = false;

            //make sure crew is actually on a planet
            if (ship.CrewLocation != FederationShip.CrewLocationEnum.Planet)
            {
                Game.Console.WriteLine("Mining party not on planet.");
                return;
            }

            //and make sure this planet actually has crystals
            if (!ship.Orbit.Crystals)
            {
                Game.Console.WriteLine("No dilithium crystals on this planet.");
                return;
            }

            //check if we already have crystals mined
            if (ship.CrystalsMined)
            {
                Game.Console.WriteLine("You've already mined enough crystals for this trip.");
                return;
            }

            //and check if we have crystals on the ship already
            if (ship.Crystals && ship.CrystalUses == 0)
            {
                Game.Console.WriteLine("With all those fresh crystals aboard the {0}", ship.Name);
                Game.Console.WriteLine("there's no reason to mine more at this time.");
                return;
            }

            //eat some time to mine
            game.Turn.Time = (0.1 + 0.2 * game.Random.Rand()) * (ship.Orbit.Class + 1);
            if (consumeTime(game))
                return;

            //ok, crystals mined
            Game.Console.WriteLine("Mining operation complete.");
            ship.CrystalsMined = true;

        }//Mine

        /// <summary>
        /// The SENSOR command for the game. If a planet is in the same
        /// quadrant as the ship, the planet will become known.
        /// </summary>
        /// <param name="galaxy"></param>
        internal static void Sensor(Galaxy.Galaxy galaxy)
        {
            FederationShip ship = galaxy.Ship;
            //Quadrant quad = galaxy[ship.GalacticCoordinate];

            Game.Console.Skip(1);
            Game.Console.chew();

            //can't sensor the planet if sensors are damaged
            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SRSensors))
            {
                Game.Console.WriteLine("Short range sensors damaged.");
                return;
            }//if

            //get the current planet if it exists, if not we cannot scan it
            SectorPlanet planet = galaxy.CurrentQuadrant.SectorPlanet;
            if (planet == null)
            {
                Game.Console.WriteLine("No planet in this quadrant.");
                return;
            }//if

            //output some stats about the planet
            Game.Console.WriteLine("Spock-  \"Sensor scan for{0}-\n", ship.GalacticCoordinate.QuadrantCoordinate.ToString(true));
            Game.Console.WriteLine("         Planet at{0} is of class {1}.", planet.Sector.ToString(true), planet.ToChar);

            //if the galileo is on the planet surface, let the ship know
            if (ship.ShuttleLocation == FederationShip.ShuttleLocationEnum.Planet && planet.GalileoPresent)
                Game.Console.WriteLine("         Sensors show Galileo still on surface.");

            //dilithium crystals may be present
            Game.Console.WriteLine("         Readings indicate{0} dilithium crystals present.\"", planet.Crystals ? "" : " no");

            //mark this planet as known
            galaxy.CurrentQuadrant.SectorPlanet.Known = true;

        }//Sensor

        /// <summary>
        /// Outputs a report on the known planets in the galaxy. Initially
        /// all planets are unknown and are discovered as the ship visits
        /// each quadrant where one exists. The ship must use the "SENSOR"
        /// command to scan the planet for it to be known. Alternatively
        /// the ship can orbit the planet and visit the surface to be known.
        /// </summary>
        /// <param name="galaxy"></param>
        internal static void PlanetReport(GameData game)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;

            Game.Console.chew();
            Game.Console.WriteLine("\nSpock-  \"Planet report follows, Captain.\"\n");

            bool iknow = false;
            foreach (QuadrantPlanet planet in galaxy.Planets)
            {
                if (!planet.Known && !GameData.DEBUGME)
                    continue;

                iknow = true;
                Game.Console.WriteLine("{0}   class {1}   {2}dilithium crystals present.", planet.QuadrantCoordinate.ToString(true), planet.ToChar, planet.Crystals ? "" : "no ");

                if(galaxy.Ship.ShuttleLocation == FederationShip.ShuttleLocationEnum.Planet && planet.GalileoPresent)
                    Game.Console.WriteLine("    Shuttle Craft Galileo on surface.");

            }//foreach
            if (!iknow)
                Game.Console.WriteLine("No information available.");

        }//PlanetReport

    }//class Planets
}