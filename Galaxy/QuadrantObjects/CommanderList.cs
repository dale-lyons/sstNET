using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.QuadrantObjects
{
    public class CommanderList : List<QuadrantCommander>
    {
        private static int nextTable = 1;

        //private class sortByQuadrant : IComparer<QuadrantCommander>
        //{
        //    private readonly QuadrantCoordinate mCompareQC;
        //    public sortByQuadrant(QuadrantCoordinate qc)
        //    {
        //        mCompareQC = qc;
        //    }
        //    int IComparer<QuadrantCommander>.Compare(QuadrantCommander x, QuadrantCommander y)
        //    {
        //        if (x.QuadrantCoordinate.DistanceTo(mCompareQC) > y.QuadrantCoordinate.DistanceTo(mCompareQC))
        //            return +1;
        //        else if (x.QuadrantCoordinate.DistanceTo(mCompareQC) < y.QuadrantCoordinate.DistanceTo(mCompareQC))
        //            return -1;
        //        else if (x.ID > y.ID)
        //            return +1;
        //        else if (x.ID < y.ID)
        //            return -1;
        //        else
        //            return 0;
        //    }//Compare
        //}//sortByQuadrant

        private class sortByID : IComparer<QuadrantCommander>
        {
            //public sortByID() { }
            int IComparer<QuadrantCommander>.Compare(QuadrantCommander x, QuadrantCommander y)
            {
                if (x.ID > y.ID)
                    return +1;
                else if (x.ID < y.ID)
                    return -1;
                else
                    return 0;
            }//Compare
        }//sortByID

        public CommanderList(Galaxy galaxy)
        {
            foreach (Quadrant quad in galaxy.Quadrants)
            {
                if (quad.Commander != null)
                    this.Add(quad.Commander);
            }//foreach
            this.Sort(new sortByID());
        }//CommanderList ctor

        public void dump()
        {
            if (!GameData.DEBUGME)
                return;

            int id = nextTable++;
            Game.Console.WriteLine("===Enemy Commanders Table:{0} ===", id);
            foreach (sstNET.Galaxy.QuadrantObjects.QuadrantCommander es in this)
            {
                Game.Console.WriteLine("{0,4}:{1,4},{2,4}", es.ID,
                    es.QuadrantCoordinate.X, es.QuadrantCoordinate.Y);

            }//foreach
            Game.Console.WriteLine("===Enemy Commanders Table:{0} ===", id);

            //Game.Console.WriteLine("Next Random:{0,2:F8}", game.Random.Peek());
            //if (game.Galaxy.SuperCommander != null)
            //{
            //    Game.Console.WriteLine("SuperCommander is at:{0}-{1}",
            //        game.Galaxy.SuperCommander.X,
            //        game.Galaxy.SuperCommander.Y);
            //}

        }//dumpcom

    }//class CommanderList
}