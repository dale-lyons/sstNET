using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects
{
    /// <summary>
    /// Represents a Star in the Current Quadrant.
    /// </summary>
    public class Star : SectorObject
    {
        public Star() 
            : base() { }

        public override bool Ramable { get { return false; } }
        public override char Symbol { get { return '*'; } }
        public override string Name { get { return "Star"; } }

        /// <summary>
        /// Star has been hit by a torpedo, determine what to do.
        /// A small chance that there is no affect.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        /// <param name="bullseye"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public override double TorpedoHit(GameData game, SectorCoordinate sc, double bullseye, double angle)
        {
            //determine if star is affected by torpedo.
            if (game.Random.Rand() > 0.10)
            {//yep, go ahead and perform nova action
                Events.Nova(game, this);
                return 0;
            }//if

            //star is unaffected by torpedo, print some output and be gone.
            Game.Console.crmena(true, this, true, this.Sector);
            Game.Console.WriteLine(" unaffected by photon blast.");
            return 0;
        }//TorpedoHit

        /// <summary>
        /// This function is called when this object is destroyed because of a star going Nova.
        /// There is a small chance that we will super-nova because of this, otherwise
        /// this star is simply destroyed.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        /// <param name="gameOver">We super-nova'd</param>
        /// <returns>True is this is a star and will also nova</returns>
        public override bool Nova(GameData game, SectorCoordinate sc, out bool gameOver)
        {
            gameOver = false;
            if (game.Random.Rand() < 0.05)
            {
                //Wow! We've supernova'ed
                Events.SuperNova(game.Galaxy.Ship.QuadrantCoordinate, this.Sector, false, game);
                gameOver = true;
                return false;
            }//if

            //output a message about this star going nova
            Game.Console.crmena(true, this, true, this.Sector);
            Game.Console.WriteLine(" novas.");

            //remove this star from current quadrant
            game.Galaxy.CurrentQuadrant[this.Sector] = new Empty();

            //decrement number of stars here
            game.Galaxy[game.Galaxy.Ship.QuadrantCoordinate].Stars--;

            //increment stars killed count(used for scoring)
            game.StarsKilled++;
            return true;
        }//Nova

    }//class Star
}