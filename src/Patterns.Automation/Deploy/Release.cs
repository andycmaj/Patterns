using System;
using Automation.Layer0;

namespace Automation.Deploy
{
    /// <summary>
    /// Used to determine the name of various <c>Layer0</c> infrastructure
    /// items (<see cref="Layer0Environment"/>, <see cref="Layer0Service"/>,
    /// etc). The format will be {Environment}_{ApplicationName}_{Version}
    /// </summary>
    /// <remarks>
    /// When the <c>Versoin</c> changes, an entirely new environment and new
    /// infrastructure will be created. This will not result in an in-place
    /// update of your existing infrastructure
    /// </remarks>
    public class Release
    {
        /// <summary>
        /// The application name used for constructing the overall environment name
        /// </summary>
        public string Application { get; set; }

        /// <summary>
        /// The base name for the <see cref="Layer0Environment"/>. Will also
        /// define the <c>HOSTING__ENVIRONMENT</c> of <see cref="Layer0Service"/>s
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Will be used to version <see cref="Layer0Deploy"/>s and
        /// <see cref="Layer0Environment"/>s.
        /// </summary>
        public string Version { get; set; }

        public Release()
        { }

        public Release(string environment, string application, string version)
        {
            if (string.IsNullOrWhiteSpace(environment))
            {
                throw new ArgumentNullException(nameof(environment));
            }
            if (string.IsNullOrWhiteSpace(application))
            {
                throw new ArgumentNullException(nameof(application));
            }

            Environment = environment;
            Application = application;
            Version = version;
        }
    }
}
