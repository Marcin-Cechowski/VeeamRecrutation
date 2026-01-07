namespace FolderSync
{
    public interface ISyncService
    {
        Task SynchronizeAsync(CancellationToken token);
    }
}
