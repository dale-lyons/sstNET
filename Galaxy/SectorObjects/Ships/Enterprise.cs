using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects.Ships
{
    public class Enterprise : FederationShip
    {
        public Enterprise()
            : base()
        {
        }

        //public override void Init(Random rand)
        //{
        //    this.QuadrantCoordinate = QuadrantCoordinate.Random(rand);
        //    this.Sector = SectorCoordinate.Random(rand);
        //    this.Probes = (int)(3.0 * rand.Rand() + 2.0);          //Give them 2-4 of these wonders
        //}

        public override char Symbol { get { return 'E'; } }
        public override string Name { get { return "Enterprise"; } }

        public override bool HasShuttleBay { get { return true; } }
        public override int InitialMainEnergy { get { return 5000; } }
        public override int InitialShieldEnergy { get { return 2500; } }
        public override double InitialLifeSupport { get {return 4.0;}}
        public override int InitialTorpedoes { get { return 10; } }
        public override bool HasDeathray { get { return true; } }
        public override bool HasProbes { get { return true; } }
    }//class Enterprise
}