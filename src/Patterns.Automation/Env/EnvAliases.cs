using System;
using Cake.Common;
using Cake.Core;
using Cake.Core.Annotations;

namespace Automation.Env
{
    /// <summary>
    /// Top-level aliases that interact with the environment
    /// </summary>
    public static class EnvAliases
    {
        /// <summary>
        /// Get an environment variable's value, if the environment variable
        /// is not set, throw an exception
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name">Name of the environment variable</param>
        /// <returns></returns>
        [CakeMethodAlias]
        [CakeAliasCategory("Env")]
        public static string RequireEnvironmentVariable(
            this ICakeContext context,
            string name
        )
        {
            var value = context.EnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception($"Required environment variable {name} is not set");
            }
            return value;
        }
    }
}
