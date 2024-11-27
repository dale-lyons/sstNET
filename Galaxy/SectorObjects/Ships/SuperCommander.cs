using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects.Ships
{
    /// <summary>
    /// This class represents the SuperCommander. A more powerful version of a Commander.
    /// There can only be at most 1 SuperCommander in the galaxy. This guy can raom the
    /// galaxy at will, attacking starbases, destroying planets.
    /// </summary>
    public class SuperCommander : EnemyShip
    {
        public SuperCommander()
            : base(true) { }

        public SuperCommander(Random rand, GameData.GameSkillEnum skill)
            : base(true)
        {
            //compute a power level based on skill level(and some randomness)
            Power = (1175.0 + 400.0 * rand.Rand() + 125.0 * (int)skill);
        }

        public override double RamDamageFactor { get { return 2.5; } }
        public override char Symbol { get { return 'S'; } }
        public override string Name { get { return "Super-commander"; } }

        public override double TorpedoHit(GameData game, SectorCoordinate sc, double bullseye, double angle)
        {
            if (game.Random.Rand() <= 0.05)
            {
                Game.Console.WriteLine("***{0} at{1} uses anti-photon device;", this.Name, this.Sector.ToString(true));
                Game.Console.WriteLine("   torpedo neutralized.");
                return 0;
            }
            return base.TorpedoHit(game, sc, bullseye, angle);
        }//TorpedoHit

        /// <summary>
        /// The Super Commander has been destroyed.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sc"></param>
        public override void deadkl(GameData game, SectorCoordinate sc)
        {
            //remove super commander from game.
            game.Galaxy[game.Galaxy.Ship.QuadrantCoordinate].SuperCommander = null;

            //inc stats for score
            game.SuperCommandersKilled++;

            //if the super commander was attacking a base, stop the attack
            game.Galaxy.SuperCommanderAttack = null;

            //no need for sc moves or base attacks
            game.Future[FutureEvents.EventTypesEnum.FSCMOVE] = FutureEvents.NEVER;
            game.Future[FutureEvents.EventTypesEnum.FSCDBAS] = FutureEvents.NEVER;
        }//deadkl

    }//class SuperCommander
}