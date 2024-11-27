using System;
using System.Collections.Generic;
using System.Text;

using sstNET.Galaxy;
using sstNET.Galaxy.QuadrantObjects;
using sstNET.Galaxy.SectorObjects;
using sstNET.Galaxy.SectorObjects.Ships;

namespace sstNET
{
    /// <summary>
    /// Handles all the moving logic of the game including deep space probes,
    /// time warp, abandon ship and calls for help.
    /// </summary>
    internal static class Moving
    {
        /// <summary>
        /// Minimum warp speed is 1
        /// </summary>
        private const double mMinWarpSpeed = 1.0;

        /// <summary>
        /// Maximum warp speed is 10
        /// </summary>
        private const double mMaxWarpSpeed = 10.0;

        /// <summary>
        /// The safest max warp speed is 6. no damage to engines possible
        /// </summary>
        private const double mMaxSafeWarpSpeed = 6.0;

        /// <summary>
        /// If the warp engines are damaged, this warp is the max
        /// </summary>
        private const double mMaxDamagedWarpSpeed = 4.0;

        /// <summary>
        /// The user has requested to set the warp speed
        /// Warp 1-10 is valid
        /// </summary>
        /// <param name="galaxy"></param>
        internal static void SetWarp(Galaxy.Galaxy galaxy)
        {
            FederationShip ship = galaxy.Ship;

            object tok;
            while ((tok = Game.Console.scan()) == null)
            {
                Game.Console.chew();
                Game.Console.Write("Warp factor-");
            }//if
            Game.Console.chew();
            if (!(tok is double))
            {
                Game.Console.huh();
                return;
            }//if
            double aaitem = (double)tok;

            //check for warp engine damage
            if (ship.ShipDevices.GetDamage(ShipDevices.ShipDevicesEnum.WarpEngines) > 10.0)
            {
                Game.Console.WriteLine("Warp engines inoperative.");
                return;
            }//if

            //if warp engines partially damaged, then warp 4 is the best
            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.WarpEngines) && aaitem > mMaxDamagedWarpSpeed)
            {
                Game.Console.WriteLine("Engineer Scott- \"I'm doing my best, Captain,");
                Game.Console.WriteLine("  but right now we can only go warp {0}.\"", (int)mMaxDamagedWarpSpeed);
                return;
            }//if

            //cannot go faster than warp 10 or less than warp 1
            if (aaitem > mMaxWarpSpeed)
            {
                Game.Console.WriteLine("Helmsman Sulu- \"Our top speed is warp {0}, Captain.\"", (int)mMaxWarpSpeed);
                return;
            }//if
            else if (aaitem < mMinWarpSpeed)
            {
                Game.Console.WriteLine("Helmsman Sulu- \"We can't go below warp {0}, Captain.\"", (int)mMinWarpSpeed);
                return;
            }//else if

            //save old warp speed for messaging
            double oldfac = ship.Warp;

            //set the new warp speed
            ship.Warp = aaitem;

            //Some messages
            if (ship.Warp <= oldfac || ship.Warp <= mMaxSafeWarpSpeed)
            {
                Game.Console.WriteLine("Helmsman Sulu- \"Warp factor {0,0:F1}, Captain.\"", ship.Warp);
                return;
            }//if
            else if (ship.Warp < 8.00)
            {
                Game.Console.WriteLine("Engineer Scott- \"Aye, but our maximum safe speed is warp {0}.\"", (int)mMaxSafeWarpSpeed);
                return;
            }//if
            else if (ship.Warp == mMaxWarpSpeed)
            {
                Game.Console.WriteLine("Engineer Scott- \"Aye, Captain, we'll try it.\"");
                return;
            }//if
            Game.Console.WriteLine("Engineer Scott- \"Aye, Captain, but our engines may not take it.\"");

        }//SetWarp

        /// <summary>
        /// The ship has entered a time warp. It will either travel backwards or forwards in time.
        /// We will only go backwards if a game snapshot has been taken.
        /// </summary>
        /// <param name="game"></param>
        private static void TimeWarp(GameData game)
        {
            Game.Console.WriteLine("***TIME WARP ENTERED.");

            //50-50 chance for forwards or backwards
            if (game.GameSnapShot != null && game.Random.Rand() < 0.5)
            {
                //Go back in time
                FederationShip ship = game.Galaxy.Ship;

                Game.Console.WriteLine("You are traveling backwards in time {0,0:F2} stardates.", game.Date - game.GameSnapShot.Date);

                game.GameSnapShot.RestoreSnapShot(game);
                game.GameSnapShot = null;

                //if any commanders are still alive schedule a tracker beam and base attack
                if (game.Galaxy.Commanders.Count > 0)
                {
                    game.Future[FutureEvents.EventTypesEnum.FTBEAM] = game.Date + game.Random.expran(game.Galaxy._intime / game.Galaxy.Commanders.Count);
                    game.Future[FutureEvents.EventTypesEnum.FBATTAK] = game.Date + game.Random.expran(0.3 * game.Galaxy._intime);
                }//if

                //schedule a supernova and a new snapshot
                game.Future[FutureEvents.EventTypesEnum.FSNOVA] = game.Date + game.Random.expran(0.5 * game.Galaxy._intime);
                game.Future[FutureEvents.EventTypesEnum.FSNAP] = game.Date + game.Random.expran(0.25 * game.RemainingTime);

                //if supercommander still around, schedule a move
                if (game.Galaxy.SuperCommander != null)
                    game.Future[FutureEvents.EventTypesEnum.FSCMOVE] = game.Date + 0.2777;

                //cancel any scheduled/current sc or commander attack
                game.Galaxy.SuperCommanderAttack = null;
                game.Galaxy.CommanderAttack = null;
                game.Future[FutureEvents.EventTypesEnum.FCDBAS] = FutureEvents.NEVER;
                game.Future[FutureEvents.EventTypesEnum.FSCDBAS] = FutureEvents.NEVER;

                //Make sure Galileo is consistant -- Snapshot may have been taken
                //when on planet, which would give us two Galileos!
                //First, determine if any planet indicates galileo is present
                bool galileoOnPlanet = false;
                foreach (QuadrantPlanet qp in game.Galaxy.Planets)
                {
                    if (qp.GalileoPresent)
                    {
                        galileoOnPlanet = true;
                        break;
                    }
                }

                //If the:
                //   Ship has a shuttle Bay (is Enterprise)
                //   Galileo is on a planet
                //   The ship thinks the Galileo is aboard:
                //Then there is a problem, Galileo cannot be in 2 places, so force it to be on the planet
                if (ship.HasShuttleBay && galileoOnPlanet && ship.ShuttleLocation == FederationShip.ShuttleLocationEnum.ShuttleBay)
                {
                    Game.Console.WriteLine("Checkov-  \"Security reports the Galileo has disappeared, Sir!");
                    ship.ShuttleLocation = FederationShip.ShuttleLocationEnum.Planet;
                }

                //Likewise, if in the original time the Galileo was abandoned, but
                //was on ship earlier, it would have vanished -- lets restore it
                if (ship.HasShuttleBay && !galileoOnPlanet && ship.ShuttleLocation != FederationShip.ShuttleLocationEnum.ShuttleBay)
                {
                    Game.Console.WriteLine("Checkov-  \"Security reports the Galileo has reappeared in the dock!\"");
                    ship.ShuttleLocation = FederationShip.ShuttleLocationEnum.ShuttleBay;
                }

                //Revert star chart to earlier era, if it was known then
                bool radioDamaged = ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio);
                if (!radioDamaged || ship.StarChartDamage > game.Date)
                {
                    for (int ii = 1; ii <= 8; ii++)
                    {
                        for (int jj = 1; jj <= 8; jj++)
                        {
                            if (game.Galaxy[ii, jj].Starch > 1)
                                game.Galaxy[ii, jj].Starch = radioDamaged ? game.Galaxy[ii, jj].ToInt + 1000 : 1;

                        }//for jj
                    }//for ii
                }//if
                Game.Console.WriteLine("Spock has reconstructed a correct star chart from memory");
                if (radioDamaged)
                    ship.StarChartDamage = game.Date;

            }//if
            else
            {//Go forward in time

                game.Turn.Time = -0.5 * game.Galaxy._intime * Math.Log(game.Random.Rand());
                Game.Console.WriteLine("You are traveling forward in time {0,0:F2} stardates.", game.Turn.Time);

                //cheat to make sure no tractor beams occur during time warp
                game.Future[FutureEvents.EventTypesEnum.FTBEAM] += game.Turn.Time;

                //damage the subspace radio so no reports come to ship during time warp.
                game.Galaxy.Ship.ShipDevices.AddDamage(ShipDevices.ShipDevicesEnum.SubspaceRadio, game.Turn.Time);

            }//else

            game.Galaxy.newquad(game, false);

        }//TimeWarp

        /// <summary>
        /// Move the ship. The basic idea is to travel the ship from its current location the
        /// desired direction and distance one sector at a time until either reaching the destination
        /// sector or leaving the current quadrant. If we hit something along the way in the current
        /// quadrant, then handle it. Generally all objects simply stop the ship, however colliding with
        /// an enemy ship results in masssive damage to ship (destroying enemy ship in the process)
        /// </summary>
        /// <param name="game"></param>
        internal static void Move(GameData game)
        {
            FederationShip ship = game.Galaxy.Ship;

            //if we are in orbit, then de-orbit
            if (ship.Orbit != null)
            {
                Game.Console.WriteLine("SULU- \"Leaving standard orbit.\"");
                ship.Orbit = null;
            }//if

            //If tractor beam is to occur, don't move full distance
            bool trbeam = false;
            if ((game.Date + game.Turn.Time) >= game.Future[FutureEvents.EventTypesEnum.FTBEAM])
            {//ship is going to be tractor-beamed
                trbeam = true;

                //make sure we are undocked
                ship.Docked = false;

                //compute the distance and time we will use before being tractor beamed.
                game.Turn.dist = game.Turn.dist * (game.Future[FutureEvents.EventTypesEnum.FTBEAM] - game.Date) / game.Turn.Time + 0.1;
                game.Turn.Time = game.Future[FutureEvents.EventTypesEnum.FTBEAM] - game.Date + 1e-5;
            }//if

            //Move within the quadrant
            //remove ship from its current location
            game.Galaxy.CurrentQuadrant[ship.Sector] = new Empty();

            //compute a course based on the desired direction and distance
            GalacticCourse course = new GalacticCourse(ship.GalacticCoordinate, game.Turn.direc, game.Turn.dist);

            //keep track of current sector and previous sector in case ship collides with something.
            //In the case of enemy ships, the enemy ship will be destroyed and ship will end up
            //in the sector where the enemy ship was. In the case of other objects such as stars, the
            //ship will stop at the sector before collision.
            SectorCoordinate previous = course.CurrentSectorCoordinate;
            SectorCoordinate current = course.CurrentSectorCoordinate;

            bool moved = false;

            //keep track of how far we have travelled
            double distSoFar = 0.0;

            //keep stepping through sectors until we hit the distance specified
            while (course.Next())
            {
                moved = true;

                //get the sector coordinate of thenext sector travelled through
                current = course.CurrentSectorCoordinate;

                //if we leave the quadrant, then handle it. this will end the move
                if (!course.SameQuadrant)
                {
                    //Leaving quadrant -- allow final enemy attack
                    //Don't do it if being pushed by Nova
                    if (game.Galaxy.CurrentQuadrant.Enemies.Count != 0 && game.Turn.iattak != 2)
                    {//let the bad guys get one last shot at the ship
                        ship.Docked = false;

                        //update all the distances from ship to bad guys
                        //game.Galaxy.CurrentQuadrant.SetNewDistances(current);
                        game.Galaxy.CurrentQuadrant.SetNewDistances(course.CurrentCoordinate);

                        //and if we are not being super-nova'd, let them attack
                        if (!game.Galaxy[ship.QuadrantCoordinate].SuperNova)
                        {
                            Battle.Attack(game, true);

                            //if ship is destroyed, then bail
                            if (game.Turn.alldone)
                                return;
                        }//if
                    }//if

                    //compute final position -- new quadrant and sector
                    GalacticCoordinate gc = ship.GalacticCoordinate.Project(course.Direction, course.Distance);

                    //check if the destination is outside of galaxy. If it is, we need to fix it up
                    //and warn the user. 3 times and its game over.
                    if (gc.Fixup())
                    {//attempted to leave galaxy

                        //increment attempt count
                        game.NumberKinks += 1;

                        //3 or more times and its game over
                        if (game.NumberKinks >= 3)
                        {
                            //Three strikes -- you're out!
                            Finish.finish(Finish.FINTYPE.FNEG3, game);
                            return;
                        }//if

                        //warn user
                        Game.Console.WriteLine("\nYOU HAVE ATTEMPTED TO CROSS THE NEGATIVE ENERGY BARRIER");
                        Game.Console.WriteLine("AT THE EDGE OF THE GALAXY.  THE THIRD TIME YOU TRY THIS,");
                        Game.Console.WriteLine("YOU WILL BE DESTROYED.\n");
                    }//if

                    //Compute final position in new quadrant
                    if (trbeam)
                        return;//Don't bother if we are to be tractor beamed

                    //update ship position
                    ship.GalacticCoordinate = gc;

                    //Inform the user and generate a new quadrant
                    Game.Console.WriteLine("\nEntering{0}", ship.QuadrantCoordinate.ToString(true));
                    game.Galaxy.newquad(game, false);
                    return;

                }//if

                //Move ship to next sector in the course (sc)
                //Get sector object at the new sector
                SectorObject iquad = game.Galaxy.CurrentQuadrant[current];

                //if its not empty, then we have a collision
                if (!(iquad is Empty))
                {
                    //ship has collided with an object. Go handle the event
                    if (iquad.Ram(game, previous, out current, out distSoFar))
                    {//ship is destroyed, end of game
                        return;
                    }//if

                    if (ship.ShipEnergy <= 0)
                    {//ship out of energy, end game
                        Finish.finish(Finish.FINTYPE.FNRG, game);
                        return;
                    }//if
                    break;
                }//if

                //get ready to move another sector. Save the current sector as previous
                //and update distance travelled so far
                previous = current;
                distSoFar = ship.Sector.DistanceTo(current) * 0.1;

            }//while

            //done moving, we are now at the final sector
            //update distance travelled. This may not be the intended distance as we may
            //have collided with an object(and survived)
            //Note:this distance is not nessecarily the current distance from the original ship position
            //to the current position. In the case of colliding with a blocked object(ie star) the ship
            //stops short of the object but the distance calculated is to the actual object position.
            //This is the calculation the original code made so I have preserved it.
            if(moved)
                game.Turn.dist = distSoFar;

            //position ship into current quadrant at the current position
            game.Galaxy.CurrentQuadrant[current] = ship;

            //if enemies in quadrant, update distances
            if (game.Galaxy.CurrentQuadrant.Enemies.Count != 0)
            {
                game.Galaxy.CurrentQuadrant.SetNewDistances(ship.Sector);
                game.Galaxy.CurrentQuadrant.CalculateDistances(ship.Sector);

                //allow attack if not supernova here, and ???
                //I think the iattak test is to prevent recursion. This function is called
                //from several places.
                if (!game.Galaxy[ship.QuadrantCoordinate].SuperNova && game.Turn.iattak == 0)
                    Battle.Attack(game, true);

                game.Galaxy.CurrentQuadrant.ResetAverageDistances();
            }//if

            //make sure ship is now undocked
            ship.Docked = false;
            game.Turn.iattak = 0;

        }//Move

        /// <summary>
        /// This program originally required input in terms of a (clock)
        /// direction and distance. Somewhere in history, it was changed to
        /// cartesian coordinates. So we need to convert. I think
        /// "manual" input should still be done this way -- it's a real
        /// pain if the computer isn't working! Manual mode is still confusing
        /// because it involves giving x and y motions, yet the coordinates
        /// are always displayed y - x, where +y is downward!
        /// </summary>
        /// <param name="isprobe"></param>
        /// <param name="akey"></param>
        /// <param name="ship"></param>
        /// <param name="turn"></param>
        private static void getcd(bool isprobe, string akey, FederationShip ship, GameData.TurnInfo turn)
        {
            //Get course direction and distance. If user types bad values, return
            //with DIREC = -1.0.
            turn.direc = -1.0;

            //todo - fix Landed
            if (ship.CrewLocation == FederationShip.CrewLocationEnum.Planet && !isprobe)
            {
                Game.Console.WriteLine("Dummy! You can't leave standard orbit until you");
                Game.Console.WriteLine("are back abourt the {0}.", ship.Name);
                Game.Console.chew();
                return;
            }

            int iprompt = 0;
            bool automatic;
            object key = null;
            while (true)
            {
                if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Computer))
                {
                    Game.Console.WriteLine("Computer damaged; manual {0} only", isprobe ? "navigation" : "movement");
                    Game.Console.chew();
                    automatic = false;
                    key = null;
                    break;
                }

                if (isprobe && akey != null)
                {
                    //For probe launch, use pre-scaned value first time
                    key = akey;
                    akey = null;
                }
                else
                    key = Game.Console.scan();

                if (key == null || ((key is string) && (string.IsNullOrEmpty(key as string))))
                {
                    Game.Console.Write("Manual or automatic- ");
                    iprompt = 1;
                    Game.Console.chew();
                }
                else if (key is string)
                {
                    if (SSTConsole.isit(key, "manual"))
                    {
                        automatic = false;
                        key = Game.Console.scan();
                        break;
                    }
                    else if (SSTConsole.isit(key, "automatic"))
                    {
                        automatic = true;
                        key = Game.Console.scan();
                        break;
                    }
                    else
                    {
                        Game.Console.huh();
                        Game.Console.chew();
                        return;
                    }
                }
                else
                {//numeric
                    Game.Console.WriteLine("(Manual {0} assumed.)", isprobe ? "navigation" : "movement");
                    automatic = false;
                    break;
                }
            }//while

            double deltax = 0;
            double deltay = 0;
            int itemp = 0;
            if (automatic)
            {
                int irowq = ship.QuadrantCoordinate.X;
                int icolq = ship.QuadrantCoordinate.Y;
                int irows = 0, icols = 0;

                while (key == null)
                {
                    if (isprobe)
                        Game.Console.Write("Target quadrant or quadrant&sector- ");
                    else
                        Game.Console.Write("Destination sector or quadrant&sector- ");
                    Game.Console.chew();
                    iprompt = 1;
                    key = Game.Console.scan();
                }

                if (!(key is double))
                {
                    Game.Console.huh();
                    return;
                }
                double xi = (double)key;
                key = Game.Console.scan();
                if (!(key is double))
                {
                    Game.Console.huh();
                    return;
                }
                double xj = (double)key;
                key = Game.Console.scan();
                if (key is double)
                {
                    //both quadrant and sector specified
                    double xk = (double)key;
                    key = Game.Console.scan();
                    if (!(key is double))
                    {
                        Game.Console.huh();
                        return;
                    }
                    double xl = (double)key;

                    irowq = (int)(xi + 0.5);
                    icolq = (int)(xj + 0.5);
                    irows = (int)(xk + 0.5);
                    icols = (int)(xl + 0.5);
                }
                else
                {
                    if (isprobe)
                    {
                        /* only quadrant specified -- go to center of dest quad */
                        irowq = (int)(xi + 0.5);
                        icolq = (int)(xj + 0.5);
                        irows = icols = 5;
                    }
                    else
                    {
                        irows = (int)(xi + 0.5);
                        icols = (int)(xj + 0.5);
                    }
                    itemp = 1;
                }
                if (irowq < 1 || irowq > 8 || icolq < 1 || icolq > 8 ||
                    irows < 1 || irows > 10 || icols < 1 || icols > 10)
                {
                    Game.Console.huh();
                    return;
                }
                Game.Console.Skip(1);
                if (!isprobe)
                {
                    if (itemp != 0)
                    {
                        if (iprompt != 0)
                            Game.Console.WriteLine("Helmsman Sulu- \"Course locked in for{0}.\"", new SectorCoordinate(irows, icols).ToString(true));
                    }
                    else
                        Game.Console.WriteLine("Ensign Chekov- \"Course laid in, Captain.\"");
                }
                deltax = icolq - ship.QuadrantCoordinate.Y + 0.1 * (icols - ship.Sector.Y);
                deltay = ship.QuadrantCoordinate.X - irowq + 0.1 * (ship.Sector.X - irows);
            }//automatic mode
            else
            {//manual
                while (key == null)
                {
                    Game.Console.Write("X and Y displacements- ");
                    Game.Console.chew();
                    iprompt = 1;
                    key = Game.Console.scan();
                }
                itemp = 2;
                if (!(key is double))
                {
                    Game.Console.huh();
                    return;
                }
                deltax = (double)key;
                key = Game.Console.scan();
                if (!(key is double))
                {
                    Game.Console.huh();
                    return;
                }
                deltay = (double)key;
            }//manual mode

            //Check for zero movement
            if (deltax == 0 && deltay == 0)
            {
                Game.Console.chew();
                return;
            }
            if (itemp == 2 && !isprobe)
                Game.Console.WriteLine("\nHelmsman Sulu- \"Aye, Sir.\"");

            turn.dist = Math.Sqrt(deltax * deltax + deltay * deltay);
            turn.direc = Math.Atan2(deltax, deltay) * 1.90985932;
            if (turn.direc < 0.0) turn.direc += 12.0;
            Game.Console.chew();

            return;
        }//getcd

        /// <summary>
        /// Set the current warp speed of the ship.
        /// </summary>
        /// <param name="game"></param>
        internal static void Warp(GameData game)
        {
            FederationShip ship = game.Galaxy.Ship;

            game.Turn.ididit = false;

            if (ship.ShipDevices.GetDamage(ShipDevices.ShipDevicesEnum.WarpEngines) > 10.0)
            {
                Game.Console.chew();
                //todo - should this not be "warp engines"?
                Game.Console.WriteLine("\nEngineer Scott- \"The impulse engines are damaged, Sir.\"");
                return;
            }//if
            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.WarpEngines) && ship.Warp > 4.0)
            {
                Game.Console.chew();
                Game.Console.WriteLine("\nEngineer Scott- \"Sorry, Captain. Until this damage");
                Game.Console.WriteLine("  is repaired, I can only give you warp 4.\"");
                return;
            }//if

            //Read in course and distance
            getcd(false, null, ship, game.Turn);
            if (game.Turn.direc == -1.0)
                return;

            //Make sure starship has enough energy for the trip
            double power = (game.Turn.dist + 0.05) * ship.Warp * ship.Warp * ship.Warp * (ship.ShieldsUp ? 2 : 1);
            if (power >= ship.ShipEnergy)
            {
                //Insufficient power for trip
                Game.Console.WriteLine("\nEngineering to bridge--");
                if (!ship.ShieldsUp || 0.5 * power > ship.ShipEnergy)
                {
                    int iwarp = (int)(Math.Pow((ship.ShipEnergy / (game.Turn.dist + 0.05)), 0.333333333));
                    if (iwarp <= 0)
                    {
                        Game.Console.WriteLine("We can't do it, Captain. We haven't the energy.");
                    }//if
                    else
                    {
                        Game.Console.Write("We haven't the energy, but we could do it at warp {0}", iwarp.ToString());
                        if (ship.ShieldsUp)
                            Game.Console.WriteLine(",\nif you'll lower the shields.");
                        else
                            Game.Console.WriteLine(".");
                    }//else
                }//if
                else
                    Game.Console.WriteLine("We haven't the energy to go that far with the shields up.");
                return;
            }//if

            //Make sure enough time is left for the trip
            game.Turn.Time = 10.0 * game.Turn.dist / Math.Pow(ship.Warp, 2);
            if (game.Turn.Time >= 0.8 * game.RemainingTime)
            {
                Game.Console.WriteLine("\nFirst Officer Spock- \"Captain, I compute that such");
                Game.Console.Write("  a trip would require approximately {0,0:F2}", (100.0 * game.Turn.Time / game.RemainingTime));
                Game.Console.WriteLine(" percent of our\n  remaining time.  Are you sure this is wise?\"");
                if (!Game.Console.ja())
                    return;
            }
            warpx(game);
        }//Warp


        private static void warpx(GameData game)
        {
            //Entry WARPX
            FederationShip ship = game.Galaxy.Ship;

            bool twarp = false;
            bool blooey = false;
            if (ship.Warp > 6.0)
            {
                //Decide if engine damage will occur
                double prob = game.Turn.dist * (6.0 - ship.Warp) * (6.0 - ship.Warp) / 66.666666666;
                if (prob > game.Random.Rand())
                {
                    blooey = true;
                    game.Turn.dist = game.Random.Rand() * game.Turn.dist;
                }//if

                //Decide if time warp will occur
                twarp = (0.5 * game.Turn.dist * Math.Pow(7.0, ship.Warp - 10.0) > game.Random.Rand());

                if (blooey || twarp)
                {
                    //If time warp or engine damage, check path
                    //If it is obstructed, don't do time warp or engine damage
                    GalacticCourse course = new GalacticCourse(ship.GalacticCoordinate, game.Turn.direc, game.Turn.dist);
                    while (course.Next() && course.SameQuadrant)
                    {
                        if ((!(game.Galaxy.CurrentQuadrant[course.CurrentCoordinate.Sector] is Empty)))
                        {
                            blooey = false;
                            twarp = false;
                            break;
                        }
                    }//while
                }//if
            }//if

            //Activate Warp Engines and pay the cost
            Move(game);
            if (game.Turn.alldone)
                return;

            //added this logic. If we happened to have been tractor beamed during move, we want to process
            //events here. No need for following logic after that.
            if (game.Date + game.Turn.Time >= game.Future[FutureEvents.EventTypesEnum.FTBEAM])
            {
                Events.ProcessEvents(game);
                return;
            }

            ship.ShipEnergy -= game.Turn.dist * ship.Warp * ship.Warp * ship.Warp * (ship.ShieldsUp ? 2 : 1);
            if (ship.ShipEnergy <= 0) 
                Finish.finish(Finish.FINTYPE.FNRG, game);
            game.Turn.Time = 10.0 * game.Turn.dist / (ship.Warp * ship.Warp);

            if (twarp)
                TimeWarp(game);

            if (blooey)
            {
                ship.ShipDevices.SetDamage(ShipDevices.ShipDevicesEnum.WarpEngines, (game.DamageFactor * (3.0 * game.Random.Rand() + 1.0)));
                Game.Console.WriteLine("\nEngineering to bridge--");
                Game.Console.WriteLine("  Scott here.  The warp engines are damaged.");
                Game.Console.WriteLine("  We'll have to reduce speed to warp 4.");
            }//if
            game.Turn.ididit = true;
        }//warpx

        /// <summary>
        /// This is called whenever the ship has to leave in a hurry and kirk is not aboard
        /// the ship. These cases can be:
        /// 1) Ship has been tractor beamed away and kirk is on planet
        /// 2) Star has gone Supernova and ship is trying to escape while kirk on planet
        /// </summary>
        /// <param name="game"></param>
        /// <param name="igrab"></param>
        internal static void atover(GameData game, bool igrab)
        {
            FederationShip ship = game.Galaxy.Ship;
            Game.Console.chew();

            //is captain on planet?
            if (ship.CrewLocation == FederationShip.CrewLocationEnum.Planet)
            {//kirk is on planet
                //is transporter damaged? If so kirk is out of luck
                if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Transporter))
                {
                    Finish.finish(Finish.FINTYPE.FPNOVA, game);
                    return;
                }

                //attempt to beam kirk up from planet
                Game.Console.WriteLine("Scotty rushes to the transporter controls.");

                //shields are up, tough luck
                if (ship.ShieldsUp)
                {
                    Game.Console.WriteLine("But with the shields up it's hopeless.");
                    Finish.finish(Finish.FINTYPE.FPNOVA, game);
                }

                //give transport a shot, looks like a 50-50 chance
                Game.Console.WriteLine("His desperate attempt to rescue you . . .");
                if (game.Random.Rand() <= 0.5)
                {
                    Game.Console.WriteLine("fails.");
                    Finish.finish(Finish.FINTYPE.FPNOVA, game);
                    return;
                }

                //yipee, transport worked
                Game.Console.WriteLine("SUCCEEDS!");

                //if we were mining crystals check if they survived ...
                if (game.Galaxy.Ship.CrystalsMined)
                {
                    game.Galaxy.Ship.CrystalsMined = false;
                    bool saved = game.Random.Rand() <= 0.25;
                    Game.Console.Write("The crystals mined were {0}.", saved ? "lost" : "saved");
                    if (saved)
                    {
                        ship.Crystals = true;
                        ship.CrystalUses = 0;
                    }
                }//if

                //make sure crew is back aboard ship
                ship.CrewLocation = FederationShip.CrewLocationEnum.Ship;
            }//if

            if (igrab)
                return;

            //Check to see if captain in shuttle craft
            if (ship.CrewLocation == FederationShip.CrewLocationEnum.Shuttle)
                Finish.finish(Finish.FINTYPE.FSTRACTOR, game);

            if (game.Turn.alldone)
                return;

            //Inform captain of attempt to reach safety
            Game.Console.Skip(1);
            do
            {
                if (game.Turn.justin)
                {
                    Game.Console.WriteLine("***RED ALERT!  READ ALERT!");
                    Game.Console.WriteLine("\nThe {0} has stopped in a quadrant containing\n   a supernova.\n\n", ship.Name);
                }//if
                Game.Console.WriteLine("***Emergency automatic override attempts to hurl {0}\nsafely out of quadrant.", ship.Name);
                game.Galaxy[ship.QuadrantCoordinate].Starch = ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) ? game.Galaxy[ship.QuadrantCoordinate].ToInt + 1000 : 1;

                //Try to use warp engines. If damaged, tough luck
                if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.WarpEngines))
                {
                    Game.Console.WriteLine("\nWarp engines damaged.");
                    Finish.finish(Finish.FINTYPE.FSNOVAED, game);
                    return;
                }//if

                //compute a random warp factor > 6 and set as ship speed
                ship.Warp = 6.0 + (2.0 * game.Random.Rand());
                Game.Console.WriteLine("Warp factor set to {0,1:F1}", ship.Warp);

                double dist = ((0.75 * ship.ShipEnergy) / (ship.Warp * ship.Warp * ship.Warp * (ship.ShieldsUp ? 2 : 1)));
                game.Turn.dist = Math.Min(dist, (1.4142 + game.Random.Rand()));
                game.Turn.Time = (10.0 * game.Turn.dist / (ship.Warp * ship.Warp));
                game.Turn.direc = GalacticCourse.RandomDirection(game.Random);//How dumb!
                game.Turn.justin = false;
                ship.Orbit = null;
                warpx(game);

                if (!game.Turn.justin)
                {//This is bad news, we didn't leave quadrant.
                    if (game.Turn.alldone)
                        return;

                    Game.Console.WriteLine("\nInsufficient energy to leave quadrant.");
                    Finish.finish(Finish.FINTYPE.FSNOVAED, game);
                    return;
                }//if

                //Repeat if another snova
            } while (game.Galaxy[ship.QuadrantCoordinate].SuperNova);

            //check if this supernova killed off the last klingons ...
            if (game.Galaxy.Klingons <= 0)
                Finish.finish(Finish.FINTYPE.FWON, game);//Snova killed remaining enemy.

        }//atover

        /// <summary>
        /// Attempt to dock the ship to a starbase. Ship must not be in orbit of a planet and 
        /// must be adjacent to a starbase to be able to dock.
        /// </summary>
        /// <param name="game"></param>
        internal static void Dock(GameData game)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;

            Game.Console.chew();

            //If ship is already docked, then inform and return
            if (ship.Docked)
            {
                Game.Console.WriteLine("Already docked.");
                return;
            }//if

            //check if ship is in orbit around a planet ...
            if (ship.Orbit != null)
            {
                Game.Console.WriteLine("You must first leave standard orbit.");
                return;
            }//if

            //if not adjacent to a starbase, then print error and return
            if (galaxy.CurrentQuadrant.SectorStarBase == null || !ship.Sector.AdjacentTo(galaxy.CurrentQuadrant.SectorStarBase.Sector))
            {
                Game.Console.WriteLine("{0} not adjacent to base.", ship.Name);
                return;
            }//if

            //ok, go ahead and dock the ship
            ship.Docked = true;
            Game.Console.WriteLine("Docked.");

            //resupply ship with consumables. If main energy is higher than inital, leave it alone.
            //This can happen if dylithium crystals were used
            if (ship.ShipEnergy < ship.InitialMainEnergy)
                ship.ShipEnergy = ship.InitialMainEnergy;

            ship.ShieldEnergy = ship.InitialShieldEnergy;
            ship.Torpedoes = ship.InitialTorpedoes;
            ship.LifeSupportReserves = ship.InitialLifeSupport;

            if (ship.StarChartDamage != FutureEvents.NEVER && (game.Future[FutureEvents.EventTypesEnum.FCDBAS] < FutureEvents.NEVER || galaxy.SuperCommanderAttack != null) && !game.Turn.iseenit)
            {
                //get attack report from base
                Game.Console.WriteLine("Lt. Uhura- \"Captain, an important message from the starbase:\"");
                Reports.AttackReport(game);
                game.Turn.iseenit = true;
            }//if

        }//Dock

        /// <summary>
        /// Call for help to a starbase. The closest starbase is found and it will make 3 attempts
        /// to transport the ship to its own location. If 3 failures then game over. Otherwise the
        /// ship is transported docked to the helping starbase.
        /// The radio must not be damaged for this to work.
        /// </summary>
        /// <param name="game"></param>
        internal static void Help(GameData game)
        {
            //There's more than one way to move in this game!
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = game.Galaxy.Ship;

            Game.Console.chew();
            //Test for conditions which prevent calling for help
            if (ship.Docked)
            {
                Game.Console.WriteLine("Lt. Uhura-  \"But Captain, we're already docked.\"");
                return;
            }//if

            //check if subspace radio is damaged. If any damage, no luck
            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio))
            {
                Game.Console.WriteLine("Subspace radio damaged.");
                return;
            }//if

            //Hey ... no starbases to actually help out
            if (galaxy.Bases.Count == 0)
            {
                Game.Console.WriteLine("Lt. Uhura-  \"Captain, I'm not getting any response from Starbase.\"");
                return;
            }//if

            //kirk is on planet, can't leave without him
            if (ship.CrewLocation == FederationShip.CrewLocationEnum.Planet)
            {
                Game.Console.WriteLine("You must be aboard the {0}.", ship.Name);
                return;
            }//if

            //OK -- call for help from nearest starbase
            //first increment call for help counter
            game.CallsForHelp++;

            //find nearest starbase(we know there is at least one in galaxy)
            //The StarBaseList ctor below will sort based on distance from ship.
            QuadrantStarBase starbase = new StarBaseList(galaxy, ship.QuadrantCoordinate)[0];

            //determine if the ship is in the same quadrant as the starbase
            bool sameQuadrant = starbase.QuadrantCoordinate.Equals(ship.QuadrantCoordinate);

            //compute distance from ship to starbase in sectors
            double ddist;

            //we need to generate a new current quadrant if the starbase is in a different quadrant
            //than the current ship one. It would make sense if this was generated after deciding
            //the call for help was successful, but we have to keep this order to keep the random
            //number generator in step.
            if (!sameQuadrant)
            {
                //Since starbase not in quadrant, set up new quadrant
                //compute the distance from ship to starbase in quadrants then convert to sectors
                ddist = (ship.QuadrantCoordinate.DistanceTo(starbase.QuadrantCoordinate) * 10.0);
                ship.GalacticCoordinate = new GalacticCoordinate(starbase.QuadrantCoordinate, ship.GalacticCoordinate.Sector);
                galaxy.newquad(game, true);
            }//if
            else
            {
                //ship is in same quadrant as starbase, compute distance in sectors from ship to starbase
                ddist = (ship.Sector.DistanceTo(galaxy.CurrentQuadrant.SectorStarBase.Sector));
            }//else

            //dematerialize starship(newquad placed ship into a sector, must remove it)
            galaxy.CurrentQuadrant[ship.Sector] = new Empty();
            //inform user ship is dematerializing ...
            Game.Console.WriteLine("Starbase in{0} responds--{1} dematerializes.", starbase.QuadrantCoordinate.ToString(true), ship.Name);

            //Give starbase three chances to rematerialize starship
            //Chance of success less as distance increases
            double probf = Math.Pow((1.0 - Math.Pow(0.98, ddist)), 0.33333333);

            int attempt = 1;
            for (attempt = 1; attempt <= 3; attempt++)
            {
                switch (attempt)
                {
                    case 1: Game.Console.Write("1st"); break;
                    case 2: Game.Console.Write("2nd"); break;
                    case 3: Game.Console.Write("3rd"); break;
                }//switch
                Game.Console.WriteLine(" attempt to re-materialize {0} . . . . . ", ship.Name);
                if (game.Random.Rand() > probf)
                    break;
                Game.Console.WriteLine("fails.");
            }//for attempt

            //all 3 attempts failed ...
            if (attempt > 3)
            {
                Finish.finish(Finish.FINTYPE.FMATERIALIZE, game);
                return;
            }//if

            //Rematerialization attempt should succeed if can get adj to base
            SectorStarBase ssb = galaxy.CurrentQuadrant.SectorStarBase;

            //going to try 5 times to find an adjacent sector
            for (int ii = 1; ii <= 5; ii++)
            {
                //generate a random sector around the starbase
                SectorCoordinate sc = ssb.Sector.RandomAround(game.Random);

                //if it is a valid sector(inside quadrant) and empty, then park the ship there
                if (sc.Valid && galaxy.CurrentQuadrant[sc] is Empty)
                {
                    //found one -- finish up
                    Game.Console.WriteLine("succeeds.");
                    galaxy.CurrentQuadrant[sc] = ship;

                    //dock the ship
                    Dock(game);

                    //compute enemy distances
                    galaxy.CurrentQuadrant.CalculateDistances(ship.Sector);
                    galaxy.CurrentQuadrant.ResetAverageDistances();

                    //and let captain know we made it
                    Game.Console.WriteLine("\nLt. Uhura-  \"Captain, we made it!\"");
                    return;
                }//if
            }//for ii

            //could not find an empty sector around starbase ... game over
            Finish.finish(Finish.FINTYPE.FMATERIALIZE, game);
        }//Help

        /// <summary>
        /// Abandon the Starship Enterprise. (Ye Farie Queen cannot be abandoned)
        /// Transfer to the Farie Queen.
        /// </summary>
        /// <param name="game"></param>
        internal static void AbandonShip(GameData game)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = game.Galaxy.Ship;

            Game.Console.chew();
            bool docked = ship.Docked;

            if (docked)
            {
                if (!ship.HasShuttleBay)
                {
                    Game.Console.WriteLine("You cannot abandon Ye {0}.", ship.Name);
                    return;
                }
            }//if
            else
            {
                //Must take shuttle craft to abandon ship. fq has no shuttle
                if (!ship.HasShuttleBay)
                {
                    Game.Console.WriteLine("Ye {0} has no shuttle craft.", ship.Name);
                    return;
                }//if

                //shuttle was lost previously and cannot be used
                if (ship.ShuttleLocation == FederationShip.ShuttleLocationEnum.Aliens)
                {
                    Game.Console.WriteLine("Shuttle craft now serving Big Mac's.");
                    return;
                }//if

                //shuttlecraft is damaged and cannot be used
                if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.ShuttleCraft))
                {
                    Game.Console.WriteLine("Shuttle craft damaged.");
                    return;
                }//if

                //crew is on planet with shuttle. Cannot abandon.
                if (ship.CrewLocation == FederationShip.CrewLocationEnum.Planet)
                {
                    Game.Console.WriteLine("You must be aboard the Enterprise.");
                    return;
                }//if

                //there is no shuttle in bay ??? (where is it?)
                //maybe enroute from planet?
                if (ship.ShuttleLocation != FederationShip.ShuttleLocationEnum.ShuttleBay)
                {
                    Game.Console.WriteLine("Shuttle craft not currently available.");
                    return;
                }//if

                //Print abandon ship messages
                Game.Console.WriteLine("\n***ABANDON SHIP!  ABANDON SHIP!");
                Game.Console.WriteLine("\n***ALL HANDS ABANDON SHIP!");
                Game.Console.WriteLine("\n\nCaptain and crew escape in shuttle craft.");
                Game.Console.WriteLine("Remainder of ship's complement beam down");
                Game.Console.WriteLine("to nearest habitable planet.");

                ship.Crystals = false;

                //get a list of remaining starbases
                StarBaseList bases = new StarBaseList(galaxy);

                //if none left, then we are totally out of luck
                if (bases.Count == 0)
                {//Ops! no place to go...
                    Finish.finish(Finish.FINTYPE.FABANDN, game);
                    return;
                }//if

                //If at least one base left, give 'em the Faerie Queene
                Game.Console.WriteLine("\nYou are captured by Klingons and released to");
                Game.Console.WriteLine("the Federation in a prisoner-of-war exchange.");

                //select a starbase at random and attempt to place fq adjacent to it
                QuadrantStarBase sb = bases.RandomStarbase(game.Random);

                //if selected starbase is in a new quadrant, then must regenerate current quad
                if (!sb.QuadrantCoordinate.Equals(ship.QuadrantCoordinate))
                {
                    //make sure fq is located in new quadrant and in the middle sector. This is to emulate
                    //the original logic and keep the positioning the same.
                    ship.GalacticCoordinate = new GalacticCoordinate(sb.QuadrantCoordinate, SectorCoordinate.Middle);

                    //generate a new quadrant.
                    galaxy.newquad(game, true);

                }//if

                //ok, now attempt to locate a sector adjacent to starbase
                //remove ship from current quadrant
                galaxy.CurrentQuadrant[ship.Sector] = new Empty();

                //get a reference to the local starbase object
                SectorStarBase starbase = galaxy.CurrentQuadrant.SectorStarBase;

                //goofy logic here, but needs to be preserved to keep random number generation in sync
                //with original program.
                while (true)
                {//position next to base by trial and error
                    int attempt;
                    for (attempt = 1; attempt <= 10; attempt++)
                    {
                        //generate a random sector around starbase
                        SectorCoordinate sc = starbase.Sector.RandomAround(game.Random);

                        //if its valid(inside quadrant) and empty, then success
                        if (sc.Valid && galaxy.CurrentQuadrant[sc] is Empty)
                        {
                            //good spot. Set ship sector, set docked and break out.
                            ship.Sector = sc;
                            docked = true;
                            break;
                        }//if
                    }//for attempt
                    if (attempt < 11)
                        break;//found a spot

                    //ok, failed after 10 attempts. May as well go ahead and regenerate
                    //another quadrant and try again. Eventually we will succeed.
                    //Set ship in the middle of the quadrant this time.
                    ship.Sector = new SectorCoordinate(SectorCoordinate.Middle);

                    //galaxy.CurrentQuadrant[SectorCoordinate.Middle] = ship;
                    galaxy.newquad(game, true);

                }//while(true)
            }//else

            //Get new commission. If enterprise was docked, then make sure fq is docked
            FaerieQueene fq = new FaerieQueene();
            fq.QuadrantCoordinate = ship.QuadrantCoordinate;
            fq.Sector = ship.Sector;
            fq.Docked = docked;

            //let user know they have a new ship ...
            Game.Console.WriteLine("Starfleet puts you in command of another ship,");
            Game.Console.WriteLine("the Faerie Queene, which is antiquated but,");
            Game.Console.WriteLine("still useable.");

            //if crystals were mined and aboard the enterprise, then bring them to the fq
            //also preserve the number of times crystals have been used.
            if (ship.Crystals)
            {
                Game.Console.WriteLine("The dilithium crystals have been moved.");
                fq.Crystals = true;
                fq.CrystalUses = ship.CrystalUses;
            }//if

            galaxy.Ship = fq;
            galaxy.CurrentQuadrant[ship.Sector] = fq;

            //if crystals had been mined but not brought aboard enterprise, then they are lost
            game.Galaxy.Ship.CrystalsMined = false;

        }//AbandonShip

        /// <summary>
        /// Launch a Deep Space Probe.
        /// </summary>
        /// <param name="game"></param>
        internal static void LaunchProbe(GameData game)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = game.Galaxy.Ship;

            //make sure we have at least 1 to launch
            //FQ has no probes
            if (ship.Probes == 0)
            {
                Game.Console.chew();
                Game.Console.Skip(1);
                if (ship.HasProbes)
                    Game.Console.WriteLine("Engineer Scott- \"We have no more deep space probes, Sir.\"");
                else
                    Game.Console.WriteLine("Ye {0} has no deep space probes.", ship.Name);
                return;
            }//if

            //Check for damage
            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.DSProbe))
            {
                Game.Console.chew();
                Game.Console.WriteLine("\nEngineer Scott- \"The probe launcher is damaged, Sir.\"");
                return;
            }//if

            //Check if a probe is already in progress. Only 1 probe at a time can be active.
            if (game.Future[FutureEvents.EventTypesEnum.FDSPROB] != FutureEvents.NEVER)
            {
                Game.Console.chew();
                Game.Console.Skip(1);
                if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) && !ship.Docked)
                {
                    Game.Console.WriteLine("Spock-  \"Records show the previous probe has not yet");
                    Game.Console.WriteLine("   reached it's destination.\"");
                }//if
                else
                {
                    Game.Console.WriteLine("Uhura- \"The previous probe is still reporting data, Sir.\"");
                }//else
                return;
            }//if

            object key = Game.Console.scan();
            if (key == null)
            {
                //slow mode, so let Kirk know how many probes there are left
                Game.Console.WriteLine("{0,1} probe{1} left.", ship.Probes, ship.Probes == 1 ? "" : "s");
                Game.Console.Write("Are you sure you want to fire a probe? ");
                if (!Game.Console.ja())
                    return;
            }//if

            bool isarmed = false;
            if (key is string && (key as string) == "armed")
            {
                isarmed = true;
                key = Game.Console.scan();
            }//if
            else if (key == null)
            {
                Game.Console.Write("Arm NOVAMAX warhead?");
                isarmed = Game.Console.ja();
            }//else if

            key = string.Empty;
            if (!Game.Console.EOL)
                key = Game.Console.scan();

            //get coordinates of probe destination
            getcd(true, (key as string), ship, game.Turn);
            if (game.Turn.direc == -1.0)
                return;

            //decrement remaining probes
            ship.Probes--;

            //Create a probe object to track it.
            galaxy.Probe = new Probe(ship.GalacticCoordinate, game.Turn.direc, game.Turn.dist, isarmed);

            //schedule a future event to move the probe
            game.Future[FutureEvents.EventTypesEnum.FDSPROB] = game.Date + 0.01; // Time to move one sector
            Game.Console.WriteLine("Ensign Chekov-  \"The deep space probe is launched, Captain.\"");
        }//LaunchProbe

    }//class Moving
}