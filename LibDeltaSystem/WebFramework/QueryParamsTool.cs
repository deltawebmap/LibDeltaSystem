using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.WebFramework
{
    public static class QueryParamsTool
    {
        public static int? GetOptionalIntField(HttpContext e, string name)
        {
            if (!e.Request.Query.ContainsKey(name))
                return null;
            if (int.TryParse(e.Request.Query[name], out int r))
                return r;
            else
                return null;
        }

        public static float? GetOptionalFloatField(HttpContext e, string name)
        {
            if (!e.Request.Query.ContainsKey(name))
                return null;
            if (float.TryParse(e.Request.Query[name], out float r))
                return r;
            else
                return null;
        }

        public static bool TryGetIntField(HttpContext e, string name, out int value)
        {
            int? v = GetOptionalIntField(e, name);
            if (v.HasValue)
            {
                value = v.Value;
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }

        public static bool TryGetFloatField(HttpContext e, string name, out float value)
        {
            float? v = GetOptionalFloatField(e, name);
            if (v.HasValue)
            {
                value = v.Value;
                return true;
            } else
            {
                value = 0;
                return false;
            }
        }
    }
}
