using FolderSync;
using Xunit;

namespace FolderSync.Tests;

public class SyncServiceTests
{
    [Fact]
    public void Synchronize_CreatesReplicaAndCopiesAllFiles()
    {
        var source = TestFs.CreateTempDir("source");
        var replica = TestFs.CreateTempDir("replica");

        try
        {
            TestFs.WriteText(Path.Combine(source, "a.txt"), "hello");
            TestFs.WriteText(Path.Combine(source, "sub", "b.txt"), "world");
            TestFs.TouchFile(Path.Combine(source, "bin.dat"), 1024);

            var options = new SyncOptions(source, replica, TimeSpan.FromSeconds(60));
            var svc = new SyncService(options);

            svc.Synchronize();

            Assert.Equal(TestFs.SnapshotTree(source), TestFs.SnapshotTree(replica));
        }
        finally
        {
            TestFs.DeleteDirSafe(source);
            TestFs.DeleteDirSafe(replica);
        }
    }

    [Fact]
    public void Synchronize_UpdatesChangedFiles_InReplica()
    {
        var source = TestFs.CreateTempDir("source");
        var replica = TestFs.CreateTempDir("replica");

        try
        {
            var fileRel = Path.Combine("sub", "x.txt");
            var sourceFile = Path.Combine(source, fileRel);
            var replicaFile = Path.Combine(replica, fileRel);

            // initial state
            TestFs.WriteText(sourceFile, "v1");
            Directory.CreateDirectory(Path.GetDirectoryName(replicaFile)!);
            File.WriteAllText(replicaFile, "old", System.Text.Encoding.UTF8);

            var options = new SyncOptions(source, replica, TimeSpan.FromSeconds(60));
            var svc = new SyncService(options);

            svc.Synchronize();

            Assert.Equal(TestFs.SnapshotTree(source), TestFs.SnapshotTree(replica));

            // change source and sync again
            TestFs.WriteText(sourceFile, "v2");
            svc.Synchronize();

            Assert.Equal(TestFs.SnapshotTree(source), TestFs.SnapshotTree(replica));
        }
        finally
        {
            TestFs.DeleteDirSafe(source);
            TestFs.DeleteDirSafe(replica);
        }
    }

    [Fact]
    public void Synchronize_RemovesOrphanFilesAndDirectories_FromReplica()
    {
        var source = TestFs.CreateTempDir("source");
        var replica = TestFs.CreateTempDir("replica");

        try
        {
            // source has only keep.txt
            TestFs.WriteText(Path.Combine(source, "keep.txt"), "keep");

            // replica has extra file + extra dir
            TestFs.WriteText(Path.Combine(replica, "keep.txt"), "oldKeep");
            TestFs.WriteText(Path.Combine(replica, "orphan.txt"), "orphan");
            TestFs.WriteText(Path.Combine(replica, "oldDir", "old.txt"), "old");

            var options = new SyncOptions(source, replica, TimeSpan.FromSeconds(60));
            var svc = new SyncService(options);

            svc.Synchronize();

            Assert.Equal(TestFs.SnapshotTree(source), TestFs.SnapshotTree(replica));
            Assert.False(File.Exists(Path.Combine(replica, "orphan.txt")));
            Assert.False(Directory.Exists(Path.Combine(replica, "oldDir")));
        }
        finally
        {
            TestFs.DeleteDirSafe(source);
            TestFs.DeleteDirSafe(replica);
        }
    }

    [Fact]
    public void Synchronize_Throws_WhenSourceDoesNotExist()
    {
        var nonExistingSource = Path.Combine(TestFs.CreateTempDir("tmp"), "does-not-exist");
        var replica = TestFs.CreateTempDir("replica");

        try
        {
            var options = new SyncOptions(nonExistingSource, replica, TimeSpan.FromSeconds(60));
            var svc = new SyncService(options);

            Assert.Throws<DirectoryNotFoundException>(() => svc.Synchronize());
        }
        finally
        {
            TestFs.DeleteDirSafe(Path.GetDirectoryName(nonExistingSource)!);
            TestFs.DeleteDirSafe(replica);
        }
    }
}
