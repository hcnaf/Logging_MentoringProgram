using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Email;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;

namespace BrainstormSessions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var emailConntectionInfo = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("logsEmailConntecitonInfo.json"));

            return Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.File("Logs\\logs.log")
                    .WriteTo.Email(new EmailConnectionInfo
                    {
                        FromEmail = emailConntectionInfo["fromEmail"],
                        ToEmail = emailConntectionInfo["toEmail"],
                        MailServer = emailConntectionInfo["mailServer"],
                        NetworkCredentials = new NetworkCredential
                        {
                            UserName = emailConntectionInfo["userName"],
                            Password = emailConntectionInfo["password"]
                        },
                        EnableSsl = bool.Parse(emailConntectionInfo["enableSsl"]),
                        Port = int.Parse(emailConntectionInfo["port"]),
                        EmailSubject = emailConntectionInfo["emailSubject"]
                    }))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
