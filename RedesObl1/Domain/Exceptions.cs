using System;

namespace Domain
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
        public NotFoundException() : base()
        {
        }
    }

    public class AlreadyExistsException : Exception
    {
        public AlreadyExistsException(string message) : base(message)
        {
        }
        public AlreadyExistsException() : base()
        {
        }
    }

    public class AlreadyModifyingException : Exception
    {
        public AlreadyModifyingException(string message) : base(message)
        {
        }
        public AlreadyModifyingException() : base()
        {
        }
    }
}