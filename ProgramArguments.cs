using CommandLine;

namespace guidfy
{
    /// <summary>
    /// The program's arguments as parsed from the command line.
    /// </summary>
    public class ProgramArguments
    {
        /// <summary>
        /// File or directory path. The root directory when restoring.
        /// </summary>
        /// <value></value>
        [Option('p', "path", Required = false, HelpText = "File or directory path. The root directory when restoring.")]
        public string Path { get; set; }

        /// <summary>
        /// Include subdirectory tree.
        /// </summary>
        /// <value></value>
        [Option('r', "recursive", Required = false, HelpText = "Include subdirectory tree.")]
        public bool Recursive { get; set; }

        /// <summary>
        /// Restore original names.
        /// </summary>
        /// <value></value>
        [Option("restore", Required = false, HelpText = "Restore original names.")]
        public bool Restore { get; set; }

        /// <summary>
        /// Enables quiet mode (only errors will be logged).
        /// </summary>
        /// <value></value>
        [Option('q', "quiet", Required = false, HelpText = "Enables quiet mode (only errors will be logged).")]
        public bool Quiet { get; set; }

        /// <summary>
        /// Enables verbose mode.
        /// </summary>
        /// <value></value>
        [Option('f', "force", Required = false, HelpText = "Enables verbose mode.")]
        public bool Force { get; set; }
    }
}