﻿using System;

namespace SereneMarine_Web.Helpers
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }

        public string Content { get; set; }

        public string GetApiErrorMessage()
        {
            return "ERROR: Status Code: " + StatusCode + " " + Content;
        }
    }
}
