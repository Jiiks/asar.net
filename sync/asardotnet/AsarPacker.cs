using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace asardotnet
{
    public class AsarIntegrity
    {
        public const int blockSize = 0x400000;

        public readonly string algorithm = "SHA256";

        public string hash;

        public string[] blocks;

        public AsarIntegrity(string filePath)
        {
            this.hash = GetSha256Sum(filePath);
            this.blocks = GetSha256SumBlocks(filePath);
        }

        private static string[] GetSha256SumBlocks(string filePath)
        {
            List<string> hashes = new List<string>();
            byte[] buffer = new byte[blockSize];
            int bytesRead;
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (BufferedStream bs = new BufferedStream(fs))
            {
                while ((bytesRead = bs.Read(buffer, 0, blockSize)) != 0)
                {
                    byte[] chunk = new byte[bytesRead];
                    Array.Copy(buffer, chunk, bytesRead);
                    hashes.Add(GetSha256Sum(chunk));
                }
            }
            return hashes.ToArray();
        }

        public static string GetSha256Sum(string filename)
        {
            using (SHA256 sha256 = SHA256.Create())
            using (FileStream stream = File.OpenRead(filename))
            {
                return BitConverter.ToString(
                    sha256.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLower();
            }
        }

        public static string GetSha256Sum(byte[] input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return BitConverter.ToString(sha256.ComputeHash(input))
                    .Replace("-", string.Empty).ToLower();
            }
        }
    }

    public class UnpackedAsarFile
    {
        public long size;

        public bool unpacked = true;
    }

    public class AsarFile
    {
        public long size;

        public string offset;

        public AsarIntegrity integrity;
    }

    public class AsarFileNoIntegrity
    {
        public long size;

        public string offset;

    }

    public class AsarDirectory
    {
        public Dictionary<string, object> files = new Dictionary<string, object>();
    }

    public class AsarPacker
    {
        internal bool skipIntegrity = false;

        private string rootDir;

        private long offset = 0;

        private readonly bool verbose = false;

        private Dictionary<string, int> unpackedFiles = new Dictionary<string, int>();

        private AsarDirectory WalkDirectory(AsarDirectory dir, string sourceDir, MemoryStream ms)
        {
            foreach (var entry in Directory.GetFileSystemEntries(sourceDir, "*", SearchOption.TopDirectoryOnly))
            {
                var attrs = File.GetAttributes(entry);
                var fileInfo = new FileInfo(entry);
                string curPath = entry.Replace(rootDir, string.Empty).Replace(@"\", "/").TrimStart('/');

                if (!attrs.HasFlag(FileAttributes.Directory))
                {
                    // Entry is a file
                    if (verbose)
                        Console.WriteLine($"Adding file {Path.GetFullPath(entry)}");

                    if (!skipIntegrity)
                    {
                        // create the integrity block
                        AsarFile asarFile = new AsarFile
                        {
                            offset = offset.ToString(),
                            size = fileInfo.Length,
                            integrity = new AsarIntegrity(entry),
                        };
                        dir.files.Add(Path.GetFileName(entry), asarFile);
                    }
                    else
                    {
                        AsarFileNoIntegrity asarFile = new AsarFileNoIntegrity
                        {
                            offset = offset.ToString(),
                            size = fileInfo.Length,
                        };
                        dir.files.Add(Path.GetFileName(entry), asarFile);
                    }

                    using (var fs = new FileStream(entry, FileMode.Open))
                    {
                        fs.CopyTo(ms);
                        offset += fileInfo.Length;
                    }
                }
                else
                {
                    // Entry is a directory
                    var dirInfo = new DirectoryInfo(entry);

                    if (verbose)
                        Console.WriteLine($"Adding directory {dirInfo.FullName}");

                    AsarDirectory asarDir = new AsarDirectory();
                    asarDir = WalkDirectory(asarDir, dirInfo.FullName, ms);

                    // Insert any "unpacked" files that belong to this directory (if any)
                    foreach (var key in unpackedFiles.Keys)
                    {
                        var uDir = Path.GetDirectoryName(key);

                        var thisDir = dirInfo.FullName.Replace(rootDir, string.Empty).TrimStart('\\');

                        if (uDir == thisDir)
                        {
                            // Mark file as unpacked and don't append data to archive
                            UnpackedAsarFile unpackedFile = new UnpackedAsarFile
                            {
                                size = unpackedFiles[key],
                                unpacked = true
                            };
                            
                            if (verbose)
                                Console.WriteLine($"Adding unpacked file {key}");
                            
                            asarDir.files.Add(Path.GetFileName(key), unpackedFile);
                        }
                    }

                    // Add new directory branch
                    dir.files.Add(dirInfo.Name, asarDir);
                }
            }
            return dir;
        }

        internal static byte[] CreateSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
        }

        public void Pack(string sourceDir, string destFile, Dictionary<string, int> unpackedFiles = null, 
            bool skipIntegrity = true, bool created = false)
        {
            if (!Directory.Exists(sourceDir))
            {
                Console.WriteLine("Source directory does not exist!");
                return;
            }

            Console.WriteLine($"Packing {sourceDir} ..");

            if (unpackedFiles != null)
                this.unpackedFiles = unpackedFiles;

            if (skipIntegrity)
                this.skipIntegrity = true;

            this.rootDir = sourceDir;

            using (var ms = new MemoryStream())
            {
                AsarDirectory root = new AsarDirectory();

                // First we generate the json table of contents
                root = WalkDirectory(root, sourceDir, ms);

                string json = JsonConvert.SerializeObject(root);

                // finally write the header
                using (var ms2 = new MemoryStream())
                using (var bw = new BinaryWriter(ms2))
                {
                    byte[] padding = created ? 
                        CreateSHA256(this.GetType().Namespace).ToArray() : 
                        new byte[] { 0x00, 0x00, 0x00, 0x00 };
                    
                    bw.Write(0x04);
                    bw.Write(json.Length + padding.Length + 8);
                    bw.Write(json.Length + padding.Length + 4);
                    bw.Write(json.Length);
                    bw.Write(Encoding.UTF8.GetBytes(json));
                    bw.Write(padding);
                    bw.Write(ms.ToArray());
                    File.WriteAllBytes(destFile, ms2.ToArray());
                }
            }
        }
    }
}
