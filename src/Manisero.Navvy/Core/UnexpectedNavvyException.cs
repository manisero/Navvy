using System;

namespace Manisero.Navvy.Core
{
    public class UnexpectedNavvyException : Exception
    {
        public UnexpectedNavvyException(
            string errorSpecificMessage,
            Exception innerException = null)
            : base(FormatMessage(errorSpecificMessage), innerException)
        {
        }

        private static string FormatMessage(
            string errorSpecificMessage)
            => $"Unexpected Navvy exception occured: '{errorSpecificMessage}'. To help fix it, please create and issue in Navvy GitHub repository (https://github.com/manisero/Navvy).";
    }
}
