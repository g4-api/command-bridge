# CommandBridge

[![Build, Test & Release](https://github.com/g4-api/command-bridge/actions/workflows/GithubActions.yml/badge.svg)](https://github.com/g4-api/command-bridge/actions/workflows/GithubActions.yml)

CommandBridge is a versatile and powerful command-line utility framework for .NET, designed to simplify the creation, management, and execution of command-line commands and their associated parameters. This framework provides a structured and extensible way to handle complex command-line interfaces with ease.

## Features

- Simple and intuitive API for defining commands and parameters.
- Automatic handling of global options like `help`.
- Extensible base class for creating custom commands.
- Comprehensive error handling and validation.
- Easy integration with existing .NET applications.
- Supports short and long forms of command parameters.
- Seamless `--help` or `-h` switches to display command help.

## Installation

CommandBridge is available as a NuGet package. You can install it using the NuGet Package Manager or the .NET CLI.

### NuGet Package Manager

```sh
Install-Package CommandBridge
```

### .NET CLI

```sh
dotnet add package CommandBridge
```

## Quick Start

Here's a quick example to get you started with CommandBridge.

1. Create a new console application:

```sh
dotnet new console -n CommandBridgeExample
cd CommandBridgeExample
```

2. Install the CommandBridge NuGet package:

```sh
dotnet add package CommandBridge
```

3. Define a custom command by inheriting from `CommandBase`:

```csharp
using System;
using System.Collections.Generic;
using CommandBridge;

namespace CommandBridgeExample
{
    [Command(name: "greet", description: "Greets the user with a message.")]
    public class GreetCommand : CommandBase
    {
        private static readonly Dictionary<string, IDictionary<string, CommandData>> s_commands = new()
        {
            ["greet"] = new Dictionary<string, CommandData>(StringComparer.Ordinal)
            {
                { "n", new() { Name = "name", Description = "The name of the user.", Mandatory = true } }
            }
        };

        public GreetCommand() : base(s_commands) { }

        protected override void OnInvoke(Dictionary<string, string> parameters)
        {
            var name = parameters["name"];
            Console.WriteLine($"Hello, {name}!");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var command = CommandBase.FindCommand(args);
            command?.Invoke(args);
        }
    }
}
```

4. Run your application:

```sh
dotnet run greet -n John
```

You should see the output:

```
Hello, John!
```

## Tutorial

### Step 1: Define Your Commands

To create a new command, you need to create a class that inherits from `CommandBase` and use the `Command` attribute to specify the command name and description. The commands dictionary supports both short and long forms of the command parameters.

```csharp
using CommandBridge;

[Command(name: "mycommand", description: "This is my custom command.")]
public class MyCommand : CommandBase
{
    private static readonly Dictionary<string, IDictionary<string, CommandData>> s_commands = new()
    {
        ["mycommand"] = new Dictionary<string, CommandData>(StringComparer.Ordinal)
        {
            { "p1", new() { Name = "Parameter1", Description = "Description for parameter 1.", Mandatory = true } },
            { "p2", new() { Name = "Parameter2", Description = "Description for parameter 2.", Mandatory = false } }
        }
    };

    public MyCommand() : base(s_commands) { }

    protected override void OnInvoke(Dictionary<string, string> parameters)
    {
        // Implement your command logic here
    }
}
```

### Step 2: Implement Command Logic

Override the `OnInvoke` method to define the behavior of your command.

```csharp
protected override void OnInvoke(Dictionary<string, string> parameters)
{
    var param1 = parameters["Parameter1"];
    var param2 = parameters.ContainsKey("Parameter2") ? parameters["Parameter2"] : "default value";

    Console.WriteLine($"Parameter 1: {param1}");
    Console.WriteLine($"Parameter 2: {param2}");
}
```

### Step 3: Execute Commands

In your `Main` method, use the `FindCommand` method to locate and execute the appropriate command based on the provided arguments.

```csharp
class Program
{
    static void Main(string[] args)
    {
        var command = CommandBase.FindCommand(args);
        command?.Invoke(args);
    }
}
```

### Step 4: Display Help Information

CommandBridge automatically handles global options like `help`. You can trigger it by passing `--help` or `-h` as an argument.

```sh
dotnet run mycommand --help
```

## Basic Implementation

Here's a complete example demonstrating the creation and usage of a simple command with both short and long forms of parameters.

```csharp
using System;
using System.Collections.Generic;
using CommandBridge;

namespace CommandBridgeExample
{
    [Command(name: "deploy", description: "Deploys an application to the specified environment.")]
    public class DeployCommand : CommandBase
    {
        private static readonly Dictionary<string, IDictionary<string, CommandData>> s_commands = new()
        {
            ["deploy"] = new Dictionary<string, CommandData>(StringComparer.Ordinal)
            {
                { "e", new() { Name = "env", Description = "Specifies the target environment (e.g., production, staging).", Mandatory = true } },
                { "v", new() { Name = "version", Description = "Specifies the version of the application to deploy.", Mandatory = true } },
                { "c", new() { Name = "config", Description = "Path to the configuration file." } },
                { "f", new() { Name = "force", Description = "Force deploy without confirmation.", Type = "Switch" } }
            }
        };

        public DeployCommand() : base(s_commands) { }

        protected override void OnInvoke(Dictionary<string, string> parameters)
        {
            var env = parameters["env"];
            var version = parameters["version"];
            var config = parameters.ContainsKey("config") ? parameters["config"] : "default-config.yml";
            var force = parameters.ContainsKey("force");

            Console.WriteLine($"Deploying version {version} to {env} environment.");
            Console.WriteLine($"Using configuration file: {config}");
            if (force)
            {
                Console.WriteLine("Force deploy enabled.");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var command = CommandBase.FindCommand(args);
            command?.Invoke(args);
        }
    }
}
```

To run the example:

```sh
dotnet run deploy -e production -v 1.0.0 -c config.yml -f
```

You should see the output:

```
Deploying version 1.0.0 to production environment.
Using configuration file: config.yml
Force deploy enabled.
```

## Contributing

We welcome contributions to CommandBridge! If you find a bug or have a feature request, please open an issue on our GitHub repository. If you'd like to contribute code, feel free to fork the repository and submit a pull request.

## License

CommandBridge is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.

---

CommandBridge is designed to make building command-line interfaces in .NET simple and efficient. We hope you find it useful and look forward to your feedback and contributions.