using System.Collections.Generic;
using Cake.Core;

namespace Automation.Tests
{
    public class FakeArguments : ICakeArguments
    {
        private IDictionary<string, string> args = new Dictionary<string, string>();

        public string GetArgument(string name)
        {
            return args[name];
        }

        public bool HasArgument(string name)
        {
            return args.ContainsKey(name);
        }

        public void SetArguments(IDictionary<string, string> arguments)
        {
            foreach (var arg in arguments)
            {
                args.Add(arg.Key, arg.Value);
            }
        }
    }
}
