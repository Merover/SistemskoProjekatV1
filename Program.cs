using SysProjekat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace SysProjekat
{
    class Program
    {
        static readonly string RootFolder = Path.Combine(Directory.GetCurrentDirectory());
        static void Main()
        {
            if (!Directory.Exists(RootFolder))
                Directory.CreateDirectory(RootFolder);

            Server.StartWebServer();

            Console.WriteLine("Started a web server on http://localhost:8080/");
            Console.ReadKey();
        }
    }
}
