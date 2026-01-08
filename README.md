# Folder Synchronization Tool

This project is a simple **folder synchronization tool** written in **C# (.NET)**.

Its purpose is to **keep one folder (replica) in sync with another folder (source)**:
- new files are copied
- changed files are updated
- removed files are deleted from the replica


## How to Run from Command Line

You can also run the application from a terminal:

```bash
dotnet run --project FolderSynchronization --source "C:\Source" --replica "C:\Replica" --interval 10

or 

Run it thru exe and follow instruction on CLI

```

Parameters:
- **source** – folder to sync from
- **replica** – folder to sync to
- **interval** – synchronization interval in seconds

---

## How to Run Tests

### Using Visual Studio
1. Open **Test Explorer**
2. Click **Run All Tests**

### Using Command Line
```bash
dotnet test
```