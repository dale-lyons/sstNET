using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET
{
    /// <summary>
    /// This class handles the scheduled future events for the game.
    /// A future event is simply a date (double) value that is the date
    /// the event should happen.
    /// </summary>
    public class FutureEvents
    {
        /// <summary>
        /// This constant defines a date that will never happen.
        /// </summary>
        public const double NEVER = 1e30;

        /// <summary>
        /// Future events enumertion.
        /// The index into the future array for the specified game event.
        /// Note:The fspy value has a -1 value, it is never used to index the array.
        /// It is a special type of event handled in the events class.
        /// ToDo:Can we get rid of this?
        /// </summary>
        public enum EventTypesEnum
        {
            FSPY = -1,   	    // Spy event happens always (no future[] entry) can cause SC to tractor beam Enterprise
            FSNOVA = 0,         // Supernova
            FTBEAM = 1,         // Commander tractor beams Enterprise
            FSNAP = 2,          // Snapshot for time warp
            FBATTAK = 3,        // Commander attacks base
            FCDBAS = 4,         // Commander destroys base
            FSCMOVE = 5,        // Supercommander moves (might attack base)
            FSCDBAS = 6,        // Supercommander destroys base
            FDSPROB = 7         // Move deep space probe
        }//EventTypes
        private const int NEVENTS = 8;

        /// <summary>
        /// The actual event array. Each entry is a Date value that specifies
        /// when the event will occur.
        /// Note:It is public for serialization purposes only.
        /// </summary>
        public double[] mFuture;

        /// <summary>
        /// Initialize the future events. Set all entries to never.
        /// Note:make sure this gets initialized fully. When de-serializing
        /// from a freeze file its possible some entries will be skipped.
        /// </summary>
        public FutureEvents()
        {
            mFuture = new double[NEVENTS];
            for (int ii = 0; ii < NEVENTS; ii++)
            {
                mFuture[ii] = NEVER;
            }//for ii
        }//FutureEvents ctor

        public double this[EventTypesEnum index]
        {
            get { return mFuture[(int)index]; }
            set { mFuture[(int)index] = value; }
        }

        /// <summary>
        /// Initialize times for extraneous events
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="date"></param>
        /// <param name="intime"></param>
        /// <param name="remcom"></param>
        /// <param name="nscrem"></param>
        public void Setup(Random rand, double date, double intime, int remcom, int nscrem)
        {
            //schedule a star super-nova
            this[EventTypesEnum.FSNOVA]  = date + rand.expran(0.5 * intime);

            //remcom better not be == 0 !!!!
            //schedule a tractor-beam by a commander (or super-commander)
            this[EventTypesEnum.FTBEAM]  = date + rand.expran(1.5 * (intime / remcom));

            //schedule a snapshot of the game (sooner)
            this[EventTypesEnum.FSNAP]   = date + 1.0 + rand.Rand();

            //schedule a commander attack starbase
            this[EventTypesEnum.FBATTAK] = date + rand.expran(0.3 * intime);

            //no starbase being attacked by commander, so no scheduled starbase destroyed
            this[EventTypesEnum.FCDBAS]  = NEVER;

            //schedule a super-commander move if one exists.
            this[EventTypesEnum.FSCMOVE] = (nscrem > 0) ? date + 0.2777 : NEVER;

            //no starbase being attacked by super-commander, so no scheduled starbase destroyed
            this[EventTypesEnum.FSCDBAS] = NEVER;

            //no deep space probe, so no probe move events
            this[EventTypesEnum.FDSPROB] = NEVER;

        }//Setup

        /// <summary>
        /// Searches the future event list for the entry with the minimum time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private EventTypesEnum NextEvent()
        {
            double min = double.MaxValue;
            int index = (int)EventTypesEnum.FSPY;
            for (int ii = 0; ii < mFuture.Length; ii++)
            {
                if (mFuture[ii] < min)
                {
                    min = mFuture[ii];
                    index = ii;
                }//if
            }//for ii
            return (EventTypesEnum)index;
        }//NextEvent

        /// <summary>
        /// Searches for next event <= time
        /// </summary>
        /// <param name="time"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public double Search(double time, out EventTypesEnum line)
        {
            line = this.NextEvent();
            if (line == EventTypesEnum.FSPY || this[line] > time)
            {
                line = EventTypesEnum.FSPY;
                return 0;
            }
            return this[line];
        }//Search

        public void Dump(Random rand)
        {
            if (!GameData.DEBUGME)
                return;

            Game.Console.WriteLine("====FUTURE EVENTS====");
            for (int ii = 0; ii < NEVENTS; ii++)
            {
                Game.Console.WriteLine("{0}:{1,8:F2}", ii + 1, mFuture[ii]);
            }
            Game.Console.WriteLine("Next Random:{0,2:F8}", rand.Peek());
            Game.Console.WriteLine("====FUTURE EVENTS====");
        }//Dump

    }//class FutureEvents
}