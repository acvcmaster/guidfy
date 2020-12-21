using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace guidfy
{
    public static class Core
    {
        public static string NextGuid => Guid.NewGuid().ToString();

        public static void ExecuteWithArgs(ProgramArguments arguments)
        {
            PrintHeader();

            if (arguments == null)
                throw new ArgumentNullException("arguments");

            arguments.Path = string.IsNullOrEmpty(arguments.Path) ? Environment.CurrentDirectory : arguments.Path;

            var isDirectory = Directory.Exists(arguments.Path);
            var isFile = File.Exists(arguments.Path);

            if (!isDirectory && !isFile)
                throw new FileNotFoundException("Unable to find the specified file or directory.");

            if (arguments.Restore && isFile)
                throw new ArgumentException("-p (--path) must refer to a directory when restoring.");

            if (isFile && arguments.Recursive)
                throw new ArgumentException("-r (--recursive) option is not valid when targeting a file.");

            if (arguments.Restore && arguments.Recursive)
                throw new ArgumentException("-r (--recursive) is invalid when restoring.");

            if (arguments.Restore && arguments.Force)
                throw new ArgumentException("-f (--force) is invalid when restoring.");

            if (!arguments.Restore)
                Guidfy(arguments);
            else
                Restore(arguments);
        }

        public static void Guidfy(ProgramArguments arguments)
        {
            Log("Acquiring file names..", arguments);
            var isDirectory = Directory.Exists(arguments.Path);
            var isFile = File.Exists(arguments.Path);

            var files = isDirectory ?
                Directory.GetFiles(arguments.Path, "*", arguments.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly) :
                new string[] { arguments.Path };

            var basePath = isDirectory ? arguments.Path : Path.GetDirectoryName(arguments.Path);

            Log("Creating database..", arguments);
            var database = new Dictionary<string, string>();

            Log("Generating GUIDs..", arguments);
            foreach (var file in files)
            {
                if (Path.GetFileName(file) == "database.json")
                    continue;

                var guid = NextGuid;
                var relativePath = Path.GetRelativePath(basePath, file);
                Log($"\t{relativePath} → {guid}", arguments);

                if (database.ContainsKey(guid))
                    throw new Exception($"GUID collision for file '{relativePath}'.");

                database[guid] = file;
            }

            Log("Writing database to disk..", arguments);
            var jsonPath = Path.Combine(basePath, "database.json");

            if (File.Exists(jsonPath) && !arguments.Force)
                throw new Exception("Previous database already exists. Run with -f (--force) to overwrite (this WILL cause data loss).");

            var json = JsonSerializer.Serialize(database, new JsonSerializerOptions() { WriteIndented = true });
            using (StreamWriter writer = new StreamWriter(jsonPath))
            {
                writer.Write(json);
                writer.Flush();
            }

            Log("Renaming files..", arguments);
            foreach (var file in database)
            {
                var extension = Path.GetExtension(file.Value);
                var directory = Path.GetDirectoryName(file.Value);

                File.Move(file.Value, Path.Combine(directory, $"{file.Key}{extension}"));
            }

            Log("Done!", arguments);
        }

        public static void Restore(ProgramArguments arguments)
        {
            Log("Reading database..", arguments);
            var basePath = arguments.Path;
            var databasePath = Path.Combine(basePath, "database.json");

            if (!File.Exists(databasePath))
                throw new FileNotFoundException("File database.json was not found on the specified path.");

            Dictionary<string, string> database = null;
            using (StreamReader reader = new StreamReader(databasePath))
            {
                var json = reader.ReadToEnd();
                database = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }

            if (database == null)
                throw new InvalidDataException("Database is not valid and may be corrupted.");

            Log("Restoring files..", arguments);
            foreach (var file in database)
            {
                var extension = Path.GetExtension(file.Value);
                var directory = Path.GetDirectoryName(file.Value);

                File.Move(Path.Combine(directory, $"{file.Key}{extension}"), file.Value);
            }


            Log("Cleaning up..", arguments);
            File.Delete(databasePath);


            Log("Done!", arguments);
        }

        public static int ExceptionWrapper(Action program)
        {
            try
            {
                program();
                Console.WriteLine("\nSTATUS CODE: 0");
                Console.WriteLine("SUCCESS");
                return 0;
            }
            catch (Exception error)
            {
                Console.WriteLine($"ERROR:\n\t{error.Message}");
                Console.WriteLine("\nSTATUS CODE: -1");
                Console.WriteLine("ABORTING");
                return -1;
            }
        }

        public static void Log(string message, ProgramArguments arguments)
        {
            if (!arguments.Quiet)
                Console.WriteLine(message);
        }

        public static void PrintHeader()
        {
            Console.WriteLine($"{CommandLine.Text.HeadingInfo.Default}");
            Console.WriteLine($"{CommandLine.Text.CopyrightInfo.Default}\n");

        }
    }
}