/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CommandBridge
{
    /// <summary>
    /// Represents the base class for commands.
    /// </summary>
    public abstract class CommandBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBase"/> class with the specified commands.
        /// </summary>
        /// <param name="commands">Dictionary containing commands and their associated parameters.</param>
        protected CommandBase(IDictionary<string, IDictionary<string, CommandData>> commands)
        {
            // Assign provided commands to the Commands property
            Commands = commands;

            // Define global options (e.g., help) applicable to all commands
            var globalOptions = new Dictionary<string, CommandData>(StringComparer.OrdinalIgnoreCase)
            {
                { "h", new CommandData { Name = "help", Description = "Displays help information for the specified command.", Type = "Switch" } }
            };

            // Add global options to each command
            foreach (var command in commands)
            {
                foreach (var option in globalOptions)
                {
                    command.Value[option.Key] = option.Value;
                }
            }
        }

        /// <summary>
        /// Gets the dictionary containing command names and their associated parameter data.
        /// </summary>
        public IDictionary<string, IDictionary<string, CommandData>> Commands { get; }

        /// <summary>
        /// Finds and returns a command instance based on the specified command name provided in the arguments.
        /// </summary>
        /// <param name="args">The arguments provided to the application, where the first argument is expected to be the command name.</param>
        /// <returns>A command instance corresponding to the specified command name, or null if the command is not found.</returns>
        public static CommandBase FindCommand(string[] args)
        {
            // Check if there are at least two arguments (command name and at least one parameter)
            if (args.Length < 1)
            {
                return default;
            }

            // Constant for case-insensitive string comparison
            const StringComparison Comparison = StringComparison.OrdinalIgnoreCase;

            // Extract the command name from the arguments
            var command = args[0];

            // Find and return a command instance matching the specified command name
            return NewCommands().Find(i => i.GetAttribute().Name.Equals(command, Comparison));
        }

        /// <summary>
        /// Retrieves the command attribute associated with the current command instance.
        /// </summary>
        /// <returns>The command attribute associated with the current command instance.</returns>
        public CommandAttribute GetAttribute()
        {
            return GetType().GetCustomAttribute<CommandAttribute>();
        }

        /// <summary>
        /// Invokes the command with the specified arguments.
        /// </summary>
        /// <param name="args">The arguments for the command. The first argument should be the command name, and subsequent arguments should be parameters for the command.</param>
        /// <exception cref="InvalidOperationException">Thrown when the command is not set or when there are insufficient arguments.</exception>
        public void Invoke(string[] args)
        {
            // Create an InvalidOperationException with a clear message when the command is not set
            const string errorMessage = "Command is not set. Please provide a valid command. Usage: <myApp> [command] -p|--parameter";

            // Check if there are at least two arguments (command name and at least one parameter)
            if (args.Length < 2)
            {
                Console.WriteLine(errorMessage);
                Console.WriteLine();
                WriteApplicationHelp();
                return;
            }

            // Get the command from the first argument
            var command = args[0];

            // Format the parameters for the command
            var parameters = FormatParameters(Commands, command, args);

            // Check if the 'help' parameter is present or if mandatory parameters are missing
            if (parameters.ContainsKey("help") || !ConfirmMandatoryParameters(Commands, parameters))
            {
                // Display help information for the commands
                WriteCommandsHelp();
                return;
            }

            // Invoke the command with the formatted parameters
            try
            {
                OnInvoke(parameters);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// When overridden in a derived class, defines the logic to execute the command.
        /// </summary>
        /// <param name="parameters">The parameters for the command.</param>
        protected abstract void OnInvoke(Dictionary<string, string> parameters);

        /// <summary>
        /// Writes application help information to the console.
        /// </summary>
        public static void WriteApplicationHelp()
        {
            // List to store CommandAttribute objects
            var attributes = new List<CommandAttribute>();

            // Retrieve CommandAttribute for each command and add it to the list
            foreach (var command in NewCommands())
            {
                var attribute = command.GetType().GetCustomAttribute<CommandAttribute>();
                attributes.Add(attribute);
            }

            // Check if any commands were found
            if (attributes.Count == 0)
            {
                return;
            }

            // Calculate the maximum length of command names
            int maxKeyLength = attributes.Max(i => i.Name.Length);

            // Write section header for commands
            Console.WriteLine("Commands:");

            // Iterate through each command attribute and write its name and description
            foreach (var attribute in attributes)
            {
                // Calculate the number of spaces needed for formatting
                int spacesCount = maxKeyLength + 2 - attribute.Name.Length;
                string spaces = new(' ', spacesCount);

                // Write command name and description with proper formatting
                Console.WriteLine("    " + attribute.Name + spaces + attribute.Description);
            }

            // Write empty line for readability
            Console.WriteLine();

            // Write section header for global options
            Console.WriteLine("Global Options:");

            // Write help information for global options
            Console.WriteLine("    -h, --help  Displays help information for the specified command.");
            Console.WriteLine();
        }

        /// <summary>
        /// Writes help information for all available commands.
        /// </summary>
        public void WriteCommandsHelp()
        {
            // Find the first type that matches the specified command name
            var commandName = GetType().GetCustomAttribute<CommandAttribute>().Name;

            // Iterate through each command in the s_commands dictionary
            foreach (var command in Commands.Select(i => i.Value))
            {
                // Print the command name and format the parameter keys
                Console.WriteLine($"{commandName ?? "N/A"} " + string.Join(" ", command.Select(p => $"-{p.Key}|--{p.Value.Name}")));

                // Sort the parameters by key
                var sorted = command.Where(i => string.IsNullOrEmpty(i.Value.Type) || !i.Value.Type.Equals("Switch")).OrderBy(p => p.Key);

                // Sort the switches by key
                var switches = command.Where(i => !string.IsNullOrEmpty(i.Value.Type) && i.Value.Type.Equals("Switch")).OrderBy(p => p.Key);

                // Combine the sorted parameters and switches
                var sortedParameters = sorted.Concat(switches);

                int maxNameLength = sortedParameters.Max(p => p.Key.Length);

                var labels = new List<(string, string)>();

                // Iterate through each parameter of the command
                foreach (var parameter in sortedParameters)
                {
                    // Extract the parameter key, name, and description
                    var key = parameter.Key;
                    var commandData = parameter.Value;
                    var labelSpaces = new string(' ', maxNameLength - key.Length);

                    var label = commandData.Name.Equals(key, StringComparison.OrdinalIgnoreCase)
                        ? $"--{commandData.Name}"
                        : $"-{key}, {labelSpaces}--{commandData.Name}";

                    labels.Add((label, commandData.Description));
                }

                // Find the longest parameter key length
                int maxKeyLength = labels.Max(i => i.Item1.Length);

                foreach (var item in labels)
                {
                    // Calculate the number of spaces needed for alignment
                    int spacesCount = maxKeyLength - item.Item1.Length;
                    string spaces = new(' ', spacesCount);

                    // Print the parameter information with alignment
                    Console.WriteLine("    " + item.Item1 + spaces + "  " + item.Item2);
                }

                // Add an empty line for better readability
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Creates a new command instance based on the specified command name.
        /// </summary>
        /// <param name="command">The name of the command.</param>
        /// <returns>A new instance of a class that derives from <see cref="CommandBase"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the command is not found.</exception>
        public static CommandBase NewCommand(string command)
        {
            // Get all types in the entryassembly that derive from CommandBase and have a CommandAttribute
            var types = Assembly
                .GetEntryAssembly()
                .GetTypes()
                .Where(i => !i.IsAbstract && typeof(CommandBase).IsAssignableFrom(i) && i.GetCustomAttribute<CommandAttribute>() != null);

            // Find the first type that matches the specified command name
            var type = types.FirstOrDefault(i => i.GetCustomAttribute<CommandAttribute>().Name == command);

            // If no matching type is found, throw an exception
            if (type == default)
            {
                throw new InvalidOperationException($"Command not found: {command}");
            }

            // Create a new instance of the found type, passing the commands dictionary as an argument
            return (CommandBase)Activator.CreateInstance(type);
        }

        // Confirms the presence of mandatory parameters for each command.
        private static bool ConfirmMandatoryParameters(
            IDictionary<string, IDictionary<string, CommandData>> commandEntries,
            Dictionary<string, string> userParameters)
        {
            // Iterate through each command in the commandEntries dictionary
            foreach (var commandEntry in commandEntries)
            {
                // Get the command name
                var commandName = commandEntry.Key;

                // Get the command data dictionary for the current command
                var commandData = commandEntry.Value;

                // Initialize a list to store missing mandatory parameters for the current command
                var missingParameters = new List<string>();

                // Iterate through each parameter data for the current command
                foreach (var parameterEntry in commandData.Select(i => i.Value))
                {
                    // Get the parameter name
                    var parameterName = parameterEntry.Name;

                    // Check if the parameter is mandatory and if it is missing in the user parameters dictionary
                    if (parameterEntry.Mandatory && !userParameters.ContainsKey(parameterName))
                    {
                        // Add the missing mandatory parameter to the list
                        missingParameters.Add(parameterName);
                    }
                }

                // If there are missing mandatory parameters for the current command, handle them accordingly
                if (missingParameters.Count > 0)
                {
                    // Construct an error message indicating the missing mandatory parameters
                    Console.WriteLine($"Missing mandatory parameters for command '{commandName}': {string.Join(", ", missingParameters)}");

                    // Return false indicating that not all mandatory parameters are present
                    return false;
                }
            }

            // Return true indicating that all mandatory parameters are present
            return true;
        }

        // Formats parameters for a given command based on provided arguments.
        private static Dictionary<string, string> FormatParameters(
            IDictionary<string, IDictionary<string, CommandData>> commands, string command, string[] args)
        {
            // Regular expression to match parameter keys
            var parameterKeyRegex = new Regex("^-{1,2}[a-zA-Z0-9]+$");

            // Dictionary to store formatted parameters
            var parameters = new Dictionary<string, string>(StringComparer.Ordinal);

            // Get parameters for the specified command
            var commandParameters = commands[command];

            // Loop through the arguments
            for (int i = 1; i < args.Length; i++)
            {
                // Check if the argument matches the parameter key pattern
                if (!parameterKeyRegex.IsMatch(args[i]))
                {
                    continue;
                }

                // Extract the key from the argument
                var key = Regex.Replace(input: args[i], pattern: "^-{1}", replacement: "");

                // If the key is found in the command parameters
                if (commandParameters.TryGetValue(key, out CommandData value))
                {
                    // Use the parameter name from the CommandData
                    key = value.Name;
                }
                // If the argument starts with "--" but doesn't match any parameter key
                else if (Regex.IsMatch(input: args[i].Trim(), pattern: "^-{2}[a-zA-Z0-9]+$"))
                {
                    // Remove the leading "--" to get the parameter key
                    var parameterKey = args[i].Replace("--", "");

                    // Check if the parameter key exists in the command parameters
                    var isValue = commandParameters.Any(p => p.Value.Name == parameterKey);

                    // If the parameter key does not exist, print an error message and return an empty dictionary
                    if (!isValue)
                    {
                        Console.WriteLine($"Invalid argument: {args[i]}");
                        return [];
                    }

                    // Use the extracted parameter key
                    key = parameterKey;
                }
                else
                {
                    // Print error message for invalid argument and return empty dictionary
                    Console.WriteLine($"Invalid argument: {args[i]}");
                    return [];
                }

                // If the argument is followed by another argument
                if (i + 1 < args.Length && !parameterKeyRegex.IsMatch(args[i + 1]))
                {
                    // Store the next argument as the value for the current key
                    parameters[key] = args[i + 1];
                }
                else
                {
                    // If no value is provided, store an empty string
                    parameters[key] = string.Empty;
                }
            }

            // Return the dictionary containing the formatted parameters
            return parameters;
        }

        // Creates a list of new command instances based on the types in the executing assembly
        private static List<CommandBase> NewCommands()
        {
            // Get all types in the executing assembly that derive from CommandBase and have a CommandAttribute
            var types = GetTypes()
                .Where(i => !i.IsAbstract && typeof(CommandBase).IsAssignableFrom(i) && i.GetCustomAttribute<CommandAttribute>() != null)
                .ToList();

            // Initialize the list to store the command instances
            var commands = new List<CommandBase>();

            // Iterate over each type and create a new instance of the command
            foreach (var type in types)
            {
                var command = (CommandBase)Activator.CreateInstance(type);
                commands.Add(command);
            }

            // Return the list of command instances
            return commands;
        }

        // Gets all types in the current AppDomain from all loaded assemblies
        // and returns them as a list of Type objects
        public static List<Type> GetTypes()
        {
            // Get all loaded assemblies in the current AppDomain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Initialize a list to store all types
            var allTypes = new List<Type>();

            // Iterate over each assembly and load its types
            foreach (var assembly in assemblies)
            {
                try
                {
                    // Load types from the assembly and add them to the list
                    var types = assembly.GetTypes();
                    allTypes.AddRange(types);
                }
                catch
                {
                    // Skip the assembly if an exception occurs
                }
            }

            // Return the list of all types
            return allTypes;
        }

        /// <summary>
        /// Represents data related to a command.
        /// </summary>
        public sealed class CommandData
        {
            /// <summary>
            /// Gets or sets the description of the command.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the name of the command.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the command is mandatory.
            /// </summary>
            public bool Mandatory { get; set; }

            /// <summary>
            /// Gets or sets the type of data associated with the command.
            /// </summary>
            public string Type { get; set; }
        }
    }
}
