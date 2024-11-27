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
    /// Handles most of the game AI
    /// </summary>
    internal static class AI
    {
        /// <summary>
        /// Move the SuperCommander
        /// the SC will either behave "Actively" or "Passively" based on how the federation is doing.
        /// If active, the SC will move towards a starbase (if none left alive will not move)
        /// Otherwise if passive, will move away from the Federation ship.
        /// </summary>
        /// <param name="game"></param>
        internal static void MoveSuperCommander(GameData game)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;
            QuadrantCoordinate sc = galaxy.SuperCommander;

            //Decide on being active or passive
            int skill = (int)game.GameSkill;
            double elaspedTime = game.Date - galaxy._indate;
            double killRate = (game.CommandersKilled + game.KlingonsKilled) / (elaspedTime + 0.01);

            //determine if the SC will be passive or active
            //looks like SC will be passive for first 3 stardates no matter what, otherwise based on kill rate
            bool passiveFlag = (killRate < (0.1 * skill * (skill + 1.0))) || (elaspedTime < 3.0);

            int ideltax = 0;
            int ideltay = 0;

            if (galaxy.CurrentQuadrant.SuperCommander == null && passiveFlag)
            {//passive
                ideltax = sc.X - ship.QuadrantCoordinate.X;
                ideltay = sc.Y - ship.QuadrantCoordinate.Y;

                //compute move away from Enterprise
                if (sc.DistanceTo(ship.QuadrantCoordinate) > 2.0)
                {
                    //circulate in space
                    ideltax = sc.Y - ship.QuadrantCoordinate.Y;
                    ideltay = ship.QuadrantCoordinate.X - sc.X;
                }//if
            }//if
            else
            {//active
                //compute distances to starbases
                if (galaxy.Bases.Count <= 0)
                {
                    //nothing left to do
                    game.Future[FutureEvents.EventTypesEnum.FSCMOVE] = FutureEvents.NEVER;
                    return;
                }//if

                //look for nearest base without a commander, no Enterprise, and
                //without too many Klingons, and not already under attack.
                QuadrantStarBase starbase = null;
                foreach (QuadrantStarBase sb in new StarBaseList(galaxy, sc))
                {
                    if ((sb.QuadrantCoordinate.Equals(ship.QuadrantCoordinate)) ||
                        (sb.QuadrantCoordinate.Equals(galaxy.CommanderAttack)) ||
                        (sb.QuadrantCoordinate.Equals(galaxy.SuperCommanderAttack)) ||
                         galaxy[sb.QuadrantCoordinate].TotalKlingons > 8)
                           continue;

                    //if there is a commander, and no other base is appropriate,
                    //we will take the one with the commander
                    if (galaxy[sb.QuadrantCoordinate].Commander != null)
                    {
                        starbase = sb;
                        break;
                    }//if
                    else if (starbase == null)
                    {
                        starbase = sb;
                        break;
                    }//else if
                }//foreach

                //no starbase found, all done
                if (starbase == null)
                    return;

                //decide how to move toward base
                ideltax = starbase.QuadrantCoordinate.X - sc.X;
                ideltay = starbase.QuadrantCoordinate.Y - sc.Y;

            }//else

            //Maximum movement is 1 quadrant in either or both axis
            ideltax = Math.Max(Math.Min(ideltax, 1), -1);
            ideltay = Math.Max(Math.Min(ideltay, 1), -1);

            //try moving in both x and y directions
            //int iqx = sc.X + ideltax;
            //int iqy = sc.Y + ideltax; // Bug!!! - this looks like a bug but it is this way in the original.
            QuadrantCoordinate qc = new QuadrantCoordinate(sc.X + ideltax, sc.Y + ideltay);

            if (checkdest(qc, game, passiveFlag))
            {
                //failed -- try some other maneuvers
                if (ideltax == 0 || ideltay == 0)
                {
                    //attempt angle move
                    if (ideltax != 0)
                    {
                        qc.Y = sc.Y + 1;
                        //iqy = sc.Y + 1;
                        if (checkdest(qc, game, passiveFlag))
                        {
                            qc.Y = sc.Y - 1;
                            //iqy = sc.Y - 1;
                            checkdest(qc, game, passiveFlag);
                        }//if
                    }//if
                    else
                    {
                        qc.X = sc.X + 1;
                        //iqx = sc.X + 1;
                        if (checkdest(qc, game, passiveFlag))
                        {
                            qc.X = sc.X - 1;
                            //iqx = sc.X - 1;
                            checkdest(qc, game, passiveFlag);
                        }//if
                    }//else
                }//if
                else
                {
                    //try moving just in x or y
                    qc.Y = sc.Y;
                    //iqy = sc.Y;
                    if (checkdest(qc, game, passiveFlag))
                    {
                        qc.Y = (int)(sc.Y + ideltay);
                        qc.X = sc.X;
                        //iqy = (int)(sc.Y + ideltay);
                        //iqx = sc.X;
                        checkdest(qc, game, passiveFlag);
                    }//if
                }//else
            }//if

            // Check if any bases left. If none, then stop moving SC.
            if (galaxy.Bases.Count <= 0)
                game.Future[FutureEvents.EventTypesEnum.FSCMOVE] = FutureEvents.NEVER;

            //Check if the SC is currently in the same quad as a starbase. If so, and the SC is not currently attacking it, go ahead and attack it.
            //note:ok if galaxy.SuperCommanderAttack is null, equals method checks for null
            if (galaxy[galaxy.SuperCommander].Base != null && !galaxy.SuperCommander.Equals(galaxy.SuperCommanderAttack))
            {
                //attack the base (unless we are in passive mode)
                if (passiveFlag)
                    return;

                game.Turn.iseenit = false;
                galaxy.SuperCommanderAttack = galaxy.SuperCommander;

                //set the destruction of the base date in the future ...
                game.Future[FutureEvents.EventTypesEnum.FSCDBAS] = game.Date + 1.0 + 2.0 * game.Random.Rand();

                //check if the ships radio is dead(and not docked), if so no warning is isssued.
                if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) && !ship.Docked)
                    return;//no warning

                //ok, warn the ship about the starbase being attacked
                game.Turn.iseenit = true;
                Game.Console.WriteLine("Lt. Uhura-  \"Captain, the starbase in{0}", galaxy.SuperCommander.ToString(true));
                Game.Console.WriteLine("   reports that it is under attack from the Klingon Super-commander.");
                Game.Console.WriteLine("   It can survive until stardate {0,0:F1} .\"", game.Future[FutureEvents.EventTypesEnum.FSCDBAS]);

                //as long as the ship is not resting we are done.
                if (!game.Turn.resting)
                    return;

                //let player decide if rest should be cancelled.
                Game.Console.WriteLine("Mr. Spock-  \"Captain, shall we cancel the rest period?\"");
                if (!Game.Console.ja())
                    return;

                game.Turn.resting = false;
                game.Turn.Time = 0.0;//actually finished
                return;
            }//if

            //Check for intelligence report
            double r = game.Random.Rand();
            if (
                (r > 0.2 || 
                (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) && !ship.Docked) || 
                galaxy[galaxy.SuperCommander].Starch > 0)
                )

                return;

            Game.Console.WriteLine("Lt. Uhura-  \"Captain, Starfleet Intelligence reports");
            Game.Console.WriteLine("   the Super-commander is in{0}.\"", galaxy.SuperCommander.ToString(true));

        }//MoveSuperCommander

        /// <summary>
        /// Move the Tholian if one is present. If we have just entered quadrant, skip the move.
        /// </summary>
        /// <param name="game">Reference to Game object</param>
        internal static void MoveTholian(GameData game)
        {
            //extract Galaxy object from Game
            Galaxy.Galaxy galaxy = game.Galaxy;

            //Find the Tholian
            Tholian tho = galaxy.CurrentQuadrant.Tholian;

            //If Tholian is not present, or we just entered quadrant, skip moving
            if (tho == null || game.Turn.justin)
                return;

            SectorCoordinate tholianSC = tho.Sector;

            //Move the tholian from one quadrant corner to another. Clockwise rotation.
            SectorCoordinate newSC = tholianSC.ClockwiseMove();
            if (newSC == null)
            {
                galaxy.CurrentQuadrant[tholianSC] = new Empty();
                return;
            }

            //Do nothing if we are blocked
            if (!(galaxy.CurrentQuadrant[newSC] is Empty) && !(galaxy.CurrentQuadrant[newSC] is TholianWeb))
                return;

            //place a tholian web at Tholian current position
            galaxy.CurrentQuadrant[tholianSC] = new TholianWeb();

            if (tholianSC.X != newSC.X)
            {//move in x axis
                int im = Math.Abs(newSC.X - tholianSC.X) / (newSC.X - tholianSC.X);
                while (tholianSC.X != newSC.X)
                {
                    tholianSC.X += im;
                    if (galaxy.CurrentQuadrant[tholianSC] is Empty)
                        galaxy.CurrentQuadrant[tholianSC] = new TholianWeb();
                }//while
            }//if
            else if (tholianSC.Y != newSC.Y)
            {//move in y axis
                int im = Math.Abs(newSC.Y - tholianSC.Y) / (newSC.Y - tholianSC.Y);
                while (tholianSC.Y != newSC.Y)
                {
                    tholianSC.Y += im;
                    if (galaxy.CurrentQuadrant[tholianSC] is Empty)
                        galaxy.CurrentQuadrant[tholianSC] = new TholianWeb();
                }//while
            }//else if
            galaxy.CurrentQuadrant[newSC] = new Tholian();

            //check to see if all holes plugged
            foreach (SectorCoordinate sc in SectorCoordinate.EdgeSectors)
            {
                if (!(galaxy.CurrentQuadrant[sc] is TholianWeb) && !(galaxy.CurrentQuadrant[sc] is Tholian))
                    return;
            }//foreach

            //All plugged up -- Tholian splits
            galaxy.CurrentQuadrant[newSC] = new TholianWeb();
            galaxy.CurrentQuadrant[galaxy.CurrentQuadrant.dropin(game.Random)] = new BlackHole();
            Game.Console.crmena(true, tho, true, newSC);
            Game.Console.WriteLine(" completes web.");

        }//MoveTholian

        /// <summary>
        /// Check that the destination quadrant is valid for a SC move.
        /// </summary>
        /// <param name="sc"></param>
        /// <param name="game"></param>
        /// <param name="avoidBase"></param>
        /// <returns>false == valid</returns>
        private static bool checkdest(QuadrantCoordinate sc, GameData game, bool avoidBase)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;

            if ((sc.Equals(ship.QuadrantCoordinate)) || !sc.Valid || galaxy[sc].TotalKlingons >= 9 || galaxy[sc].SuperNova)
                return true;

            //Avoid quadrants with bases if we want to avoid Enterprise
            if (avoidBase && galaxy[sc].Base != null)
                return true;

            //do the move
            galaxy[galaxy.SuperCommander].SuperCommander = null;
            galaxy[sc].SuperCommander = new QuadrantSuperCommander(sc);

            if (galaxy.CurrentQuadrant.SuperCommander != null)
            {
                //SC has scooted, Remove him from current quadrant
                galaxy.CurrentQuadrant[galaxy.CurrentQuadrant.SuperCommander.Sector] = new Empty();
                galaxy.SuperCommanderAttack = null;
                game.Turn.ientesc = false;
                game.Future[FutureEvents.EventTypesEnum.FSCDBAS] = FutureEvents.NEVER;
            }//if

            //check for a helpful planet
            QuadrantPlanet planet = galaxy[sc].Planet;
            if (planet != null && planet.Crystals)
            {
                //destroy the planet
                galaxy[sc].Planet = null;
                if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio) || ship.Docked)
                {
                    Game.Console.WriteLine("Lt. Uhura-  \"Captain, Starfleet Intelligence reports");
                    Game.Console.WriteLine("   a planet in{0} has been destroyed", sc.ToString(true));
                    Game.Console.WriteLine("   by the Super-commander.\"");
                }//if
            }//if

            //false means move was good!
            return false;
        }//checkdest

        private static bool TryExit(GameData game, SectorCoordinate look, EnemyShip es, bool irun)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;

            int iqx = ship.QuadrantCoordinate.X + (look.X + 9) / 10 - 1;
            int iqy = ship.QuadrantCoordinate.Y + (look.Y + 9) / 10 - 1;

            if (iqx < 1 || iqx > 8 || iqy < 1 || iqy > 8 || galaxy[iqx, iqy].TotalKlingons >= 9 || galaxy[iqx, iqy].SuperNova)
            {
                return false;//no can do -- neg energy, supernovae, or >8 Klingons
            }

            if (es is Romulan)
                return false;//Romulans cannot escape!

            if (!irun)
            {
                //avoid intruding on another commander's territory
                if (es is Commander)
                {
                    if (galaxy[iqx, iqy].Commander != null)
                        //if (galaxy.CurrentQuadrant.Commander != null)
                        return false;

                    //refuse to leave if currently attacking starbase
                    //todo - check this logic
                    if (galaxy.CommanderAttack == ship.QuadrantCoordinate || galaxy.SuperCommanderAttack == ship.QuadrantCoordinate)
                        return false;
                }//if

                //don't leave if over 1000 units of energy
                if (es.Power > 1000.0)
                    return false;
            }//if

            //print escape message and move out of quadrant.
            //We know this if either short or long range sensors are working
            if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SRSensors) || !ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.LRSensors) || ship.Docked)
                Game.Console.WriteLine("***{0}({1}) escapes to{2} (and regains strength).", es.Name, es.Sector.ToString(false), new QuadrantCoordinate(iqx, iqy).ToString(true));

            //Handle global matters related to escape
            if (es is SuperCommander)
            {
                galaxy[ship.QuadrantCoordinate].SuperCommander = null;
                galaxy[iqx, iqy].SuperCommander = new QuadrantSuperCommander(new QuadrantCoordinate(iqx, iqy));
                game.Turn.ientesc = false;
                galaxy.SuperCommanderAttack = null;
                game.Future[FutureEvents.EventTypesEnum.FSCMOVE] = 0.2777 + game.Date;
                game.Future[FutureEvents.EventTypesEnum.FSCDBAS] = FutureEvents.NEVER;
                galaxy.CurrentQuadrant[es.Sector] = new Empty();
            }//if
            else if (es is Commander)
            {
                galaxy[iqx, iqy].Commander = galaxy[ship.QuadrantCoordinate].Commander;
                galaxy[iqx, iqy].Commander.QuadrantCoordinate = new QuadrantCoordinate(iqx, iqy);
                galaxy[ship.QuadrantCoordinate].Commander = null;
                galaxy.CommanderAttack = null;
                galaxy.CurrentQuadrant[es.Sector] = new Empty();
            }//else
            else
            {
                --galaxy[ship.QuadrantCoordinate].OrdinaryKlingons;
                ++galaxy[iqx, iqy].OrdinaryKlingons;
                galaxy.CurrentQuadrant[es.Sector] = new Empty();
            }
            return true;//success
        }//TryExit

        private static void MoveEnemyShip(GameData game, EnemyShip es)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;

            int motion = 0;
            bool irun = false;

            //This should probably be just comhere + ishere
            int comhere = (galaxy.CurrentQuadrant.Commander == null) ? 0 : 1;
            int ishere = (galaxy.CurrentQuadrant.SuperCommander == null) ? 0 : 1;
            int irhere = galaxy.CurrentQuadrant.Romulans.Count;
            int klhere = galaxy.CurrentQuadrant.Enemies.Count - irhere;

            int nbaddys = (game.GameSkill > GameData.GameSkillEnum.Good) ? (int)((comhere * 2 + ishere * 2 + klhere * 1.23 + irhere * 1.5) / 2.0) : (comhere + ishere);
            double dist1 = es.Distance;
            int mdist = (int)(dist1 + 0.5);//Nearest integer distance

            double forces = 0;

            //If SC, check with spy to see if should hi-tail it
            if (es is SuperCommander && (es.Power <= 500.0 || (ship.Docked && !ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.PhotonTubes))))
            {
                irun = true;
                motion = -10;
            }//if
            else
            {
                //decide whether to advance, retreat, or hold position
                // Algorithm:
                //   * Enterprise has "force" based on condition of phaser and photon torpedoes.
                //     If both are operating full strength, force is 1000. If both are damaged,
                //     force is -1000. Having shields down subtracts an additional 1000.

                //   * Enemy has forces equal to the energy of the attacker plus
                //     100*(K+R) + 500*(C+S) - 400 for novice through good levels OR
                //     346*K + 400*R + 500*(C+S) - 400 for expert and emeritus.

                //     Attacker Initial energy levels (nominal):
                //              Klingon   Romulan   Commander   Super-Commander
                //     Novice    400        700        1200        
                //     Fair      425        750        1250
                //     Good      450        800        1300        1750
                //     Expert    475        850        1350        1875
                //     Emeritus  500        900        1400        2000
                //     VARIANCE   75        200         200         200

                //     Enemy vessels only move prior to their attack. In Novice - Good games
                //     only commanders move. In Expert games, all enemy vessels move if there
                //     is a commander present. In Emeritus games all enemy vessels move.

                //  *  If Enterprise is not docked, an agressive action is taken if enemy
                //     forces are 1000 greater than Enterprise.

                //     Agressive action on average cuts the distance between the ship and
                //     the enemy to 1/4 the original.

                //  *  At lower energy advantage, movement units are proportional to the
                //     advantage with a 650 advantage being to hold ground, 800 to move forward
                //     1, 950 for two, 150 for back 4, etc. Variance of 100.

                //     If docked, is reduced by roughly 1.75*skill, generally forcing a
                //     retreat, especially at high skill levels.

                //  *  Motion is limited to skill level, except for SC hi-tailing it out.
                //
                forces = es.Power + 100.0 * galaxy.CurrentQuadrant.Enemies.Count + 400 * (nbaddys - 1);
                if (!ship.ShieldsUp)
                    forces += 1000;//Good for enemy if shield is down!

                if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Phasers) ||
                    !ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.PhotonTubes))
                {
                    if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.Phasers))//phasers damaged
                        forces += 300.0;
                    else
                        forces -= 0.2 * (ship.ShipEnergy - 2500.0);

                    if (ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.PhotonTubes))//photon torpedoes damaged
                        forces += 300.0;
                    else
                        forces -= 50.0 * ship.Torpedoes;
                }//if
                else
                {
                    //phasers and photon tubes both out!
                    forces += 1000.0;
                }//else
                motion = 0;
                if (forces <= 1000.0 && !ship.Docked)//Typical situation
                {
                    motion = (int)(((forces + 200.0 * game.Random.Rand()) / 150.0) - 5.0);
                }
                else
                {
                    if (forces > 1000.0)
                        //Very strong -- move in for kill
                        motion = (int)((1.0 - Math.Pow(game.Random.Rand(), 2.0)) * dist1 + 1.0);

                    if (ship.Docked)
                        //protected by base -- back off !
                        motion = (int)((double)motion - (double)((int)game.GameSkill * (2.0 - Math.Pow(game.Random.Rand(), 2.0))));

                }//else

                //don't move if no motion
                if (motion == 0)
                    return;

                //Limit motion according to skill
                if (Math.Abs(motion) > (int)game.GameSkill)
                    motion = (motion < 0) ? -(int)game.GameSkill : (int)game.GameSkill;

            }//else

            //calcuate preferred number of steps
            int nsteps = motion < 0 ? -motion : motion;
            if (motion > 0 && nsteps > mdist) nsteps = mdist;//don't overshoot

            nsteps = Math.Min(nsteps, 10);//This shouldn't be necessary
            nsteps = Math.Max(nsteps, 1);//This shouldn't be necessary

            //Compute preferred values of delta X and Y
            int mx = ship.Sector.X - es.Sector.X;
            int my = ship.Sector.Y - es.Sector.Y;

            if (2.0 * Math.Abs(mx) < Math.Abs(my))
                mx = 0;
            if (2.0 * Math.Abs(my) < Math.Abs(ship.Sector.X - es.Sector.X))
                my = 0;
            if (mx != 0)
                mx = mx * motion < 0 ? -1 : 1;
            if (my != 0)
                my = my * motion < 0 ? -1 : 1;

            int nextx = es.Sector.X;
            int nexty = es.Sector.Y;

            //remove enemy ship from current quadrant
            //main move loop
            for (int ll = 1; ll <= nsteps; ll++)
            {
                //Check if preferred position available
                int lookx = nextx + mx;
                int looky = nexty + my;
                int krawlx = mx < 0 ? 1 : -1;
                int krawly = my < 0 ? 1 : -1;
                bool success = false;
                int attempts = 0;//Settle mysterious hang problem

                while (attempts++ < 20 && !success)
                {
                    if (lookx < 1 || lookx > 10)
                    {
                        if (motion < 0 && TryExit(game, new SectorCoordinate(lookx, looky), es, irun))
                            return;

                        if (krawlx == mx || my == 0)
                            break;

                        lookx = nextx + krawlx;
                        krawlx = -krawlx;
                    }//if
                    else if (looky < 1 || looky > 10)
                    {
                        if (motion < 0 && TryExit(game, new SectorCoordinate(lookx, looky), es, irun))
                            return;

                        if (krawly == my || mx == 0)
                            break;

                        looky = nexty + krawly;
                        krawly = -krawly;
                    }//else if
                    else if (!(galaxy.CurrentQuadrant[lookx, looky] is Empty) && (lookx != es.Sector.X || looky != es.Sector.Y))
                    {
                        //See if we should ram ship
                        if (galaxy.CurrentQuadrant[lookx, looky] is FederationShip && (es is Commander || es is SuperCommander))
                        {
                            Battle.Ram(game, true, es);
                            return;
                        }//if
                        if (krawlx != mx && my != 0)
                        {
                            lookx = nextx + krawlx;
                            krawlx = -krawlx;
                        }//if
                        else if (krawly != my && mx != 0)
                        {
                            looky = nexty + krawly;
                            krawly = -krawly;
                        }//else if
                        else
                            break;//we have failed
                    }//else if
                    else
                        success = true;
                }//while

                if (success)
                {
                    nextx = lookx;
                    nexty = looky;
                }//if
                else
                    break;//done early
            }//for ll

            //Put commander in place within same quadrant
            int orgx = es.Sector.X;
            int orgy = es.Sector.Y;

            //remove enemy ship from current quadrant
            galaxy.CurrentQuadrant[es.Sector] = new Empty();

            bool moved = (nextx != es.Sector.X || nexty != es.Sector.Y);
            galaxy.CurrentQuadrant[nextx, nexty] = es;
            if (moved)
            {
                //it moved, calcluate new distance to ship AND reset average distance
                es.CalculateDistance(ship.Sector.DistanceTo(es.Sector));
                es.ResetAverageDistance();
                if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SRSensors) || ship.Docked)
                    Game.Console.WriteLine("***{0}({1}) {2}{3}", es.Name, new SectorCoordinate(orgx, orgy).ToString(false), es.Distance < dist1 ? "advances to" : "retreats to", es.Sector.ToString(true));

            }//if
        }//MoveEnemyShip

        /// <summary>
        /// Move the enemy ships.
        /// First try moving the local Commander
        /// Then the Super-Commander
        /// Then at high skill levels move the rest of enemy ships
        /// </summary>
        /// <param name="game"></param>
        internal static void MoveEnemyShips(GameData game)
        {
            Galaxy.Galaxy galaxy = game.Galaxy;

            //Check if a Commander exists and if so, move it.
            if (galaxy.CurrentQuadrant.Commander != null)
                MoveEnemyShip(game, galaxy.CurrentQuadrant.Commander);

            //Check if a Super-Commander exists and if so, move it.
            if (galaxy.CurrentQuadrant.SuperCommander != null)
                MoveEnemyShip(game, galaxy.CurrentQuadrant.SuperCommander);

            //if skill level is high, move other Klingons and Romulans too!
            //Move these last so they can base their actions on what the commander(s) do.
            if (game.GameSkill > GameData.GameSkillEnum.Good)
            {
                foreach (EnemyShip es in galaxy.CurrentQuadrant.Enemies)
                {
                    if ((es is Klingon) || (es is Romulan))
                        MoveEnemyShip(game, es);
                }//foreach
            }//if
        }//MoveEnemyShips

    }//class AI
}
