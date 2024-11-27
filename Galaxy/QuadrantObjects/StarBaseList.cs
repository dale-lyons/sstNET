using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.QuadrantObjects
{
    /// <summary>
    /// Provides a list of Quadrant starbases sorted by creation index or sorted by
    /// distance from a given quadrant. Overloaed constructors specify which to use.
    /// This is required to keep logic with the original code in sync. Starbases sometimes
    /// need to be processed in the same order they were created. Sometimes they are required
    /// to be sorted by distance from the ship. (ie call for help)
    /// </summary>
    public class StarBaseList : List<QuadrantStarBase>
    {
        /// <summary>
        /// This class provides the sorting logic for distance from a given quadrant.
        /// If 2 starbases are exactly the same distance away, then select the one with the
        /// lowest creation index.
        /// </summary>
        private class sortByQuadrant : IComparer<QuadrantStarBase>
        {
            private readonly QuadrantCoordinate _compareQC;
            public sortByQuadrant(QuadrantCoordinate qc)
            {
                _compareQC = qc;
            }
            int IComparer<QuadrantStarBase>.Compare(QuadrantStarBase x, QuadrantStarBase y)
            {
                if (x.QuadrantCoordinate.DistanceTo(_compareQC) > y.QuadrantCoordinate.DistanceTo(_compareQC))
                    return +1;
                else if (x.QuadrantCoordinate.DistanceTo(_compareQC) < y.QuadrantCoordinate.DistanceTo(_compareQC))
                    return -1;
                else if (x.ID > y.ID)
                    return +1;
                else if (x.ID < y.ID)
                    return -1;
                else
                    return 0;
            }//Compare
        }//sortByQuadrant

        /// <summary>
        /// This class provides the sorting logic based on creation index. Starbases with the smallest
        /// creation index are first.
        /// </summary>
        private class sortByID : IComparer<QuadrantStarBase>
        {
            public sortByID(){}
            int IComparer<QuadrantStarBase>.Compare(QuadrantStarBase x, QuadrantStarBase y)
            {
                if (x.ID > y.ID)
                    return +1;
                else if (x.ID < y.ID)
                    return -1;
                else
                    return 0;
            }//Compare
        }//sortByID

        /// <summary>
        /// Create a StarbaseList sorted by creation index. Starbases with the lower creation index(oldest)
        /// are first.
        /// </summary>
        /// <param name="galaxy">The galaxy</param>
        public StarBaseList(Galaxy galaxy)
        {
            //build the unsorted starbase list
            BuildStarbaseList(galaxy);
            //return bases sorted by creation order
            this.Sort(new sortByID());
        }//StarBaseList ctor

        /// <summary>
        /// Create a StarbaseList sorted by distance from a given quadrant.
        /// If 2 starbases are the same distance, the one with the lower creation
        /// index is first.
        /// </summary>
        /// <param name="galaxy">The galaxy</param>
        /// <param name="qc">Quadrant to measure distance from</param>
        public StarBaseList(Galaxy galaxy, QuadrantCoordinate qc)
        {
            //build the unsorted starbase list
            BuildStarbaseList(galaxy);
            //sort starbases by distance from the given quadrant
            this.Sort(new sortByQuadrant(qc));
        }//StarBaseList ctor

        /// <summary>
        /// Build an unsorted starbase list from the galaxy. Simply enumerate all the starbases
        /// and add to the list.
        /// </summary>
        /// <param name="galaxy">The galaxy</param>
        private void BuildStarbaseList(Galaxy galaxy)
        {
            foreach (Quadrant quad in galaxy.Quadrants)
            {
                if (quad.Base != null)
                    this.Add(quad.Base);
            }//foreach
        }//BuildStarbaseList

        /// <summary>
        /// Select a randomly chosen starbase. Simply select one of the
        /// starbases loaded into the current list.
        /// </summary>
        /// <param name="rand">Random number generator to use</param>
        /// <returns>The selected starbase</returns>
        public QuadrantStarBase RandomStarbase(Random rand)
        {
            return this[(int)(rand.Rand() * this.Count)];
        }//RandomStarbase

        public void Dump()
        {
            if (!GameData.DEBUGME)
                return;

            Game.Console.WriteLine(string.Format("========= Starbases ========="));
            foreach (QuadrantStarBase sb in this)
            {
                Game.Console.WriteLine(string.Format("ID:{0} X:{1} Y:{2}", sb.ID, sb.QuadrantCoordinate.X, sb.QuadrantCoordinate.Y));
            }
            Game.Console.WriteLine(string.Format("========= Starbases ========="));
        }

    }//class StarBaseList
}