using System;
using System.Collections.Generic;
using System.Text;

using sstNET.Galaxy.SectorObjects;
using sstNET.Galaxy.QuadrantObjects;
using sstNET.Galaxy.SectorObjects.Ships;

namespace sstNET.Galaxy
{
    /// <summary>
    /// The Galaxy class represents all the game objects.
    /// </summary>
    public class Galaxy
    {
        public const int GALAXYWIDTH = 8;
        public const int GALAXYHEIGHT = 8;
        private const int PLNETMAX = 10;

        //These next data items are the initial number of various things
        public int _inbase;                    //bases
        public int _inplan;                    //planets
        public int _nromrem;                   //romulans
        public double _intime;                 //time
        public int _inkling;                   //klingons
        public int _incom;                     //commanders
        public int _nscrem;                    //super commanders
        public double _inresor;                //resources

        /// <summary>
        /// The initial star date.
        /// Not readonly! (set from Game class)
        /// </summary>
        public double _indate;

        /// <summary>
        /// The quadrant grid that makes up the galaxy.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Quadrant[,] Quadrants { get; private set; }

        //[XmlArray(), XmlArrayItem("QuadrantArrayDale", typeof(Quadrant[]), IsNullable = false)]
        //[System.Xml.Serialization.XmlIgnore]
        public Quadrant[][] QuadrantsArray
        {
            get
            {
                Quadrant[][] quads = new Quadrant[GALAXYWIDTH][];
                for (int ix = 0; ix < GALAXYWIDTH; ix++)
                {
                    quads[ix] = new Quadrant[8];
                    for (int iy = 0; iy < GALAXYHEIGHT; iy++)
                    {
                        quads[ix][iy] = Quadrants[ix, iy];
                    }
                }
                return quads;
            }
            set
            {
                Quadrants = new Quadrant[GALAXYWIDTH, GALAXYHEIGHT];
                for (int ix = 0; ix < GALAXYWIDTH; ix++)
                {
                    for (int iy = 0; iy < GALAXYHEIGHT; iy++)
                    {
                        Quadrants[ix, iy] = value[ix][iy];
                    }
                }
            }
        }//QuadrantsArray

        /// <summary>
        // The current quadrant of the federation ship
        /// </summary>
        //[System.Xml.Serialization.XmlIgnore]
        public CurrentQuadrant CurrentQuadrant { get; set; }

        /// <summary>
        /// indicates if a starbase is being attacked by a Commander.
        /// null indicates no attack underway otherwise its the quadrant
        /// coordinate of the starbase being attacked.
        /// </summary>
        public QuadrantCoordinate CommanderAttack { get; set; }

        /// <summary>
        /// same as Commander attack above but for SuperCommander instead
        /// </summary>
        public QuadrantCoordinate SuperCommanderAttack { get; set; }

        /// <summary>
        /// A deep-space probe currently travelling through the galaxy.
        /// null indicates no probe being used.
        /// </summary>
        public Probe Probe { get; set; }

        /// <summary>
        /// A reference to the current Federation ship. This can either be
        /// the "Enterprise" or the "Faerie Queene" if the enterprise was abandoned.
        /// (Can also be null when score is being computed)
        /// </summary>
        public FederationShip Ship { get; set; }

        public Galaxy()
        {
        }

        /// <summary>
        /// The galaxy object holds all information/objects about the current galaxy.
        /// </summary>
        /// <param name="game"></param>
        public Galaxy(GameData game)
        {
            //This next section generates many of the initial conditions of the galaxy based
            //on the skill level selected. The order that these computations are performed is
            //critical to keep random number generation the same as the original code.

            //Determine the initial number of starbases
            _inbase = (int)(3.0 * game.Random.Rand() + 2.0);

            //Number of planets in galaxy
            _inplan = (int)((PLNETMAX / 2) + (PLNETMAX / 2 + 1) * game.Random.Rand());

            //Number of Romulans
            _nromrem = (int)((2.0 + game.Random.Rand()) * (int)game.GameSkill);

            //Initial amount of time available
            _intime = 7.0 * (int)game.GameLength;

            //Initial number of Klingons. Note that this number is the total including
            //Klingons, Commanders and possibly 1 Super-Commander
            _inkling = (int)(2.0 * _intime * (((int)game.GameSkill + 1 - 2 * game.Random.Rand()) * (int)game.GameSkill * 0.1 + .15));

            //Initial number of Commanders(max 10)
            _incom = Math.Min(10, (int)((int)game.GameSkill + 0.0625 * _inkling * game.Random.Rand()));

            //Possibly one Super-Commander, but only on higher skill levels
            _nscrem = ((int)game.GameSkill > 2 ? 1 : 0);

            //Initial resources for the galaxy
            _inresor = (_inkling + 4 * _incom) * _intime;

            //bump up number of starbases if many klingons
            if (_inkling > 50)
                ++_inbase;

            //Setup the current quadrant. This is the quadrant the current Federation ship is in.
            this.CurrentQuadrant = new CurrentQuadrant();

        }//Galaxy ctor

        /// <summary>
        /// Allows index access to quadrant using x,y coordinates
        /// Note:we use 1 based indexing outside of this class.
        /// </summary>
        /// <param name="ix">X position (1-8)</param>
        /// <param name="iy">Y position (1-8)</param>
        /// <returns></returns>
        public Quadrant this[int ix, int iy]
        {
            get { return Quadrants[ix - 1, iy - 1]; }
            set { Quadrants[ix - 1, iy - 1] = value; }
        }

        /// <summary>
        /// Allows index access to quadrant using a QuadrantCoordinate
        /// Note:we use 1 based indexing outside of this class.
        /// </summary>
        /// <param name="qc"></param>
        /// <returns></returns>
        public Quadrant this[QuadrantCoordinate qc]
        {
            get { return Quadrants[qc.X - 1, qc.Y - 1]; }
            set { Quadrants[qc.X - 1, qc.Y - 1] = value; }
        }

        /// <summary>
        /// Allows index access to quadrant using a GalacticCoordinate
        /// </summary>
        /// <param name="gc"></param>
        /// <returns></returns>
        public Quadrant this[GalacticCoordinate gc]
        {
            get { return this[gc.QuadrantCoordinate]; }
            set { this[gc.QuadrantCoordinate] = value; }
        }

        /// <summary>
        /// Generate a new galaxy.
        /// Division of the galaxy creation is split between the constructor and this method.
        /// It was nessecary to preserve the order of creation to keep it in sync with the original game.
        /// This is important to keep the random number generator in sync.
        /// </summary>
        /// <param name="game"></param>
        public void Generate(GameData game)
        {
            //create the starship enterprise
            //important - do this in the given order!!
            this.Ship = new Enterprise();
            this.Ship.QuadrantCoordinate = QuadrantCoordinate.Random(game.Random);
            this.Ship.Sector = SectorCoordinate.Random(game.Random);
            this.Ship.Probes = (int)(3.0 * game.Random.Rand() + 2.0);          //Give them 2-4 of these wonders

            //hack ... must pop back to game to setup various game items
            game.Generate();

            //create the galaxy by creating each quadrant.
            //set the intital number of stars in each quadrant
            Quadrants = new Quadrant[GALAXYWIDTH, GALAXYHEIGHT];
            for (int ix = 1; ix <= GALAXYWIDTH; ix++)
            {
                for (int iy = 1; iy <= GALAXYHEIGHT; iy++)
                {
                    this[ix, iy] = new Quadrant(ix, iy);
                    this[ix, iy].Stars = (int)(((game.Random.Rand() * 9.0) + 1.0));    // Put stars in the quadrant
                }//for jj
            }//for ii

            //Locate star bases in galaxy
            //Place randomly but extra logic here to prevent starbases
            //from clustering too close together.
            for (int ii = 0; ii < _inbase; ii++)
            {
                bool contflag;
                QuadrantCoordinate baseQC;
                do
                {
                    //locate a quadrant that currently does not have a starbase.
                    do
                    {
                        //generate a random quadrant coordinate and test if a star base is already there
                        baseQC = QuadrantCoordinate.Random(game.Random);
                    } while (this[baseQC].Base!=null);

                    //and check that it is not too close to any existing starbase. If too close then try again.
                    contflag = false;
                    foreach(QuadrantStarBase starbase in this.Bases)
                    {
                        double distq = Math.Pow(starbase.QuadrantCoordinate.X - baseQC.X, 2) + Math.Pow(starbase.QuadrantCoordinate.Y - baseQC.Y, 2);
                        if (distq < 6.0 * (6 - _inbase) && game.Random.Rand() < 0.75)
                        {
                            contflag = true;
                            break;
                        }//if
                    }//foreach
                } while (contflag);

                //ok, found a good quadrant for starbase, create one here.
                this[baseQC].Base = new QuadrantStarBase(baseQC);

                //set starchart to known for this quadrant
                this[baseQC].Starch = -1;
            }//for ii

            //Position ordinary Klingon Battle Cruisers. Note that any single Quadrant can not have
            //more than 9 Klingons
            //compute number of ordinary Klingons which is the total number of Klingons
            //minus the Commanders and SuperCommander
            int krem = _inkling - _incom - _nscrem;

            //determine a random number between 1 and 9 weighted by the skill level. This number
            //is the "Clumpiness" of the allocation of klingons.
            int klumper = (int)(0.25 * (int)game.GameSkill * (9.0 - (int)game.GameLength) + 1.0);

            // Can't have more than 9 in quadrant
            klumper = Math.Min(klumper, 9);

            //keep allocating klingons to quadrants until they are all allocated. Make sure that no
            //single quadrant has more than 9 klingons.
            do
            {
                //select a random number of klingons based on the "Clumpiness"
                double random = game.Random.Rand();
                int klump = (int)((1.0 - random * random) * klumper);

                //and subtract from total being placed
                klump = Math.Min(klump, krem);
                krem -= klump;

                //select a random quadrant and if the klingons will fit (not exceed 9 limit)
                //then place them.
                QuadrantCoordinate qc;
                do
                {
                    qc = QuadrantCoordinate.Random(game.Random);
                } while ((this[qc].OrdinaryKlingons + klump) > 9);

                //place these Klingons
                this[qc].OrdinaryKlingons += klump;

                //and keep allocating until all placed
            } while (krem > 0);

            //Position Klingon Commander Ships. Only one Commander is allowed in a quadrant.
            //Only quadrants with at least 1 Klingon will be considered.
            for (int ii = 1; ii <= _incom; ii++)
            {
                while (true)
                {
                    //generate a random quadrant coordinate
                    QuadrantCoordinate qc = QuadrantCoordinate.Random(game.Random);

                    //Favor quadrant with at least 1 klingon.
                    if (this[qc].TotalKlingons < 1 && game.Random.Rand() < 0.75)
                        continue;

                    //do not violate more than 9 klingons in a single quadrant rule
                    if(this[qc].OrdinaryKlingons >= 9)
                        continue;

                    //and cannot have 2 Commanders in same quadrant
                    if(this[qc].Commander != null)
                        continue;

                    //go create Commander for this quadrant
                    this[qc].Commander = new QuadrantCommander(qc);
                    break;
                }//while
            }//for ii

            //Locate planets in galaxy. Only a max of 1 planet per quadrant allowed.
            for (int ii = 1; ii <= _inplan; ii++)
            {
                while (true)
                {
                    //generate a random quadrant coordinate
                    QuadrantCoordinate qc = QuadrantCoordinate.Random(game.Random);

                    //if a planet already exists here, try again
                    if (this[qc].Planet != null)
                        continue;

                    //otherwise create the planet
                    this[qc].Planet = new QuadrantPlanet(game.Random, qc);
                    break;
                }//while
            }//for ii

            //Locate Romulans. There can be any number of Romulans in a single quadrant.
            for (int ii = 1; ii <= _nromrem; ii++)
            {
                //generate a random quadrant coordinate and place the Romulan.
                this[QuadrantCoordinate.Random(game.Random)].Romulans++;
            }//for ii

            // Locate the Super Commander(If one exists)
            if (_nscrem > 0)
            {
                while (true)
                {
                    //generate a random quadrant coordinate
                    QuadrantCoordinate qc = QuadrantCoordinate.Random(game.Random);

                    //make sure we don't violate max 9 klingons per quadrant rule
                    if (this[qc].TotalKlingons >= 9)
                        continue;

                    //create the SuperCommander in this quadrant
                    this[qc].SuperCommander = new QuadrantSuperCommander(qc);
                    break;
                }//while
            }//if

            //todo - check thing logic
            // Place thing (in tournament game, thingx == -1, don't want one!)
            //if (game.Random.Rand() < 0.1 && game.GameType != Choose.GameTypes.Tournament)
            if (game.Random.Rand() < 0.1)
            {
                QuadrantCoordinate qc = QuadrantCoordinate.Random(game.Random);
                this[qc].Thing = true;
            }//if

        }//Generate

        public void UpdateStarChart(bool fix)
        {
            foreach (Quadrant quad in Quadrants)
            {
                if (fix)
                {
                    if (quad.Starch > 999)
                        quad.Starch = 1;
                }
                else
                {
                    if (quad.Starch == 1)
                        quad.Starch = quad.ToInt + 1000;
                }
            }//foreach
        }//updateStarChart

        /// <summary>
        /// Generate a new Current Quadrant. The Current Quadrant is the quadrant the
        /// current Federation ship is located in.(Either Enterprise or Fairie Queen)
        /// </summary>
        /// <param name="game"></param>
        public void newquad(GameData game, bool shutup)
        {
            //determine if we are escaping the SC
            game.Turn.justin = true;
            game.Turn.iattak = 1;

            //cannot start in new quadrant docked
            this.Ship.Docked = false;

            //make sure we are no longer in orbit
            this.Ship.Orbit = null;

            game.Turn.ientesc = (CurrentQuadrant != null && CurrentQuadrant.SuperCommander != null);
            CurrentQuadrant.Generate(this[Ship.GalacticCoordinate], shutup, Ship.QuadrantCoordinate, game);
        }//newquad

        /// <summary>
        /// Scan through the quadrants and if we find a quadrant with the Super-Commander,
        /// return its quadrant coordinate
        /// Otherwise return a null
        /// </summary>
        public QuadrantCoordinate SuperCommander
        {
            get
            {
                foreach (Quadrant quad in Quadrants)
                {
                    if (quad.SuperCommander != null)
                        return quad.Coordinate;
                }
                return null;
            }
        }

        /// <summary>
        /// Scan through the quadrants and if we find a quadrant with the Thing,
        /// return its quadrant coordinate
        /// Otherwise return a null
        /// </summary>
        public QuadrantCoordinate Thing
        {
            get
            {
                foreach (Quadrant quad in Quadrants)
                {
                    if (quad.Thing)
                        return quad.Coordinate;
                }
                return null;
            }
        }

        /// <summary>
        /// Generate a list of all Commanders in the galaxy.
        /// Note that the list is sorted by creation ID, in order that the
        /// Commanders were created. This is to maintain the same logic as original code.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public QuadrantObjects.CommanderList Commanders
        {
            get { return new QuadrantObjects.CommanderList(this); }
        }//Commanders

        /// <summary>
        ///Returns total number of klingons, commanders and supercommanders in galaxy
        /// </summary>
        public int Klingons
        {
            get
            {
                int count = 0;
                foreach (Quadrant quad in Quadrants)
                    count += quad.TotalKlingons;
                return count;
            }//get
        }//Klingons

        /// <summary>
        /// Return a count of stars in the galaxy
        /// </summary>
        public int Stars
        {
            get
            {
                int count = 0;
                foreach (Quadrant quad in Quadrants)
                    count += quad.Stars;
                return count;
            }
        }

        /// <summary>
        ///Returns total number of romulans in galaxy
        /// </summary>
        public int Romulans
        {
            get
            {
                int count = 0;
                foreach (Quadrant quad in Quadrants)
                    count += quad.Romulans;
                return count;
            }//get
        }//Romulans

        /// <summary>
        /// Generates a list of Planets in the galaxy.
        /// Note that the list is sorted by creation ID, in order that the
        /// Planets were created. This is to maintain the same logic as original
        /// logic.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public QuadrantObjects.PlanetList Planets
        {
            get
            {
                return new QuadrantObjects.PlanetList(this);
            }//get
        }//Planets

        /// <summary>
        /// Generate a list of starbases in the galaxy.
        /// Note that the list is sorted by creation ID, in order that the
        /// Starbases were created. This is to maintain the same logic as original
        /// logic.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public StarBaseList Bases
        {
            get
            {
                return new StarBaseList(this);
            }//get
        }//Bases

        /// <summary>
        /// Select a random star from the galaxy in response to a random star going super-nova.
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        public QuadrantCoordinate RandomStar(Random rand)
        {
            //Scheduled supernova -- select star
            //logic changed here so that we won't favor quadrants in top
            int stars = this.Stars;
            if (stars == 0) //nothing to supernova exists
                return null;

            //select a random star
            int num = (int)(rand.Rand() * stars + 1);

            //find the quadrent of the nth star in the galaxy
            for (int nqx = 1; nqx <= GALAXYWIDTH; nqx++)
            {
                for (int nqy = 1; nqy <= GALAXYHEIGHT; nqy++)
                {
                    num -= this[nqx, nqy].Stars;
                    if (num <= 0)
                        return new QuadrantCoordinate(nqx, nqy);
                }//for nqy
            }//for nqx

            //can never get here ...
            return null;

        }//RandomStar

    }//class Galaxy
}