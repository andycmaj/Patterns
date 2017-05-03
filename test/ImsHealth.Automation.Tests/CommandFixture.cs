using Cake.Core;
using Cake.Core.IO;
using Cake.Testing;

namespace Automation.Tests
{
    public abstract class CommandFixture
    {
        public FilePath ToolPath { get; set; }

        public IFileSystem FileSystem { get; set; }
        public IProcessRunner ProcessRunner { get; set; }
        public ICakeEnvironment Environment { get; set; }
        public IGlobber Globber { get; set; }

        public FakeFileSystem FakeFileSystem { get; set; }
        public FakeProcessRunner FakeProcessRunner { get; set; }
        public FakeEnvironment FakeEnvironment { get; set; }

        public FakeContext Context
        {
            get
            {
                // Use mocks if provided, otherwise use fakes.
                return new FakeContext(
                    FileSystem ?? FakeFileSystem,
                    Environment ?? FakeEnvironment,
                    Globber,
                    ProcessRunner ?? FakeProcessRunner
                );
            }
        }

        protected CommandFixture(string toolExecutable)
        {
            FakeEnvironment = FakeEnvironment.CreateUnixEnvironment();
            FakeFileSystem = new FakeFileSystem(FakeEnvironment);
            FakeProcessRunner = new FakeProcessRunner();
            Globber = new Globber(FakeFileSystem, FakeEnvironment);

            ToolPath = GetDefaultToolPath(toolExecutable);
            FakeFileSystem.CreateFile(ToolPath);
        }

        public virtual FilePath GetDefaultToolPath(string toolFilename)
        {
            return new FilePath("./tools/" + toolFilename).MakeAbsolute(Environment ?? FakeEnvironment);
        }
    }
}
