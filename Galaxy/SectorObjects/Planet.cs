using System;
using System.Collections.Generic;
using System.Text;

using sstNET.Galaxy.QuadrantObjects;
using sstNET.Galaxy.SectorObjects.Ships;

namespace sstNET.Galaxy.SectorObjects
{
    /// <summary>
    /// This class represents a single planet in the current quadrant.
    /// It is a special version of SectorObject because it only holds
    /// a reference to the real planet, the QuadrantObject version.
    /// Since planets hold their state across different current quadrants there
    /// needs to be a quadrant version and a sector version of planets.
    /// This class is constructed with a quadrant version and it is aggregated by this class.
    /// </summary>
    public class Planet : SectorObject
    {
        /// <summary>
        /// Reference to the quadrant version of the planet
        /// This must be first so it is set for de-serialization before the other properties.
        /// </summary>
        public QuadrantPlanet QuadrantPlanet { get; set; }

        public Planet() { }

        /// <summary>
        /// Construct a SectorObject version of a planet. Hold the QuadrantObject version 
        /// as a local reference.
        /// </summary>
        /// <param name="qplanet"></param>
        public Planet(QuadrantPlanet quadrantPlanet)
            : base()
        {
            QuadrantPlanet = quadrantPlanet;
        }

        public override char Symbol { get { return 'P'; } }
        public override string Name { get { return "Planet"; } }

        public bool Known { get { return QuadrantPlanet.Known; } set { QuadrantPlanet.Known = value; } }
        public int Class { get { return QuadrantPlanet.Class; } }
        public bool Crystals { get { return QuadrantPlanet.Crystals; } }
        public bool GalileoPresent { get { return QuadrantPlanet.GalileoPresent; } set { QuadrantPlanet.GalileoPresent = value; } }

        public override bool Ramable { get { return false; } }

        /// <summary>
        /// Obtain the planet class in character form from the planet.
        /// </summary>
        public char ToChar { get { return QuadrantPlanet.ToChar; } }

        private void PrintDestroyedMessage()
        {
            Game.Console.crmena(true, this, true, this.Sector);
            Game.Console.WriteLine(" destroyed.");
        }

        /// <summary>
        /// Handle the planet being hit by a torpedo.
        /// </summary>
        /// <param name="game">Game object</param>
        /// <param name="sc">The sector torpedo was fired from</param>
        /// <param name="bullseye">The true angle from firer to this planet</param>
        /// <param name="angle">The actual angle torpedo was fired at</param>
        /// <returns>Hit amount(only for ship hits so we return a 0)</returns>
        public override double TorpedoHit(GameData game, SectorCoordinate sc, double bullseye, double angle)
        {
            //output a message about this planet being destroyed
            PrintDestroyedMessage();

            //increment planet killed count
            game.PlanetsKilled++;

            //remove reference to planet in Quadrant
            game.Galaxy[game.Galaxy.Ship.GalacticCoordinate].Planet = null;

            //remove this planet from current quadrant
            game.Galaxy.CurrentQuadrant[this.Sector] = new Empty();

            //if crew was on planet, tough luck game over
            if (game.Galaxy.Ship.CrewLocation == Ships.FederationShip.CrewLocationEnum.Planet)
            {//captain parishes on planet
                Finish.finish(Finish.FINTYPE.FDPLANET, game);
            }//if

            //return value is hit amount on ship, which is 0 in this case
            return 0;
        }//TorpedoHit

        /// <summary>
        /// Handle planet being destroyed in a super-nova
        /// </summary>
        /// <param name="game">Game object</param>
        /// <param name="sc">Quadrant of super-nova</param>
        /// <param name="gameOver">flag indicating if game should end</param>
        /// <returns>True if this is a star destroyed by super-nova(which is false in this case)</returns>
        public override bool Nova(GameData game, SectorCoordinate sc, out bool gameOver)
        {
            //assume game will carry-on
            gameOver = false;

            //remove this planet from galaxy
            game.Galaxy[game.Galaxy.Ship.QuadrantCoordinate].Planet = null;

            //and increment the planets killed count
            game.PlanetsKilled++;

            //print a planet destroyed message
            PrintDestroyedMessage();

            //if crew was on planet, tough luck game over
            if (game.Galaxy.Ship.CrewLocation == FederationShip.CrewLocationEnum.Planet)
            {
                Finish.finish(Finish.FINTYPE.FPNOVA, game);
                gameOver = true;
                return false;
            }//if

            //remove this planet from current quadrant
            game.Galaxy.CurrentQuadrant[this.Sector] = new Empty();
            return false;
        }//Nova

    }//class Planet
}