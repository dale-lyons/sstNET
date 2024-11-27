using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace sstNET
{
    /// <summary>
    /// This class is used to hold the state of the current snapshot of the game.
    /// A snapshot is taken periodically during the game. Its purpose is to restore
    /// the state of the game if the player goes back in time due to a time warp.
    /// The player is sent back to the time of the last snapshot taken.
    /// Various aspects of the game are set to the previous dates value, however certain
    /// things are not. The state of the ship for example is preserved.
    /// For simplicity the snapshot data is represented in an xml document. In this way we
    /// can serialize the snapshot in case the game is saved (freeze) and restored (thaw)
    /// as well as taking and restoring a snapshot.
    /// </summary>
    public class SnapShot : IXmlSerializable
    {
        /// <summary>
        /// Names of the xml attributes used for serialization
        /// </summary>
        private const string mSnapShotAttr = "SnapShot";
        private const string mStarsKilledAttr = "StarsKilled";
        private const string mKlingonsKilledAttr = "KlingonsKilled";
        private const string mCommandersKilledAttr = "CommandersKilled";
        private const string mSuperCommandersKilledAttr = "SuperCommandersKilled";
        private const string mRomulansKilledAttr = "RomulansKilled";
        private const string mPlanetsKilledAttr = "PlanetsKilled";
        private const string mBasesKilledAttr = "BasesKilled";
        private const string mRemainingResourcesAttr = "RemainingResources";
        private const string mRemainingTimeAttr = "RemainingTime";
        private const string mDateAttr = "Date";

        /// <summary>
        /// The date this snapshot was taken.
        /// </summary>
        public double Date { get; set; }

        /// <summary>
        /// An xml representation of the snapshot
        /// </summary>
        private StringBuilder mCurrentXMLText;

        /// <summary>
        /// public ctor required for xml serialization
        /// </summary>
        public SnapShot() { }

        /// <summary>
        /// Take a snapshot of the game
        /// All the critical data of the game are serialized to an xml document and saved for future use.
        /// Either the snapshot is restored (from a time warp and going back in time)
        /// or the game is saved to a file (freeze game).
        /// </summary>
        /// <param name="game"></param>
        public void TakeSnapShot(GameData game)
        {
            //record the date snapshot is taken
            Date = game.Date;

            //blank out old snapshot xml text
            mCurrentXMLText = new StringBuilder();

            //create an xml writer with some settings. Use the string builder to hold the xml text
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;

            //Write out the game data we are interested int
            using (XmlWriter writer = XmlWriter.Create(mCurrentXMLText, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement(mSnapShotAttr);
                WriteLocalDataItem(writer, mStarsKilledAttr, game.StarsKilled);
                WriteLocalDataItem(writer, mKlingonsKilledAttr, game.KlingonsKilled);
                WriteLocalDataItem(writer, mCommandersKilledAttr, game.CommandersKilled);
                WriteLocalDataItem(writer, mSuperCommandersKilledAttr, game.SuperCommandersKilled);
                WriteLocalDataItem(writer, mRomulansKilledAttr, game.RomulansKilled);
                WriteLocalDataItem(writer, mPlanetsKilledAttr, game.PlanetsKilled);
                WriteLocalDataItem(writer, mBasesKilledAttr, game.BasesKilled);
                WriteLocalDataItem(writer, mRemainingResourcesAttr, game.RemainingResources);
                WriteLocalDataItem(writer, mRemainingTimeAttr, game.RemainingTime);
                WriteLocalDataItem(writer, mDateAttr, game.Date);

                //manually serialize the galaxy quadrants
                for (int ix = 1; ix <= sstNET.Galaxy.Galaxy.GALAXYWIDTH; ix++)
                {
                    for (int iy = 1; iy <= sstNET.Galaxy.Galaxy.GALAXYHEIGHT; iy++)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(sstNET.Galaxy.Quadrant));
                        serializer.Serialize(writer, game.Galaxy[ix, iy]);
                    }//for iy
                }//for ix
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }//using

        }//TakeSnapShot

        /// <summary>
        /// Restore a previously saved snapshot to the game.
        /// This can happen because of a time warp and we go back in time
        /// or a game is being thawed.
        /// </summary>
        /// <param name="game"></param>
        public void RestoreSnapShot(GameData game)
        {
            //if no saved snapshot then do nothing.
            if (mCurrentXMLText == null)
                return;

            //create an xml reader from the xml text
            using (StringReader sr = new StringReader(mCurrentXMLText.ToString()))
            {
                //and read in the game data
                using (XmlReader reader = XmlReader.Create(sr))
                {
                    reader.ReadStartElement();
                    game.StarsKilled = int.Parse(ReadLocalDataItem(reader, mStarsKilledAttr));
                    game.KlingonsKilled = int.Parse(ReadLocalDataItem(reader, mKlingonsKilledAttr));
                    game.CommandersKilled = int.Parse(ReadLocalDataItem(reader, mCommandersKilledAttr));
                    game.SuperCommandersKilled = int.Parse(ReadLocalDataItem(reader, mSuperCommandersKilledAttr));
                    game.RomulansKilled = int.Parse(ReadLocalDataItem(reader, mRomulansKilledAttr));
                    game.PlanetsKilled = int.Parse(ReadLocalDataItem(reader, mPlanetsKilledAttr));
                    game.BasesKilled = int.Parse(ReadLocalDataItem(reader, mBasesKilledAttr));
                    game.RemainingResources = double.Parse(ReadLocalDataItem(reader, mRemainingResourcesAttr));
                    game.RemainingTime = double.Parse(ReadLocalDataItem(reader, mRemainingTimeAttr));
                    game.Date = double.Parse(ReadLocalDataItem(reader, mDateAttr));

                    //must deserialize the galaxy quadrants manually
                    for (int ix = 1; ix <= sstNET.Galaxy.Galaxy.GALAXYWIDTH; ix++)
                    {
                        for (int iy = 1; iy <= sstNET.Galaxy.Galaxy.GALAXYHEIGHT; iy++)
                        {
                            //we will preserve the current star chart setting
                            int starch = game.Galaxy[ix, iy].Starch;
                            XmlSerializer serializer = new XmlSerializer(typeof(sstNET.Galaxy.Quadrant));
                            game.Galaxy[ix, iy] = serializer.Deserialize(reader) as sstNET.Galaxy.Quadrant;
                            game.Galaxy[ix, iy].Starch = starch;
                        }//for iy
                    }//for ix
                }//using reader
            }//using sr
        }//RestoreSnapShot

        /// <summary>
        /// Debug method to dump snapshot status
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        public static void DumpDate(double date1, double date2)
        {
            if (!GameData.DEBUGME)
                return;

            Game.Console.WriteLine("Snapshot date:{0,0:F2}", date1);
            Game.Console.WriteLine("Snapshot next:{0,0:F2}", date2);
        }//DumpDate

        /// <summary>
        /// Helper function for writing an xml element
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private static void WriteLocalDataItem(XmlWriter writer, string name, object value)
        {
            writer.WriteStartElement(name);
            writer.WriteValue(value);
            writer.WriteEndElement();
        }//WriteLocalDataItem

        /// <summary>
        /// Helper function for reading an xml element
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string ReadLocalDataItem(XmlReader reader, string name)
        {
            reader.ReadStartElement();
            string str = reader.Value as string;
            reader.Skip();
            reader.ReadEndElement();
            return str;
        }//ReadLocalDataItem

        /// <summary>
        /// IXmlSerializable method to serialize snapshot to game save file.
        /// Since the snapshot has been serialized to an xml document, we can just write
        /// this document out to the serialization stream.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {

            writer.WriteStartElement(mDateAttr);
            writer.WriteValue(Date);
            writer.WriteEndElement();

            //if no saved snapshot then do not attempt to write out xml
            if (mCurrentXMLText != null)
                writer.WriteValue(mCurrentXMLText.ToString());

        }//WriteXml

        /// <summary>
        /// IXmlSerializable method to deserialize snapshot from game save file.
        /// The snapshot is saved as an xml document, we can read the text into the current xml
        /// representation of the snapshot.
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            Date = double.Parse(ReadLocalDataItem(reader, mDateAttr));
            mCurrentXMLText = new StringBuilder(reader.Value as string);

            reader.Skip();
            reader.ReadEndElement();
        }//ReadXml

        public XmlSchema GetSchema()
        {
            return (null);
        }

    }//class SnapShot
}