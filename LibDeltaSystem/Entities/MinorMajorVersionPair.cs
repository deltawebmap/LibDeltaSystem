using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities
{
    public struct MinorMajorVersionPair
    {
        public byte major;
        public byte minor;

        public MinorMajorVersionPair(byte major, byte minor)
        {
            this.major = major;
            this.minor = minor;
        }
    }
}
