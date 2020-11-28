using System.Collections.Concurrent;
using System.Collections.Generic;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.Logging.Diagnostics
{
    public class DiagnosticsLog
    {
        private readonly ConcurrentBag<Diagnostic> _diagnostics = new ConcurrentBag<Diagnostic>();

        /// <summary>Note: wrapped in reference type to ensure atomic access to Diagnostic.</summary>
        private Wrapper<Diagnostic> _latestDiagnostic;

        public DiagnosticsLog(
            Diagnostic firstDiagnostic)
        {
            AddDiagnostic(firstDiagnostic);
        }

        public void AddDiagnostic(
            Diagnostic diagnostic)
        {
            _diagnostics.Add(diagnostic);
            _latestDiagnostic = new Wrapper<Diagnostic>(diagnostic);
        }

        public bool HasFirstDiagnosticOnly() => _diagnostics.Count == 1;

        public Diagnostic GetLatestDiagnostic() => _latestDiagnostic.Wrapped;

        public IReadOnlyCollection<Diagnostic> GetDiagnostics() => _diagnostics;
    }
}
