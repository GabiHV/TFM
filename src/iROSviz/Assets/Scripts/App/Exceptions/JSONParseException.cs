using UnityEngine;

using System;

namespace App.Exceptions
{
    public class JSONParseException : Exception
    {
        public string message;
        public JSONParseException()
        {
        }

        public JSONParseException(string message) : base(message)
        {
            this.message = message;
        }

        public JSONParseException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }
}
