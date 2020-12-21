using System;
using CommandLine;

namespace guidfy
{
    class Program
    {
        static int Main(string[] args)
        {
            var parser = Parser.Default;
            var statusCode = 0;

            parser.ParseArguments<ProgramArguments>(args)
                .WithParsed<ProgramArguments>(arguments =>
                statusCode = Core.ExceptionWrapper(() =>
                Core.ExecuteWithArgs(arguments)));

            return statusCode;
        }
    }
}
