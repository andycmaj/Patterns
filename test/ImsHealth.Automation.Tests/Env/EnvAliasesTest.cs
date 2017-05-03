using System;
using System.Collections.Generic;
using Automation.Env;
using Cake.Core;
using Cake.Testing;
using Xunit;
using FakeItEasy;

namespace Automation.Tests.Env
{
    public class EnvAliasesTest
    {
        private const string EnvironmentVariableName =
            "AUTOMATION_TEST_ENVIRONMENT_VARIABLE";
        private const string EnvironmentVariableValue = "test-value";

        private readonly FakeEnvironment fakeEnvironment;
        private readonly ICakeContext fakeContext;

        public EnvAliasesTest()
        {
            this.fakeEnvironment = FakeEnvironment.CreateUnixEnvironment();
            this.fakeContext = new FakeContext(
                environment: this.fakeEnvironment
            );
        }

        [Fact]
        public void RequireEnvironmentVariable_ShouldReturnString_OnFind()
        {
            this.fakeEnvironment.SetEnvironmentVariable(
                EnvironmentVariableName,
                EnvironmentVariableValue
            );

            var result = EnvAliases.RequireEnvironmentVariable(
                fakeContext,
                EnvironmentVariableName
            );

            Assert.Equal(result, EnvironmentVariableValue);

        }

        [Fact]
        public void RequireEnvironmentVariable_Throw_OnNotFound()
        {
            Assert.Throws<Exception>(() =>
                EnvAliases.RequireEnvironmentVariable(
                    fakeContext,
                    EnvironmentVariableName
                )
            );
        }
    }
}
