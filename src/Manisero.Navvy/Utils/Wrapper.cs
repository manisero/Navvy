namespace Manisero.Navvy.Utils
{
    internal class Wrapper<TWrapped>
    {
        public TWrapped Wrapped { get; }

        public Wrapper(TWrapped wrapped)
        {
            Wrapped = wrapped;
        }
    }
}
