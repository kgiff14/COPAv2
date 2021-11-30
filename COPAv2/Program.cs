using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace COPAv2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using var process = new Process();
            process.StartInfo.FileName = "C:\\Users\\kordellgifford\\source\\repos\\COPAv2\\COPA.Template\\bin\\Debug\\net5.0\\COPA.Template.exe";
            process.StartInfo.CreateNoWindow = false;
            process.Start();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
