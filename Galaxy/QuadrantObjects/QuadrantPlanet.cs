using System;
using System.Collections.Generic;
using System.Text;

using sstNET.Galaxy.SectorObjects;

namespace sstNET.Galaxy.QuadrantObjects
{
    /// <summary>
    /// This class represents a single Planet in the galaxy. Each planet is randomly
    /// created when the galaxy is created. The state of the planet is kept intact
    /// throughout the game. The only property that changes during the game is the
    /// known property, where the planet is Unknown until it is discovered, then changes
    /// to known. Otherwise the other properties are constant.
    /// Added - GalileoPresent property tracks if the shuttle is present.
    /// </summary>
    public class QuadrantPlanet : QuadrantObject
    {
        /// <summary>
        /// These are the 3 classes a planet can be and their ascii representation
        /// </summary>
        private static char[] classes = new char[3] { 'M', 'N', 'O' };

        /// <summary>
        /// The class of the planet in integer form
        /// </summary>
        public int Class { get; set; }

        /// <summary>
        /// Indicates if this planet has dylithium crystals.
        /// </summary>
        public bool Crystals { get; set; }

        /// <summary>
        /// This flag indciates if the planet has been discovered yet.
        /// All planets are initially not discovered.
        /// </summary>
        public bool Known { get; set; }

        /// <summary>
        /// Indicates if the shuttle is present on the planet.
        /// It is possible that the shuttle is abandoned on the planet
        /// and it is possible to pick it up later.
        /// </summary>
        public bool GalileoPresent { get; set; }

        /// <summary>
        /// Default ctor required for serialization.
        /// </summary>
        public QuadrantPlanet() { } 

        /// <summary>
        /// Construct a new planet. Use the given random number generator to determine the
        /// initial properties of the planet.
        /// </summary>
        /// <param name="rand">Random number generator to use</param>
        /// <param name="qc">Quadrant of new planet</param>
        public QuadrantPlanet(Random rand, QuadrantCoordinate qc) 
            : base(qc)
        {
            Class = (int)(rand.Rand() * 3.0);           // Planet class 0,1 or 2 (M,N or O)
            Crystals = ((int)(1.5 * rand.Rand()) != 0);	// 1 in 3 chance of crystals
        }//QuadrantPlanet ctor

        /// <summary>
        /// Obtain the planet class in character form.
        /// </summary>
        public char ToChar { get { return classes[Class]; } }

    }//class QuadrantPlanet
}