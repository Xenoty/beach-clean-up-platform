using Newtonsoft.Json.Linq;
using System;

namespace SereneMarine_Web.Helpers
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }

        public string Content { get; set; }

        private JObject contentJObject { get { return JObject.Parse(Content); } }

        private string ContentMessage { get { return (string)contentJObject["message"]; } }

        public string GetApiErrorMessage()
        {
            return "ERROR: Status Code(" + StatusCode + ") - \"" + ContentMessage + " \"" ;
        }
    }
}