using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.QuadrantObjects
{
    public class QuadrantSuperCommander : QuadrantObject
    {
        public QuadrantSuperCommander() { }

        public QuadrantSuperCommander(QuadrantCoordinate qc)
                        : base(qc)
        {
        }

        //public object Clone()
        //{
        //    QuadrantSuperCommander qsc = new QuadrantSuperCommander(this.QuadrantCoordinate);
        //    qsc.ID = this.ID;
        //    return qsc;
        //}

    }
}
