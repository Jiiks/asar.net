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
using Newtonsoft.Json.Linq;

namespace asardotnet {
	public class AsarArchive {
		private const int SIZE_UINT = 4;

		private readonly int _baseOffset;
		public int GetBaseOffset() { return _baseOffset; }

		private readonly byte[] _bytes;
		public byte[] GetBytes() { return _bytes; }

		private readonly String _filePath;
		public String GetFilePath() { return _filePath; }

		private Header _header;
		public Header GetHeader() { return _header; }

		public struct Header {
			private readonly byte[] _headerInfo;
			public byte[] GetHeaderInfo() { return _headerInfo; }
			private readonly int _headerLength;
			public int GetHeaderLenth() { return _headerLength; }

			private readonly byte[] _headerData;
			public byte[] GetHeaderData() { return _headerData; }
			private readonly JObject _headerJson;
			public JObject GetHeaderJson() { return _headerJson; }

			public Header(byte[] hinfo, int length, byte[] data, JObject hjson) {
				_headerInfo = hinfo;
				_headerLength = length;
				_headerData = data;
				_headerJson = hjson;
			}
		}

		public AsarArchive(String filePath) {
			if(!File.Exists(filePath))
				throw new AsarExceptions(AsarException.ASAR_FILE_CANT_FIND);

			_filePath = filePath;

			try {
				_bytes = File.ReadAllBytes(filePath);
			} catch(Exception ex) {
				throw new AsarExceptions(AsarException.ASAR_FILE_CANT_READ, ex.ToString());
			}

			try {
				_header = ReadAsarHeader(ref _bytes);
				_baseOffset = _header.GetHeaderLenth();
			} catch(Exception _ex) {
				throw _ex;
			}
		}

		/*
		 * Exceptions should never be thrown as long as the file 
		 * was created with nodejs asar algorithm
		 */
		private static Header ReadAsarHeader(ref byte[] bytes) {
			int SIZE_LONG = 2 * SIZE_UINT;
			int SIZE_INFO = 2 * SIZE_LONG;

			// Header Info
			byte[] headerInfo = bytes.Take(SIZE_INFO).ToArray();

			if(headerInfo.Length < SIZE_INFO)
				throw new AsarExceptions(AsarException.ASAR_INVALID_FILE_SIZE);

			byte[] asarFileDescriptor = headerInfo.Take(SIZE_LONG).ToArray();
			byte[] asarPayloadSize = asarFileDescriptor.Take(SIZE_UINT).ToArray();

			int payloadSize = BitConverter.ToInt32(asarPayloadSize, 0);
			int payloadOffset = asarFileDescriptor.Length - payloadSize;

			if(payloadSize != SIZE_UINT && payloadSize != SIZE_LONG)
				throw new AsarExceptions(AsarException.ASAR_INVALID_DESCRIPTOR);

			byte[] asarHeaderLength = asarFileDescriptor.Skip(payloadOffset).Take(SIZE_UINT).ToArray();

			int headerLength = BitConverter.ToInt32(asarHeaderLength, 0);

			byte[] asarFileHeader = headerInfo.Skip(SIZE_LONG).Take(SIZE_LONG).ToArray();
			byte[] asarHeaderPayloadSize = asarFileHeader.Take(SIZE_UINT).ToArray();

			int headerPayloadSize = BitConverter.ToInt32(asarHeaderPayloadSize, 0);
			int headerPayloadOffset = headerLength - headerPayloadSize;

			byte[] dataTableLength = asarFileHeader.Skip(headerPayloadOffset).Take(SIZE_UINT).ToArray();
			int dataTableSize = BitConverter.ToInt32(dataTableLength, 0);

			// Data Table
			byte[] hdata = bytes.Skip(SIZE_INFO).Take(dataTableSize).ToArray();

			if(hdata.Length != dataTableSize)
				throw new AsarExceptions(AsarException.ASAR_INVALID_FILE_SIZE);

			int asarDataOffset = asarFileDescriptor.Length + headerLength;
	
			JObject jObject = JObject.Parse(System.Text.Encoding.Default.GetString(hdata));

			return new Header(headerInfo, asarDataOffset, hdata, jObject);
		}
	}
}
