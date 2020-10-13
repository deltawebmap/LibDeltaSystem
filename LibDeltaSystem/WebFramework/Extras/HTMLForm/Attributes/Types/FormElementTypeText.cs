using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types
{
    public class FormElementTypeText : InputBaseFormElementType
    {
        public FormElementTypeText(string title, string placeholder, string id) : base(title, id)
        {
            this.placeholder = placeholder;
        }

        public string placeholder;

        public override void ApplyInputProperties(Dictionary<string, string> props)
        {
            props.Add("type", "text");
            props.Add("placeholder", placeholder);
        }

        public override object DeserializeResponse(StringValues value)
        {
            return (string)value;
        }
    }
}
