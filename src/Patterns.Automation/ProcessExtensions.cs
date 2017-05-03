using Cake.Core.IO;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Automation
{
    public static class ProcessExtensions
    {
        public static bool WasSuccessful(
            this IProcess process,
            out IEnumerable<string> outputLines
        )
        {
            process.WaitForExit();

            // ToList the standardoutput enumerable because it's reading a stream.
            // ToList captures the output as a List because you can't re-read this stream.
            outputLines = process.GetStandardOutput().ToList();

            return process.GetExitCode() == 0;
        }

        public static bool WasSuccessful(
            this IProcess process,
            out string fullOutput
        )
        {
            process.WaitForExit();

            fullOutput = string.Join(Environment.NewLine, process.GetStandardOutput().ToArray());

            return process.GetExitCode() == 0;
        }

        public static bool WasSuccessful(
            this IProcess process
        )
        {
            process.WaitForExit();
            return process.GetExitCode() == 0;
        }
    }
}
