using System;
using System.Collections.Generic;
using System.Text;

using sstNET.Galaxy.SectorObjects;
using sstNET.Galaxy.SectorObjects.Ships;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace sstNET.Galaxy
{
    /// <summary>
    /// This class represents the current quadrant. It is the quadrant the ship is currently in,
    /// and is populated by all the quadrant sector objects including the ship. When the ship leaves
    /// this quadrant, a new current quadrant is generated. There is always only 1 current quadrant
    /// in the game at any one time.
    /// The current quadrant is composed of a 10x10 grid of sectors. Any sector object can be located in
    /// a single sector. The ship is considered a sector object and will occupy one sector.
    /// </summary>
    public class CurrentQuadrant : IXmlSerializable
    {
        /// <summary>
        /// The quadrant coordinate of the current quadrant.
        /// </summary>
        private QuadrantCoordinate mQuadrantCoordinate;

        /// <summary>
        /// These are the Sectors, a 10x10 grid of them
        /// </summary>
        private SectorObject[,] mSectors;

        /// <summary>
        /// When a new quadrant is created this flag is set to true
        /// if it is empty except one Romulan. For the first attack
        /// the romulan will not attack, however after the first attack
        /// romulan(s) will attack.
        /// </summary>
        public bool RomulanAttack { get; set; }

        /// <summary>
        /// CurrentQuadrant ctor. Create the grid of sectors and clear them
        /// (Fill with the Empty sector object)
        /// </summary>
        /// <param name="game">A reference to the Game object</param>
        public CurrentQuadrant()
        {
            //create the 10x10 sector grid
            mSectors = new SectorObject[10, 10];

            //and fill with all Empty objects for now
            ClearQuadrant();

        }//CurrentQuadrant ctor

        /// <summary>
        /// Clear the quadrant. Fill each sector with an Empty object.
        /// </summary>
        private void ClearQuadrant()
        {
            //and initialize with all Empty objects
            for (int ix = 1; ix <= 10; ix++)
            {
                for (int iy = 1; iy <= 10; iy++)
                {
                    this[ix, iy] = new Empty();
                }//for iy
            }//for ix
        }//ClearQuadrant

        /// <summary>
        /// Generate a new Current Quadrant. This is required whenever the Federation ship enters
        /// a new Quadrant.
        /// </summary>
        /// <param name="quad">The galactic quadrant we are setting up</param>
        /// <param name="shutup">Flag indicating to place a "Thing" object in quadrant</param>
        public void Generate(Quadrant quad, bool shutup, QuadrantCoordinate quadrant, GameData game)
        {
            //stash the coordinate of the quadrant we are generating
            mQuadrantCoordinate = quadrant;

            //make sure quadrant is all clear
            ClearQuadrant();

            //get a reference to the ship
            FederationShip ship = game.Galaxy.Ship;

            //place the Federation ship into its sector in this quadrant
            this[ship.Sector] = ship;

            //if this quadrant contains a super-nova, we are done.
            //(Game will end very shortly)
            //Note that this test must be done in this position to keep random numbers in
            //sync with original code.
            if (quad.SuperNova)
                return;

            //extract the random number generator
            Random rand = game.Random;

            // Decide if quadrant needs a Tholian, lighten up if skill is low
            if ((game.GameSkill < GameData.GameSkillEnum.Good && rand.Rand() <= 0.02) ||
                (game.GameSkill == GameData.GameSkillEnum.Good && rand.Rand() <= 0.05) ||
                (game.GameSkill > GameData.GameSkillEnum.Good && rand.Rand() <= 0.08))
            {//select a random location at one of 4 corners of quadrant
                while (true)
                {
                    //generate a random sectorcoordinate at one of the 4 corners
                    SectorCoordinate sc = SectorCoordinate.RandomCorner(rand);

                    //if it is not empty, then try again
                    if (!(this[sc] is Empty))
                        continue;

                    //place tholian at empty corner sector
                    this[sc] = new Tholian();

                    //Reserve unocupied corners
                    //We use the placeholder sectorobject Reserved for this purpose.
                    //We must make sure these are removed before this function returns.

                    //Construct a reserved sector object for now
                    Reserved reserved = new Reserved();

                    //And if a corner is empty, fill with the reserved object
                    if (this[SectorCoordinate.UpperLeft] is Empty)
                        this[SectorCoordinate.UpperLeft] = reserved;
                    if (this[SectorCoordinate.UpperRight] is Empty)
                        this[SectorCoordinate.UpperRight] = reserved;
                    if (this[SectorCoordinate.LowerLeft] is Empty)
                        this[SectorCoordinate.LowerLeft] = reserved;
                    if (this[SectorCoordinate.LowerRight] is Empty)
                        this[SectorCoordinate.LowerRight] = reserved;

                    break;
                }//while
            }//if

            // Position ordinary Klingons
            // keep track of first klingon allocated and last. This is used
            // to promote one to a commander and the other to Super-Commander if required.
            {
                SectorCoordinate firstSC = null;
                SectorCoordinate lastSC = null;
                for (int ii = 0; ii < quad.TotalKlingons; ii++)
                {
                    //first lets find an empty sector
                    SectorCoordinate sc = dropin(rand);

                    //if this is the first allocation, record it as the first
                    if (ii == 0)
                        firstSC = sc;

                    //and record the last
                    lastSC = sc;

                    //place a fresh new klingon at this  location
                    this[sc] = new Klingon(rand, game.GameSkill);

                }//for ii

                //If we need a commander, promote a Klingon. Use the last one placed.
                //We know that at least 1 klingon was in the quadrant, so safe to use lastSC
                if (quad.Commander != null)
                    this[lastSC] = new Commander(rand, game.GameSkill);

                //If we need a super-commander, promote a Klingon. Use the first one placed.
                if (quad.SuperCommander != null)
                    this[firstSC] = new SuperCommander(rand, game.GameSkill);

            }//let lastSC and firstSC go out of scope

            // Put in Romulans if needed
            for (int ii = 0; ii < quad.Romulans; ii++)
                this[dropin(rand)] = new Romulan(rand, game.GameSkill);

            // If quadrant needs a starbase, put it in
            if (quad.Base != null)
                this[dropin(rand)] = new SectorStarBase();

            //If quadrant needs a planet, put it in
            //Note that we are creating a new Sector planet from the original Quadrant planet.
            //Planets are one of the few objects in the game that retain their state between
            //generations of CurrentQuadrants. (the ship is another)
            if (quad.Planet != null)
                this[dropin(rand)] = new SectorPlanet(quad.Planet);

            // And finally the stars
            for (int ii = 0; ii < quad.Stars; ii++)
                this[dropin(rand)] = new Star();

            // Check for RNZ
            if (this.NeutralZone)
            {
                //if the ship radio is not damaged, show a warning from romulan
                if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SubspaceRadio))
                {
                    Game.Console.WriteLine("\nLT. UHURA- \"Captain, an urgent message.");
                    Game.Console.WriteLine("  I'll put it on audio.\"  CLICK\n");
                    Game.Console.WriteLine("INTRUDER! YOU HAVE VIOLATED THE ROMULAN NEUTRAL ZONE.");
                    Game.Console.WriteLine("LEAVE AT ONCE, OR YOU WILL BE DESTROYED!");
                }//if
            }//if

            // Put in THING if needed
            if (!shutup)
            {
                if (quad.Thing)
                {
                    this[dropin(rand)] = new Thing();
                    if (!ship.ShipDevices.IsDamaged(ShipDevices.ShipDevicesEnum.SRSensors))
                    {
                        Game.Console.WriteLine("\nMR. SPOCK- \"Captain, this is most unusual.");
                        Game.Console.WriteLine("    Please examine your short-range scan.\"");
                    }//if
                    quad.Thing = false;
                }//if
            }//if

            // Put in a few black holes
            for (int ii = 1; ii <= 3; ii++)
            {
                if (rand.Rand() > 0.5)
                    this[dropin(rand)] = new BlackHole();
            }//for ii

            // Take out X's in corners if Tholian present
            if (this.Tholian != null)
            {
                if (this[SectorCoordinate.UpperLeft] is Reserved)
                    this[SectorCoordinate.UpperLeft] = new Empty();
                if (this[SectorCoordinate.UpperRight] is Reserved)
                    this[SectorCoordinate.UpperRight] = new Empty();
                if (this[SectorCoordinate.LowerLeft] is Reserved)
                    this[SectorCoordinate.LowerLeft] = new Empty();
                if (this[SectorCoordinate.LowerRight] is Reserved)
                    this[SectorCoordinate.LowerRight] = new Empty();
            }//if

            //make sure the distances from the ship to all enemies are calculated
            this.CalculateDistances(ship.Sector);
            this.ResetAverageDistances();

            this.RomulanAttack = this.NeutralZone;

        }//CurrentQuadrant ctor

        /// <summary>
        /// Return/Set the SectorObject at the given x,y coordinate
        /// Note: Coordinates in the game are 1 based. We must adjust for 0 based array indexes
        /// When setting the object, the objects SectorCoordinate is updated to the coordinate it
        /// is being placed into.
        /// </summary>
        /// <param name="ix">xpos (1 based)</param>
        /// <param name="iy">ypos (1 based)</param>
        /// <returns></returns>
        public SectorObject this[int ix, int iy]
        {
            get { return mSectors[ix - 1, iy - 1]; }
            set { mSectors[ix - 1, iy - 1] = value; if (value != null) value.Sector = new SectorCoordinate(ix, iy); }
        }

        /// <summary>
        /// Return/Set the SectorObject at a given SectorCoordinate
        /// </summary>
        /// <param name="sc">Sector coordinate to set/get</param>
        /// <returns></returns>
        public SectorObject this[SectorCoordinate sc]
        {
            get { return this[sc.X, sc.Y]; }
            set { this[sc.X, sc.Y] = value;}
        }

        /// <summary>
        /// Find an empty sector by generating random SectorCoordinates and returning when
        /// a sector is found of type Empty
        /// </summary>
        /// <param name="rand">Random number generator</param>
        /// <returns>SectorCoordinate of empty sector</returns>
        public SectorCoordinate dropin(Random rand)
        {
            while (true)
            {
                SectorCoordinate sc = SectorCoordinate.Random(rand);
                if (this[sc] is Empty)
                    return sc;
            }//while
        }//dropin

        public List<Star> Stars { get { return ScanTypes<Star>(); } }
        public List<Romulan> Romulans { get { return ScanTypes<Romulan>(); } }
        public List<Klingon> Klingons { get { return ScanTypes<Klingon>(); } }
        public Tholian Tholian { get { return ScanType<Tholian>(); } }
        public SuperCommander SuperCommander { get { return ScanType<SuperCommander>(); } }
        public Commander Commander { get { return ScanType<Commander>(); } }
        public SectorPlanet SectorPlanet { get { return ScanType<SectorPlanet>(); } }
        public SectorStarBase SectorStarBase { get { return ScanType<SectorStarBase>(); } }

        /// <summary>
        /// Return a list of enemy ships in the current quadrant. The list is sorted by
        /// the distance from the ship. (closest first)
        /// Enemies are defined to be:
        ///klingons+romulans+commanders+supercommanders (exclude tholians)
        /// </summary>
        //[System.Xml.Serialization.XmlIgnore]
        public EnemyShipList Enemies { get { return new EnemyShipList(mSectors, true); } }

        /// <summary>
        /// Is ship in neutral zone?
        /// neutral zone is when there is at least 1 romulan present and no starbase and
        /// (Klingons, Commanders, SuperCommanders).
        /// </summary>
        //[System.Xml.Serialization.XmlIgnore]
        public bool NeutralZone
        {
            get
            {
                if (this.Romulans.Count <= 0)
                    return false;

                if (this.SectorStarBase != null)
                    return false;

                if (this.Klingons.Count > 0 || this.Commander != null || this.SuperCommander != null)
                    return false;

                return true;
            }//get
        }//NeutralZone

        /// <summary>
        /// Compute distance from ship to each Enemy in the current quadrant. Set
        /// the distance as a property of the enemy ship
        /// </summary>
        /// <param name="sc"></param>
        public void CalculateDistances(SectorCoordinate sc)
        {
            foreach (EnemyShip es in this.Enemies)
            {
                es.CalculateDistance(sc.DistanceTo(es.Sector));
            }

        }//CalculateDistances

        public void CalculateDistances(GalacticCoordinate gc)
        {
            foreach (EnemyShip es in this.Enemies)
            {
                GalacticCoordinate enemyGC = new GalacticCoordinate(mQuadrantCoordinate, es.Sector);
                double distance = gc.DistanceTo(enemyGC);
                es.CalculateDistance(distance);
            }

        }//CalculateDistances

        /// <summary>
        /// Sets the Average distance = distance for each enemy ship
        /// </summary>
        public void ResetAverageDistances()
        {
            foreach (EnemyShip es in this.Enemies)
                es.ResetAverageDistance();

        }//ResetAverageDistances

        /// <summary>
        /// Takes the current distance from ship to enemy ship and computes
        /// a running average between the 2.
        /// </summary>
        /// <param name="sc"></param>
        public void SetNewDistances(SectorCoordinate sc)
        {
            foreach (EnemyShip es in this.Enemies)
                es.SetNewDistance(sc.DistanceTo(es.Sector));

        }//SetNewDistances

        public void SetNewDistances(GalacticCoordinate gc)
        {
            foreach (EnemyShip es in this.Enemies)
            {
                GalacticCoordinate enemyGC = new GalacticCoordinate(mQuadrantCoordinate, es.Sector);
                double distance = gc.DistanceTo(enemyGC);
                es.SetNewDistance(distance);
            }//foreach

        }//SetNewDistances

        /// <summary>
        /// Generate a list of a specific type of SectorObject from the CurrentQuadrant."/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private List<T> ScanTypes<T>() where T : SectorObject
        {
            List<T> ret = new List<T>();
            foreach (SectorObject so in mSectors)
            {
                if (so is T)
                {
                    ret.Add(so as T);
                }//if
            }//foreach
            return ret;
        }//ScanTypes<T>

        /// <summary>
        /// Search CurrentQuadrant for a sectorobject of a specific type. If
        /// found return it, otherwise return a null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T ScanType<T>() where T : SectorObject
        {
            foreach (SectorObject so in mSectors)
            {
                if (so is T)
                {
                    return (so as T);
                }//if
            }//foreach
            return null;
        }//ScanType<T>

        /// <summary>
        /// Select a random star in the current quadrant
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        public SectorCoordinate RandomStar(Random rand)
        {
            int num = (int)(rand.Rand() * (this.Stars.Count) + 1);
            if (num == 0)
                return null;

            for (int nsx = 1; nsx < 10; nsx++)
            {
                for (int nsy = 1; nsy < 10; nsy++)
                {
                    if (this[nsx, nsy] is Star)
                    {
                        num--;
                        if (num == 0)
                            return new SectorCoordinate(nsx, nsy);
                    }//if
                }//for nsy
            }//for nsx
            return null;

        }//RandomStar

        /// <summary>
        /// Fill the current quadrant with Things.
        /// Only empty sectors are filled.
        /// </summary>
        public void FillWithThings()
        {
            for (int ii = 1; ii <= 10; ii++)
            {
                for (int jj = 1; jj <= 10; jj++)
                {
                    if (this[ii, jj] is Empty)
                        this[ii, jj] = new Thing();
                }
            }//for ii
        }//FillWithThings

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            //writer.WriteStartElement("CurrentQuadrant");

            XmlSerializer serializer = new XmlSerializer(typeof(QuadrantCoordinate));
            serializer.Serialize(writer, mQuadrantCoordinate);

            //and initialize with all Empty objects
            for (int ix = 0; ix < 10; ix++)
            {
                for (int iy = 0; iy < 10; iy++)
                {
                    SectorObject so = mSectors[ix, iy];
                    if (so is FederationShip)
                        so = new Empty();

                    serializer = new XmlSerializer(typeof(SectorObject));
                    serializer.Serialize(writer, so);
                }//for iy
            }//for ix

            //writer.WriteEndElement();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadToDescendant("QuadrantCoordinate");
            XmlSerializer serializer = new XmlSerializer(typeof(QuadrantCoordinate));
            mQuadrantCoordinate = (QuadrantCoordinate)serializer.Deserialize(reader);

            //and initialize with all Empty objects
            for (int ix = 0; ix < 10; ix++)
            {
                for (int iy = 0; iy < 10; iy++)
                {
                    serializer = new XmlSerializer(typeof(SectorObject));
                    SectorObject so = (SectorObject)serializer.Deserialize(reader);

                    //if (so is SectorPlanet)
                    //    continue;
                    //{
                        //QuadrantCoordinate qc = (so as SectorPlanet).QuadrantPlanet.QuadrantCoordinate;
                        //(so as SectorPlanet).QuadrantPlanet = mGalaxy[qc].Planet;
                    //}

                    this[ix + 1, iy + 1] = so;
                }//for iy
            }//for ix
            reader.ReadEndElement();
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return (null);
        }

    }//class CurrentQuadrant
}