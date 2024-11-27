using System;
using System.Collections.Generic;
using System.Text;

using sstNET.Galaxy.SectorObjects.Ships;

namespace sstNET.Galaxy.SectorObjects.Ships
{
    public class EnemyShipList : List<EnemyShip>
    {
        /// <summary>
        /// An index value assigned to sector objects. Used for sorting.
        /// </summary>
        private static int mNextTable = 1;

        private class sortByID : IComparer<EnemyShip>
        {
            //public sortByID() { }
            int IComparer<EnemyShip>.Compare(EnemyShip x, EnemyShip y)
            {
                if (x.ID > y.ID)
                    return +1;
                else if (x.ID < y.ID)
                    return -1;
                else
                    return 0;
            }//Compare
        }//sortByID

        private class sortByDistance : IComparer<EnemyShip>
        {
            public int Compare(EnemyShip x, EnemyShip y)
            {
                if (x.Distance > y.Distance)
                    return +1;
                else if (x.Distance < y.Distance)
                    return -1;
                else if (x.ID < y.ID)
                    return -1;
                else if (x.ID > y.ID)
                    return +1;
                else
                    return 0;
            }
        }

        public EnemyShipList(SectorObject[,] sectors, bool sortByDistance)
        {
            foreach (SectorObject so in sectors)
            {
                //todo - check if tholians should be excluded
                if (so is Tholian)
                    continue;

                if (so is EnemyShip)
                {
                    this.Add(so as EnemyShip);
                }
            }//foreach

            if(sortByDistance)
                this.Sort(new sortByDistance());
            else
                this.Sort(new sortByID());

        }//EnemyShipList ctor

        public void Dump(GameData game)
        {
            if (!GameData.DEBUGME)
                return;

            int id = mNextTable++;

            Game.Console.WriteLine("===Enemy Table:{0} ===", id);
            int ii = 1;
            foreach (EnemyShip es in this)
            {
                Game.Console.WriteLine("{0,4}:{1,4},{2,4}:{3,8:F2},{4,8:F2},{5,8:F2}", ii, es.Sector.X, es.Sector.Y, es.Distance, es.AverageDistance, es.Power);
                ii++;

            }//foreach
            Game.Console.WriteLine("===Enemy Table:{0} ===", id);
            Game.Console.WriteLine("Next Random:{0,2:F8}", game.Random.Peek());

            if (game.Galaxy.SuperCommander != null)
            {
                Game.Console.WriteLine("SuperCommander is at:{0}-{1}",
                    game.Galaxy.SuperCommander.X,
                    game.Galaxy.SuperCommander.Y);
            }

            QuadrantCoordinate thingQC = game.Galaxy.Thing;
            if (thingQC != null)
            {
                Game.Console.WriteLine("Thing is at X:{0,2},Y:{1,2}", thingQC.X, thingQC.Y);
            }

        }//Dump

    }//class EnemyShipList
}