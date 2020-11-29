using System.Collections.Concurrent;
using System.Collections.Generic;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.Logging.Diagnostics
{
    public class DiagnosticsLog
    {
        public GlobalDiagnostic GlobalDiagnostic { get; }

        private readonly ConcurrentBag<Diagnostic> _diagnostics = new ConcurrentBag<Diagnostic>();

        public IReadOnlyCollection<Diagnostic> Diagnostics => _diagnostics;

        /// <summary>Note: wrapped in reference type to ensure atomic access to Diagnostic.</summary>
        private Wrapper<Diagnostic> _latestDiagnostic;

        public DiagnosticsLog(
            GlobalDiagnostic globalDiagnostic,
            Diagnostic firstDiagnostic)
        {
            GlobalDiagnostic = globalDiagnostic;
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
    }
}
