using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects
{
    /// <summary>
    /// This class represents an empty sector in the current quadrant.
    /// </summary>
    public class Empty : SectorObject
    {
        public Empty() 
            : base() { }

        public override bool Ramable { get { return false; } }
        public override char Symbol { get { return '.'; } }
        public override string Name { get { return "Empty"; } }

    }//class Empty
}
