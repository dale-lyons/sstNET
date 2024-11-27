using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.QuadrantObjects
{
    /// <summary>
    /// This class represents an object in a Quadrant. Any kind of object that can live
    /// outside of the current quadrant derives from this class. Examples are Commanders,
    /// the Super-Command, planets etc.
    /// The nextID is maintained to allow for sorting of quadrant objects based on their
    /// creation order. This was done mainly to stay syncronized with the original code
    /// which maintained arrays sorted by creation order.
    /// </summary>
    public abstract class QuadrantObject
    {
        /// <summary>
        /// The next id to assign newly created quadrant objects
        /// </summary>
        //[System.Xml.Serialization.XmlIgnoreAttribute]
        private static int _nextID = 1;

        /// <summary>
        /// Return the id of this object
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The quadrant coordinate of this object. It is assumed that once created quadrant objects
        /// don't change quadrants. This is not true of the Super-Commander. If the quadrant object
        /// must move, then it must be destroyed and re-created.
        /// </summary>
        public QuadrantCoordinate QuadrantCoordinate  { get; set; }

        public QuadrantObject() { }

        /// <summary>
        /// Construct a Quadrant object
        /// </summary>
        /// <param name="qc">Quadrant coordinate of this object</param>
        public QuadrantObject(QuadrantCoordinate qc)
        {
            this.QuadrantCoordinate = qc;

            //assign the next available id
            ID = _nextID++;
        }

    }//class QuadrantObject
}