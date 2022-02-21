using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace SereneMarine_Web.Helpers
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }

        public string Content { get; set; }

        public ApiException()
        {

        }

        public ApiException(HttpResponseMessage httpResponseMessage)
        {
            var testing = httpResponseMessage.Content.ReadAsStringAsync();
            string response = testing.Result;

            if (string.IsNullOrEmpty(response))
            {
                response = httpResponseMessage.ReasonPhrase;
            }
            else
            {
                JObject contentJObject = JObject.Parse(response);
                response = contentJObject["message"].ToString();
            }

            StatusCode = (int)httpResponseMessage.StatusCode;
            Content = response;
        }

        public string GetApiErrorMessage()
        {
            return "ERROR: Status Code(" + StatusCode + ") - \"" + Content + " \"" ;
        }
    }
}