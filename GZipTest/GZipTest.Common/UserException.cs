using System;

namespace GZipTest.Common
{
    /// <summary>
    ///     Exception for expected user errors
    /// </summary>
    public class UserException : ApplicationException
    {
        public UserException(string message) : base(message) { }
    }
}