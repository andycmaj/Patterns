using System;
using Cake.Core;
using Cake.Core.Annotations;

namespace Automation.Ssh
{
    /// <summary>
    /// Ssh commands that can be called from a cake task.
    /// </summary>
    [CakeAliasCategory("Ssh")]
    [CakeNamespaceImport("Automation.Ssh")]
    [CakeNamespaceImport("Patterns.Commands")]
    public static class SshAliases
    {
        /// <summary>
        /// Generate a new public/private key pair
        /// </summary>
        /// <param name="context">Current <see cref="ICakeContext"/></param>
        /// <param name="command">The <c>generage keypair</c> command arguments</param>
        /// <returns></returns>
        [CakeMethodAlias]
        [CakeAliasCategory("Deploy")]
        public static SshKeyPair SshGenerateKeyPair(
            this ICakeContext context,
            SshGenerateKeyPairCommand command
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var handler = new SshGenerateKeyPairCommand.Handler(context);
            return handler.Execute(command);
        }

        /// <summary>
        /// Run a remote command from the Ssh service
        /// </summary>
        /// <param name="context">The current <see cref="ICakeContext"/></param>
        /// <param name="command">The <c>run remotely</c> command arguments</param>
        [CakeMethodAlias]
        [CakeAliasCategory("Deploy")]
        public static void SshRunRemotely(
            this ICakeContext context,
            SshRunRemotelyCommand command
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var handler = new SshRunRemotelyCommand.Handler(context);
            handler.Execute(command);
        }
    }
}
