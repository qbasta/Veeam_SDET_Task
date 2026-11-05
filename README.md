# FolderSync

Console application (C#, .NET 8) that synchronizes two folders in one direction.
After every run the **replica** folder has exactly the same content as the **source** folder.

## Usage

```
dotnet run --project FolderSync -- <source> <replica> <intervalSeconds> <logFile>
```

| Argument          | Description                                       |
|-------------------|---------------------------------------------------|
| `source`          | Folder to copy from (must exist)                  |
| `replica`         | Folder to keep in sync (created if it is missing) |
| `intervalSeconds` | Time between synchronizations, whole seconds > 0  |
| `logFile`         | Log file path (created if it is missing)          |

Example:

```
dotnet run --project FolderSync -- ./data ./backup 30 ./logs/sync.log
```

The program works until you press `Ctrl+C`. The synchronization that is in progress is finished first.

## How it works

Every run goes through the folders recursively and:

- creates folders and copies files that are missing in the replica,
- updates files that changed (different size, or same size but different SHA-256 hash),
- deletes files and folders that do not exist in the source anymore,
- replaces a file with a folder (or a folder with a file) when they share the same name.

Every create / copy / update / delete is written to the console and to the log file.
If a run fails (for example the source folder is gone) the error is logged and the next run tries again.

The program does not start if source and replica are the same or nested in each other,
or if the log file is inside the replica folder (it would be deleted).

## Tests

```
dotnet test
```

Tests use xUnit and temporary folders.

## Possible improvements

- Handle errors per file (skip a locked file instead of failing the whole run).
- Compare last write time before calculating hashes to avoid reading big files.
- Copy to a temporary file and move it, so the replica never contains a half-copied file.
- Handle symbolic links.
