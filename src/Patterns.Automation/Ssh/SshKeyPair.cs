namespace Automation.Ssh
{
    public class SshKeyPair
    {
        public string PublicKeyContent { get; }
        public string PrivateKeyPath { get; }

        public SshKeyPair(string publicKeyContent, string privateKeyPath)
        {
            PublicKeyContent = publicKeyContent;
            PrivateKeyPath = privateKeyPath;
        }
    }
}
