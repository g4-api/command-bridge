/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System;
using System.Collections.Generic;
using System.IO;

namespace CommandBridge.UnitTests
{
    /// <summary>
    /// Unit tests for the CommandBase functionality.
    /// </summary>
    [TestClass]
    public class CommandBaseTests
    {
        // Predefined commands for testing.
        private static readonly Dictionary<string, IDictionary<string, CommandBase.CommandData>> s_commands = new()
        {
            ["testCommand"] = new Dictionary<string, CommandBase.CommandData>
            {
                ["param1"] = new CommandBase.CommandData { Name = "Parameter1", Description = "", Mandatory = true }
            }
        };

        /// <summary>
        /// Gets or sets the test context, which provides information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod(displayName: "Verify that command with mandatory parameter is validated correctly")]
        public void ValidCommandAndParametersTest()
        {
            // Instantiate the TestCommand class with the predefined commands
            var commandBase = new TestCommand(s_commands);

            // Define the arguments to be passed to the command
            var args = new[] { "testCommand", "-param1", "value" };

            // Invoke the command with the arguments
            commandBase.Invoke(args);

            // Assert that the OnInvoke method was called
            Assert.IsTrue(commandBase.OnInvokeCalled);
        }

        [TestMethod(displayName: "Verify that invalid command displays error message")]
        public void InvalidCommandTest()
        {
            // Create a StringWriter to capture console output
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Create an empty dictionary of commands
            var commands = new Dictionary<string, IDictionary<string, CommandBase.CommandData>>();

            // Instantiate the TestCommand class with the empty commands dictionary
            var commandBase = new TestCommand(commands);

            // Define the arguments to be passed to the command (invalid command)
            var args = new[] { "invalidCommand" };

            // Define the expected error message
            const string expectedErrorMessage = "Command is not set. Please provide a valid command. Usage: <myApp> [command] -p|--parameter";

            // Invoke the command with the arguments
            commandBase.Invoke(args);

            // Capture the console output
            var consoleOutput = stringWriter.ToString().Trim();

            // Verify that the console output matches the expected error message
            Assert.AreEqual(expectedErrorMessage, consoleOutput);
        }

        [TestMethod(displayName: "Verify that valid command without parameters displays error message")]
        public void ValidCommandNoParametersTest()
        {
            // Create a StringWriter to capture console output
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Instantiate the TestCommand class with predefined commands
            var commandBase = new TestCommand(s_commands);

            // Define the arguments to be passed to the command (no parameters provided)
            var args = new[] { "testCommand" };

            // Define the expected error message
            const string expectedErrorMessage = "Command is not set. Please provide a valid command. Usage: <myApp> [command] -p|--parameter";

            // Invoke the command with the arguments
            commandBase.Invoke(args);

            // Capture the console output
            var consoleOutput = stringWriter.ToString().Trim();

            // Verify that the console output matches the expected error message
            Assert.AreEqual(expectedErrorMessage, consoleOutput);
        }

        [TestMethod(displayName: "Verify that missing mandatory parameter displays error message")]
        public void ValidCommandMissingMandatoryParameterTest()
        {
            // Create a StringWriter to capture console output
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Instantiate the TestCommand class with predefined commands
            var commandBase = new TestCommand(s_commands);

            // Define the arguments to be passed to the command (missing mandatory parameter)
            var args = new[] { "testCommand", "--param2", "value" };

            // Define the expected error message
            const string expectedErrorMessage = "Missing mandatory parameters for command 'testCommand': Parameter1";

            // Invoke the command with the arguments
            commandBase.Invoke(args);

            // Capture the console output
            var consoleOutput = stringWriter.ToString().Trim();

            // Verify that the console output starts with the expected error message
            Assert.IsTrue(consoleOutput.Contains(expectedErrorMessage));
        }

        [TestMethod(displayName: "Verify that command help information is displayed correctly")]
        public void WriteCommandHelpTest()
        {
            // Create a StringWriter to capture console output
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Instantiate the TestCommand class with predefined commands
            var commandBase = new TestCommand(s_commands);

            // Define the arguments to be passed to the command (help parameter)
            var args = new[] { "testCommand", "--help" };

            // Define the expected help message
            const string expectedMessage =
                "testCommand -param1|--Parameter1 -h|--help\r\n    " +
                "-param1, --Parameter1  \r\n    " +
                "-h,      --help        Displays help information for the specified command.";

            // Act: Invoke the command with the arguments
            commandBase.Invoke(args);

            // Capture the console output
            var consoleOutput = stringWriter.ToString().Trim();

            // Assert that the console output matches the expected help message
            Assert.AreEqual(expectedMessage, consoleOutput);
        }

        [TestMethod(displayName: "Verify that invalid short hyphen displays error message")]
        public void ValidCommandInvalidShortHyphenTest()
        {
            // Create a StringWriter to capture console output
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Instantiate the TestCommand class with predefined commands
            var commandBase = new TestCommand(s_commands);

            // Define the expected error message
            const string expectedMessage =
                "Invalid argument: --param1\r\n" +
                "Missing mandatory parameters for command 'testCommand': Parameter1\r\n" +
                "testCommand -param1|--Parameter1 -h|--help\r\n" +
                "    -param1, --Parameter1  \r\n" +
                "    -h,      --help        Displays help information for the specified command.";

            // Define the arguments to be passed to the command (invalid short hyphen)
            var args = new[] { "testCommand", "--param1", "value" };

            // Act: Invoke the command with the arguments
            commandBase.Invoke(args);

            // Capture the console output
            var consoleOutput = stringWriter.ToString().Trim();

            // Assert that the console output matches the expected error message
            Assert.AreEqual(expectedMessage, consoleOutput);
        }

        // test method for valid command with invalid long hyphen test case
        [TestMethod(displayName: "Verify that valid command with invalid long hyphen displays error message")]
        public void ValidCommandInvalidLongHyphenTest()
        {
            // Create a StringWriter to capture console output
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Instantiate the TestCommand class with predefined commands
            var commandBase = new TestCommand(s_commands);

            // Define the expected error message
            const string expectedMessage =
                "Invalid argument: -Parameter1\r\n" +
                "Missing mandatory parameters for command 'testCommand': Parameter1\r\n" +
                "testCommand -param1|--Parameter1 -h|--help\r\n" +
                "    -param1, --Parameter1  \r\n" +
                "    -h,      --help        Displays help information for the specified command.";

            // Define the arguments to be passed to the command (invalid long hyphen)
            var args = new[] { "testCommand", "-Parameter1", "value" };

            // Act: Invoke the command with the arguments
            commandBase.Invoke(args);

            // Capture the console output
            var consoleOutput = stringWriter.ToString().Trim();

            // Assert that the console output matches the expected error message
            Assert.AreEqual(expectedMessage, consoleOutput);
        }
    }

    /// <summary>
    /// Represents a test command for unit testing purposes.
    /// </summary>
    /// <param name="commands">Dictionary containing command names and their corresponding command data.</param>
    [Command(name: "testCommand", description: "This is a test command used for unit testing purposes.")]
    public class TestCommand(IDictionary<string, IDictionary<string, CommandBase.CommandData>> commands) : CommandBase(commands)
    {
        /// <inheritdoc />
        protected override void OnInvoke(Dictionary<string, string> parameters)
        {
            // Set the OnInvokeCalled property to true indicating that the method was called
            OnInvokeCalled = true;
        }

        /// <summary>
        /// Gets a value indicating whether the OnInvoke method was called.
        /// </summary>
        public bool OnInvokeCalled { get; private set; }
    }
}
