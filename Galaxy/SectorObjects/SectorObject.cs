using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects
{
    /// <summary>
    /// Represents an in game sector object
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(Ships.Tholian))]
    [System.Xml.Serialization.XmlInclude(typeof(TholianWeb))]
    [System.Xml.Serialization.XmlInclude(typeof(Thing))]
    [System.Xml.Serialization.XmlInclude(typeof(SectorStarBase))]
    [System.Xml.Serialization.XmlInclude(typeof(SectorPlanet))]
    [System.Xml.Serialization.XmlInclude(typeof(BlackHole))]
    [System.Xml.Serialization.XmlInclude(typeof(Star))]
    [System.Xml.Serialization.XmlInclude(typeof(Empty))]
    [System.Xml.Serialization.XmlInclude(typeof(Ships.Enterprise))]
    [System.Xml.Serialization.XmlInclude(typeof(Ships.FaerieQueene))]
    [System.Xml.Serialization.XmlInclude(typeof(Ships.Klingon))]
    [System.Xml.Serialization.XmlInclude(typeof(Ships.Romulan))]
    [System.Xml.Serialization.XmlInclude(typeof(Ships.Commander))]
    [System.Xml.Serialization.XmlInclude(typeof(Ships.SuperCommander))]
    public abstract class SectorObject
    {
        private static int _nextID = 0;

        public SectorObject()
        {
            ID = _nextID++;
        }

        public int ID { get; set; }
        public abstract char Symbol { get ; }
        public abstract string Name { get; }

        [System.Xml.Serialization.XmlIgnoreAttribute]
        public SectorCoordinate Sector { get; set; }

        //can this object be rammed by friendly ship?
        public abstract bool Ramable { get; }

        public virtual double TorpedoHit(GameData game, SectorCoordinate sc, double bullseye, double angle)
        {
            Game.Console.Write("\nDon't know how to handle collision with ");
            Game.Console.crmena(true, this, true, this.Sector);
            return 0;
        }

        public virtual bool Ram(GameData game, SectorCoordinate previous, out SectorCoordinate final, out double distSoFar)
        {
            Game.Console.Skip(1);
            Game.Console.Write(game.Galaxy.Ship.Name);

            if (this is TholianWeb)
                Game.Console.Write(" encounters Tholian web at");
            else
                Game.Console.Write(" blocked by object at");

            Game.Console.WriteLine("{0};", this.Sector.ToString(true));
            Game.Console.Write("Emergency stop required ");

            double stopegy = 50.0 * game.Turn.dist / game.Turn.Time;
            Game.Console.WriteLine("{0,0:F2} units of energy.", stopegy);
            game.Galaxy.Ship.ShipEnergy -= stopegy;

            distSoFar = 0.1 * this.Sector.DistanceTo(game.Galaxy.Ship.Sector);
            final = previous;

            if (game.Galaxy.Ship.ShipEnergy <= 0)
            {
                Finish.finish(Finish.FINTYPE.FNRG, game);
                return true;
            }//if

            return false;
        }//Ram

        /// <summary>
        /// Default Nova implementation.
        /// This method is invoked when a star has nova'ed beside this object.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        /// <param name="gameOver"></param>
        /// <returns>True is this is a star and will also nova</returns>
        public virtual bool Nova(GameData game, SectorCoordinate sc, out bool gameOver)
        {
            gameOver = false;
            return false;
        }

    }//class SectorObject
}