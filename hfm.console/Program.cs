using hfm.core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace hfm.console
{
    class Program
    {
        private enum ProcessNextStep
        {
            ShowList,
            ShowPrompt,
            Exit
        }

        private static IHostFileService service = new HostFileService();
        private const string configFile = "hfm.config.json";
        private static string guiPath;

        static void Main(string[] args)
        {
            ProcessNextStep nextStep = ProcessNextStep.ShowList;
        StartPoint:

            if (args != null && args.Any() && args[0]?.ToLower() == "gui")
            {
                if (guiPath == null)
                {
                    var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile);
                    if (File.Exists(configPath))
                    {
                        var json = File.ReadAllText(configPath);
                        var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
                        guiPath = obj.Value<string>("GUI");

                        if (File.Exists(guiPath))
                        {
                            ProcessStartInfo start = new ProcessStartInfo();
                            start.FileName = guiPath;
                            start.WindowStyle = ProcessWindowStyle.Normal;
                            start.UseShellExecute = true;
                            start.CreateNoWindow = true;

                            Process.Start(start);
                            return;
                        }
                    }
                }
            }
            else
            {
                nextStep = ProcessInput(args);
            }

            switch (nextStep)
            {
                case ProcessNextStep.ShowPrompt:
                default:
                    Console.WriteLine(Environment.NewLine);
                    break;
                case ProcessNextStep.ShowList:
                    Get(args);
                    break;
                case ProcessNextStep.Exit:
                    return;
            }

            Console.WriteLine("Please type next action");
            args = Console.ReadLine().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            goto StartPoint;
        }

        private static ProcessNextStep ProcessInput(string[] args)
        {
            string cmd = null;

            try
            {
                cmd = args?[0]?.ToLower();
            }
            catch
            {
                // Make sure not to get killed by an index out of range or null ref exception when getting cmd
            }

            switch (cmd)
            {
                case "a":
                case "add":
                    Add(args);
                    return ProcessNextStep.ShowList;
                case "d":
                case "del":
                case "delete":
                case "r":
                case "rem":
                case "remove":
                    Del(args);
                    return ProcessNextStep.ShowList;
                case "t":
                case "tg":
                case "toggle":
                case "s":
                case "sw":
                case "switch":
                    Toggle(args);
                    return ProcessNextStep.ShowList;
                case "g":
                case "get":
                default:
                    Console.Clear();
                    Get(args);
                    return ProcessNextStep.ShowPrompt;
                case "exit":
                case "quit":
                    return ProcessNextStep.Exit;
            }
        }

        private static void Get(string[] args)
        {
            Console.WriteLine($"Trying to read host file");
            Console.WriteLine(Environment.NewLine);
            foreach (var item in service.GetHostFileEntries())
            {
                string active = item.IsActive ? "" : "#\t";
                Console.WriteLine($"{active}{item.IPString}\t{item.Domain}");
            }
            Console.WriteLine(Environment.NewLine);
        }

        private static void Add(string[] args)
        {
            var msg = "";
            // We should have just been passed a domain
            var entry = new HostFileEntry();
            IPAddress ip;
            bool activate = true;

            if (args.Length == 2)
            {
                if (IPAddress.TryParse(args[1], out ip))
                {
                    msg = "Cannot create an entry from just an IP";
                }
                else
                {
                    var domain = args[1].Trim();
                    entry.Domain = domain;
                }
            }
            else // Lets have fun working out what order the args were submitted in
            {
                if (IPAddress.TryParse(args[1], out ip))
                {
                    // If 3 elements we should have just IPAdress and Domain args
                    if (args.Length == 3)
                    {
                        entry.Domain = args[2];
                    }
                    // If 4 elements we should have IPAdress, Domain and IsActive
                    else if (args.Length == 4)
                    {
                        if (bool.TryParse(args[3], out activate))
                        {
                            entry.Domain = args[2];
                        }
                        else if (bool.TryParse(args[2], out activate))
                        {
                            entry.Domain = args[3];
                        }
                    }
                }
                else if (IPAddress.TryParse(args[2], out ip))
                {
                    // If 3 elements we should have just IPAdress and Domain args
                    if (args.Length == 3)
                    {
                        entry.Domain = args[1];
                    }
                    // If 4 elements we should have IPAdress, Domain and IsActive
                    else if (args.Length == 4)
                    {
                        if (bool.TryParse(args[3], out activate))
                        {
                            entry.Domain = args[1];
                        }
                        else if (bool.TryParse(args[1], out activate))
                        {
                            entry.Domain = args[3];
                        }
                    }
                }
                else if (IPAddress.TryParse(args[3], out ip))
                {
                    if (bool.TryParse(args[3], out activate))
                    {
                        entry.Domain = args[1];
                    }
                    else if (bool.TryParse(args[1], out activate))
                    {
                        entry.Domain = args[3];
                    }
                }
                entry.IPAddress = ip;
            }

            entry.IsActive = activate;
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Trying to add {entry.Domain} with IP {entry.IPAddress}");
            var result = service.AddSingleAndWrite(entry);
            PrintResult(result, msg);
        }

        private static void Del(string[] args)
        {
            string msg = "";
            var entry = new HostFileEntry();
            if (args.Length == 2)
            {
                entry.Domain = args[1];
            }
            else if (args.Length == 3)
            {
                IPAddress ip;
                string domain = "";
                if (IPAddress.TryParse(args[1], out ip))
                {
                    domain = args[2];
                }
                else if (IPAddress.TryParse(args[2], out ip))
                {
                    domain = args[1];
                }
                entry.IPAddress = ip;
                entry.Domain = domain;
            }

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Trying to remove {entry.Domain} with IP {entry.IPAddress}");
            var result = service.RemoveSingleEntry(entry);
            PrintResult(result, msg);
        }

        private static void Toggle(string[] args)
        {
            string msg = "";
            var entry = new HostFileEntry();
            if (args.Length == 2)
            {
                entry.Domain = args[1];
            }
            else if (args.Length == 3)
            {
                IPAddress ip;
                string domain = "";
                if (IPAddress.TryParse(args[1], out ip))
                {
                    domain = args[2];
                }
                else if (IPAddress.TryParse(args[2], out ip))
                {
                    domain = args[1];
                }
                entry.IPAddress = ip;
                entry.Domain = domain;
            }

            string active = entry.IsActive ? "active" : "inactive";
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Trying to toggle {entry.Domain} with IP {entry.IPAddress} to be {active}");
            var result = service.ToggleSingleEntry(entry);
            PrintResult(result, msg);
        }

        private static void PrintResult(ResultMessage result, string msg)
        {
            Console.WriteLine(Environment.NewLine);
            if (string.IsNullOrWhiteSpace(msg))
            {
                var successMsg = result.Success ? "Sucess - yay! :)" : "Failed - boo! :(";
                Console.WriteLine(successMsg);
                msg = result.Message;
            }
            Console.WriteLine(msg);
            Console.WriteLine(Environment.NewLine);
        }
    }
}
