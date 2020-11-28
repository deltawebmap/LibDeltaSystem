using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.IO.Entities
{
    public struct IoPingResponse
    {
        public byte lib_version_major;
        public byte lib_version_minor;
        public byte app_version_major;
        public byte app_version_minor;
    }
}
