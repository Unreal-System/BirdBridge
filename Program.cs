using System.Diagnostics;
using System.Text;
using Tweetinvi;

namespace BirdBridge
{
    internal class Program
    {
        private static readonly string _serviceFile = "/lib/systemd/system/BirdBridge.service";
        internal static ITwitterClient Client { get; private set; }

        private static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (args.Length > 0)
                switch (args[0].ToLower())
                {
                    case "-install":
                        Install();
                        break;
                    case "-uninstall":
                        Uninstall();
                        break;
                    case "-run":
                        Client = new TwitterClient("", "", "", "");
                        CreateHostBuilder(args).Build().Run();
                        break;
                    default:
                        break;
                }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                }).UseSystemd();

        private static void Install()
        {
            if (Environment.UserName != "root")
            {
                Console.WriteLine("Please Run On Root User!");
                Console.WriteLine("Install Failed!");
            }
            else
            {
                try
                {
                    if (!File.Exists(_serviceFile))
                    {
                        using (var file = File.Create(_serviceFile))
                        {
                            using (var sw = new StreamWriter(file))
                            {
                                sw.AutoFlush = true;

                                sw.WriteLine("[Unit]");
                                sw.WriteLine("Description=BirdBridge Service");
                                sw.WriteLine();
                                sw.WriteLine("[Service]");
                                sw.WriteLine("Type=notify");
                                sw.WriteLine($"ExecStart={Process.GetCurrentProcess().MainModule.FileName} -run");
                                sw.WriteLine();
                                sw.WriteLine("[Install]");
                                sw.WriteLine("WantedBy=multi-user.target");
                            }
                        }

                        using (var proc = new Process())
                        {
                            proc.StartInfo.FileName = "/usr/bin/systemctl";
                            proc.StartInfo.Arguments = "enable BirdBridge";
                            proc.Start();
                            proc.WaitForExit();
                            Console.WriteLine("Has Add AutoRunc.");
                        }
						
						using (var proc = new Process())
						{
							proc.StartInfo.FileName = "/usr/bin/systemctl";
                            proc.StartInfo.Arguments = "restart BirdBridge";
                            proc.Start();
                            proc.WaitForExit();
                            Console.WriteLine("Has Start.");
						}
						
                        Console.WriteLine("Install Done.");
                    }
                    else Console.WriteLine("Service config file has exists.");
                }
                catch (Exception ex) { Console.WriteLine($"Install Exception：{ex.GetType()}\r\nException Info：{ex.Message}"); }
            }
        }

        private static void Uninstall()
        {
            if (Environment.UserName != "root")
            {
                Console.WriteLine("Please Run On Root User!");
                Console.WriteLine("UnInstall Fail!");
            }
            else
            {
                try
                {
                    using (var proc = new Process())
                    {
                        proc.StartInfo.FileName = "/usr/bin/systemctl";
                        proc.StartInfo.Arguments = "disable BirdBridge";
                        proc.Start();
                        proc.WaitForExit();
                        Console.WriteLine("Has Del AutoRunc.");
					}
					
					using (var proc = new Process())
					{
                        proc.StartInfo.FileName = "/usr/bin/systemctl";
                        proc.StartInfo.Arguments = "stop BirdBridge";
                        proc.Start();
                        proc.WaitForExit();
                        Console.WriteLine("Has Stop.");
                    }

                    if (File.Exists(_serviceFile))
                    {
                        File.Delete(_serviceFile);
                    }
                    Console.WriteLine("UnInstall Done.");
                }
                catch (Exception ex) { Console.WriteLine($"Delete Exception：{ex.GetType()}\r\nException Info：{ex.Message}"); }
            }
        }
    }
}
