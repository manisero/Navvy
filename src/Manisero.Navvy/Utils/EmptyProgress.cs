using System;

namespace Manisero.Navvy.Utils
{
    internal class EmptyProgress<T> : IProgress<T>
    {
        public void Report(T value)
        {
        }
    }
}
