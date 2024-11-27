using System;
using System.Collections.Generic;

//using System.Xml;
//using System.Xml.Schema;
//using System.Xml.Serialization;

namespace sstNET
{
    /// <summary>
    /// Class handles random number generation.
    /// The algorithm used in Rand is the same as in the C runtime libraries.
    /// As long as the seed provided in Seed is the same, this version will
    /// produce the exact same sequence as the C runtimes.
    /// </summary>
    public class Random
    {
        //private int mRandomSeed;
        public int RandomSeed { get; set; }

        /// <summary>
        /// Create 2 random numbers between 1 and 8
        /// Used to create a random sector coordinate
        /// </summary>
        /// <param name="ix"></param>
        /// <param name="iy"></param>
        public void iran8(out int ix, out int iy)
        {
            ix = (int)(Rand() * 8.0 + 1.0);
            iy = (int)(Rand() * 8.0 + 1.0);
        }//iran8

        /// <summary>
        /// Create 2 random numbers between 1 and 10
        /// Used to create a random quadrant coordinate
        /// </summary>
        /// <param name="ix"></param>
        /// <param name="iy"></param>
        public void iran10(out int ix, out int iy)
        {
            ix = (int)(Rand() * 10.0 + 1.0);
            iy = (int)(Rand() * 10.0 + 1.0);
        }//iran10
        public double expran(double avrage)
        {
            return (-avrage * Math.Log(1e-7 + Rand()));
        }

        ///// <summary>
        ///// Set the random number seed value.
        ///// </summary>
        ///// <param name="seed"></param>
        //public void Seed(int seed)
        //{
        //    RandomSeed = seed;
        //}

        /// <summary>
        /// Peek at the next random number. It is saved for the next
        /// random number fetch. Used for debugging.
        /// </summary>
        /// <returns></returns>
        public double Peek()
        {
            int seed = RandomSeed * 0x343FD + 0x269EC3;
            int ran = ((seed >> 0x10) & 0x7FFF);
            double dran = (double)ran / (1.0 + (double)32767);
            return dran;
        }//Peek

        /// <summary>
        /// Generate a random number 0 <= r <= 1.0
        /// </summary>
        /// <returns></returns>
        public double Rand()
        {
            RandomSeed = RandomSeed * 0x343FD + 0x269EC3;
            int ran = ((RandomSeed >> 0x10) & 0x7FFF);
            double dran = (double)ran / (1.0 + (double)32767);
            return dran;
        }//Rand

        public void dump()
        {
            if (!GameData.DEBUGME)
                return;
            Game.Console.WriteLine("Next Random:{0,2:F8}", this.Peek());
        }

        //public void WriteXml(XmlWriter writer)
        //{
        //    writer.WriteValue(RandomSeed);
        //}

        //public void ReadXml(XmlReader reader)
        //{
        //    //personName = reader.ReadString();
        //}

        //public XmlSchema GetSchema()
        //{
        //    return (null);
        //}

    }//class Random
}