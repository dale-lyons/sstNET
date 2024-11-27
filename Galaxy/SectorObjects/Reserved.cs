using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects
{
    /// <summary>
    /// This class represents a Reserved sector. It is used to block certain sectors
    /// so other objects will not occupy them. Specifically this is done during the
    /// placing of the Tholian as the Tholian can only occupy one of the 4 corners
    /// of the quadrant.
    /// </summary>
    public class Reserved : SectorObject
    {
        public Reserved()
            : base() { }

        public override bool Ramable { get { return false; } }
        public override char Symbol { get { return 'X'; } }
        public override string Name { get { return "Reserved"; } }

    }
}