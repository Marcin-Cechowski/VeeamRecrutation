using System.Security.Cryptography;

namespace FolderSync
{
    public class Md5FileComparer : IFileComparer
    {
        public bool AreDifferent(string file1, string file2)
        {
            FileInfo f1 = new FileInfo(file1);
            FileInfo f2 = new FileInfo(file2);
            if (f1.Length != f2.Length) return true;

            using (MD5 md5 = MD5.Create())
            using (FileStream s1 = File.OpenRead(file1))
            using (FileStream s2 = File.OpenRead(file2))
            {
                byte[] h1 = md5.ComputeHash(s1);
                byte[] h2 = md5.ComputeHash(s2);
                return !h1.SequenceEqual(h2);
            }
        }
    }
}
