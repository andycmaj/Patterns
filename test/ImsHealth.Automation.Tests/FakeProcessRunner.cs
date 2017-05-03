using System.Collections.Generic;
using Cake.Core.IO;
using Cake.Testing;
using Cake.Testing.Fixtures;

namespace Automation.Tests
{
    public class FakeProcessRunner : IProcessRunner
    {
        private readonly List<ToolFixtureResult> results;

        /// <summary>
        /// Gets or sets the process that will be returned
        /// when starting a process.
        /// </summary>
        /// <value>The process.</value>
        public FakeProcess Process { get; set; }

        /// <summary>
        /// Gets the results.
        /// </summary>
        /// <value>The results.</value>
        public IReadOnlyList<ToolFixtureResult> Results
        {
            get { return results; }
        }

        public FakeProcessRunner()
        {
            results = new List<ToolFixtureResult>();

            Process = new FakeProcess();
        }

        /// <summary>
        /// Starts the specified file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>The (fake) tool fixture process.</returns>
        public IProcess Start(FilePath filePath, ProcessSettings settings)
        {
            // Invoke the intercept action.
            results.Add(CreateResult(filePath, settings));

            // Return a dummy result.
            return Process;
        }

        /// <summary>
        /// Creates a <see cref="ToolFixtureResult"/> from a tool path and process settings.
        /// </summary>
        /// <param name="path">The tool path.</param>
        /// <param name="process">The process settings.</param>
        /// <returns>A tool fixture result.</returns>
        protected ToolFixtureResult CreateResult(FilePath path, ProcessSettings process)
        {
            return new ToolFixtureResult(path, process);
        }
    }
}
