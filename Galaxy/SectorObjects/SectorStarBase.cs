using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects
{
    /// <summary>
    /// This class represents a starbase in the current quadrant.
    /// Note:there can only be a max of 1 starbase in a single quadrant.
    /// </summary>
    public class SectorStarBase : SectorObject
    {
        public SectorStarBase()
            : base() { }

        public override bool Ramable { get { return false; } }
        public override char Symbol { get { return 'B'; } }
        public override string Name { get { return "Starbase"; } }

        private void DestroyStarbase(GameData game)
        {
            //remove starbase from galaxy
            game.Galaxy[game.Galaxy.Ship.GalacticCoordinate].Base = null;

            //remove starbase from current quadrant
            game.Galaxy.CurrentQuadrant[this.Sector] = new Empty();

            //increment starbase killed count(for final score)
            game.BasesKilled++;

            //make sure ship is undocked(in case it was docked when we killed it)
            game.Galaxy.Ship.Docked = false;
        }//DestroyStarbase

        /// <summary>
        /// Starbase is hit by a torpedo? "Wait the hell are you doing Jim?"
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        /// <param name="bullseye"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public override double TorpedoHit(GameData game, SectorCoordinate sc, double bullseye, double angle)
        {
            //tell user bad news
            Game.Console.WriteLine("***STARBASE DESTROYED..");

            //update star chart
            if (game.Galaxy[game.Galaxy.Ship.GalacticCoordinate].Starch < 0)
                game.Galaxy[game.Galaxy.Ship.GalacticCoordinate].Starch = 0;

            //go cleanup, remove starbase from game
            DestroyStarbase(game);

            return 0;
        }//TorpedoHit

        /// <summary>
        /// Starbase is damaged because of a nova explosion
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        /// <param name="gameOver"></param>
        /// <returns></returns>
        public override bool Nova(GameData game, SectorCoordinate sc, out bool gameOver)
        {
            gameOver = false;

            //print a base destroyed message
            Game.Console.crmena(true, this, true, this.Sector);
            Game.Console.WriteLine(" destroyed.");

            //go cleanup, remove starbase from game
            DestroyStarbase(game);

            return false;
        }//Nova

    }//class StarBase
}