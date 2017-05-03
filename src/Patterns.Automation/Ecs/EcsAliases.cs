using System;
using Cake.Core;
using Cake.Core.Annotations;

namespace Automation.Ecs
{
    /// <summary>
    /// ECS commands that can be run from Cake tasks.
    /// </summary>
    [CakeNamespaceImport("Automation.Ecs")]
    [CakeNamespaceImport("Patterns.Commands")]
    public static class EcsAliases
    {
        /// <summary>
        /// Update an existing ECS task definition
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        [CakeMethodAlias]
        public static void EcsUpdateTaskDefinition(
            this ICakeContext context,
            EcsUpdateTaskDefinitionCommand command
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var handler = new EcsUpdateTaskDefinitionCommand.Handler(context);
            handler.Execute(command);
        }
    }
}
