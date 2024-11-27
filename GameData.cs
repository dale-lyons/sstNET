using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using sstNET.Galaxy;
using sstNET.Galaxy.QuadrantObjects;

namespace sstNET
{
    public class GameData
    {
        /// <summary>
        /// Type of game the user is playing.
        /// Regular games select a random seed for the random number generator.
        /// Tournament games use a pre-selected seed so the game can be re-created
        /// Frozen games are games saved to an xml file and can be reloaded.
        /// </summary>
        public enum GameTypeEnum
        {
            None,
            Regular,
            Tournament,
            Frozen
        }

        /// <summary>
        /// Length of the game. Mostly determines the number of bad guys needed to kill.
        /// </summary>
        public enum GameLengthEnum
        {
            None = 0,
            Short = 1,
            Medium = 2,
            Long = 4
        }

        /// <summary>
        /// Skill level of the game.
        /// </summary>
        public enum GameSkillEnum
        {
            None = 0,
            Novice = 1,
            Fair = 2,
            Good = 3,
            Expert = 4,
            Emeritus = 5,
        }

        /// <summary>
        /// Some global data that needs to be passed around druing play.
        /// Mostly this is required to keep the logic and gameplay the same as the original.
        /// </summary>
        public class TurnInfo
        {
            public bool ididit = false;

            /// <summary>
            /// This flag indicates if the game is over.
            /// </summary>
            public bool alldone = false;

            /// <summary>
            /// The time computed for the current turn to complete.
            /// </summary>
            public double Time = 0.0;

            public bool justin = false;

            /// <summary>
            /// The user is resting. (The Rest command)
            /// </summary>
            public bool resting = false;

            /// <summary>
            /// This flag indicates if the user has been notified of a starbase under attack.
            /// It is used to prevent multiple warnings of the same attack.
            /// </summary>
            public bool iseenit = false;
            public bool alive = true;
            public int iattak = 0;
            public bool ientesc = false;
            public bool gamewon = false;
        
            /// <summary>
            /// When the user specifies a destination it is converted to a distance and direction.
            /// </summary>
            public double dist;
            public double direc;

        }//class TurnInfo

        public bool GameThawed { get; set; }
        public int GameTourn { get; set; }
        public string GamePassword { get; set; }

        public GameTypeEnum GameType { get; set; }
        public GameLengthEnum GameLength { get; set; }
        public GameSkillEnum GameSkill { get; set; }

        //These counters count how many kills by user actions.
        //There are cases where kills occur that are not user actions(ie commander destroys a starbase).
        public int StarsKilled { get; set; }
        public int KlingonsKilled { get; set; }
        public int CommandersKilled { get; set; }
        public int SuperCommandersKilled { get; set; }
        public int RomulansKilled { get; set; }
        public int PlanetsKilled { get; set; }
        public int BasesKilled { get; set; }

        /// <summary>
        /// Number of crew causalties so far
        /// </summary>
        public int Casualties { get; set; }

        /// <summary>
        /// Number of calls to help to a starbase
        /// </summary>
        public int CallsForHelp { get; set; }

        /// <summary>
        /// Number of times the ship attempted to leave galaxy
        /// (3 is fatal)
        /// </summary>
        public int NumberKinks { get; set; }

        /// <summary>
        /// Remaining time of the game. Computed as the game progresses.
        /// More time is allocated as klingons are killed.
        /// </summary>
        public double RemainingTime { get; set; }

        /// <summary>
        /// Remaining resources are computed as the game progresses.
        /// Based on the number of baddies killed.
        /// </summary>
        public double RemainingResources { get; set; }

        /// <summary>
        /// Current date of game
        /// </summary>
        public double Date { get; set; }

        /// <summary>
        /// damage factor - higher value for higher skill level
        /// </summary>
        public double DamageFactor { get; set; }

        /// <summary>
        /// The random number generator
        /// </summary>
        public Random Random { get; set; }

        /// <summary>
        /// The galaxy ... and all that it contains
        /// </summary>
        public Galaxy.Galaxy Galaxy { get; set; }

        /// <summary>
        /// Future scheduled events
        /// </summary>
        public FutureEvents Future { get; set; }

        /// <summary>
        /// Information about the current turn.
        /// </summary>
        public TurnInfo Turn { get; set; }

        /// <summary>
        /// Debug flag for the debug command.
        /// </summary>
        public static bool DEBUGME { get; set; }

        /// <summary>
        /// This is a snapshot of the state of the galaxy. It is a scheduled future event
        /// and is used if the ship enters a time warp and must go back in time.
        /// </summary>
        public SnapShot GameSnapShot { get; set; }

        /// <summary>
        /// public ctor required for xml serialization
        /// </summary>
        public GameData()
        {
        }

        /// <summary>
        /// As part of the galaxy generation this method will setup various parameters.
        /// It is done this way to make sure the order of creation is exactly the same
        /// as the original code to preserve the random number sequence.
        /// </summary>
        public void Generate()
        {
            //Set up assorted game parameters
            Casualties = 0;
            KlingonsKilled = 0;
            CommandersKilled = 0;

            RemainingResources = Galaxy._inresor;

            //compute a random start date
            Date = 100.0 * (int)(31.0 * Random.Rand() + 20.0);
            Galaxy._indate = Date;

            //and set the remaining time to the initial computed earlier
            RemainingTime = Galaxy._intime;

            //Setup the future events data structures and initialize them.
            Future = new FutureEvents();
            Future.Setup(Random, Galaxy._indate, Galaxy._intime, Galaxy._incom, Galaxy._nscrem);

        }//Generate

    }//class Game
}