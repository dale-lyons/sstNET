using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects.Ships
{
    /// <summary>
    /// This class represents a Romulan ship in the current quadrant. Note that for
    /// Nova damage purposes a Romulan is considered a "Powerful" ship. (base class ctor param)
    /// </summary>
    public class Romulan : EnemyShip
    {
        public Romulan()
            : base(true) { }

        public Romulan(Random rand, GameData.GameSkillEnum skill)
            : base(true)
        {
            //compute a power level based on skill level(and some randomness)
            Power = (rand.Rand() * 400.0 + 450.0 + 50.0 * (int)skill);
        }

        public override double RamDamageFactor { get { return 1.5; } }
        public override char Symbol { get { return 'R'; } }
        public override string Name { get { return "Romulan"; } }

        /// <summary>
        /// Override this function to handle a Romulan kill.
        /// 1) Decrement number of Romulans in same quadrant as ship
        /// 2) Increment number of killed Romulans(for final score)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        public override void deadkl(GameData game, SectorCoordinate sc)
        {
            game.Galaxy[game.Galaxy.Ship.GalacticCoordinate].Romulans--;
            game.RomulansKilled++;
        }//deadkl

    }//class Romulan
}