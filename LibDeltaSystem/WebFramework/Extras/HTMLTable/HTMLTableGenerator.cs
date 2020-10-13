using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace LibDeltaSystem.WebFramework.Extras.HTMLTable
{
    public class HTMLTableGenerator<T>
    {
        public HTMLTableGenerator(List<string> titles, HTMLTableDelegate<T> handler)
        {
            this.titles = titles;
            this.handler = handler;
        }

        private List<string> titles;
        private HTMLTableDelegate<T> handler;

        public string GenerateTable(IEnumerable<T> data, string style = "")
        {
            //Build
            string html = $"<table style=\"{style}\">";
            html += BuildRow(titles, "tr", "td", "<b>", "</b>");
            foreach (var d in data)
                html += BuildRow(handler(d));
            html += "</table>";
            return html;
        }

        private string BuildRow(List<string> data, string rowType = "tr", string colType = "td", string hPre = "", string hPost = "")
        {
            //Build
            string html = "<" + rowType + ">";
            foreach(var d in data)
            {
                html += "<" + colType + ">" + hPre + d + hPost + "</" + colType + ">";
            }
            html += "</" + rowType + ">";
            return html;
        }
    }
}
