using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy
{
    /// <summary>
    /// Represents an absolute start position (GalacticCoordinate) and a direction in 2D space.
    /// Methods allow caller to project a certain distance, or step 1 sector at a time
    /// in the given direction.
    /// A typical use of this would be to track a torpedo thru the quadrant, or to track
    /// a deep space probe thru the galaxy.
    /// 
    /// A Direction represents a direction through space.
    /// Uses a compass type number to describe direction where 0 is up,
    /// 3.0 is right(East), 6.0 is down(South), 9.0 is left(West) and 12.0 is up(North)
    /// 0.0 == 12.0
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Start={StartCoordinate.X},{StartCoordinate.Y}")]
    public class GalacticCourse
    {
        public GalacticCoordinate StartCoordinate { get; set; }

        /// <summary>
        /// Direction of this course using compass numbers (0.0 - 12.0)
        /// </summary>
        private double mDirection;
        public double Direction
        {
            get { return mDirection; }
            set
            {
                mDirection = NormalizeDirection(value);
                double angle = DirectionToRadians(mDirection);

                mDeltax = -Math.Sin(angle);
                mDeltay = Math.Cos(angle);
                mBigger = Math.Max(Math.Abs(mDeltax), Math.Abs(mDeltay));

                mDeltax /= mBigger;
                mDeltay /= mBigger;
            }
        }

        public int NumberSteps { get; set; }

        private double mDeltax;
        private double mDeltay;
        private double mBigger;

        public double CurrentX { get; set; }
        public double CurrentY { get; set; }

        public double Distance { get; set; }

        public GalacticCourse() { }

        public GalacticCourse(GalacticCoordinate gc, double direction)
        {
            StartCoordinate = gc;
            Direction = direction;

            CurrentX = gc.X;
            CurrentY = gc.Y;
            NumberSteps = -1;
            Distance = -1;
        }

        public GalacticCourse(GalacticCoordinate gc, double direction, double distance)
            : this(gc, direction)
        {
            Distance = distance;
            NumberSteps = (int)(10.0 * distance * mBigger + 0.5);
        }

        //public double Distance { get { return mDistance; } }
        public double CurrentSectorX
        {
            get { return ((CurrentX - 1) % 10) + 1; }
        }

        public double CurrentSectorY
        {
            get { return ((CurrentY - 1) % 10) + 1; }
        }

        public bool SameQuadrant
        {
            get { return StartCoordinate.QuadrantCoordinate.Equals(CurrentCoordinate.QuadrantCoordinate); }
        }

        public SectorCoordinate CurrentSectorCoordinate
        {
            get { return CurrentCoordinate.Sector; }
        }

        /// <summary>
        /// Compute a random direction.
        /// Value:0.0 - 12.0
        /// </summary>
        /// <param name="rand">Random number generator</param>
        /// <returns></returns>
        public static double RandomDirection(Random rand)
        {
            return (12.0 * rand.Rand());
        }

        public GalacticCoordinate CurrentCoordinate
        {
            get
            {
                int xpos = (int)(CurrentX + 0.5);
                int ypos = (int)(CurrentY + 0.5);
                return new GalacticCoordinate(xpos, ypos);
            }
        }

        /// <summary>
        /// Step to the next sector of the course.
        /// If a distance was specified, then the return value indicates if it has reached that
        /// distance. If no distance was specified, then will always return true.
        /// </summary>
        /// <returns></returns>
        public bool Next()
        {
            CurrentX += mDeltax;
            CurrentY += mDeltay;

            if (Distance > 0)
            {
                return (--NumberSteps >= 0);
            }//if
            else
            {
                return true;
            }//else

        }//Next

        /// <summary>
        /// Ensure that the direction is >= 0.0 and direction is <= 12.0
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static double NormalizeDirection(double direction)
        {
            while (direction < 0.0)
                direction += 12.0;
            while (direction > 12.0)
                direction -= 12.0;

            return direction;
            //return ((direction < 0.0) ? (direction + 12.0) : direction);
        }//NormalizeDirection

        /// <summary>
        /// Convert the given compass course (0.0 - 12.0) into a radian angle.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static double DirectionToRadians(double direction)
        {
            return (15.0 - NormalizeDirection(direction)) * 0.5235988;
        }//DirectionToAngle

    }
}