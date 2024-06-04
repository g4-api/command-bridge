/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System;

namespace CommandBridge
{
    /// <summary>
    /// An attribute to specify a command name for a class.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <param name="description">The description of the command.</param>
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute(string name, string description) : Attribute
    {
        /// <summary>
        /// Gets the description of the command.
        /// </summary>
        public string Description { get; } = description;

        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public string Name { get; } = name;
    }
}
