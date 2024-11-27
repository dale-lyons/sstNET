using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.QuadrantObjects
{
    public class QuadrantStarBase : QuadrantObject
    {
        public QuadrantStarBase() { }

        public QuadrantStarBase(QuadrantCoordinate qc)
            : base(qc)
        {
        }

        //public object Clone()
        //{
        //    QuadrantStarBase qb = new QuadrantStarBase(this.QuadrantCoordinate);
        //    qb.ID = this.ID;
        //    return qb;
        //}

    }
}
