using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using asardotnetasync;

namespace Tests {
    class Program {

        private static string TestPath =>
            $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Discord\\0.0.300\\modules\\discord_desktop_core\\";

        static void Main(string[] args) {

            var arch = new AsarArchive($"{TestPath}core.asar");

            var extractor = new AsarExtractor();

           extractor.FileExtracted += (s, e) => {
               Console.WriteLine(e.Progress);
           };

            extractor.Finished += (s, e) => {
                Console.WriteLine("ALL DONE!");
            };

            Task.Run(() => extractor.ExtractAll(arch, $"{TestPath}test\\", true));
            //extractor.ExtractAll(arch, TestPath + "hi\\");

            Console.ReadLine();

        }
    }
}
