using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.AutoDocsFramework.Definitions
{
    /// <summary>
    /// Applied on top of an object.
    /// An embedded object does not appear in a list of global objects
    /// </summary>
    public class AutoDocObjectAttribute : Attribute
    {
        public AutoDocObjectAttribute(string name, string id, bool embed)
        {

        }
    }
}
