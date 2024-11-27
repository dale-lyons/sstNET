using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace sstNET.Galaxy
{
    /// <summary>
    /// This class represents an absolute coordinate position in the galaxy.
    /// It is expressed in Sectors where there are 10 horizontal and 10 vertical
    /// sectors per quadrant. Since the galaxy is an 8x8 quadrant setup, the galactic
    /// coordinates range from 1-80 in both the x and y axis.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("X={X} Y={Y}")]
    public class GalacticCoordinate
    {
        //private int mXpos;
        //private int mYpos;
        public int X { get; set; }
        public int Y { get; set; }

        public GalacticCoordinate() { }

        /// <summary>
        /// Form a galactic coordinate from an absolute x,y position
        /// </summary>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        public GalacticCoordinate(int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
        }//GalacticCoordinate ctor

        public GalacticCoordinate(SectorCoordinate sc) :
            this(sc.X, sc.Y)
        {
        }//GalacticCoordinate ctor

        /// <summary>
        /// Construct a GalacticCoordinate coordinate from another GalacticCoordinate coordinate.
        /// </summary>
        /// <param name="gc"></param>
        public GalacticCoordinate(GalacticCoordinate gc) :
            this(gc.X, gc.Y)
        {
        }//GalacticCoordinate ctor

        /// <summary>
        /// Construct a GalacticCoordinate from a Quadrant coordinate.
        /// The sector is assumed to be 1,1
        /// </summary>
        /// <param name="qc"></param>
        public GalacticCoordinate(QuadrantCoordinate qc)
        {
            X = ((qc.X - 1) * 10) + 1;
            Y = ((qc.Y - 1) * 10) + 1;
        }//GalacticCoordinate ctor

        /// <summary>
        /// Form a galactic coordinate from both a quadrant and sector position
        /// </summary>
        /// <param name="qc"></param>
        /// <param name="sc"></param>
        public GalacticCoordinate(QuadrantCoordinate qc, SectorCoordinate sc) 
            : this(qc)
        {
            X += (sc.X - 1);
            Y += (sc.Y - 1);
        }//GalacticCoordinate ctor

        public double DistanceTo(GalacticCoordinate gc)
        {
            int dx = Math.Abs(gc.X - X);
            int dy = Math.Abs(gc.Y - Y);
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        /// <summary>
        /// Return a flag indicating if this is a valid Galactic coordinate. It is valid if the
        /// coordinate is with the bounds of the galaxy.
        /// (1-80) in both x and y axis.
        /// </summary>
        public bool Valid
        {
            get { return (X >= 1 && X <= 80 && Y >= 1 && Y <= 80); }
        }//Valid

        /// <summary>
        /// Return the sector portion of a galactic coordinate.
        /// </summary>
        public SectorCoordinate Sector
        {
            get
            {
                int xpos = X < 1 ? 0 : ((X - 1) % 10) + 1;
                int ypos = Y < 1 ? 0 : ((Y - 1) % 10) + 1;
                return new SectorCoordinate(xpos, ypos);
            }
        }

        /// <summary>
        /// Return the quadrant portion of a galactic coordinate
        /// </summary>
        public QuadrantCoordinate QuadrantCoordinate
        {
            get
            {
                int xpos = X < 1 ? 0 : ((X - 1) / 10) + 1;
                int ypos = Y < 1 ? 0 : ((Y - 1) / 10) + 1;
                return new QuadrantCoordinate(xpos, ypos);
            }
        }

        /// <summary>
        /// From the current galactic coordinate, project out a specified course and direction
        /// and compute a new galactic coordinate. Note that the returned coordinate may be outside
        /// the galaxy. (check Valid flag)
        /// </summary>
        /// <param name="course"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public GalacticCoordinate Project(double direction, double distance)
        {
            double angle = GalacticCourse.DirectionToRadians(direction);
            double deltax = -Math.Sin(angle);
            double deltay = Math.Cos(angle);
            double bigger = Math.Max(Math.Abs(deltax), Math.Abs(deltay));

            deltax /= bigger;
            deltay /= bigger;

            int ix = (int)(X + 10.0 * distance * bigger * deltax + 0.5);
            int iy = (int)(Y + 10.0 * distance * bigger * deltay + 0.5);

            return new GalacticCoordinate(ix, iy);
        }

        /// <summary>
        /// This code was pulled from the original code. It checks if the current galactic coordinate
        /// is within the bounds of the galaxy. If not it "fixes" it by making some weird subtraction
        /// that appears to mirror the extent outside of the galaxy.The return flag indicates if a fixup
        /// was required.
        /// </summary>
        /// <returns>True if fixup was required</returns>
        public bool Fixup()
        {
            bool fixedup = false;
            bool kink;
            do
            {
                kink = false;
                if (X <= 0)
                {
                    X = -X + 1;
                    kink = true;
                }//if
                if (Y <= 0)
                {
                    Y = -Y + 1;
                    kink = true;
                }//if
                if (X > 80)
                {
                    X = 161 - X;
                    kink = true;
                }//if
                if (Y > 80)
                {
                    Y = 161 - Y;
                    kink = true;
                }//if
                if (kink)
                    fixedup = true;

            } while (kink);

            return fixedup;
        }//Fixup

        public bool SameQuadrant(QuadrantCoordinate qc)
        {
            if (qc == null)
                return false;

            return (this.QuadrantCoordinate.X == qc.X && this.QuadrantCoordinate.Y == qc.Y);
        }

    }//class GalacticCoordinate
}
