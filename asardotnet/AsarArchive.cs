﻿/*
 *  asar.net Copyright (c) 2015 Jiiks | http://jiiks.net
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
using System.IO;
using System.Linq;
using System.Text;

namespace asardotnet
{
    public class AsarArchive
    {
        private readonly int _baseOffset;
        public int GetBaseOffset() { return _baseOffset; }

        private readonly byte[] _bytes;
        public byte[] GetBytes() { return _bytes; }

        private readonly FileInfo _fileInfo;
        public FileInfo GetFileInfo() { return _fileInfo; }

        private readonly String _filePath;
        public String GetFilePath() { return _filePath; }

        private Header _header;
        public Header GetHeader() { return _header; }

        public struct Header
        {
            private byte[] _headerInfo;
            private readonly int _headerLength;
            public int GetHeaderLenth() { return _headerLength; }

            private byte[] _headerData;
            public byte[] GetHeaderData() { return _headerData; }

            public Header(byte[] hinfo, int length, byte[] data)
            {
                _headerInfo = hinfo;
                _headerLength = length;
                _headerData = data;
            }
        }


        public AsarArchive(FileInfo fileinfo)
        {
            
        }
        public AsarArchive(String filePath)
        {
            _filePath = filePath;
            _bytes = File.ReadAllBytes(filePath);

            byte[] _headerInfo = _bytes.Take(16).ToArray();
            byte[] _headerLength = _headerInfo.Skip(12).Take(4).ToArray();

            int hlength = BitConverter.ToInt16(_headerLength, 0);

            _header = new Header(_headerInfo, hlength, _bytes.Skip(16).Take(hlength).ToArray());
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
