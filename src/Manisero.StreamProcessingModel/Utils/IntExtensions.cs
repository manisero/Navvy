namespace Manisero.StreamProcessingModel.Utils
{
    public static class IntExtensions
    {
        public static byte ToPercentageOf(this int value, int total) => (byte)(value * 100 / total);
    }
}
