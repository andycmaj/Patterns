using System;
using Patterns.Commands;
using Patterns.Tests.ExampleCommands;
using Patterns.Tests.ExampleDependencies;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace Patterns.Tests
{
    public class CommandRouterTest : AbstractCommandIntegrationTest
    {
        [Fact]
        public void Execute_Should_Preserve_Exception_StackTrace()
        {
            MockUserService
                .Setup(svc => svc.GetUser(It.IsAny<int>()))
                .Throws(new Exception("TestMessage"));

            var action = new ResetPasswordAction {
                UserId = 0
            };

            try
            {
                DoOuterThing(action);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Assert.Contains(nameof(Execute_Should_Preserve_Exception_StackTrace), ex.StackTrace);
                Assert.Contains(nameof(DoOuterThing), ex.StackTrace);
                Assert.Contains(nameof(DoInnerThing), ex.StackTrace);
            }
        }

        [Fact]
        public async Task ExecuteAsync_Should_Preserve_Exception_StackTrace()
        {
            MockUserService
                .Setup(svc => svc.GetUser(It.IsAny<int>()))
                .Throws(new Exception("TestMessage"));

            var action = new ResetPasswordAction
            {
                UserId = 0
            };

            try
            {
                await DoOuterThingAsync(action);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Assert.Contains(nameof(ExecuteAsync_Should_Preserve_Exception_StackTrace), ex.StackTrace);
                Assert.Contains(nameof(DoOuterThing), ex.StackTrace);
                Assert.Contains(nameof(DoInnerThing), ex.StackTrace);
            }
        }

        private async Task DoOuterThingAsync(IAction action)
        {
            await DoInnerThingAsync(action);
        }

        private async Task DoInnerThingAsync(IAction action)
        {
            await CommandRouter.ExecuteActionAsync(action);
        }

        private void DoOuterThing(IAction action)
        {
            DoInnerThing(action);
        }

        private void DoInnerThing(IAction action)
        {
            CommandRouter.ExecuteAction(action);
        }
    }
}
