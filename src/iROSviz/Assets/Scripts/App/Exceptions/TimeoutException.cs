using System;

namespace App.Exceptions
{
    public class TimeoutException : Exception
    {
        public string message;

        public TimeoutException()
        {
        }

        public TimeoutException(string message) : base(message)
        {
            this.message = message;
        }

        public TimeoutException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }
}
