using Automation.Ecs;
using Xunit;
using System.Linq;

namespace Automation.Tests.Ssh
{
    public class EcsUpdateTaskDefinitionCommandTest
    {
        [Fact]
        public void Should_Call_Tool_With_Currect_Args()
        {
            // Arrange
            const string taskDefinitionPath = "/Workspaces/appature/git/TheChunnel/.artifacts/one-off-activity-worker.aws.json";
            const string family = "task-family";
            var fixture = CreateFixture();

            // Act
            fixture.RunCommand(new EcsUpdateTaskDefinitionCommand(taskDefinitionPath, family));

            // Assert
            Assert.Equal(
                $"ecs register-task-definition --family {family} --cli-input-json file://{taskDefinitionPath}",
                fixture.FakeProcessRunner.Results.Single().Args
            );
        }

        private ActionFixture<EcsUpdateTaskDefinitionCommand, EcsUpdateTaskDefinitionCommand.Handler> CreateFixture()
        {
            return new ActionFixture<EcsUpdateTaskDefinitionCommand, EcsUpdateTaskDefinitionCommand.Handler>("aws");
        }
    }
}
