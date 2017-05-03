using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Automation
{
    public class CliRunner : ICliRunner
    {
        public Task<CliResult> RunProcessAsync(
            string executablePath,
            params string[] args
        )
        {
            return RunProcessAsync(
                executablePath,
                CancellationToken.None,
                args
            );
        }

        public Task<CliResult> RunProcessAsync(
            string executablePath,
            CancellationToken cancellationToken,
            params string[] args
        )
        {
            var completionSource = new TaskCompletionSource<CliResult>();

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };

            var standardOutput = new List<string>();
            var standardError = new List<string>();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    // Console.WriteLine($"Out: {e.Data}");
                    standardOutput.Add(e.Data);
                }
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    // Console.WriteLine($"Err: {e.Data}");
                    standardError.Add(e.Data);
                }
            };

            process.Exited += (sender, e) =>
            {
                process.WaitForExit();
                completionSource.SetResult(
                    new CliResult(process, standardOutput, standardError)
                );
            };

            cancellationToken.Register(() =>
            {
                completionSource.TrySetCanceled();
                process.CloseMainWindow();
            });

            cancellationToken.ThrowIfCancellationRequested();

            if (process.Start() == false)
            {
                completionSource.TrySetException(
                    new InvalidOperationException("Failed to start process")
                );
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return completionSource.Task;
        }
    }
}
