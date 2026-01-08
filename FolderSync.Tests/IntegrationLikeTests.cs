using FolderSync;
using Xunit;

namespace FolderSync.Tests;

/// <summary>
/// Integration-style tests (still in-process) that exercise multiple sync cycles.
/// </summary>
public class IntegrationLikeTests
{
    [Fact]
    public void MultipleRuns_AreIdempotent_AndReplicaAlwaysMatchesSource()
    {
        var source = TestFs.CreateTempDir("source");
        var replica = TestFs.CreateTempDir("replica");

        try
        {
            var options = new SyncOptions(source, replica, TimeSpan.FromSeconds(1));
            var svc = new SyncService(options);

            // Run 1: empty -> empty
            svc.Synchronize();
            Assert.Equal(TestFs.SnapshotTree(source), TestFs.SnapshotTree(replica));

            // Run 2: add a bunch
            TestFs.WriteText(Path.Combine(source, "a.txt"), "A");
            TestFs.WriteText(Path.Combine(source, "sub", "b.txt"), "B");
            svc.Synchronize();
            Assert.Equal(TestFs.SnapshotTree(source), TestFs.SnapshotTree(replica));

            // Run 3: modify + delete + add
            TestFs.WriteText(Path.Combine(source, "a.txt"), "A2");
            File.Delete(Path.Combine(source, "sub", "b.txt"));
            TestFs.WriteText(Path.Combine(source, "sub2", "c.txt"), "C");
            svc.Synchronize();
            Assert.Equal(TestFs.SnapshotTree(source), TestFs.SnapshotTree(replica));

            // Run 4: idempotent
            svc.Synchronize();
            Assert.Equal(TestFs.SnapshotTree(source), TestFs.SnapshotTree(replica));
        }
        finally
        {
            TestFs.DeleteDirSafe(source);
            TestFs.DeleteDirSafe(replica);
        }
    }
}
