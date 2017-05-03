using System;
using System.Collections.Generic;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Tooling;
using Patterns.Commands;

namespace Automation.Ecs
{
    /// <summary>
    /// Represents an <c>update task definition</c> command to execute.
    /// </summary>
    public class EcsUpdateTaskDefinitionCommand : ToolAction<ToolSettings>
    {
        /// <summary>
        /// Path to the Task definition
        /// </summary>
        public string DefinitionPath { get; }

        /// <summary>
        /// The Family of the Task
        /// </summary>
        public string TaskFamily { get; }

        public EcsUpdateTaskDefinitionCommand(
            string definitionPath,
            string taskFamily
        )
        {
            DefinitionPath = definitionPath;
            TaskFamily = taskFamily;
        }

        public class Handler : Tool<ToolSettings>, IActionHandler<EcsUpdateTaskDefinitionCommand>
        {
            private readonly ICakeLog log;

            public Handler(
                ICakeContext cakeContext
            ) : base(cakeContext.FileSystem, cakeContext.Environment, cakeContext.ProcessRunner, cakeContext.Globber)
            {
                log = cakeContext.Log;
            }

            public void Execute(EcsUpdateTaskDefinitionCommand command)
            {
                if (command == null)
                {
                    throw new ArgumentNullException(nameof(command));
                }

                Run(command.Settings, BuildArguments(command));
            }

            private ProcessArgumentBuilder BuildArguments(EcsUpdateTaskDefinitionCommand command)
            {
                var builder = new ProcessArgumentBuilder();

                builder.Append("ecs");
                builder.Append("register-task-definition");

                builder.AppendSwitch("--family", command.TaskFamily);

                builder.AppendSwitch("--cli-input-json", $"file://{command.DefinitionPath}");

                return builder;
            }

            protected override IEnumerable<string> GetToolExecutableNames()
            {
                return new[] { "aws", "aws.exe" };
            }

            protected override string GetToolName()
            {
                return "AWS ECS";
            }
        }
    }
}
