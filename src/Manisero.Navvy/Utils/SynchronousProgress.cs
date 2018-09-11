using System;

namespace Manisero.Navvy.Utils
{
    internal class SynchronousProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;

        public SynchronousProgress(
            Action<T> handler)
        {
            _handler = handler;
        }

        public void Report(T value)
            => _handler(value);
    }
}
