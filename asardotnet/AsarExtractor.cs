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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace asardotnet
{
    public class AsarExtractor
    {

        public Boolean Extract(AsarArchive archive, String filepath, String destination)
        {
            String[] path = filepath.Split('/');

            JToken token = archive.GetHeader().GetHeaderJson();

            for (int i = 0; i < path.Length; i++)
            {
                token = token["files"][path[i]];
            }

            int size = token.Value<int>("size");
            int offset = archive.GetBaseOffset() + 2 + token.Value<int>("offset");

            byte[] fileBytes = archive.GetBytes().Skip(offset).Take(size).ToArray();

            Utilities.WriteFile(fileBytes, destination);

            return false;
        }

        private List<AFile> _filesToExtract; 

        public Boolean ExtractAll(AsarArchive archive, String destination)
        {
            _filesToExtract = new List<AFile>();

            JToken token = archive.GetHeader().GetHeaderJson();

            TokenIterator(token);

            foreach (AFile aFile in _filesToExtract)
            {
                String[] path = aFile.GetPath().Split('/');

                int size = aFile.GetSize();
                int offset = archive.GetBaseOffset() + 2 + aFile.GetOffset();

                byte[] fileBytes = archive.GetBytes().Skip(offset).Take(size).ToArray();

                Utilities.WriteFile(fileBytes, destination + aFile.GetPath());
            }

            return false;
        }

        private struct AFile
        {
            private String _path;
            public String GetPath() { return _path; }
            private int _size;
            public int GetSize() { return _size; }
            private int _offset;
            public int GetOffset() { return _offset; }

            public AFile(String path, int size, int offset)
            {

                path = path.Replace(".files.", "/").Replace("files.", "").Replace("['", "").Replace("']", "");

                _path = path;
                _size = size;
                _offset = offset;

            }
        }

        private String _lastPath = "";
        private int _lastSize;

        private void TokenIterator(JToken token)
        {
            JTokenType type = token.Type;

            if (type == JTokenType.Object)
            {
                foreach (JToken jToken in token.Children())
                {
                    if (jToken is JProperty)
                    {
                        JProperty jProperty = jToken as JProperty;
                        if (jProperty.Name != "size" && jProperty.Name != "offset")
                        {
                           // Debug.Print("File: " + jProperty.Path);
                            _lastPath = jProperty.Path;
                            TokenIterator(token[jProperty.Name]);
                        }
                        else
                        {
                            if (jProperty.Name == "size")
                            {
                                _lastSize = Int32.Parse(token["size"].ToString());
                            }

                            if (jProperty.Name == "offset")
                            {
                                AFile afile = new AFile(_lastPath, _lastSize, Int32.Parse(token["offset"].ToString()));
                                _filesToExtract.Add(afile);
                            }
                        }

                        
                    }
                }
            }
        }

    }
}
