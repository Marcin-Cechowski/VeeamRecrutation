using System.Security.Cryptography;
using System.Text;

namespace FolderSync.Tests;

public static class TestFs
{
    public static string CreateTempDir(string? prefix = null)
    {
        var root = Path.Combine(Path.GetTempPath(), "FolderSyncTests", prefix ?? "run", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    public static void WriteText(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, Encoding.UTF8);
    }

    public static void TouchFile(string path, int sizeBytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var bytes = new byte[sizeBytes];
        RandomNumberGenerator.Fill(bytes);
        File.WriteAllBytes(path, bytes);
    }

    public static void DeleteDirSafe(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
        }
    }

    public static Dictionary<string, string> SnapshotTree(string root)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(root))
            return result;

        foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(root, file).Replace('\\', '/');
            result[rel] = Md5Hex(file);
        }

        return result;
    }

    private static string Md5Hex(string path)
    {
        using var md5 = MD5.Create();
        using var s = File.OpenRead(path);
        var hash = md5.ComputeHash(s);
        return Convert.ToHexString(hash);
    }
}
