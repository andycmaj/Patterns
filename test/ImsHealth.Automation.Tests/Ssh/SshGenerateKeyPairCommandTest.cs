using Automation.Ssh;
using Xunit;
using Cake.Testing;
using System.Linq;
using FakeItEasy;
using Cake.Core.IO;
using Cake.Core.Tooling;

namespace Automation.Tests.Ssh
{
    public class SshGenerateKeyPairCommandTest
    {
        private readonly ToolSettings toolSettings;

        public SshGenerateKeyPairCommandTest()
        {
            toolSettings = new ToolSettings
            {
                ToolPath = "ssh-keygen"
            };
        }

        [Fact]
        public void Should_Call_Tool_With_Currect_Args()
        {
            // Arrange
            const string keyPath = "/foo/bar/key";
            var fixture = CreateFixture();
            var mockFileSystem = A.Fake<IFileSystem>();
            fixture.FileSystem = mockFileSystem;

            A.CallTo(() => mockFileSystem.GetFile(A<FilePath>.Ignored)).ReturnsLazily(call =>
            {
                var path = (FilePath)call.Arguments.Single();
                return Generators.File(path.FullPath);
            });

            // Act
            var result = fixture.RunCommand(new SshGenerateKeyPairCommand(keyPath, true) { Settings = toolSettings });

            // Assert
            Assert.Equal($"-q -N \"\" -f \"{keyPath}\"", fixture.FakeProcessRunner.Results.Single().Args);
        }

        [Fact]
        public void Should_Return_Correct_Key_Info()
        {
            // Arrange
            const string keyPath = "/foo/bar/key";
            const string pubKeyPath = "/foo/bar/key.pub";
            const string pubKeyContent = "publickey";
            var fixture = CreateFixture();
            CreateFakeKeys(fixture.FakeFileSystem, keyPath, pubKeyPath, pubKeyContent);

            // Act
            var result = fixture.RunCommand(new SshGenerateKeyPairCommand(keyPath));

            // Assert
            Assert.Equal(result.PrivateKeyPath, keyPath);
            Assert.Equal(result.PublicKeyContent, pubKeyContent);
        }

        private static void CreateFakeKeys(
            FakeFileSystem fileSystem,
            string keyPath,
            string pubKeyPath = null,
            string pubKeyContent = null
        )
        {
            fileSystem.CreateFile(keyPath);
            fileSystem
                .CreateFile(pubKeyPath ?? $"{keyPath}.pub")
                .SetContent(pubKeyContent ?? "pub");
        }

        private FunctionFixture<SshGenerateKeyPairCommand, SshKeyPair, SshGenerateKeyPairCommand.Handler> CreateFixture()
        {
            return new FunctionFixture<SshGenerateKeyPairCommand, SshKeyPair, SshGenerateKeyPairCommand.Handler>("ssh-keygen");
        }
    }
}
