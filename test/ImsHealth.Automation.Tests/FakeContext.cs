using System;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Tooling;
using Cake.Testing;
using FakeItEasy;

namespace Automation.Tests
{
    public sealed class FakeContext : ICakeContext
    {
        public FakeContext(
            IFileSystem fileSystem = null,
            ICakeEnvironment environment = null,
            IGlobber globber = null,
            IProcessRunner processRunner = null,
            ICakeArguments arguments = null
        )
        {
            Environment = environment ?? FakeEnvironment.CreateUnixEnvironment();
            FileSystem = fileSystem ?? new FakeFileSystem(Environment);
            Globber = globber ?? new Globber(FileSystem, Environment);
            ProcessRunner = processRunner ?? A.Fake<IProcessRunner>();
            Arguments = arguments ?? new FakeArguments();
            Configuration = new FakeConfiguration();
            Tools =
                new ToolLocator(
                    Environment,
                    new ToolRepository(Environment),
                    new ToolResolutionStrategy(FileSystem, Environment, Globber, Configuration)
                );
            Log = new FakeLog();
        }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public FakeConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets the file system.
        /// </summary>
        /// <value>
        /// The file system.
        /// </value>
        public IFileSystem FileSystem { get; set; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        /// <value>
        /// The environment.
        /// </value>
        public ICakeEnvironment Environment { get; set; }

        /// <summary>
        /// Gets the globber.
        /// </summary>
        /// <value>
        /// The globber.
        /// </value>
        public IGlobber Globber { get; set; }

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        public ICakeLog Log { get; set; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public ICakeArguments Arguments { get; set; }

        /// <summary>
        /// Gets the process runner.
        /// </summary>
        /// <value>
        /// The process runner.
        /// </value>
        public IProcessRunner ProcessRunner { get; set; }

        /// <summary>
        /// Gets the registry.
        /// </summary>
        /// <value>
        /// The registry.
        /// </value>
        public IRegistry Registry
        {
            get { throw new NotImplementedException(); }
        }

        public IToolLocator Tools { get; set; }
    }
}
