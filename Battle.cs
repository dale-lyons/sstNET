using System;
using System.Collections.Generic;
using System.Text;

using sstNET.Galaxy;
using sstNET.Galaxy.SectorObjects;
using sstNET.Galaxy.SectorObjects.Ships;

namespace sstNET
{
    internal static class Battle
    {
        private const double mPhaserFactor = 2.0;

        private enum ShieldMode
        {
            NOTSET,
            MANUAL,
            FORCEMAN,
            AUTOMATIC
        }

        private enum ShieldAction
        {
            NONE,
            SHUP,
            SHDN,
            NRG
        }

        /// <summary>
        /// Raise or lower the shields or transfer energy between ship main energy and shields.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="forceShieldsUp"></param>
        internal static void Shield(GameData game, bool forceShieldsUp)
        {
            ShieldAction action = forceShieldsUp ? ShieldAction.SHUP : ShieldAction.NONE;
            FederationShip ship = game.Galaxy.Ship;

            game.Turn.ididit = false;

            if (!forceShieldsUp)
            {
                object key = Game.Console.scan();
                if (key is string)
                {
                    if (SSTConsole.isit(key, "transfer"))
                    {
                        action = ShieldAction.NRG;
                    }
                    else
                    {
                        Game.Console.chew();
                        if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Shields))
                        {
                            Game.Console.WriteLine("Shields damaged and down.");
                            return;
                        }//if
                        if (SSTConsole.isit(key, "up"))
                            action = ShieldAction.SHUP;
                        else if (SSTConsole.isit(key, "down"))
                            action = ShieldAction.SHDN;
                    }//else
                }//if

                if (action == ShieldAction.NONE)
                {
                    Game.Console.Write("Do you wish to change shield energy? ");
                    if (Game.Console.ja())
                    {
                        Game.Console.Write("Energy to transfer to shields- ");
                        action = ShieldAction.NRG;
                    }//if
                    else if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Shields))
                    {
                        Game.Console.WriteLine("Shields damaged and down.");
                        return;
                    }//else if
                    else if (ship.ShieldsUp)
                    {
                        Game.Console.Write("Shields are up. Do you want them down? ");
                        if (Game.Console.ja())
                            action = ShieldAction.SHDN;
                        else
                        {
                            Game.Console.chew();
                            return;
                        }//else
                    }//else if
                    else
                    {
                        Game.Console.Write("Shields are down. Do you want them up? ");
                        if (Game.Console.ja())
                            action = ShieldAction.SHUP;
                        else
                        {
                            Game.Console.chew();
                            return;
                        }//else
                    }//else
                }//if
            }//if

            switch (action)
            {
                case ShieldAction.SHUP://raise shields
                    if (ship.ShieldsUp)
                    {
                        Game.Console.WriteLine("Shields already up.");
                        return;
                    }//if
                    ship.ShieldsUp = true;
                    ship.ShieldChange = true;

                    if (!ship.Docked)
                        ship.ShipEnergy -= 50.0;

                    Game.Console.WriteLine("Shields raised.");
                    if (ship.ShipEnergy <= 0)
                    {
                        Game.Console.WriteLine("\nShields raising uses up last of energy.");
                        Finish.finish(Finish.FINTYPE.FNRG, game);
                        return;
                    }//if
                    game.Turn.ididit = true;
                    break;

                case ShieldAction.SHDN:
                    if (!ship.ShieldsUp)
                    {
                        Game.Console.WriteLine("Shields already down.");
                        return;
                    }//if
                    ship.ShieldsUp = false;
                    ship.ShieldChange = true;
                    Game.Console.WriteLine("Shields lowered.");
                    game.Turn.ididit = true;
                    break;

                case ShieldAction.NRG:
                    {
                        object tok;
                        while (!((tok = Game.Console.scan()) is double))
                        {
                            Game.Console.chew();
                            Game.Console.Write("Energy to transfer to shields- ");
                        }//while

                        if (!(tok is double))
                            return;

                        double aaitem = (double)tok;
                        if (aaitem == 0)
                            return;

                        if (aaitem > ship.ShipEnergy)
                        {
                            Game.Console.WriteLine("Insufficient ship energy.");
                            return;
                        }//if

                        if (ship.ShieldEnergy + aaitem >= ship.InitialShieldEnergy)
                        {
                            Game.Console.WriteLine("Shield energy maximized.");
                            if (ship.ShieldEnergy + aaitem > ship.InitialShieldEnergy)
                            {
                                Game.Console.WriteLine("Excess energy requested returned to ship energy");
                            }//if
                            ship.ShipEnergy -= (ship.InitialShieldEnergy - ship.ShieldEnergy);
                            ship.ShieldEnergy = ship.InitialShieldEnergy;
                            game.Turn.ididit = true;
                            return;
                        }//if
                        if (aaitem < 0.0 && ship.ShipEnergy - aaitem > ship.InitialMainEnergy)
                        {
                            //Prevent shield drain loophole
                            Game.Console.WriteLine("\nEngineering to bridge--");
                            Game.Console.WriteLine("  Scott here. Power circuit problem, Captain.");
                            Game.Console.WriteLine("  I can't drain the shields.");
                            return;
                        }//if
                        if (ship.ShieldEnergy + aaitem < 0)
                        {
                            Game.Console.WriteLine("All shield energy transferred to ship.");
                            ship.ShipEnergy += ship.ShieldEnergy;
                            ship.ShieldEnergy = 0.0;
                            game.Turn.ididit = true;
                            return;
                        }//if
                        Game.Console.Write("Scotty- \"");
                        if (aaitem > 0)
                            Game.Console.WriteLine("Transferring energy to shields.\"");
                        else
                            Game.Console.WriteLine("Draining energy from shields.\"");

                        ship.ShieldEnergy += aaitem;
                        ship.ShipEnergy -= aaitem;
                        game.Turn.ididit = true;
                    } break;
                default: return;
            }//switch
        }//Shield

        /// <summary>
        /// A collision between Federation ship and an Enemy ship
        /// </summary>
        /// <param name="rammed">federation ship is being rammed(true)</param>
        /// <param name="es">Enemy ship we are ramming or getting rammed by</param>
        /// <param name="sc">The coordinate of the enemy ship we collided with</param>
        internal static void Ram(GameData game, bool rammed, EnemyShip es)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;

            Game.Console.WriteLine("***RED ALERT!  RED ALERT!\n");
            Game.Console.WriteLine("***COLLISION IMMINENT.\n\n");
            Game.Console.Write("***{0}{1}{2} at{3}", ship.Name, rammed ? " rammed by " : " rams ", es.Name, es.Sector.ToString(true));

            if (rammed)
                Game.Console.Write(" (original position)");

            Game.Console.Skip(1);

            if (rammed)
                es.killShip(game, ship.Sector); //Enemy ship rammed friendly
            else
                es.killShip(game); //Friendly rammed enemy ship

            Game.Console.WriteLine("***{0} heavily damaged.", ship.Name);
            int icas = (int)(10.0 + 20.0 * game.Random.Rand());
            Game.Console.WriteLine("***Sickbay reports {0} casualties.", icas);
            game.Casualties += icas;

            ship.ShipDevices.DamageDevices(game, es.RamDamageFactor);
            ship.ShieldsUp = false;

            if (galaxy.Klingons > 0)
                Reports.DamageReport(game);
            else
                Finish.finish(Finish.FINTYPE.FWON, game); //Rammed last enemy ship!

        }//Ram

        /// <summary>
        /// Handle a single torpedo. Can be fired by either Federation ship or enemy.
        /// Track the torpedo until it either hits something or runs out of the current quad
        /// </summary>
        /// <param name="course">This is the actual course the torpedo was aimed at</param>
        /// <param name="r">A random course altering amount, error if you like</param>
        /// <param name="sc">The source coordinate of the torpedo</param>
        /// <param name="hit">The hit amount, only set if torpedo hits Federation ship</param>
        /// <param name="game">The current game</param>
        private static void Torpedo(double course, double r, SectorCoordinate sc, out double hit, GameData game)
        {
            //assume no hit
            hit = 0;

            //compute the torpedo course based on aim course and random amount
            double ac = course + 0.25 * r;

            //convert actual course to angle in radians
            double angle = GalacticCourse.DirectionToRadians(ac);

            //convert aiming course to angle
            double bullseye = GalacticCourse.DirectionToRadians(course);

            //setup a GalacticCourse object to track torpedo through quadrant
            GalacticCourse torpCourse = new GalacticCourse(new GalacticCoordinate(sc), ac);

            //just keep looping here, we will break out when done
            //Either the torpedo hits something or it runs out of the quadrant
            for (int ii = 1; ; ii++)
            {
                //increment torpedo location to next position
                torpCourse.Next();

                //extract new location and check if out of quadrant. If so, we are done.
                if (!torpCourse.SameQuadrant)
                    break;

                //for text output format. This is critical to keep output the same as original code.
                if (ii == 4 || ii == 9)
                    Game.Console.Skip(1);

                //output torpedo track (can't use tostring, format is unique)
                //"7.0 - 6.0"
                Game.Console.Write("{0,0:F1} - {1,0:F1}   ", torpCourse.CurrentSectorX, torpCourse.CurrentSectorY);

                //get the sector coordinates of the new location of torpedo
                SectorCoordinate ixy = torpCourse.CurrentCoordinate.Sector;

                //get the Sector object at torpedos new location
                SectorObject so = game.Galaxy.CurrentQuadrant[ixy];

                //if its an empty sector, continue tracking torpedo
                if (so is Empty)
                    continue;

                //hit something
                Game.Console.Skip(1);

                //let sector object handle the hit logic
                //the actual angle and aim angle ae used to compute intensity of hit
                hit = so.TorpedoHit(game, sc, bullseye, angle);

                //since we hit something we are all done, just return
                return;
            }//for

            //if we get here we ran off edge of quadrant without hitting anything
            Game.Console.WriteLine("\nTorpedo missed.");

        }//Torpedo

        /// <summary>
        /// Called when the ship takes a serious hit.
        /// Check if it is critical and if so damage some devices.
        /// </summary>
        /// <param name="hit">Amount of hit</param>
        /// <param name="game"></param>
        private static void Fry(GameData game, double hit)
        {
            //check if a critical hit occured
            if (hit < (275.0 - 25.0 * (int)game.GameSkill) * (1.0 + 0.5 * game.Random.Rand()))
                return;

            //A critical hit has ocurred. Damage some devices on the ship
            FederationShip ship = game.Galaxy.Ship;
            ship.ShipDevices.DamageRandomDevices(game, hit);

            //Check if shields damaged. If so, knock them down.
            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Shields) && ship.ShieldsUp)
            {
                Game.Console.WriteLine("***Shields knocked down.");
                ship.ShieldsUp = false;
            }//if
        }//Fry

        /// <summary>
        /// Attack the current ship.
        /// The bad guys get a chance to fight back. Possibly
        /// move locations and attack the friendly ship.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="forcePhasers">Phasers only used (for low skill levels)</param>
        internal static void Attack(GameData game, bool forcePhasers)
        {
            game.Turn.iattak = 1;
            if (game.Turn.alldone)
                return;

            Galaxy.Galaxy galaxy = game.Galaxy;

            //Check if tholian is present. If so, move it.
            if (galaxy.CurrentQuadrant.Tholian != null)
                AI.MoveTholian(game);

            //The one chance not to be attacked
            //When a new quadrant is created, the federation ship is granted
            //one turn grace before being attacked by Romulan(s). We check if the
            //grace is valid and if so, reset the flag and return with no attack.
            if (galaxy.CurrentQuadrant.RomulanAttack)
            {
                galaxy.CurrentQuadrant.RomulanAttack = false;
                return;
            }//if

            //if Commander or SuperCommander is present, OR game skill is high enough, let bad guys move first.
            if (((galaxy.CurrentQuadrant.Commander != null || galaxy.CurrentQuadrant.SuperCommander != null) && (!game.Turn.justin)) || game.GameSkill == GameData.GameSkillEnum.Emeritus)
                AI.MoveEnemyShips(game);

            EnemyShipList enemies = galaxy.CurrentQuadrant.Enemies;

            //No enemies around to attack us so all done. (possible bad guys fled in the move)
            if (enemies.Count == 0)
                return;

            FederationShip ship = galaxy.Ship;
            double pfac = 1.0 / ship.InitialShieldEnergy;
            double chgfac = 1.0;

            if (ship.ShieldChange)
                chgfac = 0.25 + 0.5 * game.Random.Rand();

            Game.Console.Skip(1);
            bool printSector = false;

            //low skill level means allow torpedo attack
            if (game.GameSkill <= GameData.GameSkillEnum.Fair)
            {
                forcePhasers = false;
                printSector = true;
            }

            double hittot = 0.0;
            double hitmax = 0;

            bool attempt = false;
            bool atackd = false;
            bool ihurt = false;

            //Let each bad guy attack us
            foreach (EnemyShip es in enemies)
            {
                //CurrentQuadrant.dumpkl(game, enemies);

                //bad guy with no power cannot attack
                if (es.Power < 0)
                    continue;

                //compute hit strength and diminsh shield power
                double r = game.Random.Rand();

                //Increase chance of photon torpedos if docked or enemy energy low
                if (ship.Docked)
                    r *= 0.25;

                if (es.Power < 500)
                    r *= 0.25;

                bool itflag = (es is Klingon && r > 0.0005) || forcePhasers || (es is Commander && r > 0.015) ||
                              (es is Romulan && r > 0.3) || (es is SuperCommander && r > 0.07);

                double hit = 0;
                if (itflag)
                {
                    //Enemy uses phasers
                    if (ship.Docked)
                        continue;//Don't waste the effort!

                    attempt = true;//Attempt to attack
                    double dustfac = 0.8 + 0.05 * game.Random.Rand();
                    hit = es.Power * Math.Pow(dustfac, es.AverageDistance);
                    es.Power *= 0.75;
                }//if
                else
                {//Enemy uses photon torpedo
                    //double course = 1.90985 * Math.Atan2((double)ship.Sector.Y - es.Sector.Y, (double)es.Sector.X - ship.Sector.X);
                    double course = es.Sector.CourseTo(ship.Sector);
                    hit = 0;

                    Game.Console.Write("***TORPEDO INCOMING");
                    if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SRSensors))
                        Game.Console.Write(" From {0} at{1}", es.Name, es.Sector.ToString(printSector));

                    attempt = true;
                    Game.Console.WriteLine("--");
                    r = (game.Random.Rand() + game.Random.Rand()) * 0.5 - 0.5;
                    r += 0.002 * es.Power * r;
                    Torpedo(course, r, es.Sector, out hit, game);

                    if (galaxy.Klingons == 0)
                        Finish.finish(Finish.FINTYPE.FWON, game);//Klingons did themselves in!

                    if (galaxy[ship.QuadrantCoordinate].SuperNova || game.Turn.alldone)
                        return;//Supernova or finished

                    if (hit == 0)
                        continue;

                }//else
                if (ship.ShieldsUp || ship.ShieldChange)
                {
                    //shields will take hits
                    double propor = pfac * ship.ShieldEnergy;
                    if (propor < 0.1)
                        propor = 0.1;

                    double hitsh = propor * chgfac * hit + 1.0;
                    atackd = true;
                    double absorb = 0.8 * hitsh;
                    if (absorb > ship.ShieldEnergy)
                        absorb = ship.ShieldEnergy;

                    ship.ShieldEnergy -= absorb;
                    hit -= hitsh;
                    if (propor > 0.1 && hit < 0.005 * ship.ShipEnergy)
                        continue;
                }//if

                //It's a hit -- print out hit size
                atackd = true;  //We weren't going to check casualties, etc. if
                //shields were down for some strange reason. This
                //doesn't make any sense, so I've fixed it
                ihurt = true;
                Game.Console.Write("{0,0:F2} unit hit", hit);
                if ((ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SRSensors) && itflag) || (int)game.GameSkill <= 2)
                    Game.Console.Write(" on the {0}", ship.Name);

                if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SRSensors) && itflag)
                    Game.Console.Write(" from {0} at{1}", es.Name, es.Sector.ToString(printSector));

                Game.Console.Skip(1);
                //Decide if hit is critical
                if (hit > hitmax)
                    hitmax = hit;

                hittot += hit;
                Fry(game, hit);

                Game.Console.Write("Hit {0,0:F1} energy {1,0:F1}\n", hit, ship.ShipEnergy);
                ship.ShipEnergy -= hit;

            }//foreach

            if (ship.ShipEnergy <= 0)
            {
                //Returning home upon your shield, not with it...
                Finish.finish(Finish.FINTYPE.FBATTLE, game);
                return;
            }//if

            if (!attempt && ship.Docked)
                Game.Console.WriteLine("***Enemies decide against attacking your ship.");

            if (!atackd)
                return;

            double percent = (int)(100.0 * pfac * ship.ShieldEnergy + 0.5);
            if (!ihurt)
            {
                //Shields fully protect ship
                Game.Console.Write("Enemy attack reduces shield strength to ");
            }//if
            else
            {
                //Print message if starship suffered hit(s)
                Game.Console.Write("\nEnergy left {0,0:F2}    shields ", ship.ShipEnergy);

                if (ship.ShieldsUp)
                    Game.Console.Write("up, ");
                else if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Shields))
                    Game.Console.Write("down, ");
                else
                    Game.Console.Write("damaged, ");
            }//else
            Game.Console.WriteLine("{0,1}%   torpedoes left {1,1}", percent, ship.Torpedoes);

            //Check if anyone was hurt
            if (hitmax >= 200 || hittot >= 500)
            {
                int icas = (int)(hittot * game.Random.Rand() * 0.015);
                if (icas >= 2)
                {
                    Game.Console.WriteLine("\nMc Coy-  \"Sickbay to bridge.  We suffered {0,1} casualties\n   in that last attack.\"", icas);
                    game.Casualties += icas;
                }//if
            }//if

            //After attack, reset average distance to enemies
            galaxy.CurrentQuadrant.ResetAverageDistances();

        }//Attack

        private static bool targetcheck(SectorCoordinate targ, SectorCoordinate ship, ref double course)
        {
            //Return TRUE if target is invalid
            if (!targ.Valid)
            {
                Game.Console.huh();
                return true;
            }

            if ((int)ship.DistanceTo(targ) == 0)
            {
                Game.Console.WriteLine("\nSpock-  \"Bridge to sickbay.  Dr. McCoy,");
                Game.Console.WriteLine("  I recommend an immediate review of");
                Game.Console.WriteLine("  the Captain's psychological profile.");
                Game.Console.chew();
                return true;
            }
            course = ship.CourseTo(targ);
            return false;
        }//targetcheck

        /// <summary>
        /// Fire the ships photon torpedoes
        /// </summary>
        /// <param name="game"></param>
        internal static void Photon(GameData game)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;

            game.Turn.ididit = false;

            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.PhotonTubes))
            {
                Game.Console.WriteLine("Photon tubes damaged.");
                Game.Console.chew();
                return;
            }//if
            if (ship.Torpedoes <= 0)
            {
                Game.Console.WriteLine("No torpedoes left.");
                Game.Console.chew();
                return;
            }//if

            object key = Game.Console.scan();
            int numTorpedoes;
            while (true)
            {
                if (key is string)
                {
                    Game.Console.huh();
                    return;
                }
                else if (key == null)
                {
                    Game.Console.WriteLine("{0} torpedoes left.", ship.Torpedoes);
                    Game.Console.Write("Number of torpedoes to fire- ");
                    key = Game.Console.scan();
                }//else if
                else
                {
                    numTorpedoes = (int)((double)key + 0.5);
                    if (numTorpedoes <= 0)
                    {//abort command
                        Game.Console.chew();
                        return;
                    }//if
                    if (numTorpedoes > 3)
                    {
                        Game.Console.chew();
                        Game.Console.WriteLine("Maximum of 3 torpedoes per burst.");
                        key = null;
                        continue;
                    }//if
                    if (numTorpedoes <= ship.Torpedoes)
                        break;
                    Game.Console.chew();
                    key = null;
                }//else
            }//for ;;

            double[] courses = new double[numTorpedoes];

            int ii;
            for (ii = 1; ii <= numTorpedoes; ii++)
            {
                key = Game.Console.scan();
                if (ii == 1 && key == null)
                    break;//we will try prompting

                if (ii == 2 && key == null)
                {//direct all torpedoes at one target
                    while (ii <= numTorpedoes)
                    {
                        courses[ii - 1] = courses[0];
                        ////todo - fix this
                        ////targ[i][1] = targ[1][1];
                        ////targ[i][2] = targ[1][2];
                        ////course[i] = course[1];
                        ii++;
                    }//while
                    break;
                }//if

                if (!(key is double))
                {
                    Game.Console.huh();
                    return;
                }//if
                int ix = (int)(double)key;

                key = Game.Console.scan();
                if (!(key is double))
                {
                    Game.Console.huh();
                    return;
                }//if
                int iy = (int)(double)key;

                if (targetcheck(new SectorCoordinate(ix, iy), ship.Sector, ref courses[ii - 1]))
                    return;

            }//for ii

            Game.Console.chew();
            if (ii == 1 && key == null)
            {//prompt for each one
                for (ii = 1; ii <= numTorpedoes; ii++)
                {
                    Game.Console.Write("Target sector for torpedo number{0,2}- ", ii);
                    key = Game.Console.scan();
                    if (!(key is double))
                    {
                        Game.Console.huh();
                        return;
                    }//if
                    int ix = (int)(double)key;
                    key = Game.Console.scan();
                    if (!(key is double))
                    {
                        Game.Console.huh();
                        return;
                    }//if
                    int iy = (int)(double)key;
                    Game.Console.chew();
                    if (targetcheck(new SectorCoordinate(ix, iy), ship.Sector, ref courses[ii - 1]))
                        return;
                }//for ii
            }//if

            game.Turn.ididit = true;
            //Loop for moving <n> torpedoes
            bool osuabor = false;
            for (ii = 1; ii <= numTorpedoes && !osuabor; ii++)
            {
                //if ship is docked use base torps, not ships
                if (!ship.Docked)
                    ship.Torpedoes--;

                double r = (game.Random.Rand() + game.Random.Rand()) * 0.5 - 0.5;
                if (Math.Abs(r) >= 0.47)
                {//misfire!
                    r = (game.Random.Rand() + 1.2) * r;
                    Game.Console.Write("***TORPEDO ");
                    if (numTorpedoes > 1)
                        Game.Console.Write("NUMBER\n{0,2} ", ii);

                    Game.Console.WriteLine("MISFIRES.\n");
                    if (ii < numTorpedoes)
                        Game.Console.WriteLine("  Remainder of burst aborted.");
                    osuabor = true;

                    if (game.Random.Rand() <= 0.2)
                    {
                        Game.Console.WriteLine("***Photon tubes damaged by misfire.");
                        double damage = game.DamageFactor * (1.0 + 2.0 * game.Random.Rand());
                        ship.ShipDevices.SetDamage(ShipDevices.ShipDevicesEnum.PhotonTubes, damage);
                        break;
                    }//if
                }//if
                if (ship.ShieldsUp || ship.Docked)
                    r *= 1.0 + 0.0001 * ship.ShieldEnergy;

                if (numTorpedoes != 1)
                    Game.Console.Write("\nTrack for torpedo number{0,2}-   ", ii);
                else
                    Game.Console.Write("\nTorpedo track- ");

                double dummy;
                Torpedo(courses[ii - 1], r, ship.Sector, out dummy, game);

                if (game.Turn.alldone || galaxy[ship.QuadrantCoordinate].SuperNova)
                    return;

            }//for ii

            if (galaxy.Klingons == 0)
                Finish.finish(Finish.FINTYPE.FWON, game);

        }//Photon

        /// <summary>
        /// Helper function to check if the phasers are over-heated by firing too much energy.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="rpow"></param>
        private static void Overheat(GameData game, double rpow)
        {
            if (rpow > 1500)
            {
                double chekbrn = (rpow - 1500.0) * 0.00038;
                if (game.Random.Rand() <= chekbrn)
                {
                    Game.Console.WriteLine("Weapons officer Sulu-  \"Phasers overheated, sir.\"");

                    double damage = game.DamageFactor * (1.0 + game.Random.Rand()) * (1.0 + chekbrn);
                    game.Galaxy.Ship.ShipDevices.SetDamage(ShipDevices.ShipDevicesEnum.Phasers, damage);
                }//if
            }//if
        }//Overheat

        private static bool checkshctrl(GameData game, double rpow)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;

            Game.Console.Skip(1);
            if (game.Random.Rand() < .998)
            {
                Game.Console.WriteLine("Shields lowered.");
                return false;
            }
            //Something bad has happened
            Game.Console.WriteLine("***RED ALERT!  RED ALERT!\n\n");
            double hit = rpow * ship.ShieldEnergy / ship.InitialShieldEnergy;
            ship.ShipEnergy -= rpow + hit * 0.8;
            ship.ShieldEnergy -= hit * 0.2;
            if (ship.ShipEnergy <= 0.0)
            {
                Game.Console.WriteLine("Sulu-  \"Captain! Shield malf***********************\"\n");
                Game.Console.stars();
                Finish.finish(Finish.FINTYPE.FPHASER, game);
                return true;
            }
            Game.Console.WriteLine("Sulu-  \"Captain! Shield malfunction! Phaser fire contained!\"\n\n");
            Game.Console.WriteLine("Lt. Uhura-  \"Sir, all decks reporting damage.\"\n");
            int icas = (int)(hit * game.Random.Rand() * 0.012);
            Fry(game, (0.8 * hit));

            if (icas != 0)
            {
                Game.Console.WriteLine("\nMcCoy to bridge- \"Severe radiation burns, Jim.");
                Game.Console.WriteLine("  {0,1} casualties so far.\"", icas);
                game.Casualties += icas;
            }//if

            Game.Console.WriteLine("\nPhaser energy dispersed by shields.");
            Game.Console.WriteLine("Enemy unaffected.");
            Overheat(game, rpow);
            return true;
        }//checkshctrl

        private static void hittem(GameData game, EnemyShipList enemies, double[] hits)
        {
            Game.Console.Skip(1);

            for (int kk = 0; kk < enemies.Count; kk++)
            {
                if (hits[kk] == 0)
                    continue;

                EnemyShip es = enemies[kk];

                double dustfac = 0.9 + 0.01 * game.Random.Rand();
                double hit = hits[kk] * Math.Pow(dustfac, es.Distance);
                double kpini = es.Power;
                double kp = Math.Abs(kpini);

                if (mPhaserFactor * hit < kp)
                    kp = mPhaserFactor * hit;

                es.Power -= (es.Power < 0 ? -kp : kp);
                double kpow = es.Power;

                if (hit > 0.005)
                    Game.Console.Write("{0,0:F2} unit hit on ", hit);
                else
                    Game.Console.Write("Very small hit on ");

                Game.Console.crmena(false, es, true, es.Sector);
                Game.Console.Skip(1);

                if (kpow == 0)
                {
                    es.killShip(game);
                    if (game.Galaxy.Klingons == 0)
                        Finish.finish(Finish.FINTYPE.FWON, game);
                    if (game.Turn.alldone)
                        return;
                }//if
                //decide whether or not to emasculate klingon
                else if (kpow > 0 && game.Random.Rand() >= 0.9 && kpow <= ((0.4 + 0.4 * game.Random.Rand()) * kpini))
                {
                    Game.Console.WriteLine("***Mr. Spock-  \"Captain, the vessel at{0}\n   has just lost its firepower.\"", es.Sector.ToString(true));
                    es.Power = -kpow;
                }//if

            }//for kk

        }//hittem

        /// <summary>
        /// Ship uses phasers
        /// </summary>
        /// <param name="game"></param>
        internal static void Phasers(GameData game)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;

            Game.Console.Skip(1);
            if (ship.Docked)
            {
                Game.Console.WriteLine("Phasers can't be fired through base shields.");
                Game.Console.chew();
                return;
            }//if
            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Phasers))
            {
                Game.Console.WriteLine("Phaser control damaged.");
                Game.Console.chew();
                return;
            }//if

            bool ifast = false;
            if (ship.ShieldsUp)
            {
                if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.ShieldControl))
                {
                    Game.Console.WriteLine("High speed shield control damaged.");
                    Game.Console.chew();
                    return;
                }//if
                if (ship.ShipEnergy <= 200.0)
                {
                    Game.Console.WriteLine("Insufficient energy to activate high-speed shield control.");
                    Game.Console.chew();
                    return;
                }//if
                Game.Console.WriteLine("Weapons Officer Sulu-  \"High-speed shield control enabled, sir.\"");
                ifast = true;
            }//if

            //SR sensors and Computer
            bool ipoop = !(ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SRSensors) || ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Computer));
            bool no = false;

            game.Turn.ididit = true;
            object key = null;

            //Original code so convoluted, I re-did it all
            ShieldMode automode = ShieldMode.NOTSET;
            int nenhere = galaxy.CurrentQuadrant.Enemies.Count;

            while (automode == ShieldMode.NOTSET)
            {
                key = Game.Console.scan();
                if (key is string)
                {
                    if (SSTConsole.isit(key, "manual"))
                    {
                        if (nenhere == 0)
                        {
                            Game.Console.WriteLine("There is no enemy present to select.");
                            Game.Console.chew();
                            key = null;
                            automode = ShieldMode.AUTOMATIC;
                        }//if
                        else
                        {
                            automode = ShieldMode.MANUAL;
                            key = Game.Console.scan();
                        }//else
                    }//if


                    else if (SSTConsole.isit(key, "automatic"))
                    {
                        if ((!ipoop) && nenhere != 0)
                        {
                            automode = ShieldMode.FORCEMAN;
                        }//if
                        else
                        {
                            if (nenhere == 0)
                                Game.Console.WriteLine("Energy will be expended into space.");
                            automode = ShieldMode.AUTOMATIC;
                            key = Game.Console.scan();
                        }//else
                    }//else if
                    else if (SSTConsole.isit(key, "no"))
                    {
                        no = true;
                    }//else if
                    else
                    {
                        Game.Console.huh();
                        game.Turn.ididit = false;
                        return;
                    }//else
                }//if
                else if (key is double)
                {
                    if (nenhere == 0)
                    {
                        Game.Console.WriteLine("Energy will be expended into space.");
                        automode = ShieldMode.AUTOMATIC;
                    }//if
                    else if (!ipoop)
                        automode = ShieldMode.FORCEMAN;
                    else
                        automode = ShieldMode.AUTOMATIC;
                }//else if
                else
                {
                    //IHEOL
                    if (nenhere == 0)
                    {
                        Game.Console.WriteLine("Energy will be expended into space.");
                        automode = ShieldMode.AUTOMATIC;
                    }//if
                    else if (!ipoop)
                        automode = ShieldMode.FORCEMAN;
                    else
                        Game.Console.Write("Manual or automatic? ");
                }//else
            }//while

            double rpow = 0;
            switch (automode)
            {
                case ShieldMode.AUTOMATIC:
                    if (key is string && SSTConsole.isit(key, "no"))
                    {
                        no = true;
                        key = Game.Console.scan();
                    }//if

                    if (!(key is double) && nenhere != 0)
                        Game.Console.WriteLine("Phasers locked on target. Energy available ={0,1:F2}", ifast ? ship.ShipEnergy - 200.0 : ship.ShipEnergy);

                    do
                    {
                        while (!(key is double))
                        {
                            Game.Console.chew();
                            Game.Console.Write("Units to fire=");
                            key = Game.Console.scan();
                        }//while
                        rpow = (double)key;
                        if (rpow >= (ifast ? ship.ShipEnergy - 200 : ship.ShipEnergy))
                        {
                            Game.Console.WriteLine("Energy available= {0,1:F2}", ifast ? ship.ShipEnergy - 200.0 : ship.ShipEnergy);
                            key = null;
                        }//if
                    } while (rpow >= (ifast ? ship.ShipEnergy - 200 : ship.ShipEnergy));

                    if (rpow <= 0)
                    {
                        //chicken out
                        game.Turn.ididit = false;
                        Game.Console.chew();
                        return;
                    }//if
                    if ((key = Game.Console.scan()) is string && SSTConsole.isit(key, "no"))
                    {
                        no = true;
                    }//if
                    if (ifast)
                    {
                        ship.ShipEnergy -= 200;//Go and do it!
                        if (checkshctrl(game, rpow)) return;
                    }//if
                    Game.Console.chew();
                    ship.ShipEnergy -= rpow;
                    double extra = rpow;

                    if (nenhere > 0)
                    {
                        extra = 0.0;
                        double powrem = rpow;

                        double[] hits = new double[nenhere];
                        int i = 0;
                        foreach (EnemyShip es in galaxy.CurrentQuadrant.Enemies)
                        {
                            hits[i] = 0.0;
                            if (powrem <= 0)
                            {
                                i++;
                                continue;
                            }
                            hits[i] = Math.Abs(es.Power) / (mPhaserFactor * Math.Pow(0.90, es.Distance));
                            double over = (0.01 + 0.05 * game.Random.Rand()) * hits[i];
                            double temp = powrem;
                            powrem -= hits[i] + over;
                            if (powrem <= 0 && temp < hits[i])
                                hits[i] = temp;
                            if (powrem <= 0)
                                over = 0.0;
                            extra += over;
                            i++;
                        }//foreach

                        if (powrem > 0.0)
                            extra += powrem;

                        hittem(game, galaxy.CurrentQuadrant.Enemies, hits);

                    }//if

                    if (extra > 0 && !game.Turn.alldone)
                    {
                        if (galaxy.CurrentQuadrant.Tholian != null)
                            Game.Console.WriteLine("*** Tholian web absorbs {0}phaser energy.", nenhere > 0 ? "excess " : "");
                        else
                            Game.Console.WriteLine("{0,1:F2} expended on empty space.", extra);

                    }//if
                    break;

                case ShieldMode.MANUAL:
                case ShieldMode.FORCEMAN:
                    {
                        if (automode == ShieldMode.FORCEMAN)
                        {
                            Game.Console.chew();
                            key = null;
                            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Computer))
                            {
                                Game.Console.WriteLine("Battle comuter damaged, manual file only.");
                            }//if
                            else
                            {
                                Game.Console.WriteLine("\n---WORKING---\n");
                                Game.Console.WriteLine("Short-range-sensors-damaged");
                                Game.Console.WriteLine("Insufficient-data-for-automatic-phaser-fire");
                                Game.Console.WriteLine("Manual-fire-must-be-used\n");
                            }//else
                        }
                        //FORCEMAN falls thru to MANUAL

                        rpow = 0.0;
                        bool msgflag = true;

                        EnemyShipList enemies = galaxy.CurrentQuadrant.Enemies;
                        double[] hits = new double[enemies.Count];
                        //int k = 0;
                        int kz = -1;

                        //todo - have to use this goofy for statement as this loop must be able
                        //to restart at beginning
                        for (int k = 0; k < enemies.Count; )
                        {
                            EnemyShip es = enemies[k];

                            if (msgflag)
                            {
                                Game.Console.Write("Energy available= {0,0:F2}\n", ship.ShipEnergy - .006 - (ifast ? 200 : 0));
                                msgflag = false;
                                rpow = 0.0;
                            }//if
                            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SRSensors) &&
                                !(Math.Abs(ship.Sector.X - es.Sector.X) < 2 && Math.Abs(ship.Sector.Y - es.Sector.Y) < 2) &&
                                (es is Commander || es is SuperCommander))
                            {
                                Game.Console.WriteLine("{0} can't be located without short range scan.", es.Name);
                                Game.Console.chew();
                                key = null;
                                hits[k] = 0;//prevent overflow -- thanks to Alexei Voitenko
                                k++;
                                continue;
                            }//if

                            if (key == null)
                            {
                                Game.Console.chew();
                                if (ipoop && k > kz)
                                {
                                    int irec = (int)((Math.Abs(es.Power) / (mPhaserFactor * Math.Pow(0.9, es.Distance))) * (1.01 + 0.05 * game.Random.Rand()) + 1.0);
                                    kz = k;
                                    Game.Console.Write("({0,1})  ", irec);
                                }//if
                                Game.Console.Write("units to fire at ");
                                Game.Console.crmena(false, es, true, es.Sector);
                                Game.Console.Write("-  ");
                                key = Game.Console.scan();
                            }//if
                            if (key is string && SSTConsole.isit(key, "no"))
                            {
                                no = true;
                                key = Game.Console.scan();
                                continue;
                            }//if
                            else if (key is string)
                            {
                                Game.Console.huh();
                                game.Turn.ididit = false;
                                return;
                            }//if
                            else if (key == null)
                            {
                                //todo - check value of k here
                                if (k == 0)
                                {//Let me say I'm baffled by this
                                    msgflag = true;
                                }//if
                                continue;
                            }//if
                            else if ((double)key < 0)
                            {
                                //abort out
                                game.Turn.ididit = false;
                                Game.Console.chew();
                                return;
                            }//if

                            hits[k] = (double)key;
                            rpow += (double)key;
                            //If total requested is too much, inform and start over

                            if (rpow >= (ifast ? ship.ShipEnergy - 200 : ship.ShipEnergy))
                            {
                                Game.Console.WriteLine("Available energy exceeded -- try again.");
                                Game.Console.chew();
                                key = null;
                                k = 0;
                                msgflag = true;
                                continue;
                            }//if
                            key = Game.Console.scan(); /* scan for next value */
                            k++;
                        }//for k
                        if (rpow == 0.0)
                        {
                            //zero energy -- abort
                            game.Turn.ididit = false;
                            Game.Console.chew();
                            return;
                        }//if
                        if (key is string & SSTConsole.isit(key, "no"))
                        {
                            no = true;
                        }//if
                        ship.ShipEnergy -= rpow;
                        Game.Console.chew();
                        if (ifast)
                        {
                            ship.ShipEnergy -= 200.0;
                            if (checkshctrl(game, rpow)) return;
                        }//if
                        hittem(game, enemies, hits);
                        game.Turn.ididit = true;
                        break;
                    }
            }//switch

            //Say shield raised or malfunction, if necessary
            if (game.Turn.alldone) return;
            if (ifast)
            {
                Game.Console.Skip(1);
                if (!no)
                {
                    if (game.Random.Rand() >= 0.99)
                    {
                        Game.Console.WriteLine("Sulu-  \"Sir, the high-speed shield control has malfunctioned . . .");
                        Game.Console.WriteLine("         CLICK   CLICK   POP  . . .");
                        Game.Console.WriteLine(" No  response, sir!");
                        ship.ShieldsUp = false;
                    }//if
                    else
                        Game.Console.WriteLine("Shields raised.");
                }//if
                else
                    ship.ShieldsUp = false;
            }//if
            Overheat(game, rpow);
        }//Phasers

        /// <summary>
        /// Attempt to use the Enterprise deathray.
        /// Experimental and has a high probability of not working as intended.
        /// If it does work, kills all enemies in current quadrant.
        /// </summary>
        /// <param name="game"></param>
        internal static void DeathRay(GameData game)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;

            double r = game.Random.Rand();

            game.Turn.ididit = false;
            Game.Console.Skip(1);
            Game.Console.chew();

            //FQ has no deathray
            if (!ship.HasDeathray)
            {
                Game.Console.WriteLine("Ye {0} has no death ray.", ship.Name);
                return;
            }

            EnemyShipList enemies = galaxy.CurrentQuadrant.Enemies;

            //No sense using deathray with no enemies present
            if (enemies.Count == 0)
            {
                Game.Console.WriteLine("Sulu-  \"But Sir, there are no enemies in this quadrant.\"");
                return;
            }

            //Can't use it if its broke
            if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Deathray))
            {
                Game.Console.WriteLine("Death Ray is damaged.");
                return;
            }

            //Warn kirk that this thing may not work
            Game.Console.WriteLine("Spock-  \"Captain, the 'Experimental Death Ray'");
            Game.Console.WriteLine("  is highly unpredictible.  Considering the alternatives,");
            Game.Console.WriteLine("  are you sure this is wise?\" ");
            if (!Game.Console.ja())
                return;

            Game.Console.WriteLine("Spock-  \"Acknowledged.\"\n");
            game.Turn.ididit = true;

            //activate deathray ...
            Game.Console.WriteLine("WHOOEE ... WHOOEE ... WHOOEE ... WHOOEE\n");
            Game.Console.WriteLine("Crew scrambles in emergency preparation.");
            Game.Console.WriteLine("Spock and Scotty ready the death ray and");
            Game.Console.WriteLine("prepare to channel all ship's power to the device.\n");
            Game.Console.WriteLine("Spock-  \"Preparations complete, sir.\"");
            Game.Console.WriteLine("Kirk-  \"Engage!\"\n");
            Game.Console.WriteLine("WHIRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR\n");

            //Check for the work case
            if (r > .30)
            {
                //message and kill all enemies in current quadrant
                Game.Console.WriteLine("Sulu- \"Captain!  It's working!\"\n\n");
                foreach (EnemyShip es in enemies)
                    es.killShip(game);

                //phew
                Game.Console.WriteLine("Ensign Chekov-  \"Congratulations, Captain!\"");
                if (galaxy.Klingons == 0)
                    Finish.finish(Finish.FINTYPE.FWON, game);

                //small chance that deathray will survive use
                Game.Console.WriteLine("Spock-  \"Captain, I believe the `Experimental Death Ray'");
                if (game.Random.Rand() <= 0.05)
                {
                    Game.Console.WriteLine("   is still operational.\"");
                }
                else
                {
                    Game.Console.WriteLine("   has been rendered disfunctional.\"");
                    ship.ShipDevices.DamageDeathray();
                }
                return;
            }//if

            //Deathray fail!!!
            //Compute failure type
            r = game.Random.Rand();	// Pick failure method

            if (r <= .30)
            {//we blow up
                Game.Console.WriteLine("Sulu- \"Captain!  It's working!\"\n");
                Game.Console.WriteLine("***RED ALERT!  RED ALERT!\n");
                Game.Console.WriteLine("***MATTER-ANTIMATTER IMPLOSION IMMINENT!\n");
                Game.Console.WriteLine("***RED ALERT!  RED A*L********************************\n");
                Game.Console.stars();
                Game.Console.WriteLine("******************   KA-BOOM!!!!   *******************\n");
                Finish.Kaboom(game);
                return;
            }//if

            if (r <= .55)
            {//mutants!!!
                Game.Console.WriteLine("Sulu- \"Captain!  Yagabandaghangrapl, brachriigringlanbla!\"\n");
                Game.Console.WriteLine("Lt. Uhura-  \"Graaeek!  Graaeek!\"\n");
                Game.Console.WriteLine("Spock-  \"Facinating!  . . . All humans aboard");
                Game.Console.WriteLine("  have apparently been transformed into strange mutations.");
                Game.Console.WriteLine("  Vulcans do not seem to be affected.\n");
                Game.Console.WriteLine("Kirk-  \"Raauch!  Raauch!\"");
                Finish.finish(Finish.FINTYPE.FDRAY, game);
                return;
            }//if

            if (r <= 0.75)
            {//Things
                Game.Console.WriteLine("Sulu- \"Captain!  It's   --WHAT?!?!\"\n\n");
                Game.Console.WriteLine("Spock-  \"I believe the word is *ASTONISHING*");
                Game.Console.WriteLine(" Mr. Sulu.");

                //Fill att empty sectors with Things
                galaxy.CurrentQuadrant.FillWithThings();

                Game.Console.WriteLine("  Captain, our quadrant is now infested with");
                Game.Console.WriteLine(" - - - - - -  *THINGS*.\n");
                Game.Console.WriteLine("  I have no logical explanation.\"");
                return;
            }//if

            //Otherwise we are overrun by tribbles ...
            Game.Console.WriteLine("Sulu- \"Captain!  The Death Ray is creating tribbles!\"\n");
            Game.Console.WriteLine("Scotty-  \"There are so many tribbles down here");
            Game.Console.WriteLine("  in Engineering, we can't move for 'em, Captain.\"");
            Finish.finish(Finish.FINTYPE.FTRIBBLE, game);

        }//DeathRay

    }//class Battle
}