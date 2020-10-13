using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types
{
    public class FormElementTypeTextarea : BaseFormElementType
    {
        public FormElementTypeTextarea(string title, string id, string defaultValue = "", int rows = 4, int cols = 80) : base(id)
        {
            this.title = title;
            this.defaultValue = defaultValue;
            this.rows = rows;
            this.cols = cols;
        }

        public string title;
        public string defaultValue;
        public int rows;
        public int cols;

        public override string GenerateHTML()
        {
            return $"<label for=\"{id}\">{HttpUtility.HtmlEncode(title)}</label><br><textarea id=\"{id}\" name=\"{id}\" rows=\"{rows}\" cols=\"{cols}\">{HttpUtility.HtmlEncode(defaultValue)}</textarea>";
        }

        public override object DeserializeResponse(StringValues value)
        {
            return (string)value;
        }
    }
}
