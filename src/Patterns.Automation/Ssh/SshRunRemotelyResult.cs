namespace Automation.Ssh
{
    public class SshRunRemotelyResult
    {
        public int ExitCode { get; }
        public string Output { get; }

        public SshRunRemotelyResult(int exitCode, string output)
        {
            ExitCode = exitCode;
            Output = output;
        }
    }
}
