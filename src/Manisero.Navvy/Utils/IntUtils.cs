namespace Manisero.Navvy.Utils
{
    internal static class IntUtils
    {
        public static int CeilingOfDivisionBy(
            this int value,
            int divisor)
        {
            var intQuotient = value / divisor;
            var remainder = value % divisor;

            return remainder == 0
                ? intQuotient
                : intQuotient + 1;
        }
    }
}
