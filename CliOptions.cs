namespace FolderSync
{
    public class CliOptions
    {
        public string SourcePath { get; }
        public string ReplicaPath { get; }
        public TimeSpan Interval { get; }

        public CliOptions(string sourcePath, string replicaPath, TimeSpan interval)
        {
            SourcePath = sourcePath;
            ReplicaPath = replicaPath;
            Interval = interval;
        }

        public static CliOptions Parse(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (args.Length < 3) throw new ArgumentException("Expected 3 arguments: <source> <replica> <intervalSeconds>");

            string source = args[0];
            string replica = args[1];
            string intervalRaw = args[2];

            if (!int.TryParse(intervalRaw, out int seconds) || seconds <= 0)
                throw new ArgumentException("intervalSeconds must be a positive integer.");

            return new CliOptions(source, replica, TimeSpan.FromSeconds(seconds));
        }

        public static CliOptions FromConsole()
        {
            Console.WriteLine("No CLI args provided. Please enter values:");

            string source = ReadRequired("Source folder path: ");
            string replica = ReadRequired("Replica folder path: ");
            int intervalSeconds = ReadInt("Sync interval (seconds): ", 1);

            return new CliOptions(source, replica, TimeSpan.FromSeconds(intervalSeconds));
        }

        private static string ReadRequired(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = (Console.ReadLine() ?? string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(value))
                    return value;

                Console.WriteLine("Value is required.");
            }
        }

        private static int ReadInt(string prompt, int minValue)
        {
            while (true)
            {
                Console.Write(prompt);
                string raw = (Console.ReadLine() ?? string.Empty).Trim();

                if (int.TryParse(raw, out int result) && result >= minValue)
                    return result;

                Console.WriteLine("Please enter an integer >= " + minValue + ".");
            }
        }
    }
}
