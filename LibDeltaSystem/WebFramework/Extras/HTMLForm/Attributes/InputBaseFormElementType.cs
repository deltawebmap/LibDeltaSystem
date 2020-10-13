using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes
{
    public abstract class InputBaseFormElementType : BaseFormElementType
    {
        public InputBaseFormElementType(string title, string id) : base(id)
        {
            this.title = title;
        }

        public string title;
        
        public override string GenerateHTML()
        {
            //Apply input properties
            Dictionary<string, string> props = new Dictionary<string, string>();
            props.Add("id", id);
            props.Add("name", id);
            ApplyInputProperties(props);

            //Build
            string html = $"<label for=\"{id}\">{HttpUtility.HtmlEncode(title)}</label><br><input";
            foreach (var p in props)
                html += " " + p.Key + "=\"" + p.Value + "\"";
            return html + "/>";
        }

        public abstract void ApplyInputProperties(Dictionary<string, string> props);
    }
}
