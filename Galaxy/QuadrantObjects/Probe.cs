using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.QuadrantObjects
{
    /// <summary>
    /// This class represents a deep space probe. The Enterprise
    /// carries around a number of them. (The Faerie Queen has none).
    /// Only a single probe can be in the galaxy at any one time.
    /// Once luanched from the Enterprise, the probe makes its way
    /// to its destination.
    /// The probe can optionally be armed which causes a super-nova to occur 
    /// in the destination quadrant(killing all occupants).
    /// </summary>
    public class Probe : QuadrantObject
    {
        /// <summary>
        /// This is the course and distance the probe will travel
        /// </summary>
        public GalacticCourse GalacticCourse { get; set; }

        /// <summary>
        /// The last quadrant the probe was in. This allows for messaging
        /// to track the probe as it crosses into new quadrants.
        /// </summary>
        public QuadrantCoordinate LastQuadrant { get; set; }

        /// <summary>
        /// Flag indicating if armed
        /// </summary>
        public bool Armed { get; set; }

        /// <summary>
        /// Default ctor for serialization
        /// </summary>
        public Probe() { }

        /// <summary>
        /// Create an instance of a probe given a location, direction and distance.
        /// Possibly armed.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="dist"></param>
        /// <param name="gc"></param>
        /// <param name="armed"></param>
        public Probe(GalacticCoordinate gc, double direction, double distance, bool armed)
        {
            GalacticCourse = new GalacticCourse(gc, direction, distance);
            Armed = armed;
            LastQuadrant = gc.QuadrantCoordinate;
            QuadrantCoordinate = gc.QuadrantCoordinate;
        }//Probe ctor

        /// <summary>
        /// Returns the number of moves left until probe reaches destination.
        /// (A move is 1 sector).
        /// </summary>
        public int Moves { get { return GalacticCourse.NumberSteps; } }

        /// <summary>
        /// Move the probe one distance unit. (1 sector).
        /// return value indicates if probe has changed quadrant from last move.
        /// </summary>
        /// <returns></returns>
        public bool Move()
        {
            //increment location by one sector
            GalacticCourse.Next();

            QuadrantCoordinate = GalacticCourse.CurrentCoordinate.QuadrantCoordinate;

            //check if changed quadrants. If not, return false
            if(GalacticCourse.CurrentCoordinate.SameQuadrant(LastQuadrant))
                return false;

            //probe has changed quadrants since last move, update last quad
            LastQuadrant = GalacticCourse.CurrentCoordinate.QuadrantCoordinate;

            return true;

        }//Move

        ///// <summary>
        ///// Return the current quadrant the probe is in.
        ///// </summary>
        //public override QuadrantCoordinate QuadrantCoordinate
        //{
        //    get { return GalacticCourse.CurrentCoordinate.QuadrantCoordinate; }
        //}

        /// <summary>
        /// Dump some debug
        /// </summary>
        public void dump()
        {
            if (!GameData.DEBUGME)
                return;

            Game.Console.WriteLine(string.Format("========= Probe ========="));
            Game.Console.WriteLine(string.Format("Armed:{0} X:{1} Y:{2}", Armed, QuadrantCoordinate.X, QuadrantCoordinate.Y));
            Game.Console.WriteLine(string.Format("========= Probe ========="));
        }

    }//class Probe
}