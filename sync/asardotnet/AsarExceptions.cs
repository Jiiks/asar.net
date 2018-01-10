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

namespace asardotnet {
    public enum AsarException {
        ASAR_FILE_CANT_FIND,
        ASAR_FILE_CANT_READ,
        ASAR_INVALID_DESCRIPTOR,
        ASAR_INVALID_FILE_SIZE
    };

    public class AsarExceptions: Exception {
        private readonly AsarException _asarException;
        private readonly string _asarMessage;

        public AsarExceptions(AsarException ex) : this(ex, "") { }

        public AsarExceptions(AsarException ex, String customMessage) {
            _asarException = ex;
            if(customMessage.Length > 0)
                _asarMessage = customMessage;
            else
                _asarMessage = GetMessage(ex);
        }

        private String GetMessage(AsarException ex) {
            String result;

            switch(ex) {
                case AsarException.ASAR_FILE_CANT_FIND:
                    result = "Error: The specified file couldn't be found.";
                    break;
                case AsarException.ASAR_FILE_CANT_READ:
                    result = "Error: File can't be read.";
                    break;
                case AsarException.ASAR_INVALID_DESCRIPTOR:
                    result = "Error: File's header size is not defined on 4 or 8 bytes.";
                    break;
                case AsarException.ASAR_INVALID_FILE_SIZE:
                    result = "Error: Data table size shorter than the size specified in in the header.";
                    break;
                default:
                    result = "Error: Unhandled exception !";
                    break;
            }

            return result;
        }

        public AsarException GetExceptionCode() {
            return _asarException;
        }

        public String GetExceptionMessage() {
            return _asarMessage;
        }

        override public String ToString() {
            return "(Code " + GetExceptionCode() + ") " + GetExceptionMessage();
        }
    }

}
