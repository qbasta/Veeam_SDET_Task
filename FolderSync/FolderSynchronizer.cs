namespace FolderSync;

public class FolderSynchronizer
{
    private readonly string _sourcePath;
    private readonly string _replicaPath;
    private readonly Logger _logger;

    public FolderSynchronizer(string sourcePath, string replicaPath, Logger logger)
    {
        _sourcePath = sourcePath;
        _replicaPath = replicaPath;
        _logger = logger;
    }

    public void Sync()
    {
        if (!Directory.Exists(_sourcePath))
        {
            throw new DirectoryNotFoundException($"Source folder does not exist: {_sourcePath}");
        }

        if (!Directory.Exists(_replicaPath))
        {
            Directory.CreateDirectory(_replicaPath);
            _logger.Log($"Created directory: {_replicaPath}");
        }

        SyncFolder(_sourcePath, _replicaPath);
    }

    private void SyncFolder(string sourceDir, string replicaDir)
    {
        foreach (string sourceSubDir in Directory.GetDirectories(sourceDir))
        {
            string replicaSubDir = Path.Combine(replicaDir, Path.GetFileName(sourceSubDir));

            if (!Directory.Exists(replicaSubDir))
            {
                Directory.CreateDirectory(replicaSubDir);
                _logger.Log($"Created directory: {replicaSubDir}");
            }

            SyncFolder(sourceSubDir, replicaSubDir);
        }

        foreach (string sourceFile in Directory.GetFiles(sourceDir))
        {
            string replicaFile = Path.Combine(replicaDir, Path.GetFileName(sourceFile));

            if (!File.Exists(replicaFile))
            {
                File.Copy(sourceFile, replicaFile);
                _logger.Log($"Copied file: {sourceFile} -> {replicaFile}");
            }
        }
    }
}