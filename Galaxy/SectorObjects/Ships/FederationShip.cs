using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects.Ships
{
    [System.Xml.Serialization.XmlInclude(typeof(FaerieQueene))]
    [System.Xml.Serialization.XmlInclude(typeof(Enterprise))]
    [System.Diagnostics.DebuggerDisplay("Energy={ShipEnergy} Location={QuadrantCoordinate.ToString(false)} Torpedoes={Torpedoes}", Name = "{Name}")]
    public abstract class FederationShip : SectorObject
    {
        public static double YellowAlertEnergyLevel = 1000.0;

        public enum ShuttleLocationEnum
        {
            Aliens,
            Planet,
            ShuttleBay
        }
        public enum CrewLocationEnum
        {
            Shuttle,
            Planet,
            Ship
        }

        private int mBuffetted;
        private int mDisplacedX;
        private int mDisplacedY;

        /// <summary>
        /// Number of deep space probes on the ship.
        /// </summary>
        public int Probes { get; set; }

        /// <summary>
        /// Flag indicating if the ship is docked at a starbase
        /// </summary>
        public bool Docked { get; set; }

        /// <summary>
        /// The current quadrant the ship is in.
        /// </summary>
        public QuadrantCoordinate QuadrantCoordinate { get; set; }

        /// <summary>
        /// all the devices on the ship. Handles mostly damage
        /// </summary>
        //private ShipDevices _devices = new ShipDevices();

        public bool CrystalsMined { get; set; }

        public FederationShip()
            : base()
        {
            CrewLocation = CrewLocationEnum.Ship;
            ShuttleLocation = ShuttleLocationEnum.ShuttleBay;

            Warp = 5.0;
            ShipEnergy = InitialMainEnergy;
            ShieldEnergy = InitialShieldEnergy;
            Torpedoes = InitialTorpedoes;
            StarChartDamage = FutureEvents.NEVER;
            LifeSupportReserves = InitialLifeSupport;

            ResetBuffettedCount();

            ShipDevices = new ShipDevices();

        }

        public GalacticCoordinate GalacticCoordinate
        {
            get
            {
                return new GalacticCoordinate(this.QuadrantCoordinate, this.Sector);
            }
            set
            {
                this.QuadrantCoordinate = value.QuadrantCoordinate;
                this.Sector = value.Sector;
            }
        }

        /// <summary>
        /// Current damage to the star chart.
        /// </summary>
        public double StarChartDamage { get; set; }

        //abstract functions implemented by the Ship
        //initial amounts of resources the ship gets when it replenishes at a starbase.
        //also the amount given when first created
        public abstract int InitialMainEnergy { get; }
        public abstract int InitialShieldEnergy { get; }
        public abstract double InitialLifeSupport { get; }
        public abstract int InitialTorpedoes { get; }

        //does this ship have a shuttlebay?
        public abstract bool HasShuttleBay { get; }

        //does this ship have a deathray?
        public abstract bool HasDeathray { get; }

        //does this ship have deep space probes?
        public abstract bool HasProbes { get; }

        /// <summary>
        /// Returns the Planet we are currently orbiting. null if not in orbit
        /// </summary>
        //[System.Xml.Serialization.XmlIgnoreAttribute]
        public SectorPlanet Orbit { get; set; }

        /// <summary>
        /// Flag indicating the shields have changed state
        /// todo - do we need this flag?
        /// </summary>
        public bool ShieldChange { get; set; }

        /// <summary>
        /// Indicates the current state of the shields. true - shields are up
        /// </summary>
        public bool ShieldsUp { get; set; }

        /// <summary>
        /// Amount of life support reserves. This is used if life-support is damaged
        /// </summary>
        public double LifeSupportReserves { get; set; }

        /// <summary>
        /// Current warp speed(0-10)
        /// </summary>
        public double Warp { get; set; }

        /// <summary>
        /// Number of Photon torpedoes aboard
        /// </summary>
        public int Torpedoes { get; set; }

        /// <summary>
        /// Amount of ship energy available
        /// </summary>
        //private double _energy;
        //energy can go negative, just to be compatible with original version output
        //public double ShipEnergy { get { return _energy; } set { _energy = Math.Max(value, 0.0); } }
        //public double ShipEnergy { get { return _energy; } set { _energy = value; } }
        public double ShipEnergy { get; set; }

        /// <summary>
        /// Amount of shield energy available
        /// </summary>
        public double ShieldEnergy { get; set; }

        /// <summary>
        /// Current height above planet we are orbiting
        /// </summary>
        public int OrbitHeight { get; set; }

        /// <summary>
        /// Current location of the ships crew
        /// </summary>
        public CrewLocationEnum CrewLocation { get; set; }

        /// <summary>
        /// Current location of the shuttle craft
        /// </summary>
        public ShuttleLocationEnum ShuttleLocation { get; set; }

        /// <summary>
        /// Are there mined dylithum crystals aboard?
        /// </summary>
        public bool Crystals { get; set; }

        /// <summary>
        /// Number of times we have used the crystals
        /// </summary>
        public int CrystalUses { get; set; }

        /// <summary>
        /// Probability of crystal failure
        /// Computed as follows:
        /// Starts at 5% and doubles for every use
        /// </summary>
        public double CrystalProbability
        {
            get
            {
                double prob = 0.05;
                for (int ii = 0; ii < this.CrystalUses; ii++)
                {
                    prob *= 2.0;
                }//for ii
                return prob;
            }
        }//CrystalProbability

        /// <summary>
        /// Returns the string version of the condition of the ship.
        /// </summary>
        /// <param name="quad"></param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public string Condition(Quadrant quad)
        {
            if (this.Docked)
                return "DOCKED";
            if (quad.TotalKlingons > 0 || quad.Romulans > 0)
                return "RED";
            if (ShipEnergy < YellowAlertEnergyLevel)
                return "YELLOW";
            return "GREEN";
        }//Condition

        /// <summary>
        /// Is this ship rammable?
        /// </summary>
        public override bool Ramable { get { return false; } }

        /// <summary>
        /// All the ship devices.
        /// </summary>
        //[System.Xml.Serialization.XmlIgnoreAttribute]
        public ShipDevices ShipDevices { get; set; }

        /// <summary>
        /// Handles the ship being hit by an enemy torpedo
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        /// <param name="bullseye"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public override double TorpedoHit(GameData game, SectorCoordinate sc, double bullseye, double angle)
        {
            Game.Console.WriteLine("\nTorpedo hits {0}.", game.Galaxy.Ship.Name);
            double hit = 700.0 + 100.0 * game.Random.Rand() -
                   1000.0 * this.Sector.DistanceTo(sc) *
                   Math.Abs(Math.Sin(bullseye - angle));
            hit = Math.Abs(hit);

            game.Galaxy.Ship.Docked = false;//undock

            //We may be displaced.
            if (game.Galaxy.Ship.CrewLocation == FederationShip.CrewLocationEnum.Planet)
                return hit;//Cheat if on a planet

            double ang = angle + 2.5 * (game.Random.Rand() - 0.5);
            double temp = Math.Abs(Math.Sin(ang));
            if (Math.Abs(Math.Cos(ang)) > temp)
                temp = Math.Abs(Math.Cos(ang));

            double xx = -Math.Sin(ang) / temp;
            double yy = Math.Cos(ang) / temp;

            SectorCoordinate jxy = new SectorCoordinate((int)(this.Sector.X + xx + 0.5), (int)(this.Sector.Y + yy + 0.5));
            if (!jxy.Valid)
                return hit;

            if (game.Galaxy.CurrentQuadrant[jxy] is BlackHole)
            {
                Finish.finish(Finish.FINTYPE.FHOLE, game);
                return hit;
            }//if

            //can't move into object
            if (!(game.Galaxy.CurrentQuadrant[jxy] is Empty))
                return hit;

            //Federation ship was shoved ... 
            game.Galaxy.CurrentQuadrant[this.Sector] = new Empty();
            game.Galaxy.CurrentQuadrant[jxy] = this;

            Game.Console.WriteLine("{0} displaced by blast to{1}", game.Galaxy.Ship.Name, jxy.ToString(true));

            game.Galaxy.CurrentQuadrant.CalculateDistances(game.Galaxy.Ship.Sector);
            game.Galaxy.CurrentQuadrant.ResetAverageDistances();

            return hit;
        }//TorpedoHit

        public void ResetBuffettedCount()
        {
            mBuffetted = 0;
            mDisplacedX = 0;
            mDisplacedY = 0;
        }

        public override bool Nova(GameData game, SectorCoordinate sc, out bool gameOver)
        {
            gameOver = false;

            Game.Console.WriteLine("***Starship buffeted by nova.");
            if (this.ShieldsUp)
            {
                if (this.ShieldEnergy >= 2000.0)
                {
                    this.ShieldEnergy -= 2000.0;
                }
                else
                {
                    double diff = 2000.0 - this.ShieldEnergy;
                    this.ShipEnergy -= diff;
                    this.ShieldEnergy = 0.0;
                    this.ShieldsUp = false;
                    Game.Console.WriteLine("***Shields knocked out.");
                    this.ShipDevices.AddDamage(ShipDevices.ShipDevicesEnum.Shields, (0.005 * game.DamageFactor * game.Random.Rand() * diff));
                }//else
            }//if
            else
            {
                this.ShipEnergy -= 2000.0;
            }

            if (this.ShipEnergy <= 0)
            {
                Finish.finish(Finish.FINTYPE.FNOVA, game);
                gameOver = true;
                return false;
            }//if
            mBuffetted++;

            //add in course nova contributes to kicking starship
            mDisplacedX += (this.Sector.X - sc.X);
            mDisplacedY += (this.Sector.Y - sc.Y);
            return false;
        }//Nova

        private static double[] course = {0.0, 10.5, 12.0, 1.5, 9.0, 0.0, 3.0, 7.5, 6.0, 4.5};
        public void FinalNovaBuffet(GameData game)
        {
            if (mBuffetted <= 0)
                return;

            int icx = 0;
            if(mDisplacedX != 0)
                icx = (mDisplacedX < 0) ? -1 : +1;

            int icy = 0;
            if (mDisplacedY != 0)
                icy = (mDisplacedY < 0) ? -1 : +1;

            game.Turn.dist = mBuffetted * 0.1;
            game.Turn.direc = course[3 * (icx + 1) + icy + 2];

            if (game.Turn.direc == 0.0)
                return;

            game.Turn.Time = 10.0 * game.Turn.dist / 16.0;
            Game.Console.WriteLine("\nForce of nova displaces starship.");

            //Eliminates recursion problem
            game.Turn.iattak = 2;
            Moving.Move(game);
            game.Turn.Time = 10.0 * game.Turn.dist / 16.0;

        }//FinalNovaBuffet

    }//class FederationShip
}