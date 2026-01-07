using Microsoft.Extensions.Logging;

namespace FolderSync
{
    public class SyncService : ISyncService
    {
        private readonly CliOptions _options;
        private readonly IFileComparer _fileComparer;
        private readonly ILogger<SyncService> _logger;

        public SyncService(CliOptions options, IFileComparer fileComparer, ILogger<SyncService> logger)
        {
            _options = options;
            _fileComparer = fileComparer;
            _logger = logger;
        }

        public Task SynchronizeAsync(CancellationToken token)
        {
            ValidatePaths();

            _logger.LogInformation($"Synchronization started. Source: {_options.SourcePath} Replica: {_options.ReplicaPath}");

            SyncDirectory(_options.SourcePath, _options.ReplicaPath, token);
            RemoveOrphans(_options.SourcePath, _options.ReplicaPath, token);

            _logger.LogInformation("Synchronization finished.");
            return Task.CompletedTask;
        }

        private void ValidatePaths()
        {
            if (!Directory.Exists(_options.SourcePath))
                throw new DirectoryNotFoundException("Source folder not found: " + _options.SourcePath);

            Directory.CreateDirectory(_options.ReplicaPath);
        }

        private void SyncDirectory(string sourceDir, string replicaDir, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Directory.CreateDirectory(replicaDir);

            foreach (string sourceFile in Directory.GetFiles(sourceDir))
            {
                token.ThrowIfCancellationRequested();

                string fileName = Path.GetFileName(sourceFile);
                string replicaFile = Path.Combine(replicaDir, fileName);

                bool shouldCopy = !File.Exists(replicaFile) || _fileComparer.AreDifferent(sourceFile, replicaFile);
                if (shouldCopy)
                {
                    File.Copy(sourceFile, replicaFile, true);
                    _logger.LogInformation("Copied/Updated file: {File}", replicaFile);
                }
            }

            foreach (string sourceSubDir in Directory.GetDirectories(sourceDir))
            {
                token.ThrowIfCancellationRequested();

                string dirName = Path.GetFileName(sourceSubDir);
                string replicaSubDir = Path.Combine(replicaDir, dirName);

                SyncDirectory(sourceSubDir, replicaSubDir, token);
            }
        }

        private void RemoveOrphans(string sourceDir, string replicaDir, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (string replicaFile in Directory.GetFiles(replicaDir))
            {
                token.ThrowIfCancellationRequested();

                string sourceFile = Path.Combine(sourceDir, Path.GetFileName(replicaFile));
                if (!File.Exists(sourceFile))
                {
                    File.Delete(replicaFile);
                    _logger.LogInformation("Deleted file: {File}", replicaFile);
                }
            }

            foreach (string replicaSubDir in Directory.GetDirectories(replicaDir))
            {
                token.ThrowIfCancellationRequested();

                string sourceSubDir = Path.Combine(sourceDir, Path.GetFileName(replicaSubDir));
                if (!Directory.Exists(sourceSubDir))
                {
                    Directory.Delete(replicaSubDir, true);
                    _logger.LogInformation("Deleted directory: {Directory}", replicaSubDir);
                }
                else
                {
                    RemoveOrphans(sourceSubDir, replicaSubDir, token);
                }
            }
        }
    }
}
