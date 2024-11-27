using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects.Ships
{
    /// <summary>
    /// This class represents the ordinary klingon enemy ship. This is the most common
    /// ship type in the game. Note that for Nova damage purposes this ship is considered
    /// a "non-Powerful" ship.
    /// </summary>
    public class Klingon : EnemyShip
    {
        public Klingon()
            : base(false) { }

        public Klingon(Random rand, GameData.GameSkillEnum skill)
            : base(false)
        {
            //compute a power level based on skill level(and some randomness)
            Power = (rand.Rand() * 150.0 + 300.0 + 25.0 * (int)skill);
        }

        public override double RamDamageFactor { get { return 1.0; } }
        public override char Symbol { get { return 'K'; } }
        public override string Name { get { return "Klingon"; } }

        /// <summary>
        /// Override this function to handle a Klingon kill.
        /// 1) Decrement number of Klingons in same quadrant as ship
        /// 2) Increment number of killed Klingons(for final score)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        public override void deadkl(GameData game, SectorCoordinate sc)
        {
            game.Galaxy[game.Galaxy.Ship.GalacticCoordinate].OrdinaryKlingons--;
            game.KlingonsKilled++;
        }//deadkl
    }//class Klingon
}