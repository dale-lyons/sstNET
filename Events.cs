using System;
using System.Collections.Generic;
using System.Text;

using sstNET.Galaxy;
using sstNET.Galaxy.SectorObjects;
using sstNET.Galaxy.QuadrantObjects;
using sstNET.Galaxy.SectorObjects.Ships;

namespace sstNET
{
    internal static class Events
    {
        /// <summary>
        /// A star has nova'd so lets handle it.
        /// The star is going to affect the 8 sectors surrounding it. Any object in any of those
        /// sectors will be damaged. If another star happens to be in one of the 8 sectors, then it to
        /// will nova, setting up a possible chain-reaction. This would be a nice recursive problem
        /// except we need to mimic the original order of novas using a stack based method.
        /// There is a random chance that any star going nova will super-nova. This is handled in another
        /// function(snova).
        /// </summary>
        /// <param name="ixy">The original star location</param>
        /// <param name="game"></param>
        internal static void Nova(GameData game, Star star)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;

            List<Star> starObjects = new List<Star>();
            starObjects.Add(star);
            ship.ResetBuffettedCount();

            //handle initial nova
            bool gameOver;
            star.Nova(game, star.Sector, out gameOver);
            if (gameOver)
                return;

            //Any stars that were adjacent to the original star will also nova, and any star
            //adjacent to that one as well. Possible chain reaction ....
            while (starObjects.Count > 0)
            {
                Star starObject = starObjects[0];
                starObjects.RemoveAt(0);

                //check the sectors around the star. Anything there will take damage.
                //If it is another star, add it to the list to be blown up.
                foreach (SectorCoordinate sc in starObject.Sector.AdjacentSectors)
                {
                    //ignore invalid sectors (out of the quadrant).
                    if (!sc.Valid)
                        continue;

                    //get the sector object at the adjacent sector and Nova it ....
                    SectorObject so = game.Galaxy.CurrentQuadrant[sc];
                    if (so.Nova(game, starObject.Sector, out gameOver))
                        starObjects.Add(so as Star);

                    //if any star super-nova we are done.
                    if (gameOver)
                        return;

                }//foreach
            }//while

            //perform any buffetting action on the ship if it was affected by any of the Novas
            ship.FinalNovaBuffet(game);

        }//Nova

        /// <summary>
        /// A starbase has been destroyed by a Commander or SuperCommander attack.
        /// (Not by user action).
        /// Remove it from the galaxy and inform the user.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="qc">The quadrant of the destroyed starbase</param>
        /// <param name="SC">true if killed by super-commander</param>
        private static void destroyStarBase(Galaxy.Galaxy galaxy, QuadrantCoordinate qc, bool SC)
        {
            FederationShip ship = galaxy.Ship;

            if (galaxy[qc].Starch == -1)
                galaxy[qc].Starch = 0;

            //Handle case where base is in same quadrant as starship
            if (qc.Equals(ship.QuadrantCoordinate))
            {
                if (galaxy[qc].Starch > 999)
                    galaxy[qc].Starch -= 10;

                //remove starbase from current quadrant
                galaxy.CurrentQuadrant[galaxy.CurrentQuadrant.SectorStarBase.Sector] = new Empty();

                //just in case we were docked at the time
                ship.Docked = false;

                //give kirk the bad news
                Game.Console.WriteLine("\nSpock-  \"Captain, I believe the starbase has been destroyed.\"");
            }
            else if ((galaxy.Bases.Count > 1) && (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) || ship.Docked))
            {
                //Get word via subspace radio
                Game.Console.WriteLine("\nLt. Uhura-  \"Captain, Starfleet Command reports that");
                Game.Console.WriteLine("   the starbase in{0} has been destroyed by", qc.ToString(true));
                if (SC)
                    Game.Console.WriteLine("the Klingon Super-Commander");
                else
                    Game.Console.WriteLine("a Klingon Commander");

            }//else if

            //Remove Starbase from galaxy
            galaxy[qc].Base = null;

            //todo - figure this out??
            //if (isatb == 2)
            //{
            //    /* reinstate a commander's base attack */
            //    batx = ixhold;
            //    baty = iyhold;
            //    isatb = 0;
            //}
            //else
            //{
            //    batx = baty = 0;
            //}
        }//destroyStarBase

        /// <summary>
        /// Some time has elapsed so lets process events from the FutureEvents list.
        /// We will continue to process events until the amount of time specified has elasped
        /// or the game ends.
        /// </summary>
        /// <param name="game"></param>
        internal static void ProcessEvents(GameData game)
        {
            FederationShip ship = game.Galaxy.Ship;

            game.Future.Dump(game.Random);
            if (ship.StarChartDamage == FutureEvents.NEVER && ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio))
            {
                //chart will no longer be updated because radio is dead
                ship.StarChartDamage = game.Date;
                game.Galaxy.UpdateStarChart(false);
            }//if

            //compute the end date of the time period we are handling
            double fintim = game.Date + game.Turn.Time;
            bool ictbeam = false;
            bool istract = false;
            double yank = 0.0;

            //process events until done
            while (true)
            {
                //if the game has ended, we are done
                if (game.Turn.alldone)
                    break;

                //find the next scheduled event in the future list.
                FutureEvents.EventTypesEnum line;
                double datemin = game.Future.Search(fintim, out line);
                if (line == FutureEvents.EventTypesEnum.FSPY)
                    datemin = fintim;

                double xtime = datemin - game.Date;
                game.Date = datemin;

                //Decrement Federation resources and recompute remaining time
                int remcom = game.Galaxy.Commanders.Count;
                game.RemainingResources -= (game.Galaxy.Klingons + 4 * remcom) * xtime;
                game.RemainingTime = game.RemainingResources / (game.Galaxy.Klingons + 4 * remcom);

                //ooops, ran out of time!!
                if (game.RemainingTime <= 0)
                {
                    Finish.finish(Finish.FINTYPE.FDEPLETE, game);
                    return;
                }//if

                //Is life support adequate?
                if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.LifeSupport) && !ship.Docked)
                {
                    if (ship.LifeSupportReserves < xtime && ship.ShipDevices.GetDamage(ShipDevices.ShipDevicesEnum.LifeSupport) > ship.LifeSupportReserves)
                    {
                        Finish.finish(Finish.FINTYPE.FLIFESUP, game);
                        return;
                    }//if
                    ship.LifeSupportReserves -= xtime;
                    if (ship.ShipDevices.GetDamage(ShipDevices.ShipDevicesEnum.LifeSupport) <= xtime)
                        ship.LifeSupportReserves = ship.InitialLifeSupport;
                }//if

                //Fix devices
                ship.ShipDevices.Repair(ship, xtime);

                //If radio repaired, update star chart and attack reports
                if (ship.StarChartDamage != FutureEvents.NEVER && !ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio))
                {
                    ship.StarChartDamage = FutureEvents.NEVER;
                    Game.Console.WriteLine("Lt. Uhura- \"Captain, the sub-space radio is working and");
                    Game.Console.WriteLine("   surveillance reports are coming in.\n");
                    game.Galaxy.UpdateStarChart(true);

                    if (!game.Turn.iseenit)
                    {
                        Reports.AttackReport(game);
                        game.Turn.iseenit = true;
                    }
                    Game.Console.WriteLine("\n   The star chart is now up to date.\"\n");
                }//if

                //Cause extraneous event LINE to occur
                game.Turn.Time -= xtime;
                switch (line)
                {   //Supernova
                    case FutureEvents.EventTypesEnum.FSNOVA:
                        //SuperNova(new GalacticCoordinate(game.Galaxy.RandomStar(game.Random)), true, game);
                        SuperNova(game.Galaxy.RandomStar(game.Random), null, true, game);
                        game.Future[FutureEvents.EventTypesEnum.FSNOVA] = game.Date + game.Random.expran(0.5 * game.Galaxy._intime);

                        // If supernova occured in same quadrant as the ship, we are done.
                        if (game.Galaxy[ship.QuadrantCoordinate].SuperNova)
                            return;
                        break;

                    //Tractor beam
                    case FutureEvents.EventTypesEnum.FTBEAM:
                    case FutureEvents.EventTypesEnum.FSPY://Check with spy to see if S.C. should tractor beam
                        {
                            CommanderList commanders = game.Galaxy.Commanders;
                            int commander = 0;
                            if (line == FutureEvents.EventTypesEnum.FSPY)
                            {//case FSPY
                                //We will bail from this iteration if any of the following are true:
                                //1) There is no Super Commander in the game
                                //2) We are about to tractor beam the ship
                                //3) The ship is docked at a starbase
                                //4) Any starbase is under attack by the SuperCommander
                                //5) The Super Commander is in the current quadrant
                                if (game.Galaxy.SuperCommander == null || 
                                    ictbeam || istract || ship.Docked || 
                                    game.Galaxy.SuperCommanderAttack != null || 
                                    game.Galaxy.CurrentQuadrant.SuperCommander != null)
                                    return;

                                //determine if a tractor beam is going to occur ...(by the SC)
                                //todo - check this mess
                                if (game.Turn.ientesc ||
                                    (ship.ShipEnergy < 2000 && ship.Torpedoes < 4 && ship.ShieldEnergy < 1250) ||
                                    (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Phasers) &&
                                    (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.PhotonTubes) || ship.Torpedoes < 4)) ||
                                    (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Shields) && (ship.ShipEnergy < 2500 || ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Phasers)) &&
                                    (ship.Torpedoes < 5 || ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.PhotonTubes))
                                    ))
                                {
                                    istract = true;
                                    yank = game.Galaxy.SuperCommander.DistanceTo(ship.QuadrantCoordinate);
                                }
                                else
                                {
                                    return;
                                }
                            }
                            else
                            {//case FTBEAM
                                if (commanders.Count == 0)
                                {
                                    game.Future[FutureEvents.EventTypesEnum.FTBEAM] = FutureEvents.NEVER;
                                    break;
                                }//if
                                game.Galaxy.Commanders.dump();
                                //game.Galaxy.CurrentQuadrant.dumpcom(1, game);
                                commander = ((int)(game.Random.Rand() * commanders.Count + 1.0)) - 1;
                                //game.Galaxy.CurrentQuadrant.dumpcom(2, game);
                                yank = commanders[commander].QuadrantCoordinate.DistanceTo(ship.QuadrantCoordinate);

                                if (istract || ship.Docked || yank == 0)
                                {
                                    //Drats! Have to reschedule
                                    game.Future[FutureEvents.EventTypesEnum.FTBEAM] = game.Date + game.Turn.Time + game.Random.expran(1.5 * game.Galaxy._intime / commanders.Count);
                                    break;
                                }//if
                            }//else

                            //tractor beaming cases merge here(FTBEAM and FSPY)
                            game.Turn.Time = (10.0 / (7.5 * 7.5)) * yank;
                            //game.Turn.Time = GameData.TurnInfo.ComputeTime(yank, 7.5); //warp 7.5 is yank rate.

                            ictbeam = true;
                            Game.Console.WriteLine("\n***{0} caught in long range tractor beam--", ship.Name);
                            //If Kirk & Co. screwing around on planet, handle
                            Moving.atover(game, true);//true is Grab
                            if (game.Turn.alldone)
                                return;

                            //Check if the crew is stuck in the shuttle. If so, game over
                            if (ship.CrewLocation == FederationShip.CrewLocationEnum.Shuttle)
                            {
                                Finish.finish(Finish.FINTYPE.FSTRACTOR, game);
                                return;
                            }//if

                            //Check to see if shuttle is on planet surface and ship is in orbit
                            //Note, the random number generated below must be done to keep in sync
                            //with the original version. Do not remove it for efficency.
                            double r = 0;
                            if (ship.ShuttleLocation == FederationShip.ShuttleLocationEnum.Planet)
                                r = game.Random.Rand();

                            //If we are being tractor beamed away while orbiting a planet with the shuttle on the planet,
                            //perform a check if the shuttle survives.
                            if (ship.ShuttleLocation == FederationShip.ShuttleLocationEnum.Planet && ship.Orbit != null && ship.Orbit.GalileoPresent)
                            {
                                Game.Console.Skip(1);
                                if (r > 0.5)
                                {
                                    Game.Console.WriteLine("Galileo, left on the planet surface, is captured");
                                    Game.Console.WriteLine("by aliens and made into a flying McDonald's.");
                                    ship.ShuttleLocation = FederationShip.ShuttleLocationEnum.Aliens;
                                }//if
                                else
                                {
                                    Game.Console.WriteLine("Galileo, left on the planet surface, is well hidden.");
                                }//else
                            }//if

                            //make sure if ship was in orbit, it no longer is
                            ship.Orbit = null;

                            //Now pull ship to tractor beam location. Either the selected Commander or the SuperCommander
                            if (line == FutureEvents.EventTypesEnum.FSPY)
                                ship.GalacticCoordinate = new GalacticCoordinate(game.Galaxy.SuperCommander);
                            else
                                ship.GalacticCoordinate = new GalacticCoordinate(commanders[commander].QuadrantCoordinate);

                            //plave the ship into a random sector in the destination quadrant
                            game.Galaxy.CurrentQuadrant[SectorCoordinate.Random(game.Random)] = ship;

                            //let the player know hes been yanked ...
                            Game.Console.WriteLine("{0} is pulled to{1}, {2}", ship.Name, ship.QuadrantCoordinate.ToString(true), ship.Sector.ToString(true));
                            if (game.Turn.resting)
                            {
                                Game.Console.WriteLine("(Remainder of rest/repair period cancelled.)");
                                game.Turn.resting = false;
                            }//if

                            //force the shields up(if not damaged and sufficient energy)
                            if (!ship.ShieldsUp)
                            {
                                if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Shields) && ship.ShieldEnergy > 0)
                                {
                                    Battle.Shield(game, true);//Shldsup
                                    ship.ShieldChange = false;
                                }
                                else
                                    Game.Console.WriteLine("(Shields not currently useable.)");
                            }//if

                            game.Galaxy.newquad(game, false);
                            //Adjust finish time to time of tractor beaming
                            fintim = game.Date + game.Turn.Time;
                            if (game.Galaxy.Commanders.Count <= 0)
                                game.Future[FutureEvents.EventTypesEnum.FTBEAM] = FutureEvents.NEVER;
                            else
                                game.Future[FutureEvents.EventTypesEnum.FTBEAM] = game.Date + game.Turn.Time + game.Random.expran(1.5 * game.Galaxy._intime / game.Galaxy.Commanders.Count);

                        }
                        break;

                    case FutureEvents.EventTypesEnum.FSNAP:
                        {//Snapshot of the universe (for time warp)
                            game.GameSnapShot = new SnapShot();
                            game.GameSnapShot.TakeSnapShot(game);
                            game.Future[FutureEvents.EventTypesEnum.FSNAP] = game.Date + game.Random.expran(0.5 * game.Galaxy._intime);
                            SnapShot.DumpDate(game.Date, game.Future[FutureEvents.EventTypesEnum.FSNAP]);
                        }break;

                    case FutureEvents.EventTypesEnum.FBATTAK:
                        {//Commander attacks starbase
                            if (game.Galaxy.Commanders.Count <= 0 || game.Galaxy.Bases.Count <= 0)
                            {
                                //no can do
                                game.Future[FutureEvents.EventTypesEnum.FBATTAK] = game.Future[FutureEvents.EventTypesEnum.FCDBAS] = FutureEvents.NEVER;
                                break;
                            }//if

                            //Find a starbase that:
                            //is co-located with a Commander
                            //AND is not co-located with ship
                            //AND is not co-located with SC
                            QuadrantStarBase starbase = null;
                            foreach (QuadrantStarBase sb in game.Galaxy.Bases)
                            {
                                QuadrantCoordinate qc = sb.QuadrantCoordinate;
                                if (game.Galaxy[qc].Commander == null)
                                    continue;

                                if (qc.Equals(ship.QuadrantCoordinate))
                                    continue;

                                if (game.Galaxy[qc].SuperCommander != null && qc.Equals(game.Galaxy[qc].SuperCommander.QuadrantCoordinate))
                                    continue;

                                starbase = sb;
                                break;
                            }//foreach

                            if (starbase == null)
                            {
                                //no match found -- try later
                                game.Future[FutureEvents.EventTypesEnum.FBATTAK] = game.Date + game.Random.expran(0.3 * game.Galaxy._intime);
                                game.Future[FutureEvents.EventTypesEnum.FCDBAS] = FutureEvents.NEVER;
                                break;
                            }//if

                            //commander + starbase combination found -- launch attack
                            game.Galaxy.CommanderAttack = starbase.QuadrantCoordinate;
                            game.Future[FutureEvents.EventTypesEnum.FCDBAS] = game.Date + 1.0 + 3.0 * game.Random.Rand();
                            if (game.Galaxy.SuperCommanderAttack != null)//extra time if SC already attacking
                                game.Future[FutureEvents.EventTypesEnum.FCDBAS] += game.Future[FutureEvents.EventTypesEnum.FSCDBAS] - game.Date;
                            game.Future[FutureEvents.EventTypesEnum.FBATTAK] = game.Future[FutureEvents.EventTypesEnum.FCDBAS] + game.Random.expran(0.3 * game.Galaxy._intime);

                            game.Turn.iseenit = false;
                            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) && !ship.Docked)
                                break;//No warning :-(
                            game.Turn.iseenit = true;

                            Game.Console.WriteLine("\nLt. Uhura-  \"Captain, the starbase in{0}", starbase.QuadrantCoordinate.ToString(true));
                            Game.Console.WriteLine("   reports that it is under atttack and that it can");
                            Game.Console.WriteLine("   hold out only until stardate {0,1:F1}.\"", game.Future[FutureEvents.EventTypesEnum.FCDBAS]);
                            if (game.Turn.resting)
                            {
                                Game.Console.Write("\nMr. Spock-  \"Captain, shall we cancel the rest period?\"");
                                if (Game.Console.ja())
                                {
                                    game.Turn.resting = false;
                                    game.Turn.Time = 0.0;
                                    return;
                                }//if
                            }//if
                        } break;
                    case FutureEvents.EventTypesEnum.FSCDBAS://Supercommander destroys base
                        {
                            game.Future[FutureEvents.EventTypesEnum.FSCDBAS] = FutureEvents.NEVER;
                            QuadrantCoordinate qc = game.Galaxy.SuperCommanderAttack;
                            if (qc == null)
                                break;

                            destroyStarBase(game.Galaxy, qc, true);
                            game.Galaxy.SuperCommanderAttack = null;
                        } break;
                    case FutureEvents.EventTypesEnum.FCDBAS://Commander destroys base
                        {
                            game.Future[FutureEvents.EventTypesEnum.FCDBAS] = FutureEvents.NEVER;
                            //find the lucky pair
                            QuadrantCoordinate qc = game.Galaxy.CommanderAttack;
                            if (qc == null)
                                break;

                            destroyStarBase(game.Galaxy, qc, false);
                            game.Galaxy.CommanderAttack = null;
                        } break;
                    case FutureEvents.EventTypesEnum.FSCMOVE://Supercommander moves
                        {
                            game.Future[FutureEvents.EventTypesEnum.FSCMOVE] = game.Date + 0.2777;
                            if (!game.Turn.ientesc && !istract &&
                                game.Galaxy.SuperCommanderAttack == null &&
                                (game.Galaxy.CurrentQuadrant.SuperCommander == null || game.Turn.justin))
                            {
                                AI.MoveSuperCommander(game);
                            }
                        } break;
                    case FutureEvents.EventTypesEnum.FDSPROB://Move deep space probe
                        {
                            Probe probe = game.Galaxy.Probe;
                            if (probe == null)
                                return;//this should never happen, but lets be paranoid

                            probe.dump();

                            //schedule a new probe move event, Time to move one sector
                            game.Future[FutureEvents.EventTypesEnum.FDSPROB] = game.Date + 0.01;

                            //move the probe one increment, check if it changed quadrants
                            if (probe.Move())
                            {
                                //probe has changed quadrants, check if the new quad is valid or has wandered into a snova
                                if (!probe.QuadrantCoordinate.Valid || game.Galaxy[probe.QuadrantCoordinate].SuperNova)
                                {
                                    //Left galaxy or ran into supernova
                                    if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) || ship.Docked)
                                    {
                                        Game.Console.Write("\nLt. Uhura-  \"The deep space probe ");
                                        if (!probe.QuadrantCoordinate.Valid)
                                            Game.Console.Write("has left the galaxy");
                                        else
                                            Game.Console.Write("is no longer transmitting");
                                        Game.Console.WriteLine(".\"");
                                    }//if

                                    //remove probe from galaxy, reset future events
                                    game.Galaxy.Probe = null;
                                    game.Future[FutureEvents.EventTypesEnum.FDSPROB] = FutureEvents.NEVER;
                                    break;
                                }//if

                                //report to the ship the new probe quadrant, unless the radio is fried.
                                if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) || ship.Docked)
                                    Game.Console.WriteLine("\nLt. Uhura-  \"The deep space probe is now in {0}.\"", probe.QuadrantCoordinate.ToString(true));

                            }//if

                            //Update star chart if Radio is working or have access to radio.
                            if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) || ship.Docked)
                            {
                                bool radioDamaged = ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio);
                                game.Galaxy[probe.QuadrantCoordinate].Starch = radioDamaged ? game.Galaxy[probe.QuadrantCoordinate].ToInt + 1000 : 1;
                            }//if

                            if (probe.Moves == 0)
                            {
                                //probe has reached its destination ...
                                //remove probe from galaxy
                                game.Galaxy.Probe = null;

                                //reset future events
                                game.Future[FutureEvents.EventTypesEnum.FDSPROB] = FutureEvents.NEVER;

                                //can only super nova in a quadrant that has at least 1 star
                                if (probe.Armed && game.Galaxy[probe.QuadrantCoordinate].Stars > 0)
                                {
                                    //lets blow the sucker!
                                    //cause super nova in current probe quadrant
                                    SuperNova(probe.QuadrantCoordinate, null, false, game);

                                    //check if we died ...
                                    if (game.Galaxy[ship.QuadrantCoordinate].SuperNova)
                                        return;
                                }//if
                            }//if
                        } break;//FDSPROB

                    default: break;

                }//switch
            }//while
        }//ProcessEvents

        internal static void Wait(GameData game)
        {
            game.Turn.ididit = false;
            object key;
            while (true)
            {
                key = Game.Console.scan();
                if (key != null)
                    break;
                Game.Console.Write("How long? ");
            }//while
            Game.Console.chew();

            if (!(key is double))
            {
                Game.Console.huh();
                return;
            }//if

            double origTime = (double)key;
            double delay = origTime;
            if (delay <= 0.0)
                return;

            if (delay >= game.RemainingTime || game.Galaxy.CurrentQuadrant.Enemies.Count != 0)
            {
                Game.Console.WriteLine("Are you sure? ");
                if (!Game.Console.ja())
                    return;
            }//if

            //Alternate resting periods (events) with attacks
            game.Turn.resting = true;
            do
            {
                if (delay <= 0)
                    game.Turn.resting = false;

                if (!game.Turn.resting)
                {
                    Game.Console.WriteLine("{0,0:F2} stardates left.", game.RemainingTime);
                    return;
                }
                double temp = delay;
                game.Turn.Time = delay;

                if (game.Galaxy.CurrentQuadrant.Enemies.Count > 0)
                {
                    double rtime = 1.0 + game.Random.Rand();
                    if (rtime < temp)
                        temp = rtime;
                    game.Turn.Time = temp;
                }
                if (game.Turn.Time < delay)
                    Battle.Attack(game, false);

                if (game.Galaxy.CurrentQuadrant.Enemies.Count == 0)
                    AI.MoveTholian(game);

                if (game.Turn.alldone)
                    return;

                ProcessEvents(game);

                game.Turn.ididit = true;
                if (game.Turn.alldone)
                    return;

                delay -= temp;

                //Repair Deathray if long rest at starbase
                if ((origTime - delay) >= 9.99 && game.Galaxy.Ship.Docked)
                    game.Galaxy.Ship.ShipDevices.SetDamage(ShipDevices.ShipDevicesEnum.Deathray, 0.0);

            } while (!game.Galaxy[game.Galaxy.Ship.QuadrantCoordinate].SuperNova);

            game.Turn.resting = false;
            game.Turn.Time = 0;

        }//Wait

        /// <summary>
        /// A supernova has occurred in the specified quadrant.
        /// If this is the same quadrant as the ship, we have an insipient supernova.
        /// </summary>
        /// <param name="qc">The quadrant the super nova has occurred in</param>
        /// <param name="sc">The sector of the star which has super nova'd (note, maybe null)</param>
        /// <param name="random">True if this is a random event</param>
        /// <param name="game"></param>
        internal static void SuperNova(QuadrantCoordinate qc, SectorCoordinate sc, bool random, GameData game)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;

            if (!qc.Equals(ship.QuadrantCoordinate) || game.Turn.justin)
            {//it isn't here, or we just entered (treat as inroute)
                if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) || ship.Docked)
                {
                    Game.Console.WriteLine("\nMessage from Starfleet Command       Stardate {0,0:F1}", game.Date);
                    Game.Console.WriteLine("     Supernova in{0}; caution advised.", qc.ToString(true));
                }
            }//if
            else
            {//we are in the quadrant!
                SectorCoordinate nsxy = (sc != null) ? sc : galaxy.CurrentQuadrant.RandomStar(game.Random);
                Game.Console.WriteLine("\n***RED ALERT!  RED ALERT!\n");
                Game.Console.WriteLine("***Incipient supernova detected at{0}", nsxy.ToString(true));

                if (nsxy.DistanceTo(ship.Sector) <= 2.1)
                {
                    Game.Console.Write("Emergency override attempts t");
                    Game.Console.WriteLine("***************\n");
                    Game.Console.stars();
                    game.Turn.alldone = true;
                }//if
            }//else

            //destroy any ordinary Klingons in supernovaed quadrant
            int kldead = galaxy[qc].TotalKlingons;
            galaxy[qc].OrdinaryKlingons = 0;

            int iscdead = 0;
            if (qc.Equals(galaxy.SuperCommander))
            {
                //if the SuperCommander killed is currently attacking a starbase we should stop the attack!
                if (galaxy.SuperCommanderAttack != null && galaxy.SuperCommanderAttack.Equals(qc))
                {
                    game.Future[FutureEvents.EventTypesEnum.FSCDBAS] = FutureEvents.NEVER;
                    galaxy.SuperCommanderAttack = null;
                }

                //did in the Supercommander!
                galaxy[qc].SuperCommander = null;

                //make sure there are no scheduled SC move or base attack events
                game.Future[FutureEvents.EventTypesEnum.FSCMOVE] = FutureEvents.NEVER;
                game.Future[FutureEvents.EventTypesEnum.FSCDBAS] = FutureEvents.NEVER;

                iscdead = 1;
                kldead--;//counted twice
            }//if

            int comdead = 0;
            if (galaxy[qc].Commander != null)
            {
                //if the commander killed is currently attacking a starbase we should stop the attack!
                if (galaxy.CommanderAttack != null && galaxy.CommanderAttack.Equals(qc))
                {
                    game.Future[FutureEvents.EventTypesEnum.FCDBAS] = FutureEvents.NEVER;
                    galaxy.CommanderAttack = null;
                }

                galaxy[qc].Commander = null;
                if (galaxy.Commanders.Count == 0)
                {//if last of the commanders was killed
                    game.Future[FutureEvents.EventTypesEnum.FTBEAM] = FutureEvents.NEVER;
                }
                comdead = 1;
                kldead--;//counted twice
            }//if

            //destroy Romulans and planets in supernovaed quadrant
            int nrmdead = galaxy[qc].Romulans;
            galaxy[qc].Romulans = 0;

            int npdead = (galaxy[qc].Planet == null) ? 0 : 1;
            galaxy[qc].Planet = null;

            //Destroy any base in supernovaed quadrant
            int nbasedead = (galaxy[qc].Base == null) ? 0 : 1;
            galaxy[qc].Base = null;

            int nstarsdead = galaxy[qc].Stars;
            galaxy[qc].Stars = 0;

            //If starship caused supernova, tally up destruction
            if (!random)
            {
                game.StarsKilled += nstarsdead;
                game.BasesKilled += nbasedead;
                game.KlingonsKilled += kldead;
                game.CommandersKilled += comdead;
                game.RomulansKilled += nrmdead;
                game.PlanetsKilled += npdead;
                game.SuperCommandersKilled += iscdead;
            }//if

            //mark supernova in galaxy and in star chart
            if ((qc.Equals(ship.QuadrantCoordinate)) || !ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) || ship.Docked)
                galaxy[qc].Starch = 1;

            galaxy[qc].SuperNova = true;

            //If supernova destroys last klingons give special message
            if (galaxy.Klingons == 0 && !(qc.Equals(ship.QuadrantCoordinate)))
            {
                Game.Console.Skip(2);
                if (random)
                    Game.Console.WriteLine("Lucky you!");

                Game.Console.WriteLine("A supernova in{0} has just destroyed the last Klingons.", qc.ToString(true));
                Finish.finish(Finish.FINTYPE.FWON, game);
                return;
            }//if

            //if some Klingons remain, continue or die in supernova
            if (game.Turn.alldone)
                Finish.finish(Finish.FINTYPE.FSNOVAED, game);

        }//SuperNova

    }//class Events
}