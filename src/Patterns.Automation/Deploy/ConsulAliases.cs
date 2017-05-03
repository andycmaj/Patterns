using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Core;
using Cake.Core.Annotations;
using Patterns.Automation.Ssh;

namespace Automation.Deploy
{
    /// <summary>
    /// Combines common <c>Layer0 Aliases</c> and <c>Ssh Aliases</c> to simplify
    /// <c>consul</c> operations.
    /// </summary>
    [CakeAliasCategory("Deploy")]
    [CakeNamespaceImport("Patterns.Automation.Deploy")]
    [CakeNamespaceImport("System.Linq")]
    public static class ConsulAliases
    {
        /// <summary>
        /// Sets key/values in <c>consul</c> for each consulKey passed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="consulLoadBalancerName"></param>
        /// <param name="sshLoadBalancerName"></param>
        /// <param name="environment"></param>
        /// <param name="sshKeyPath"></param>
        /// <param name="keyPrefix"></param>
        /// <param name="consulKeyValues"></param>
        [CakeMethodAlias]
        [CakeAliasCategory("Deploy")]
        public static void SetConsulKvs(
            this ICakeContext context,
            string consulLoadBalancerName,
            string sshLoadBalancerName,
            string environment,
            string sshKeyPath,
            string keyPrefix,
            IDictionary<string, string> consulKeyValues
        )
        {
            // TODO: this whole thing simplifies the build.cake file but is super ugly
            // if we want to keep this for future users, should probably spend some more time
            // cleaning this up
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // var consulLoadBalancer = context.L0LoadBalancerGet(
            //     new Layer0LoadBalancerGetCommand(
            //         consulLoadBalancerName,
            //         environment
            //     )
            // );
            // var sshLoadBalancer = context.L0LoadBalancerGet(
            //     new Layer0LoadBalancerGetCommand(
            //         sshLoadBalancerName,
            //         environment
            //     )
            // );

            foreach (var keyValue in consulKeyValues)
            {
                var command = $"curl -sLf -w %{{http_code}} -X PUT -d {keyValue.Value} http://{consulLoadBalancer.Url}:8500/v1/kv/{keyPrefix}/{keyValue.Key}";
                context.SshRunRemotely(
                    new SshRunRemotelyCommand(
                        privateKeyPath: sshKeyPath,
                        host: sshLoadBalancer.Url,
                        command: command,
                        port: sshLoadBalancer.Ports.Single().ListensOn
                    )
                );
            }
        }
    }
}
