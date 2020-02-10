using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.WebFramework
{
    /// <summary>
    /// Defines a web service and it's information
    /// </summary>
    public abstract class DeltaWebServiceDefinition
    {
        public const string ARG_SERVER = "SERVER";
        public const string ARG_TRIBE = "TRIBE";
        
        /// <summary>
        /// Returns a DeltaWebService used to handle the request
        /// </summary>
        /// <returns></returns>
        public abstract DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e);

        /// <summary>
        /// Gets the template URL that is used to check this. Args passed as "{ARG}" are interpeted as wildcard args
        /// </summary>
        /// <returns></returns>
        public abstract string GetTemplateUrl();
    }
}
