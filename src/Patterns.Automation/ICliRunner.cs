using System.Threading;
using System.Threading.Tasks;

namespace Automation
{
    public interface ICliRunner
    {
        Task<CliResult> RunProcessAsync(
            string executablePath,
            params string[] args
        );

        Task<CliResult> RunProcessAsync(
            string executablePath,
            CancellationToken cancellationToken,
            params string[] args
        );
    }
}
