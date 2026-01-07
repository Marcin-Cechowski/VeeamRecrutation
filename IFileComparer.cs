namespace FolderSync
{
    public interface IFileComparer
    {
        bool AreDifferent(string file1, string file2);
    }
}
