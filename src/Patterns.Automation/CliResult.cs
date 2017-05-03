using System.Collections.Generic;
using System.Diagnostics;

namespace Automation
{
    public class CliResult
    {
        private readonly Process process;

        public int ExitCode { get; private set; }
        public IEnumerable<string> StandardOut { get; private set; }
        public IEnumerable<string> StandardError { get; private set; }

        public bool IsSuccess { get { return ExitCode == 0; } }

        public CliResult(
            Process process,
            IEnumerable<string> standardOut,
            IEnumerable<string> standardError
        )
        {
            this.process = process;

            this.ExitCode = process.ExitCode;
            this.StandardOut = standardOut;
            this.StandardError = standardError;
        }
    }
}
