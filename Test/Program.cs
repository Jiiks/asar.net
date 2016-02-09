using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using asardotnet;
using Newtonsoft.Json.Linq;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
  
            

            
            AsarArchive asarArhive = new AsarArchive("G:\\Asardotnet\\app.asar");
            AsarExtractor extractor = new AsarExtractor();;
//
    //        String filepath = "G:\\Asardotnet\\out\\lel";

      //      Debug.Print(Path.GetDirectoryName(filepath));


            //Directory.CreateDirectory("G:\\Asardotnet\\out\\lel\\lel\\lel");

             extractor.ExtractAll(asarArhive, "G:\\Asardotnet\\out\\");

            // extractor.Extract(asarArhive, "app/index.js", "G:\\Asardotnet\\out\\index.js");

            // AsarExtractor extractor = new AsarExtractor();
            // extractor.Extract(asarArhive, "NotificationWindow.js", "");

            // AsarExtractor asarExtractor = new AsarExtractor();

            //asarExtractor.Extract(asarArhive, "G:\\Asardotnet\\extract\\");

            // asarExtractor.ExtractFile(asarArhive, 8528, 6479);
        }
    }
}
