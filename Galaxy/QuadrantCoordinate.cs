using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy
{
    [System.Diagnostics.DebuggerDisplay("Location={ToString(false).Trim()}")]
    public class QuadrantCoordinate
    {
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Default ctor for serialization only.
        /// </summary>
        public QuadrantCoordinate() { }

        public QuadrantCoordinate(int ix, int iy)
        {
            X = ix;
            Y = iy;
        }
        public QuadrantCoordinate(QuadrantCoordinate qc)
        {
            X = qc.X;
            Y = qc.Y;
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
                return false;

            // If parameter cannot be cast to QuadrantCoordinate return false.
            if ((obj as QuadrantCoordinate) == null)
                return false;

            return ( ((obj as QuadrantCoordinate).X == X) && ((obj as QuadrantCoordinate).Y == Y));
        }

        public override int GetHashCode()
        {
            return X ^ Y;
        }

        public double DistanceTo(QuadrantCoordinate qc)
        {
            double dx = Math.Abs(qc.X - X);
            double dy = Math.Abs(qc.Y - Y);
            double dist = Math.Sqrt(dx * dx + dy * dy);
            return dist;
        }

        static public QuadrantCoordinate Random(Random rand)
        {
            int ix, iy;
            rand.iran8(out ix, out iy);
            return new QuadrantCoordinate(ix, iy);
        }

        public bool Valid { get { return ((X >= 1) && (X <= 8) && (Y >= 1) && (Y <= 8)); } }

        public string ToString(bool label)
        {
            return string.Format("{0} {1,1} - {2,1}", label ? " Quadrant" : "", X, Y);
        }
    }//class QuadrantCoordinate
}