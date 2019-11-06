using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.PrivateNet.Packages
{
    public class PackageIndex
    {
        public string latest_patch;
        public DateTime latest_patch_time;
        public PackageIndexPatch[] packages;

        public PackageIndexPatch GetPackageById(string id)
        {
            PackageIndexPatch pack = null;
            foreach (var i in packages)
            {
                if (i.id == id)
                    pack = i;
            }
            return pack;
        }
    }
}
