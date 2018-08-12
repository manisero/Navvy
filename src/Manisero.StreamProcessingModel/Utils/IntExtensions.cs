namespace Manisero.StreamProcessingModel.Utils
{
    internal static class IntExtensions
    {
        public static byte ToPercentageOf(this int value, int total) => (byte)(value * 100 / total);
    }
}
