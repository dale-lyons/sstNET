using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.QuadrantObjects
{
    public class PlanetList : List<QuadrantPlanet>
    {
        private class sortByID : IComparer<QuadrantPlanet>
        {
            int IComparer<QuadrantPlanet>.Compare(QuadrantPlanet x, QuadrantPlanet y)
            {
                if (x.ID > y.ID)
                    return +1;
                else if (x.ID < y.ID)
                    return -1;
                else
                    return 0;
            }//Compare
        }//class sortByID

        public PlanetList(Galaxy galaxy)
        {
            foreach (Quadrant quad in galaxy.Quadrants)
            {
                if (quad.Planet != null)
                    this.Add(quad.Planet);
            }//foreach
            //always return planets sorted by creation order
            this.Sort(new sortByID());
        }//PlanetList ctor

    }//class PlanetList
}