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
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace asardotnet {
	public class AsarExtractor {

		public Boolean Extract(AsarArchive archive, String filepath, String destination) {
			String[] path = filepath.Split('/');

			JToken token = archive.GetHeader().GetHeaderJson();

			for(int i = 0; i < path.Length; i++) {
				token = token["files"][path[i]];
			}

			int size = token.Value<int>("size");
			int offset = archive.GetBaseOffset() + token.Value<int>("offset");

			byte[] fileBytes = archive.GetBytes().Skip(offset).Take(size).ToArray();

			Utilities.WriteFile(fileBytes, destination);

			return false;
		}

		private List<AFile> _filesToExtract;

		public Boolean ExtractAll(AsarArchive archive, String destination) {
			_filesToExtract = new List<AFile>();

			JObject jObject = archive.GetHeader().GetHeaderJson();
			if(jObject.HasValues)
				TokenIterator(jObject.First);

			byte[] bytes = archive.GetBytes();

			foreach(AFile aFile in _filesToExtract) {
				int size = aFile.GetSize();
				int offset = archive.GetBaseOffset() + aFile.GetOffset();
				if(size > 0) {
					byte[] fileBytes = new byte[size];

					Buffer.BlockCopy(bytes, offset, fileBytes, 0, size);
					Utilities.WriteFile(fileBytes, destination + aFile.GetPath());
				} else {
					Utilities.CreateDirectory(destination + aFile.GetPath());
				}
			}

			return false;
		}

		private struct AFile {
			private String _path;
			public String GetPath() { return _path; }
			private int _size;
			public int GetSize() { return _size; }
			private int _offset;
			public int GetOffset() { return _offset; }

			public AFile(String path, String fileName, int size, int offset) {
				path = path.Replace("['", "").Replace("']", "");
				path = path.Substring(0, path.Length - fileName.Length);
				path = path.Replace(".files.", "/").Replace("files.", "");
				path += fileName;

				_path = path;
				_size = size;
				_offset = offset;
			}
		}

		private void TokenIterator(JToken jToken) {
			JProperty jProperty = jToken as JProperty;

			foreach(JProperty prop in jProperty.Value.Children()) {
				int size = -1;
				int offset = -1;
				foreach(JProperty nextProp in prop.Value.Children()) {
					if(nextProp.Name == "files") {
						/* ENABLE FOR EMPTY FOLDERS (ONLY IF NEEDED)
						AFile afile = new AFile(prop.Path, "", size, offset);
						_filesToExtract.Add(afile);*/
						TokenIterator(nextProp);
					} else {
						if(nextProp.Name == "size")
							size = Int32.Parse(nextProp.Value.ToString());
						if(nextProp.Name == "offset")
							offset = Int32.Parse(nextProp.Value.ToString());
					}
				}

				if(size > -1 && offset > -1) {
					AFile afile = new AFile(prop.Path, prop.Name, size, offset);
					_filesToExtract.Add(afile);
				}
			}
		}
	}
}
