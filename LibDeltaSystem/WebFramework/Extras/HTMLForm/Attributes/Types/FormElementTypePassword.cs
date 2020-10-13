using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types
{
    public class FormElementTypePassword : InputBaseFormElementType
    {
        public FormElementTypePassword(string title, string id) : base(title, id)
        {

        }

        public override void ApplyInputProperties(Dictionary<string, string> props)
        {
            props.Add("type", "password");
            props.Add("placeholder", title);
        }

        public override object DeserializeResponse(StringValues value)
        {
            return (string)value;
        }
    }
}
