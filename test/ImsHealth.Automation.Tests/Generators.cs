using Cake.Core.IO;
using FakeItEasy;
using System.IO;
using System.Linq;
using System.Text;
using Automation.Layer0;
using Bogus;

namespace Automation.Tests
{
    public static class Generators
    {
        public static IFile File(string path, bool exists = true, string content = "")
        {
            var file = A.Fake<IFile>();
            A.CallTo(() => file.Path).Returns(new FilePath(path));
            A.CallTo(() => file.Exists).Returns(exists);
            if (file.Exists)
            {
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                A.CallTo(() => file.Open(FileMode.Open, FileAccess.Read, FileShare.Read)).Returns(stream);
            }
            return file;
        }

        public static Faker<Layer0Deploy> FakeDeploy =
            new Faker<Layer0Deploy>()
                .RuleFor(deploy => deploy.Id, f => f.Hacker.Noun())
                .RuleFor(deploy => deploy.Name, f => f.Hacker.Noun())
                .RuleFor(deploy => deploy.Version, f => f.Random.Number());

        public static Faker<Layer0LoadBalancer.Port> FakePorts =
            new Faker<Layer0LoadBalancer.Port>()
                .RuleFor(port => port.ForwardsTo, f => f.Random.Number())
                .RuleFor(port => port.ListensOn, f => f.Random.Number())
                .RuleFor(port => port.Protocol, f => f.Hacker.Noun());

        public static Faker<Layer0Service.Deploy> FakeServiceDeploy =
            new Faker<Layer0Service.Deploy>()
                .RuleFor(deploy => deploy.Id, f => f.Hacker.Noun())
                .RuleFor(deploy => deploy.Name, f => f.Hacker.Noun())
                .RuleFor(deploy => deploy.Version, f => f.Random.Number())
                .RuleFor(deploy => deploy.Status, f => f.Hacker.Noun());

        public static Faker<Layer0Service> FakeL0Service =
            new Faker<Layer0Service>()
                .RuleFor(service => service.Name, f => f.Hacker.Noun())
                .RuleFor(service => service.Environment, f => f.Hacker.Noun())
                .RuleFor(service => service.Deploys, f => FakeServiceDeploy.Generate(1).ToArray())
                .RuleFor(service => service.DesiredCount, f => f.Random.Number())
                .RuleFor(service => service.PendingCount, f => f.Random.Number())
                .RuleFor(service => service.RunningCount, f => f.Random.Number());

        public static Faker<Layer0LoadBalancer> FakeL0LoadBalancer =
            new Faker<Layer0LoadBalancer>()
            .RuleFor(loadBalancer => loadBalancer.Id, f => f.Hacker.Noun())
                .RuleFor(loadBalancer => loadBalancer.Name, f => f.Hacker.Noun())
                .RuleFor(loadBalancer => loadBalancer.Url, f => f.Hacker.Noun())
                .RuleFor(loadBalancer => loadBalancer.Environment, f => f.Hacker.Noun())
                .RuleFor(loadBalancer => loadBalancer.Ports, f => FakePorts.Generate(2).ToArray());

        public static Faker<Layer0LoadBalancer> FakeL0LoadBalancerWithHealthCheck =
            new Faker<Layer0LoadBalancer>()
                .RuleFor(loadBalancer => loadBalancer.Id, f => f.Hacker.Noun())
                .RuleFor(loadBalancer => loadBalancer.Name, f => f.Hacker.Noun())
                .RuleFor(loadBalancer => loadBalancer.Url, f => f.Hacker.Noun())
                .RuleFor(loadBalancer => loadBalancer.Environment, f => f.Hacker.Noun())
                .RuleFor(loadBalancer => loadBalancer.Ports, f => FakePorts.Generate(2).ToArray())
                .RuleFor(loadBalancer => loadBalancer.HealthCheckOptions, f => FakeLayer0HealthCheck.Generate());

        public static Faker<Layer0HealthCheckOptions> FakeLayer0HealthCheck =
            new Faker<Layer0HealthCheckOptions>()
                .RuleFor(healthCheck => healthCheck.Target, f => f.Hacker.Noun())
                .RuleFor(healthCheck => healthCheck.Interval, f => f.Random.Number(1, 99))
                .RuleFor(healthCheck => healthCheck.Timeout, f => f.Random.Number(1, 99))
                .RuleFor(healthCheck => healthCheck.HealthyThreshold, f => f.Random.Number(1, 99))
                .RuleFor(healthCheck => healthCheck.UnhealthyThreshold, f => f.Random.Number(1, 99));

        public static Faker<Layer0Environment> FakeL0Environment =
            new Faker<Layer0Environment>()
                .RuleFor(env => env.Name, f => f.Hacker.Noun());

        public static Faker<Layer0Task> FakeL0Task =
            new Faker<Layer0Task>()
                .RuleFor(task => task.Id, f => f.Hacker.Noun())
                .RuleFor(task => task.Name, f => f.Hacker.Noun())
                .RuleFor(task => task.EnvironmentId, f => f.Hacker.Noun())
                .RuleFor(task => task.EnvironmentName, f => f.Hacker.Noun())
                .RuleFor(task => task.DeployName, f => f.Hacker.Noun())
                .RuleFor(task => task.DeployId, f => f.Hacker.Noun())
                .RuleFor(task => task.DesiredCount, f => f.Random.Number())
                .RuleFor(task => task.PendingCount, f => f.Random.Number())
                .RuleFor(task => task.RunningCount, f => f.Random.Number())
                .RuleFor(task => task.Copies, f => FakeTaskCopy.Generate(2).ToArray());

        public static Faker<Layer0Task.TaskCopy> FakeTaskCopy =
            new Faker<Layer0Task.TaskCopy>()
                .RuleFor(copy => copy.Reason, f => f.Hacker.Noun())
                .RuleFor(copy => copy.Details, f => FakeTaskCopyDetails.Generate(1).ToArray());

        public static Faker<Layer0Task.TaskCopyDetails> FakeTaskCopyDetails =
            new Faker<Layer0Task.TaskCopyDetails>()
                .RuleFor(copy => copy.ExitCode, f => f.Random.Number())
                .RuleFor(copy => copy.LastStatus, f => (Layer0TaskStatus)f.Random.Number(2));
    }
}
