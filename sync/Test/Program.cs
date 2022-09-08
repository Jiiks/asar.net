using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using asardotnet;

namespace asardotnet
{
    class Program
    {
        static void PrintUsage()
        {
            Console.WriteLine("\n Usage: Test.exe [command] [options]");
            Console.WriteLine("\n Commands:");
            Console.WriteLine("\n    pack|p <dir> <output>\n      create asar archive");
            Console.WriteLine("\n    list|l <archive>\n      list files of asar archive");
            Console.WriteLine("\n    extract-file|ef <archive> <filename>\n      extract one file from asar archive");
            Console.WriteLine("\n    extract|e <archive> <dest>\n      extract asar archive");
            Console.WriteLine("\n");
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }

            AsarArchive asarArchive;
            AsarExtractor extractor = new AsarExtractor();
            AsarPacker packer = new AsarPacker();

            switch (args[0].ToLower())
            {
                case "e":
                case "extract":
                    if (args.Length != 3 || !File.Exists(args[1]))
                    {

                        PrintUsage();
                        return;
                    }
                    asarArchive = new AsarArchive(args[1]);
                    extractor.ExtractAll(asarArchive, args[2]);
                    return;
                case "ef":
                case "extract-file":
                    if (args.Length != 3)
                    {
                        PrintUsage();
                        return;
                    }
                    asarArchive = new AsarArchive(args[1]);
                    extractor.Extract(asarArchive, args[1], args[2]);
                    break;
                case "l":
                case "list":
                    if (!File.Exists(args[1]))
                    {
                        PrintUsage();
                        return;
                    }
                    asarArchive = new AsarArchive(args[1]);
                    extractor.ListAll(asarArchive);
                    return;
                case "p":
                case "pack":
                    if (args.Length != 3)
                    {
                        PrintUsage();
                        break;
                    }
                    packer.Pack(args[1], args[2]);
                    return;
                default:
                    break;
            }
        }
    }
}