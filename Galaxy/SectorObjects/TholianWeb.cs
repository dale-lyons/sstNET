using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects
{
    /// <summary>
    /// Thie class represents a Tholian web sector object in the current quadrant.
    /// It is created when a tholian moves in the quadrant leaving behind web objects
    /// in its wake.
    /// </summary>
    public class TholianWeb : SectorObject
    {
        public TholianWeb() 
            : base() { }

        public override bool Ramable { get { return false; } }
        public override char Symbol { get { return '#'; } }
        public override string Name { get { return "Tholean web"; } }

        /// <summary>
        /// Tholian web is hit by a torpedo. Not much happens as web is immune
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        /// <param name="bullseye"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public override double TorpedoHit(GameData game, SectorCoordinate sc, double bullseye, double angle)
        {
            //let user know torpedo has no effect
            Game.Console.WriteLine("\n***Torpedo absorbed by Tholian web.");
            return 0;
        }//TorpedoHit

    }//class TholianWeb
}