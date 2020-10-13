using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types
{
    public class FormElementTypeNumber : InputBaseFormElementType
    {
        public FormElementTypeNumber(string title, string id, int min, int max, int value) : base(title, id)
        {
            this.min = min;
            this.max = max;
            this.value = value;
        }

        public int min;
        public int max;
        public int value;

        public override void ApplyInputProperties(Dictionary<string, string> props)
        {
            props.Add("type", "number");
            props.Add("value", value.ToString());
            props.Add("min", min.ToString());
            props.Add("max", max.ToString());
        }

        public override object DeserializeResponse(StringValues value)
        {
            if (!int.TryParse(value, out int r))
                throw new Exception("Invalid number.");
            if (r > max)
                r = max;
            if (r < min)
                r = min;
            return r;
        }
    }
}
