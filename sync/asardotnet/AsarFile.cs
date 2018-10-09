using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asardotnet
{
    public struct AsarFile
    {
        private String _path;
        public String GetPath() { return _path; }
        private int _size;
        public int GetSize() { return _size; }
        private int _offset;
        public int GetOffset() { return _offset; }

        public AsarFile(String path, String fileName, int size, int offset)
        {
            path = path.Replace("['", "").Replace("']", "");
            path = path.Substring(0, path.Length - fileName.Length);
            path = path.Replace(".files.", "/").Replace("files.", "");
            path += fileName;

            _path = path;
            _size = size;
            _offset = offset;
        }
    }
}
