using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Email;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
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
            var emailConneciton = new EmailConnectionInfo
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
                EmailSubject = emailConntectionInfo["emailSubject"],
            };

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
                    .WriteTo.Console()
                    .WriteTo.File("Logs\\logs.log",
                        restrictedToMinimumLevel: LogEventLevel.Debug,
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:dd-MM HH:mm}[{Level:u3}{Message:lj}{Excepiton}{NewLine}]")
                    .WriteTo.EventLog("Brainstorm App", manageEventSource: true)
                    .WriteTo.Email(emailConneciton,
                        restrictedToMinimumLevel: LogEventLevel.Fatal, mailSubject: "FATAL Error on Brainstorm application!")
                    .WriteTo.Email(emailConneciton,
                        restrictedToMinimumLevel: LogEventLevel.Warning, mailSubject: "Warnings, Errors for the last week on Brainstorm application", period: new System.TimeSpan(7, 0, 0, 0)))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
