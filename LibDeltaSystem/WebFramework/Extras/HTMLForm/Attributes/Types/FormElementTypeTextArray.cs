using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types
{
    /// <summary>
    /// Comma-separated
    /// </summary>
    public class FormElementTypeTextArray : InputBaseFormElementType
    {
        public FormElementTypeTextArray(string title, string placeholder, string id) : base(title, id)
        {
            this.placeholder = placeholder;
        }

        public string placeholder;

        public override void ApplyInputProperties(Dictionary<string, string> props)
        {
            props.Add("type", "text");
            props.Add("placeholder", placeholder + "(Comma Separated)");
        }

        public override object DeserializeResponse(StringValues value)
        {
            string[] data = ((string)value).Split(',');
            for (int i = 0; i < data.Length; i++)
                data[i] = data[i].Trim(' ');
            if (data[0].Length == 0)
                return new string[0];
            else
                return data;
        }
    }
}
