using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects.Ships
{
    /// <summary>
    /// This class represents the base of all Enemy ships in the game.
    /// Each Enemy ship has certain common properties such as power, distance from
    /// friendly ship etc.
    /// The powerful flag is used to distinguish between ordinary klingons and more powerful
    /// ones such as Commanders and SuperCommanders and Romulans.
    /// </summary>
    public abstract class EnemyShip : SectorObject
    {
        /// <summary>
        /// Amount of power this enemy ship has
        /// </summary>
        public double Power { get; set; }

        /// <summary>
        /// Distance this ship is from the friendly ship(sectors)
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// The Average distance this ship is from the friendly ship(sectors)
        /// </summary>
        public double AverageDistance { get; set; }

        /// <summary>
        /// Flag indicating if this is a more powerful enemy ship(Commander,SuperCommander or Romulan)
        /// </summary>
        protected readonly bool mPowerful;

        public EnemyShip(bool powerful) 
            : base()
        {
            mPowerful = powerful;
        }

        public override bool Ramable { get { return true; } }
        public abstract double RamDamageFactor { get; }

        //public void CalculateDistance222(SectorCoordinate sc)
        //{
        //    Distance = sc.DistanceTo(this.Sector);
        //}//CalculateDistance

        /// <summary>
        /// Compute distance from this ship to friendly ship and save as distance
        /// </summary>
        /// <param name="sc"></param>
        public void CalculateDistance(double distance)
        {
            Distance = distance;
        }//CalculateDistance

        /// <summary>
        /// Reset the average distance by making it equal to the current distance
        /// </summary>
        public void ResetAverageDistance()
        {
            AverageDistance = Distance;
        }//ResetAverageDistance

        //public void SetNewDistance(SectorCoordinate sc)
        //{
        //    double dist = sc.DistanceTo(this.Sector);
        //    AverageDistance = 0.5 * (dist + Distance);
        //    //_dist = dist;
        //}//ResetDistance

        /// <summary>
        /// Compute a new average distance by computing new distance and averaging with
        /// original distance
        /// </summary>
        /// <param name="sc"></param>
        public void SetNewDistance(double distance)
        {
            //double dist = sc.DistanceTo(this.Sector);
            AverageDistance = 0.5 * (distance + Distance);
            //_dist = dist;
        }//ResetDistance

        /// <summary>
        /// Handle being hit by a torpedo
        /// </summary>
        /// <param name="game">Game object</param>
        /// <param name="sc">Sector coordinate where torpedo came from</param>
        /// <param name="bullseye">Exact analge from source to target</param>
        /// <param name="angle">Tainted angle, or error angle</param>
        /// <returns>Energy hit sustained</returns>
        public override double TorpedoHit(GameData game, SectorCoordinate sc, double bullseye, double angle)
        {
            //get energy level of this ship
            double kp = Math.Abs(this.Power);

            //compute a hit value based on distance and angle difference plus a little random
            double hitValue = 700.0 + 100.0 * game.Random.Rand() -
                 1000.0 * this.Sector.DistanceTo(sc) *
                 Math.Abs(Math.Sin(bullseye - angle));

            //make it positive and not larger than available energy
            hitValue = Math.Min(Math.Abs(hitValue), kp);

            //decrement energy level of this ship
            this.Power -= (this.Power < 0 ? -hitValue : hitValue);

            //check for dead enemy
            if (this.Power == 0)
            {
                killShip(game);
                return 0;
            }//if

            Game.Console.crmena(true, this, true, this.Sector);

            //If enemy damaged but not destroyed, try to displace
            double ang = angle + 2.5 * (game.Random.Rand() - 0.5);
            double temp = Math.Abs(Math.Sin(ang));
            if (Math.Abs(Math.Cos(ang)) > temp)
                temp = Math.Abs(Math.Cos(ang));
            double xx = -Math.Sin(ang) / temp;
            double yy = Math.Cos(ang) / temp;

            //compute a new SectorCoordinate
            SectorCoordinate jxy = new SectorCoordinate((int)(this.Sector.X + xx + 0.5), (int)(this.Sector.Y + yy + 0.5));

            //if its not valid(outside quadrant) then we are done here
            if (!jxy.Valid)
            {
                Game.Console.WriteLine(" damaged but not destroyed.");
                return 0;
            }//if

            //if we are pushed into a black-hole then we are toast
            if (game.Galaxy.CurrentQuadrant[jxy] is BlackHole)
            {
                Game.Console.WriteLine(" buffeted into black hole.");
                this.killShip(game, jxy);
                return 0;
            }//if

            //if not an empty sector then we are blocked, we are done here
            if (!(game.Galaxy.CurrentQuadrant[jxy] is Empty))
            {//can't move into object
                Game.Console.WriteLine(" damaged but not destroyed.");
                return 0;
            }//if

            //Enemy ship was shoved ... remove from orginal sector
            game.Galaxy.CurrentQuadrant[this.Sector] = new Empty();

            //and move into new displaced sector
            game.Galaxy.CurrentQuadrant[jxy] = this;

            //print damaged message
            Game.Console.WriteLine(" damaged--");
            Game.Console.WriteLine(" displaced by blast to{0}", jxy.ToString(true));

            //compute new distance from friendly ship and reset the average distance
            game.Galaxy.CurrentQuadrant.CalculateDistances(game.Galaxy.Ship.Sector);
            game.Galaxy.CurrentQuadrant.ResetAverageDistances();

            return 0;
        }//TorpedoHit

        /// <summary>
        /// Kill this enemy ship. The extra position parameter is required for reporting of where
        /// this enemy ship was before ramming the ship.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="originalPosition">Position of this ship when it decided to ram ship</param>
        public void killShip(GameData game, SectorCoordinate originalPosition)
        {
            Game.Console.crmena(true, this, true, originalPosition);
            deadkl(game, this.Sector);

            Game.Console.WriteLine(" destroyed.");
            game.Galaxy.CurrentQuadrant[this.Sector] = new Empty();

            if (game.Galaxy.Klingons == 0)
                return;

            game.RemainingTime = game.RemainingResources / (game.Galaxy.Klingons + 4 * game.Galaxy.Commanders.Count);
            //    //todo - check dist and average dist

        }//killShip

        /// <summary>
        /// Overloaded version of killship. This is called when the original and final position of this
        /// ship is the same. This is almost always the case except when enemy ships ram the ship.
        /// </summary>
        /// <param name="game"></param>
        public void killShip(GameData game)
        {
            this.killShip(game, this.Sector);
        }

        /// <summary>
        /// This function needs to be implemented by the different types of enemy ships.
        /// They all die slightly differently ...
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        public abstract void deadkl(GameData game, SectorCoordinate sc);

        public override bool Ram(GameData game, SectorCoordinate previous, out SectorCoordinate final, out double distSoFar)
        {
            //game.Galaxy.Ship.Sector = new SectorCoordinate(this.Sector);
            Battle.Ram(game, false, this);
            final = this.Sector;
            distSoFar = 0.1 * this.Sector.DistanceTo(game.Galaxy.Ship.Sector);
            return false;
        }

        /// <summary>
        /// This enemy ship was caught in a nova explosion.
        /// For non-powerful ships (ordinary klingons) death is immediate. Otherwise for all other
        /// ships(assumed to be powerful) if it has enough power to absorb the nova it may survive.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        /// <param name="gameOver"></param>
        /// <returns></returns>
        public override bool Nova(GameData game, SectorCoordinate sc, out bool gameOver)
        {
            gameOver = false;

            //if ordinary non-powerful destroy immediately
            if (!mPowerful)
            {
                this.killShip(game);
                return false;
            }//if

            //otherwise see if it can handle this hit
            this.Power -= 800.0;//If firepower is lost, die
            if (this.Power <= 0.0)
            {//nope, die anyways
                this.killShip(game);
                return false;
            }//if

            //Enemy ship is buffeted, compute new location of ship
            SectorCoordinate newcxy = new SectorCoordinate(this.Sector.X * 2 - sc.X, this.Sector.Y * 2 - sc.Y);

            Game.Console.crmena(true, this, true, this.Sector);
            Game.Console.Write(" damaged");

            if (!newcxy.Valid)
            {//can't leave quadrant
                Game.Console.Skip(1);
                return false;
            }//if

            //get contents of new sector and check what is there
            SectorObject so = game.Galaxy.CurrentQuadrant[newcxy];

            //if buffetted into a black-hole then bad-luck
            if (so is BlackHole)
            {
                Game.Console.Write(", blasted into ");
                Game.Console.crmena(false, so, true, newcxy);
                Game.Console.Skip(1);
                this.deadkl(game, newcxy);
                return false;
            }//if

            if (!(so is Empty))
            {//can't move into something else
                Game.Console.Skip(1);
                return false;
            }//else if

            //print some info about move
            Game.Console.Write(", buffeted to{0}", newcxy.ToString(true));

            //remove this ship from its current location
            game.Galaxy.CurrentQuadrant[this.Sector] = new Empty();

            //and move into its new location
            game.Galaxy.CurrentQuadrant[newcxy] = this;

            //compute new distance from friendly ship and reset the average distance
            this.CalculateDistance(game.Galaxy.Ship.Sector.DistanceTo(this.Sector));
            this.ResetAverageDistance();
            Game.Console.Skip(1);

            return false;
        }//Nova

    }//class EnemyShip
}