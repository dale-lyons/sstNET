using System;
using System.Collections.Generic;
using System.Text;

namespace sstNET.Galaxy.SectorObjects.Ships
{
    public class FaerieQueene : FederationShip
    {
        public FaerieQueene()
            : base()
        {
        }

        public override char Symbol { get { return 'F'; } }
        public override string Name { get { return "Faerie Queene"; } }

        public override bool HasShuttleBay { get { return false; } }
        public override int InitialMainEnergy { get { return 3000; } }
        public override int InitialShieldEnergy { get { return 1250; } }
        public override double InitialLifeSupport { get { return 3.0; } }
        public override int InitialTorpedoes { get { return 6; } }
        public override bool HasDeathray { get { return false; } }
        public override bool HasProbes { get { return false; } }

    }
}
