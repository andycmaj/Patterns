using System;
using System.Collections.Generic;
using System.Text;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Tooling;
using Patterns.Commands;

namespace Automation.Ssh
{
    /// <summary>
    /// Represents a <c>generate key pair</c> command to execute.
    /// </summary>
    public class SshGenerateKeyPairCommand : ToolFunction<ToolSettings, SshKeyPair>
    {
        /// <summary>
        /// Path to where the key pair should be saved
        /// </summary>
        public string KeyFilePath { get; }

        /// <summary>
        /// Use <c>true</c> if an existing key pair should be replaced
        /// </summary>
        public bool ReplaceExistingKey { get; }

        public SshGenerateKeyPairCommand(string keyFilePath, bool replaceExistingKey = false)
        {
            KeyFilePath = keyFilePath;
            ReplaceExistingKey = replaceExistingKey;
        }

        public class Handler :
            Tool<ToolSettings>,
            IFunctionHandler<SshGenerateKeyPairCommand, SshKeyPair>
        {
            private readonly ICakeLog log;
            private readonly IFileSystem fileSystem;

            public Handler(
                ICakeContext context
            ) : base(
                context.FileSystem,
                context.Environment,
                context.ProcessRunner,
                context.Globber
            )
            {
                log = context.Log;
                fileSystem = context.FileSystem;
            }

            public SshKeyPair Execute(SshGenerateKeyPairCommand command)
            {
                if (string.IsNullOrWhiteSpace(command.KeyFilePath))
                {
                    throw new ArgumentNullException(nameof(command.KeyFilePath));
                }

                var privateKeyPath = fileSystem.GetFile(command.KeyFilePath).Path;
                var publicKeyPath = fileSystem.GetFile($"{privateKeyPath}.pub").Path;

                // Delete keys if they already exist.
                if (fileSystem.Exist(privateKeyPath))
                {
                    if (command.ReplaceExistingKey)
                    {
                        fileSystem.GetFile(privateKeyPath).Delete();
                        fileSystem.GetFile(publicKeyPath).Delete();
                    }
                    else
                    {
                        return new SshKeyPair(
                            GetPublicKeyContent(publicKeyPath),
                            privateKeyPath.FullPath
                        );
                    }
                }

                GenerateKeys(command, privateKeyPath, publicKeyPath);

                string publicKeyContent = GetPublicKeyContent(publicKeyPath);

                return new SshKeyPair(publicKeyContent, privateKeyPath.FullPath);
            }

            private string GetPublicKeyContent(FilePath publicKeyPath)
            {
                return string.Join(
                        Environment.NewLine,
                        fileSystem.GetFile(publicKeyPath).ReadLines(Encoding.ASCII)
                    );
            }

            private void GenerateKeys(SshGenerateKeyPairCommand command, FilePath privateKeyPath, FilePath publicKeyPath)
            {
                var process = RunProcess(command.Settings, BuildArguments(command));
                if (!process.WasSuccessful())
                {
                    throw new CakeException("Failed to generate ssh keypair for SSHd service");
                }

                if (!(
                    fileSystem.Exist(privateKeyPath) &&
                    fileSystem.Exist(publicKeyPath)
                ))
                {
                    throw new CakeException(
                        $"Could not find generated keys at {command.KeyFilePath}"
                    );
                }
            }

            protected ProcessArgumentBuilder BuildArguments(
                SshGenerateKeyPairCommand command
            )
            {
                var builder = new ProcessArgumentBuilder();

                // quiet
                builder.Append("-q");

                // no passphrase
                builder.AppendSwitchQuoted("-N", string.Empty);

                // set output file
                builder.AppendSwitchQuoted("-f", command.KeyFilePath);

                return builder;
            }

            protected override IEnumerable<string> GetToolExecutableNames()
            {
                return new[] { "ssh-keygen", "ssh-keygen.exe" };
            }

            protected override string GetToolName()
            {
                return "ssh-keygen";
            }
        }
    }
}
