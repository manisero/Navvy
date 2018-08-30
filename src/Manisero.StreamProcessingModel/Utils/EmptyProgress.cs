using System;

namespace Manisero.StreamProcessingModel.Utils
{
    internal class EmptyProgress<T> : IProgress<T>
    {
        public void Report(T value)
        {
        }
    }
}
