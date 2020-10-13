using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.WebFramework
{
    public class DeltaWebException : Exception
    {
        public string text;
        public int httpCode;

        public DeltaWebException(string text, int httpCode) : base("A DeltaWebException was thrown.")
        {
            this.text = text;
            this.httpCode = httpCode;
        }
    }
}
