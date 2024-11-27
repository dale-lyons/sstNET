using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects
{
    /// <summary>
    /// This class represents a black hole in the current quadrant.
    /// It is a very special sector object as anything that enters its sector
    /// will be destroyed(including torpedos)
    /// </summary>
    public class BlackHole : SectorObject
    {
        public BlackHole() 
            : base() { }

        public override bool Ramable { get { return false; } }
        public override char Symbol { get { return ' '; } }
        public override string Name { get { return "Black hole"; } }

        /// <summary>
        /// A torpedo has hit the black-hole. Not much damage here
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        /// <param name="bullseye"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public override double TorpedoHit(GameData game, SectorCoordinate sc, double bullseye, double angle)
        {
            //output a message indicating the torpeo got swallowed
            Game.Console.Skip(1);
            Game.Console.crmena(true, this, true, this.Sector);
            Game.Console.WriteLine(" swallows torpedo.");
            return 0;
        }//TorpedoHit

        /// <summary>
        /// The ship has rammed a black-hole. Not good.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="previous"></param>
        /// <param name="final"></param>
        /// <param name="distSoFar"></param>
        /// <returns></returns>
        public override bool Ram(GameData game, SectorCoordinate previous, out SectorCoordinate final, out double distSoFar)
        {
            distSoFar = 0.0;
            final = this.Sector;

            //tell user the bad news
            Game.Console.WriteLine("\n***RED ALERT!  RED ALERT!\n");
            Game.Console.WriteLine("***{0} pulled into black hole at{1}", game.Galaxy.Ship.Name, this.Sector.ToString(true));

            //game over
            Finish.finish(Finish.FINTYPE.FHOLE, game);
            return true;
        }//Ram
    }//class BlackHole
}