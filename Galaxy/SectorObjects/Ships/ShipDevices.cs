using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace sstNET.Galaxy.SectorObjects.Ships
{
    public class ShipDevices : IXmlSerializable
    {
        //docking factor - repair speedup if docked
        private static double mDockingFactor = 0.25;

        private class ShipDevice
        {
            private double mDamage;
            public string Name { get; set; }

            public double Damage
            {
                get { return mDamage; }
                set { mDamage = Math.Max(0, value); }
            }

            public bool Damaged { get { return Damage > 0.0; } }
        }//class Device

        /// <summary>
        /// A list of all the ship devices.
        /// Values are used to index an array
        /// </summary>
        public enum ShipDevicesEnum
        {
            SRSensors = 0,
            LRSensors = 1,
            Phasers = 2,
            PhotonTubes = 3,
            LifeSupport = 4,
            WarpEngines = 5,
            ImpulseEngines = 6,
            Shields = 7,
            SubspaceRadio = 8,
            ShuttleCraft = 9,
            Computer = 10,
            Transporter = 11,
            ShieldControl = 12,
            Deathray = 13,
            DSProbe = 14
        }

        private ShipDevice[] mDevices = new ShipDevice[]
            {
            new ShipDevice{ Name = "S. R. Sensors"},
            new ShipDevice{ Name = "L. R. Sensors"},
            new ShipDevice{ Name = "Phasers"},
            new ShipDevice{ Name = "Photon Tubes"},
            new ShipDevice{ Name = "Life Support"},
            new ShipDevice{ Name = "Warp Engines"},
            new ShipDevice{ Name = "Impulse Engines"},
            new ShipDevice{ Name = "Shields"},
            new ShipDevice{ Name = "Subspace Radio"},
            new ShipDevice{ Name = "Shuttle Craft"},
            new ShipDevice{ Name = "Computer"},
            new ShipDevice{ Name = "Transporter"},
            new ShipDevice{ Name = "Shield Control"},
            new ShipDevice{ Name = "Death Ray"},
            new ShipDevice{ Name = "D. S. Probe"}
            };

        /// <summary>
        /// Return the amount of damage for a given device
        /// </summary>
        /// <param name="device">Device to check</param>
        /// <returns>Amount of damage ( >= 0 )</returns>
        public double GetDamage(ShipDevicesEnum device)
        {
            return mDevices[(int)device].Damage;
        }//Damage

        //public double this[ShipDevicesEnum index]
        //{
        //    get { return mDevices[(int)index].Damage; }
        //    set { mDevices[(int)index].Damage = value; }
        //}

        /// <summary>
        /// Check if a given device is damaged.
        /// </summary>
        /// <param name="device">Device to check</param>
        /// <returns>True if damaged, false otherwise</returns>
        public bool IsDamaged(ShipDevicesEnum device)
        {
            return mDevices[(int)device].Damaged;
        }//IsDamaged

        /// <summary>
        /// Helper function to damage the death-ray
        /// It is a special device that is damaged a set amount and is not repairable
        /// (Except in the case of resting 9.9 or greater days while docked)
        /// </summary>
        public void DamageDeathray()
        {
            mDevices[(int)ShipDevicesEnum.Deathray].Damage = 39.95;
        }//DamageDeathray

        /// <summary>
        /// Set an absolute amount of damage for a given device
        /// </summary>
        /// <param name="device">Device to set</param>
        /// <param name="damage">Amount to damage device by</param>
        public void SetDamage(ShipDevices.ShipDevicesEnum device, double damage)
        {
            //System.Diagnostics.Debug.Assert(damage >= 0.0);
            mDevices[(int)device].Damage = damage;
        }//SetDamage

        /// <summary>
        /// Increase amount of damage by a given amount
        /// </summary>
        /// <param name="device">Device to damage</param>
        /// <param name="damage">Amount to increase damage by</param>
        public void AddDamage(ShipDevices.ShipDevicesEnum device, double damage)
        {
            //System.Diagnostics.Debug.Assert(damage >= 0.0);
            mDevices[(int)device].Damage += damage;
        }//AddDamage

        /// <summary>
        /// Repair a given device by an amount
        /// </summary>
        /// <param name="device">Device to repair</param>
        /// <param name="damage">Amount to repair by</param>
        private void RepairDamage(ShipDevices.ShipDevicesEnum device, double damage)
        {
            //System.Diagnostics.Debug.Assert(damage >= 0.0);
            mDevices[(int)device].Damage -= damage;
        }//RepairDamage

        /// <summary>
        /// Repair all devices(except deathray) by amount of time
        /// adjust by docking factor if docked
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="time"></param>
        public void Repair(FederationShip ship, double time)
        {
            double repair = time;
            if (ship.Docked)
                repair /= mDockingFactor;

            for (int ii = 0; ii < mDevices.Length; ii++)
            {
                ShipDevicesEnum device = (ShipDevicesEnum)ii;

                //Don't fix Deathray here
                if (device != ShipDevicesEnum.Deathray && mDevices[ii].Damaged)
                    RepairDamage(device, repair);

            }//for ii
        }//Repair

        /// <summary>
        /// Determines if the given device can be damaged.
        /// uses the following logic:
        /// 1) Deathray cannot be damaged in this fashion. It is damaged when used
        /// 2) Shuttle can only be damaged if it exists and is aboard the ship
        /// </summary>
        /// <param name="device">device to check</param>
        /// <param name="ship"></param>
        /// <returns>True if device can be damaged, otherwise false</returns>
        private static bool CanDamageDevice(ShipDevicesEnum device, FederationShip ship)
        {
            //no damage to deathray
            if (device == ShipDevicesEnum.Deathray)
                return false;

            //no damage to shuttle if ship has no shuttlecraft
            if ((device == ShipDevices.ShipDevicesEnum.ShuttleCraft && !ship.HasShuttleBay))
                return false;

            //no damage to shuttle if it is not onboard ship
            if ((device == ShipDevicesEnum.ShuttleCraft && ship.ShuttleLocation != FederationShip.ShuttleLocationEnum.ShuttleBay))
                return false;

            return true;
        }//CanDamageDevice

        /// <summary>
        /// Damage all ship devices by a random amount.
        /// Exclude devices that cannot be damaged(see CanDamageDevice)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="damageFactor">The damage multiplier. based on skill level</param>
        public void DamageDevices(GameData game, double damageFactor)
        {
            for (int ii = 0; ii < mDevices.Length; ii++)
            {
                ShipDevicesEnum device = (ShipDevicesEnum)ii;
                if (CanDamageDevice(device, game.Galaxy.Ship))
                {
                    double extradm = (10.0 * damageFactor * game.Random.Rand() + 1.0) * game.DamageFactor;
                    AddDamage(device, (game.Turn.Time + extradm));
                }//if
            }//for ii
        }//DamageDevices

        /// <summary>
        /// Damage randomly selected devices.
        /// The number of devices to damage is random based on the hit amount
        /// (higher the amount more devices damaged)
        /// Exclude devices that cannot be damaged(see CanDamageDevice)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="hit">Amount to damage device by</param>
        public void DamageRandomDevices(GameData game, double hit)
        {
            //this list keeps track of the devices that are damaged so far
            //it is used to prevent any single device damaged twice
            List<ShipDevicesEnum> damaged = new List<ShipDevicesEnum>();

            //determine number of devices damaged
            double ncrit = 1.0 + hit / (500.0 + 100.0 * game.Random.Rand());

            //start barfing some text output
            Game.Console.Write("***CRITICAL HIT--");

            int ktr = 1;

            //Select devices and cause damage
            for (int ii = 1; ii <= ncrit; ii++)
            {
                ShipDevicesEnum device;
                do
                {
                    //randomly select a device and keep selecting until a device
                    //is picked that can be damaged
                    device = (ShipDevicesEnum)(mDevices.Length * game.Random.Rand());
                } while (!CanDamageDevice(device, game.Galaxy.Ship));

                //determine the damage amount
                double extradm = (hit * game.DamageFactor) / (ncrit * (75.0 + 25.0 * game.Random.Rand()));

                //damage the device
                AddDamage(device, extradm);

                if (damaged.Count > 0)
                {
                    //if this device has been damaged already, then don't print its name again
                    if (damaged.Contains(device))
                        continue;

                    ktr++;

                    //make sure we don't print too many devices on the same line
                    if (ktr == 3)
                        Game.Console.Skip(1);

                    Game.Console.Write(" and ");
                }//if

                //add this device to the damaged list
                damaged.Add(device);

                //print some info and go do the next one
                Game.Console.Write(mDevices[(int)device].Name);
            }//for ii
            Game.Console.WriteLine(" damaged.");

        }//DamageRandomDevices

        /// <summary>
        /// Produce a damage report for the ship devices.
        /// </summary>
        public void DamageReport()
        {
            bool jdam = false;
            for (int ii = 0; ii < mDevices.Length; ii++)
            {
                ShipDevice device = mDevices[ii];
                if (device.Damaged)
                {
                    if (!jdam)
                    {
                        Game.Console.WriteLine("\nDEVICE            -REPAIR TIMES-");
                        Game.Console.WriteLine("                IN FLIGHT   DOCKED");
                        jdam = true;
                    }//if
                    Game.Console.WriteLine("  {0,16} {1,8:F2}  {2,8:F2}", device.Name, device.Damage + 0.05, mDockingFactor * device.Damage + 0.005);
                }//if
            }//foreach
            if (!jdam)
                Game.Console.WriteLine("All devices functional.");

        }//DamageReport

        public void WriteXml(XmlWriter writer)
        {
            foreach (ShipDevice sd in mDevices)
            {
                writer.WriteStartElement("Device");
                writer.WriteStartAttribute("Damage");
                writer.WriteValue(sd.Damage);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            foreach (ShipDevice sd in mDevices)
            {
                reader.ReadToFollowing("Device");
                sd.Damage = double.Parse(reader.GetAttribute("Damage") as string);
            }
            reader.Skip();
            reader.ReadEndElement();
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return (null);
        }

    }//class ShipDevices
}