using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes
{
    public abstract class BaseFormElementType : Attribute
    {
        public string id;

        public BaseFormElementType(string id)
        {
            this.id = id;
        }
        
        public abstract string GenerateHTML();

        public abstract object DeserializeResponse(StringValues value);
    }
}
