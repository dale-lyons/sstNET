using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects.Ships
{
    public class Commander : EnemyShip
    {
        public Commander()
            : base(true) { }

        /// <summary>
        /// This class represents a klingon Commander ship. Commanders are more powerful
        /// versions of Klingons with some special capabilities. They can move during combat
        /// (in higher skill levels), can tractor beam the friendly ship, can possibly ram
        /// the friendly ship.
        /// There can only be a max of 1 Commander in a single quadrant.
        /// Note that for Nova damage purposes this ship is considered a "Powerful" ship.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="skill"></param>
        public Commander(Random rand, GameData.GameSkillEnum skill)
            : base(true)
        {
            //compute a power level based on skill level(and some randomness)
            Power = (950.0 + 400.0 * rand.Rand() + 50.0 * (int)skill);
        }

        public override double RamDamageFactor { get { return 2.0; } }
        public override char Symbol { get { return 'C'; } }
        public override string Name { get { return "Commander"; } }

        /// <summary>
        /// Handle being hit by a torpedo
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        /// <param name="bullseye"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public override double TorpedoHit(GameData game, SectorCoordinate sc, double bullseye, double angle)
        {
            //todo - this code is exactly the same in Super-Commander class
            if (game.Random.Rand() <= 0.05)
            {
                Game.Console.WriteLine("***{0} at{1} uses anti-photon device;", this.Name, this.Sector.ToString(true));
                Game.Console.WriteLine("   torpedo neutralized.");
                return 0;
            }
            return base.TorpedoHit(game, sc, bullseye, angle);
        }//TorpedoHit


        /// <summary>
        /// Override this function to handle a Commander kill.
        /// 1) Remove Commander from same quadrant as ship
        /// 2) If no more Commanders left, remove any tractor beam future events.
        ///    otherwise schedule one.
        /// 3) Increment number of killed Commander(for final score)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        public override void deadkl(GameData game, SectorCoordinate sc)
        {
            Galaxy galaxy = game.Galaxy;
            FederationShip ship = galaxy.Ship;

            //remove Commander from game
            galaxy[ship.GalacticCoordinate].Commander = null;

            //If we kill the commander that is currently attacking a base, then stop the attack.
            if (game.Future[FutureEvents.EventTypesEnum.FCDBAS] < FutureEvents.NEVER)
            {
                if(ship.QuadrantCoordinate.Equals(galaxy.CommanderAttack))
                {
                    game.Future[FutureEvents.EventTypesEnum.FCDBAS] = FutureEvents.NEVER;
                    galaxy.CommanderAttack = null;
                }
            }

            //schedule a tractor beam in the future only if Commanders still exist.
            if (galaxy.Commanders.Count == 0)
                game.Future[FutureEvents.EventTypesEnum.FTBEAM] = FutureEvents.NEVER;
            else
                game.Future[FutureEvents.EventTypesEnum.FTBEAM] = game.Date + game.Random.expran(1.0 * galaxy._incom / galaxy.Commanders.Count);

            game.CommandersKilled++;
        }//deadkl

    }//class Commander
}