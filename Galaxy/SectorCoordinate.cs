using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy
{
    /// <summary>
    /// Provides a one-based coordinate system for the current quadrant. X,Y coordinates are
    /// in sectors ranging in value from 1-10. Top left is coordinate 1,1, bottom right is 10,10
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Location={ToString(false).Trim()}")]
    public class SectorCoordinate
    {
        //Some helpful coordinates inside a quadrant
        public static SectorCoordinate UpperLeft = new SectorCoordinate(1, 1);
        public static SectorCoordinate UpperRight = new SectorCoordinate(1, 10);
        public static SectorCoordinate LowerLeft = new SectorCoordinate(10, 1);
        public static SectorCoordinate LowerRight = new SectorCoordinate(10, 10);
        public static SectorCoordinate Middle = new SectorCoordinate(5, 5);

        public SectorCoordinate() { }

        /// <summary>
        /// Construct a SectorCoordinate from an x,y location
        /// Note that invalid values are allowed.
        /// </summary>
        /// <param name="ix">X position(1-10)</param>
        /// <param name="iy">Y position(1-10)</param>
        public SectorCoordinate(int ix, int iy)
        {
            X = ix;
            Y = iy;
        }

        /// <summary>
        /// Construct a SectorCoordinate from another SectorCoordinate
        /// </summary>
        /// <param name="sc">SectorCoordinate to copy from</param>
        public SectorCoordinate(SectorCoordinate sc)
            : this(sc.X, sc.Y)
        {
        }

        /// <summary>
        /// The current x position. (1-10)
        /// Note that this value is allowed to go out of range.
        /// The Valid property returns flag indicating within range values for both x and y
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The current y position. (1-10)
        /// Note that this value is allowed to go out of range.
        /// The Valid property returns flag indicating within range values for both x and y
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Determine equality of SectorCoordinates.
        /// Simply the result of comparing the x and y values.
        /// </summary>
        /// <param name="obj">SectorCoordinate to test against</param>
        /// <returns>True if same</returns>
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
                return false;

            // If parameter cannot be cast to SectorCoordinate return false.
            if ((obj as SectorCoordinate) == null)
                return false;

            //if both x and y values are same, then coordinates are the same.
            return (((obj as SectorCoordinate).X == this.X) && ((obj as SectorCoordinate).Y == this.Y));
        }

        public double CourseTo(SectorCoordinate sc)
        {
            double deltax = (0.1 * (sc.Y - this.Y));
            double deltay = (0.1 * (this.X - sc.X));
            double course = 1.90985932 * Math.Atan2(deltax, deltay);
            return course;
        }

        public override int GetHashCode()
        {
            return X ^ Y;
        }

        /// <summary>
        /// Compute the distance from this sector to a given sector.
        /// </summary>
        /// <param name="sc">Sector to compute distance to</param>
        /// <returns>Distance in sectors</returns>
        public double DistanceTo(SectorCoordinate sc)
        {
            double dx = Math.Abs(sc.X - this.X);
            double dy = Math.Abs(sc.Y - this.Y);
            return Math.Sqrt(dx * dx + dy * dy);
        }//DistanceTo

        /// <summary>
        /// Creates a random SectorCoordinate
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        static public SectorCoordinate Random(Random rand)
        {
            int ix, iy;
            rand.iran10(out ix, out iy);
            return new SectorCoordinate(ix, iy);
        }

        /// <summary>
        /// Creates a SectorCoordinate that is 1 of 9 random locations around a
        /// given SectorCoordinate including the centre sector
        /// ie: 1,2,3 and 4,5,6 and 7,8,9 are all valid locations surrounding sector 5
        /// . . .
        /// 1 2 3
        /// 4 5 6
        /// 7 8 9
        /// . . .
        /// </summary>
        /// <param name="rand">Random number generator</param>
        /// <returns>A random SectorCoordinate around this one</returns>
        public SectorCoordinate RandomAround(Random rand)
        {
            int ix = (int)(this.X + 3.0 * rand.Rand() - 1);
            int iy = (int)(this.Y + 3.0 * rand.Rand() - 1);
            return new SectorCoordinate(ix, iy);
        }

        /// <summary>
        /// Create a SectorCoordinate at one of the 4 corners of a quad randomly.
        /// </summary>
        /// <param name="rand">Random number generator</param>
        /// <returns>A random SectorCoordinate at one of 4 corners</returns>
        static public SectorCoordinate RandomCorner(Random rand)
        {
            int ix = rand.Rand() > 0.5 ? 10 : 1;
            int iy = rand.Rand() > 0.5 ? 10 : 1;
            return new SectorCoordinate(ix, iy);
        }

        /// <summary>
        /// Generate a string describing this sector coordinate.
        /// Optionally generate a label.
        /// </summary>
        /// <param name="label">True if sector label desired</param>
        /// <returns></returns>
        public string ToString(bool label)
        {
            return string.Format("{0} {1,1} - {2,1}", label ? " Sector" : "", this.X, this.Y);
        }//ToString

        public bool Valid { get { return (this.X >= 1 && this.X <= 10 && this.Y >= 1 && this.Y <= 10); } }

        /// <summary>
        /// Determines if a given sector is adjacent to this one
        /// </summary>
        /// <param name="sc">SectorCoordinate to check against</param>
        /// <returns>True if adjacent</returns>
        public bool AdjacentTo(SectorCoordinate sc)
        {
            if (sc == null)
                return false;

            return ((Math.Abs(this.X - sc.X) <= 1) && (Math.Abs(this.Y - sc.Y) <= 1));
        }

        /// <summary>
        /// Return a list of all Edge sectors in a quadrant. Note that the order is not
        /// specified, in fact this is implemented by the top row first, followed by the bottom
        /// row followed by the left side then right side.
        /// Note that no 2 coordinates are the same.
        /// This list will contain exactly 36 sectors
        /// </summary>
        public static List<SectorCoordinate> EdgeSectors
        {
            get
            {
                List<SectorCoordinate> ret = new List<SectorCoordinate>();
                for (int ii = 1; ii <= 10; ii++)
                {
                    ret.Add(new SectorCoordinate(1, ii));
                    ret.Add(new SectorCoordinate(10, ii));
                }
                for (int ii = 2; ii <= 9; ii++)
                {
                    ret.Add(new SectorCoordinate(ii, 1));
                    ret.Add(new SectorCoordinate(ii, 10));
                }
                System.Diagnostics.Debug.Assert(ret.Count == 36);
                return ret;
            }//get
        }//EdgeSectors

        /// <summary>
        /// Returns the 8 sectors around a given sector. This includes sectors outside of the quadrent
        /// (Invalid sectors) if the given sector is on the edge of the quadrant.
        /// </summary>
        public List<SectorCoordinate> AdjacentSectors
        {
            get
            {
                List<SectorCoordinate> ret = new List<SectorCoordinate>();
                for (int xx = -1; xx <= 1; xx++)
                {
                    for (int yy = -1; yy <= 1; yy++)
                    {
                        if (xx == 0 && yy == 0) continue;
                        ret.Add(new SectorCoordinate(this.X + xx, this.Y + yy));
                    }//for yy
                }//for xx
                return ret;
            }
        }//AdjacentSectors

        /// <summary>
        /// Returns the next corner coordinate assuming a clockwise movement.
        /// </summary>
        /// <returns></returns>
        public SectorCoordinate ClockwiseMove()
        {
            if (this.Equals(SectorCoordinate.UpperLeft))
                return SectorCoordinate.UpperRight;
            else if (this.Equals(SectorCoordinate.UpperRight))
                return SectorCoordinate.LowerRight;
            else if (this.Equals(SectorCoordinate.LowerRight))
                return SectorCoordinate.LowerLeft;
            else if (this.Equals(SectorCoordinate.LowerLeft))
                return SectorCoordinate.UpperLeft;
            else
                return null;
        }//ClockwiseMove

    }//class SectorCoordinate
}