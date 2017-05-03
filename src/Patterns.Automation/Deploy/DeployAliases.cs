using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cake.Common.IO;
using Cake.Common.Text;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Automation.Extensions;
using Automation.Layer0;
using Automation.Ssh;

namespace Automation.Deploy
{
    /// <summary>
    /// Top-level <c>Aliases</c> that wrap up common Layer0 deploy <c>Aliases</c>
    /// </summary>
    [CakeNamespaceImport("Automation.Deploy")]
    [CakeNamespaceImport("Patterns.Commands")]
    [CakeNamespaceImport("System.Linq")]
    public static class DeployAliases
    {
        public const string OutputPath = ".artifacts/";

        /// <summary>
        /// Get the path in which build/deploy artifacts should be created.
        /// <remarks>
        /// Defaults to <c>".artifacts/"</c>
        /// The output path will be created if it does not exist
        /// </remarks>
        /// </summary>
        /// <param name="context">The current <see cref="ICakeContext"/></param>
        /// <returns>The output path that will be used</returns>
        [CakeMethodAlias]
        public static string GetDefaultOutputPath(this ICakeContext context)
        {
            if (!context.DirectoryExists(OutputPath))
            {
                context.CreateDirectory(OutputPath);
            }

            return OutputPath;
        }

        /// <summary>
        /// Get the full length name of the current <see cref="Layer0Environment"/>
        /// based on the current <see cref="Release"/>.
        /// </summary>
        /// <param name="context">The current <see cref="ICakeContext"/></param>
        /// <param name="release">The current <see cref="Release"/></param>
        /// <returns>The generated environment name</returns>
        [CakeMethodAlias]
        public static string GetFullLayer0EnvironmentName(
            this ICakeContext context,
            Release release
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (release == null)
            {
                throw new ArgumentNullException(nameof(release));
            }

            return GenerateLayer0EnvironmentName(release);
        }

        /// <summary>
        /// Determine the <see cref="Release"/> from the name of the given
        /// <see cref="Layer0Environment"/>
        /// </summary>
        /// <param name="context">The current <see cref="ICakeContext"/></param>
        /// <param name="environmentName">The name of the environment</param>
        /// <returns>The generated environment name</returns>
        [CakeMethodAlias]
        public static Release GetReleaseFromEnvironmentName(
            this ICakeContext context,
            string environmentName
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (string.IsNullOrEmpty(environmentName))
            {
                throw new ArgumentNullException(nameof(environmentName));
            }

            var pieces = environmentName.Split('_');

            // api at a minimum will not follow any expected naming conventions, so make no
            // assumptions other than a single element
            return new Release
            {
                Environment = pieces[0],
                Application = pieces.Count() > 1 ? pieces[1] : null,
                Version = pieces.Count() > 2 ? pieces[2] : null
            };
        }

        /// <summary>
        /// Get or create a new <see cref="Layer0Environment"/> with the name
        /// constructed from the Release information.
        /// <remarks>
        /// The <see cref="Layer0Environment.Name"/> will be of the pattern
        /// <c>{<see cref="Release.Environment"/>_<see cref="Release.Version"/></c>
        /// </remarks>
        /// </summary>
        /// <param name="context">The current <see cref="ICakeContext"/></param>
        /// <param name="release">The current release information</param>
        /// <param name="createOptions">Additional environment options to apply</param>
        /// <returns></returns>
        [CakeMethodAlias]
        public static Layer0Environment L0EnvironmentGetOrCreate(
            this ICakeContext context,
            Release release,
            EnvironmentCreateOptions createOptions
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (createOptions == null)
            {
                throw new ArgumentNullException(nameof(createOptions));
            }
            
            var fullEnvName = GetFullLayer0EnvironmentName(context, release);

            var environmentGetCommand = new Layer0EnvironmentGetCommand(fullEnvName);

            var environmentCreateCommand = new Layer0EnvironmentCreateCommand(
                name: fullEnvName,
                instanceSize: createOptions.InstanceSize,
                userDataPath: createOptions.UserDataPath,
                minimumCount: createOptions.MinimumCount
            );

            var environment =
                context.L0EnvironmentGet(environmentGetCommand) ??
                context.L0EnvironmentCreate(environmentCreateCommand);

            if (environment == null)
            {
                throw new CakeException($"Unable to get or create environment {fullEnvName}");
            }

            return environment;
        }

        /// <summary>
        /// Get or create a new <see cref="Layer0Environment"/> with the name
        /// constructed from the Release information.
        /// <remarks>
        /// The <see cref="Layer0Environment.Name"/> will be of the pattern
        /// <c>{<see cref="Release.Environment"/>_<see cref="Release.Version"/></c>
        /// </remarks>
        /// </summary>
        /// <param name="context">The current <see cref="ICakeContext"/></param>
        /// <param name="release">The current release information</param>
        /// <returns></returns>
        [CakeMethodAlias]
        public static Layer0Environment L0EnvironmentGetOrCreate(
            this ICakeContext context,
            Release release
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.L0EnvironmentGetOrCreate(
                GetFullLayer0EnvironmentName(context, release)
            );
        }

        /// <summary>
        /// Will create or update a <see cref="Layer0LoadBalancer"/> and
        /// <see cref="Layer0Service"/> for any service. No additional
        /// <c>dockerrun.aws.json</c> template replacement will be performed.
        /// </summary>
        /// <param name="context">The current <see cref="ICakeContext"/></param>
        /// <param name="serviceDefinition">
        /// The <see cref="Layer0Service"/> to create or update
        /// </param>
        /// <param name="release">The current release information</param>
        /// <returns></returns>
        [CakeMethodAlias]
        public static Layer0Service EnsureService(
            this ICakeContext context,
            Layer0ServiceDefinition serviceDefinition,
            Release release
        )
        {
            return EnsureService(context, serviceDefinition, release, null);
        }

        /// <summary>
        /// Will create or update a <see cref="Layer0LoadBalancer"/> and
        /// <see cref="Layer0Service"/>.
        /// </summary>
        /// <remarks>
        /// Additional key/values will be used for <c>dockerrun.aws.json</c>
        /// template replacement during deploy creation
        /// </remarks>
        /// <param name="context">The current <see cref="ICakeContext"/></param>
        /// <param name="serviceDefinition">Service to get or create</param>
        /// <param name="release">The current release information</param>
        /// <param name="environmentOverrides">
        /// Additional environment varibles to be added to the
        /// <see cref="Layer0Service"/>'s <see cref="Layer0Deploy"/>
        /// </param>
        /// <returns>The service that was updated or created</returns>
        [CakeMethodAlias]
        public static Layer0Service EnsureService(
            this ICakeContext context,
            Layer0ServiceDefinition serviceDefinition,
            Release release,
            IDictionary<string, string> environmentOverrides
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (serviceDefinition is Layer0ServiceDefinitionWithConsul)
            {
                serviceDefinition = HandleServiceWithConsul(
                    context,
                    (Layer0ServiceDefinitionWithConsul)serviceDefinition,
                    release
                );
            }
            if (serviceDefinition is Layer0SshServiceDefinition)
            {
                serviceDefinition = HandleSshService(
                    context,
                    (Layer0SshServiceDefinition)serviceDefinition
                );
            }

            if (environmentOverrides != null)
            {
                serviceDefinition = AddEnvironmentVariables(
                    context,
                    environmentOverrides,
                    serviceDefinition
                );
            }

            return FinishService(
                context,
                serviceDefinition,
                release
            );
        }

        /// <summary>
        /// Will create or update a Consul <see cref="Layer0LoadBalancer"/>
        /// and <see cref="Layer0Service"/>.
        /// </summary>
        /// <remarks>
        /// Will create the Consul ELB and fill in the <c>dockerrun.aws.json</c>
        /// template values <c>%CONSUL_URL%</c> and <c>%CONSUL_SCALE%</c> with
        /// the URL of the Consul ELB and the Scale provided.
        /// </remarks>
        /// <param name="context">The current <see cref="ICakeContext"/></param>
        /// <param name="serviceDefinition">
        /// Definition of the Consul service and ELB
        /// </param>
        /// <param name="release">The current release</param>
        /// <returns></returns>
        [CakeMethodAlias]
        [CakeAliasCategory("Deploy")]
        public static Layer0Service EnsureConsulService(
            this ICakeContext context,
            Layer0ServiceDefinition serviceDefinition,
            Release release
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var consulLoadBalancer = context.L0LoadBalancerGetOrCreate(
                new Layer0LoadBalancer(
                    serviceDefinition.LoadBalancer.Name,
                    GetFullLayer0EnvironmentName(context, release),
                    serviceDefinition.LoadBalancer.PortConfiguration,
                    serviceDefinition.LoadBalancer.IsPublic
                ),
                serviceDefinition.LoadBalancer.CertificateName
            );

            var overrides = new Dictionary<string, string> {
                { "CONSUL_URL", consulLoadBalancer.Url },
                { "CONSUL_SCALE", serviceDefinition.Scale.ToString() }
            };

            return EnsureService(context, serviceDefinition, release, overrides);
        }

        /// <summary>
        /// enerate a deploy from the dockerrun file specified. If this is a templatized
        /// dockerrun file, the values provided will be used to fill in the template.
        /// <summary>
        [CakeMethodAlias]
        public static Layer0Deploy L0GenerateDeploy(
            this ICakeContext context,
            Layer0DeployDefinition deployDefinition,
            Release release
        )
        {
            return context.L0GenerateDeploy(deployDefinition, release, null);
        }

        /// <summary>
        /// Generate a deploy from the dockerrun file specified. If this is a templatized
        /// dockerrun file, the values provided will be used to fill in the template. Allows
        /// for additional template values to be provided.
        /// <summary>
        [CakeMethodAlias]
        public static Layer0Deploy L0GenerateDeploy(
            this ICakeContext context,
            Layer0DeployDefinition deployDefinition,
            Release release,
            Dictionary<string, string> environmentOverrides
        )
        {
            var deployName = GenerateDeployName(deployDefinition.Name, release);

            if (environmentOverrides != null)
            {
                foreach (var pair in environmentOverrides)
                {
                    try
                    {
                        deployDefinition.EnvironmentVariables.Add(pair.Key, pair.Value);
                    }
                    catch (ArgumentException)
                    {
                        context.Log.Error($"Attempted to add existing key '{pair.Key}'");
                    }
                }
            }

            var transformedDeployDefinitionPath = context.TransformDeployTemplate(
                deployDefinition,
                release,
                deployName
            );

            return context.L0DeployCreate(
                new Layer0DeployCreateCommand(
                    deployName,
                    transformedDeployDefinitionPath
                )
            );
        }

        private static Layer0Service FinishService(
            ICakeContext context,
            Layer0ServiceDefinition serviceDefinition,
            Release release
        )
        {
            var deploy = context.L0GenerateDeploy(
                serviceDefinition.DeployDefinition,
                release
            );

            var serviceLoadBalancer = new Layer0LoadBalancer();

            if (serviceDefinition.LoadBalancer != null)
            {
                // we're doing a lot of "building things from basically the same things"
                // here. need to do some rethinking around the need for all these objects
                // and definitions
                var loadBalancer = new Layer0LoadBalancer(
                    serviceDefinition.LoadBalancer.Name,
                    GetFullLayer0EnvironmentName(context, release),
                    serviceDefinition.LoadBalancer.PortConfiguration,
                    serviceDefinition.LoadBalancer.IsPublic
                );
                if (serviceDefinition.LoadBalancer.HealthCheck != null)
                {
                    loadBalancer.HealthCheckOptions = new Layer0HealthCheckOptions
                    {
                        Target = serviceDefinition.LoadBalancer.HealthCheck.Target,
                        Interval = serviceDefinition.LoadBalancer.HealthCheck.Interval,
                        Timeout = serviceDefinition.LoadBalancer.HealthCheck.Timeout,
                        HealthyThreshold = serviceDefinition.LoadBalancer.HealthCheck.HealthyThreshold,
                        UnhealthyThreshold = serviceDefinition.LoadBalancer.HealthCheck.UnhealthyThreshold,
                    };
                }

                serviceLoadBalancer = context.L0LoadBalancerGetOrCreate(
                    loadBalancer,
                    serviceDefinition.LoadBalancer.CertificateName
                );

                // if we created a loadbalancer we need to wait for the security
                // groups to apply before assigning a service
                Thread.Sleep(10 * 1000);
            }

            return context.L0ServiceUpdateOrCreate(
                new Layer0Service(
                    serviceDefinition.Name,
                    GetFullLayer0EnvironmentName(context, release),
                    serviceLoadBalancer.Name,
                    deploy.Id,
                    serviceDefinition.Scale
                ),
                serviceDefinition.WaitParameters
            );
        }

        private static string TransformDeployTemplate(
            this ICakeContext context,
            Layer0DeployDefinition deployDefinition,
            Release release,
            string deployName
        )
        {
            var outputPath = GetOutputPath(context, $"{deployName}.aws.json");

            var initialTransform = context.TransformTextFile(deployDefinition.TemplatePath)
                .WithToken("ENVIRONMENT", release.Environment)
                .WithToken("IDENTITY", deployName)
                .WithToken("REV", release.Version);

            foreach (var specialValue in deployDefinition.EnvironmentVariables)
            {
                initialTransform = initialTransform
                    .WithToken(specialValue.Key, specialValue.Value);
            }

            initialTransform.Save(outputPath);

            return outputPath;
        }

        private static string GenerateDeployName(string name, Release release)
        {
            return string.IsNullOrEmpty(release.Version)
                ? $"{release.Environment}_{name}"
                : $"{release.Environment}_{name}_{release.Version}";
        }

        private static string GenerateLayer0EnvironmentName(Release release)
        {
            var baseName = $"{release.Environment}_{release.Application}";

            return string.IsNullOrEmpty(release.Version)
                ? baseName
                : $"{baseName}_{release.Version}";
        }

        private static string GetOutputPath(ICakeContext context, string fileName)
        {
            return string.Concat(
                context.Arguments.GetArgument("OutputPath") ?? GetDefaultOutputPath(context),
                fileName
            );
        }

        private static string GetEnvironmentVariable(this ICakeContext context, string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
            {
                context.Log.Warning(name, $"Environment variable {name} was not set");
            }
            return value;
        }

        private static Layer0ServiceDefinition AddEnvironmentVariables(
            ICakeContext context,
            IDictionary<string, string> overrides,
            Layer0ServiceDefinition serviceDefinition
        )
        {
            foreach (var pair in overrides)
            {
                try
                {
                    serviceDefinition
                        .DeployDefinition
                        .EnvironmentVariables
                        .Add(pair.Key, pair.Value);
                }
                catch (ArgumentException)
                {
                    context.Log.Error($"Attempted to add existing key '{pair.Key}'");
                }
            }
            return serviceDefinition;
        }

        private static Layer0ServiceDefinition HandleServiceWithConsul(
            ICakeContext context,
            Layer0ServiceDefinitionWithConsul serviceDefinition,
            Release release
        )
        {
            var consulLoadBalancerUrl = context.L0LoadBalancerGet(
                new Layer0LoadBalancerGetCommand(
                    serviceDefinition.ConsulLoadBalancerName,
                    GetFullLayer0EnvironmentName(context, release)
                )).Url;

            var environmentOverrides = new Dictionary<string, string>
            {
                { "CONSUL_URL", consulLoadBalancerUrl }
            };

            return AddEnvironmentVariables(
                context,
                environmentOverrides,
                serviceDefinition
            );
        }

        private static Layer0ServiceDefinition HandleSshService(
            ICakeContext context,
            Layer0SshServiceDefinition serviceDefinition
        )
        {
            var keyPair = context.SshGenerateKeyPair(
                new SshGenerateKeyPairCommand(
                    serviceDefinition.SshKeyPath
                )
            );

            var environmentOverrides = new Dictionary<string, string>
            {
                { "PUBLIC_KEY", keyPair.PublicKeyContent }
            };

            return AddEnvironmentVariables(
                context,
                environmentOverrides,
                serviceDefinition
            );
        }
    }
}
