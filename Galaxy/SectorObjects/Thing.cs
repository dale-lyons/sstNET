using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects
{
    public class Thing : SectorObject
    {
        public Thing() 
            : base() { }

        public override bool Ramable { get { return false; } }
        public override char Symbol { get { return '?'; } }
        public override string Name { get { return "Thing"; } }

        public override double TorpedoHit(GameData game, SectorCoordinate sc, double bullseye, double angle)
        {
            Game.Console.WriteLine("\nAAAAIIIIEEEEEEEEAAAAAAAAUUUUUGGGGGHHHHHHHHHHHH!!!\n");
            Game.Console.WriteLine("    HACK!     HACK!    HACK!        *CHOKE!*  \n");
            Game.Console.WriteLine("Mr. Spock-  \"Facinating!\"\n");

            game.Galaxy.CurrentQuadrant[this.Sector] = new Empty();
            game.Galaxy[game.Galaxy.Ship.QuadrantCoordinate].Thing = false;
            return 0;
        }

    }//class Thing
}