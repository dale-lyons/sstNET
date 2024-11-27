using System;
using System.Collections.Generic;
using System.Text;

using sstNET.Galaxy.QuadrantObjects;

namespace sstNET.Galaxy
{
    /// <summary>
    /// This class represents a quadrant in the galaxy. The galaxy is composed
    /// of an 8x8 grid of quadrants.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Location={Coordinate.ToString(false).Trim()}")]
    public class Quadrant
    {
        /// <summary>
        /// This is the quadrant coordinate of this quadrant in the galaxy
        /// Set in ctor, never changes during the game.
        /// </summary>
        public QuadrantCoordinate Coordinate { get; set; }

        /// <summary>
        /// Number of Romulans here
        /// </summary>
        public int Romulans { get; set; }

        /// <summary>
        /// Number of ordinary klingons are in this quadrant (0-9)
        /// </summary>
        public int OrdinaryKlingons { get; set; }

        /// <summary>
        /// if a Commander is here
        /// </summary>
        public QuadrantCommander Commander { get; set; }

        /// <summary>
        /// if a super-commander is here
        /// 
        /// </summary>
        public QuadrantSuperCommander SuperCommander { get; set; }

        /// <summary>
        /// If a Thing is here
        /// </summary>
        public bool Thing { get; set; }

        /// <summary>
        /// If this quadrant has a super-nova in it
        /// </summary>
        public bool SuperNova { get; set; }

        /// <summary>
        /// If a Planet is here
        /// </summary>
        public QuadrantPlanet Planet { get; set; }

        /// <summary>
        /// Number of stars in the quadrant(0-9)
        /// </summary>
        public int Stars { get; set; }

        /// <summary>
        /// If a starbase is here
        /// </summary>
        public QuadrantStarBase Base { get; set; }

        /// <summary>
        /// Current discovered state of this quadrant by the player.
        /// Starch == -1 : Quad has a starbase and is unknown
        /// Starch ==  0 : Quad is unknown
        /// Starch == +1 : Quad is known and proper state is displayed.
        /// 999 < Starch : Quad was known but is now unknown (broken radio). state is last known value.
        /// </summary>
        public int Starch { get; set; }

        public Quadrant() { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="ix"></param>
        /// <param name="iy"></param>
        public Quadrant(int ix, int iy)
        {
            this.Coordinate = new QuadrantCoordinate(ix, iy);
        }

        //public object Clone22()
        //{
        //    Quadrant quad = new Quadrant(Coordinate.X, Coordinate.Y);
        //    quad.Romulans = Romulans;
        //    quad.OrdinaryKlingons = OrdinaryKlingons;

        //    if (Commander != null)
        //        quad.Commander = Commander.Clone() as QuadrantCommander;

        //    if (SuperCommander != null)
        //        quad.SuperCommander = SuperCommander.Clone() as QuadrantSuperCommander;

        //    quad.Thing = Thing;
        //    quad.SuperNova = SuperNova;

        //    if (Planet != null)
        //        quad.Planet = Planet.Clone() as QuadrantPlanet;

        //    quad.Stars = Stars;

        //    if (Base != null)
        //        quad.Base = Base.Clone() as QuadrantStarBase;

        //    //quad.Starch = Starch;
        //    return quad;
        //}

        /// <summary>
        /// Returns total number of klingons, commanders and supercommanders in this quadrant
        /// </summary>
        public int TotalKlingons
        {
            get
            {
                int ret = this.OrdinaryKlingons;
                if (Commander != null)
                    ++ret;
                if (SuperCommander != null)
                    ++ret;
                return ret;
            }
        }//TotalKlingons

        /// <summary>
        /// returns xyz
        /// x - number of ordinary klingons+commanders+super commanders(0-9)
        /// y - number of bases(0 or 1)
        /// z - number of stars(0-9)
        /// </summary>
        /// <returns></returns>
        public int ToInt
        {
            get
            {
                if (SuperNova)
                    return 1000;
                else
                    return ((TotalKlingons * 100) + (((Base == null) ? 0 : 1) * 10) + Stars);
            }
        }//ToInt


        //public void WriteXml(System.Xml.XmlWriter writer)
        //{
        //    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(QuadrantCoordinate));
        //    serializer.Serialize(writer, Coordinate);
        //}

        //public void ReadXml(System.Xml.XmlReader reader)
        //{
        //    //personName = reader.ReadString();
        //}

        //public System.Xml.Schema.XmlSchema GetSchema()
        //{
        //    return (null);
        //}

    }//class Quadrant
}
