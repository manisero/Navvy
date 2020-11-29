namespace Manisero.Navvy.Logging.Diagnostics
{
    public struct GlobalDiagnostic
    {
        public int ProcessorCount { get; }

        public GlobalDiagnostic(
            int processorCount)
        {
            ProcessorCount = processorCount;
        }
    }
}
