using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.QuadrantObjects
{
    public class QuadrantCommander : QuadrantObject
    {
        public QuadrantCommander() { }

        public QuadrantCommander(QuadrantCoordinate qc)
            : base(qc)
        {
        }
    }//class QuadrantCommander
}