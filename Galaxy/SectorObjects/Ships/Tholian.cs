using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects.Ships
{
    public class Tholian : EnemyShip
    {
        public Tholian() 
            : base(false) { }

        public override double RamDamageFactor { get { return 0.5; } }
        public override char Symbol { get { return 'T'; } }

        //I suspect this spelling is incorrect ... but keep it the same
        public override string Name { get { return "Tholean"; } }

        //Tholian hit by torpedo
        public override double TorpedoHit(GameData game, SectorCoordinate sc, double bullseye, double angle)
        {
            Game.Console.Skip(1);
            Game.Console.crmena(true, this, true, this.Sector);

            double h1 = 700.0 + 100.0 * game.Random.Rand() -
                 1000.0 * this.Sector.DistanceTo(sc) *
                 Math.Abs(Math.Sin(bullseye - angle));

            if (Math.Abs(h1) >= 600)
            {
                Game.Console.WriteLine(" destroyed.");
                game.Galaxy.CurrentQuadrant[this.Sector] = new Empty();
                return 0;
            }//if

            if (game.Random.Rand() > 0.05)
            {
                Game.Console.WriteLine(" survives photon blast.");
                return 0;
            }//if

            Game.Console.WriteLine(" disappears.");
            game.Galaxy.CurrentQuadrant[this.Sector] = new TholianWeb();

            //add a black-hole somewhere in the quadrant
            game.Galaxy.CurrentQuadrant[game.Galaxy.CurrentQuadrant.dropin(game.Random)] = new BlackHole();

            return 0;
        }//TorpedoHit

        /// <summary>
        /// No action required if Tholian is destroyed.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        public override void deadkl(GameData game, SectorCoordinate sc) { }

        /// <summary>
        /// Tholian is unaffected by star nove
        /// </summary>
        /// <param name="game"></param>
        /// <param name="gameOver"></param>
        /// <returns></returns>
        public override bool Nova(GameData game, SectorCoordinate sc, out bool gameOver)
        {
            gameOver = false;
            return false;
        }

    }//class Tholian
}