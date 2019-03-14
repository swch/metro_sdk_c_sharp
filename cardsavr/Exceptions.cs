using System;


namespace Switch.CardSavr.Exceptions
{
    public class InvalidSafeKeyException: Exception
    {
        public InvalidSafeKeyException()
        {
        }
        public InvalidSafeKeyException(string message)
            : base(message)
        {
        }
        public InvalidSafeKeyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class InvalidSignatureException : Exception
    {
        public InvalidSignatureException()
        {
        }
        public InvalidSignatureException(string message)
            : base(message)
        {
        }
        public InvalidSignatureException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class InvalidQueryException : Exception
    {
        public InvalidQueryException()
        {
        }
        public InvalidQueryException(string message)
            : base(message)
        {
        }
        public InvalidQueryException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class RequestException : Exception
    {
        public RequestException()
        {
        }
        public RequestException(string message)
            : base(message)
        {
        }
        public RequestException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
