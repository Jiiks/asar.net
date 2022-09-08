/*
 *  asar.net Copyright (c) 2015 Jiiks | http://jiiks.net
 * 
 *  https://github.com/Jiiks/asar.net
 * 
 *  For: https://github.com/atom/asar
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * */


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace asardotnet
{
    public class AsarExtractor
    {
        public bool Extract(AsarArchive archive, string filepath, string destination)
        {
            string[] path = filepath.Split('/');

            JToken token = archive.GetHeader().GetHeaderJson();

            for (int i = 0; i < path.Length; i++)
            {
                token = token["files"][path[i]];
            }

            int size = token.Value<int>("size");
            int offset = archive.GetBaseOffset() + token.Value<int>("offset");

            byte[] fileBytes = archive.GetBytes().Skip(offset).Take(size).ToArray();

            Utilities.WriteFile(fileBytes, destination);

            return false;
        }

        private List<AFile> filesToExtract;

        public Dictionary<string, int> unpackedFiles = new Dictionary<string, int>();

        private readonly bool verbose = false;

        private struct AFile
        {
            private readonly string path;

            public string GetPath() { return path; }

            private readonly int size;

            public int GetSize() { return size; }

            private readonly int offset;

            public int GetOffset() { return offset; }

            public AFile(string path, int size, int offset)
            {
                this.path = path;
                this.size = size;
                this.offset = offset;
            }
        }

        private void TokenIterator(JObject jObj, string fullPath)
        {
            foreach (KeyValuePair<string, JToken> entry in jObj)
            {
                if (entry.Value["files"] != null)
                {
                    var newPath = fullPath + entry.Key + Path.DirectorySeparatorChar;
                    var newDir = new AFile(newPath, -1, -1);
                    this.filesToExtract.Add(newDir);
                    TokenIterator((JObject)entry.Value["files"], newPath);
                }
                if (entry.Value["unpacked"] != null && entry.Value["size"] != null)
                {
                    if (bool.Parse(entry.Value["unpacked"].ToString()))
                    {
                        this.unpackedFiles.Add(fullPath + entry.Key, int.Parse(entry.Value["size"].ToString()));
                    }
                }
                if (entry.Value["size"] != null && entry.Value["offset"] != null)
                {
                    int size = int.Parse(entry.Value["size"].ToString());
                    int offset = int.Parse(entry.Value["offset"].ToString());
                    var aFile = new AFile(fullPath + entry.Key, size, offset);
                    this.filesToExtract.Add(aFile);
                }
            }
        }

        public bool ExtractAll(AsarArchive archive, string destination)
        {
            filesToExtract = new List<AFile>();

            JObject jObject = archive.GetHeader().GetHeaderJson();

            if (jObject.HasValues)
                TokenIterator((JObject)jObject["files"], "");

            Console.WriteLine($"Extracting files to: {destination} ..");

            byte[] bytes = archive.GetBytes();

            foreach (AFile aFile in filesToExtract)
            {
                if (verbose)
                    Console.WriteLine($"Extracting.. {aFile.GetPath()}");

                int size = aFile.GetSize();

                int offset = archive.GetBaseOffset() + aFile.GetOffset();

                if (size > -1)
                {
                    byte[] fileBytes = new byte[size];
                    Buffer.BlockCopy(bytes, offset, fileBytes, 0, size);
                    try
                    {
                        Utilities.WriteFile(fileBytes, Path.Combine(destination, aFile.GetPath()));
                    }
                    catch (PathTooLongException)
                    {
                        Console.WriteLine($"Error unpacking {aFile.GetPath()}");
                        Console.WriteLine("File name is too long. Try setting current directory to a shorter path (e.g. c:\temp)");
                        return false;
                    }
                }
                else
                {
                    Utilities.CreateDirectory(Path.Combine(destination, aFile.GetPath()));
                }
            }

            return true;
        }

        public void ListAll(AsarArchive archive)
        {
            filesToExtract = new List<AFile>();

            JObject jObject = archive.GetHeader().GetHeaderJson();

            if (jObject.HasValues)
                TokenIterator((JObject)jObject["files"], "");

            foreach (var aFile in filesToExtract)
            {
                Console.WriteLine(aFile.GetPath());
            }
        }
    }
}
