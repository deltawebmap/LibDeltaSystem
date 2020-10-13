using LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes;
using LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.Extras.HTMLForm
{
    public class HTMLFormProcessor<T>
    {
        public HTMLFormProcessor(string submitBtnText)
        {
            this.submitBtnText = submitBtnText;
        }

        public string submitBtnText;
        
        /// <summary>
        /// Generates the form HTML
        /// </summary>
        /// <returns></returns>
        public string BuildHTML()
        {
            //Begin building
            string html = "<form method=\"post\">";
            
            //Loop through properties
            var props = typeof(T).GetProperties();
            foreach(var p in props)
            {
                //Get property
                var attrib = p.GetCustomAttribute<BaseFormElementType>();
                if (attrib == null)
                    throw new Exception("No form element specified for " + p.Name);

                //Build
                html += attrib.GenerateHTML();
                html += "<br>";
            }

            //End building
            html += $"<br><input type=\"submit\" value=\"{submitBtnText}\"></form>";
            return html;
        }

        public async Task ProcessResponse(T output, HttpContext e)
        {
            //Deserialize
            var form = await e.Request.ReadFormAsync();
            
            //Loop through properties
            var props = typeof(T).GetProperties();
            foreach (var p in props)
            {
                //Get property
                var attrib = p.GetCustomAttribute<BaseFormElementType>();
                if (attrib == null)
                    throw new Exception("No form element specified for " + p.Name);

                //Find
                if (!form.ContainsKey(attrib.id))
                    throw new Exception("Incomplete form.");

                //Read
                object r = attrib.DeserializeResponse(form[attrib.id]);

                //Apply
                p.SetValue(output, r);
            }
        }
    }
}
