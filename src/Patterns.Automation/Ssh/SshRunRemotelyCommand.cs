using System;
using System.Collections.Generic;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Tooling;
using Patterns.Commands;

namespace Automation.Ssh
{
    /// <summary>
    /// Represents a <c>run remotely</c> command to execute.
    /// </summary>
    public class SshRunRemotelyCommand : ToolFunction<ToolSettings, SshRunRemotelyResult>
    {
        /// <summary>
        /// Path to the existing private key
        /// </summary>
        public string PrivateKeyPath { get; }

        /// <summary>
        /// The user to run the remote command as
        /// </summary>
        public string User { get; }

        /// <summary>
        /// The host to run the command on
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// The host port to run the command on
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// The remote command to run
        /// </summary>
        public string Command { get; }

        public SshRunRemotelyCommand(
            string privateKeyPath,
            string host,
            string command,
            string user = "root",
            int port = 22
        )
        {
            PrivateKeyPath = privateKeyPath;
            Host = host;
            Command = command;
            User = user;
            Port = port;
        }

        public class Handler :
            Tool<ToolSettings>,
            IFunctionHandler<SshRunRemotelyCommand, SshRunRemotelyResult>
        {
            private readonly ICakeLog log;

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
            }

            public SshRunRemotelyResult Execute(SshRunRemotelyCommand command)
            {
                if (string.IsNullOrWhiteSpace(command.Command))
                {
                    throw new ArgumentNullException(nameof(command.Command));
                }
                if (string.IsNullOrWhiteSpace(command.Host))
                {
                    throw new ArgumentNullException(nameof(command.Host));
                }

                log.Information(
                    "Running '{0}' remotely on {1}:{2}",
                    command.Command,
                    command.Host,
                    command.Port
                );

                Run(command.Settings, BuildArguments(command));

                // Log extra newline in case command output doesn't end in a linebreak
                log.Information(Environment.NewLine);

                return new SshRunRemotelyResult(0, null);
            }

            private ProcessArgumentBuilder BuildArguments(SshRunRemotelyCommand command)
            {
                var builder = new ProcessArgumentBuilder();

                // bypass known_hosts
                builder.AppendSwitch("-o", "CheckHostIP=no");

                // TODO: temp workaround to add our host to the trusted list
                builder.AppendSwitch("-o", "StrictHostKeyChecking=no");

                // use the specified private key
                builder.AppendSwitch("-i", command.PrivateKeyPath);

                // connect on the specified port.
                builder.AppendSwitch("-p", command.Port.ToString());

                // user and/or host
                if (string.IsNullOrWhiteSpace(command.User))
                {
                    builder.Append(command.Host);
                }
                else
                {
                    builder.Append($"{command.User}@{command.Host}");
                }

                // quoted remote command to run
                builder.AppendQuoted(command.Command);

                return builder;
            }

            protected override IEnumerable<string> GetToolExecutableNames()
            {
                return new[] { "ssh", "ssh.exe" };
            }

            protected override string GetToolName()
            {
                return "SSH";
            }
        }
    }
}
