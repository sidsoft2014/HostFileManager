using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace hfm.core
{
    public class HostFileService : IHostFileService
    {
        const string _hostfilePath = @"C:\Windows\System32\drivers\etc\hosts";
        readonly string[] _defaultDomains = { "rhino.acme.com", "x.acme.com", "localhost" };

        public IEnumerable<HostFileEntry> GetHostFileEntries()
        {
            if (!File.Exists(_hostfilePath))
                yield break;

            var fileLines = File.ReadAllLines(_hostfilePath);
            foreach (var line in fileLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                    continue;

                string ip, domain;
                bool isActive;

                if (parts[0] == "#")
                {
                    ip = parts[1];
                    domain = parts.Length > 2 ? parts[2] : "";
                    isActive = false;
                }
                else if (parts[0].StartsWith("#"))
                {
                    ip = parts[0].Replace("#", "");
                    domain = parts[1];
                    isActive = false;
                }
                else
                {
                    ip = parts[0];
                    domain = parts[1];
                    isActive = true;
                }


                if (IPAddress.TryParse(ip, out IPAddress parsedIp))
                {
                    if (_defaultDomains.Contains(domain))
                        continue;

                    yield return new HostFileEntry(parsedIp, domain, isActive);
                }
            }
        }

        public ResultMessage AddSingleAndWrite(HostFileEntry entry)
        {
            var entries = GetHostFileEntries().ToList();
            if (entries.Contains(entry))
                return new ResultMessage
                {
                    Success = false,
                    Message = "Domain already exists"
                };

            try
            {
                entries.Add(entry);
                var success = Write(entries);
                var msg = success ? $"Domain {entry.Domain} added with IP of {entry.IPString}" : "No changes made";
                return new ResultMessage
                {
                    Success = success,
                    Message = msg
                };
            }
            catch (Exception ex)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public ResultMessage RemoveSingleEntry(HostFileEntry entry)
        {
            var entries = GetHostFileEntries().ToList();
            if (!entries.Contains(entry))
            {
                if (entry.IPString == "127.0.0.1")
                {
                    Console.WriteLine($"Could not find entry with IP: 127.0.0.1. Looking for single match on domain: {entry.Domain}");
                    HostFileEntry otherTry = null;
                    try
                    {
                        otherTry = entries.SingleOrDefault(p => p.Domain == entry.Domain);
                    }
                    catch
                    {

                    }

                    if (otherTry != null)
                    {
                        Console.WriteLine($"Found single match for domain: {entry.Domain}.{Environment.NewLine}Will remove this.{Environment.NewLine}Hope that is what you wanted. :P");
                        return RemoveSingleEntry(otherTry);
                    }
                }

                return new ResultMessage
                {
                    Success = false,
                    Message = "Domain not in host file"
                };
            }

            try
            {
                entries.Remove(entry);
                var success = Write(entries);
                var msg = success ? $"Domain { entry.Domain} removed" : "No changes made";
                return new ResultMessage
                {
                    Success = success,
                    Message = msg
                };
            }
            catch (Exception ex)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public ResultMessage ToggleSingleEntry(HostFileEntry entry)
        {
            var entries = GetHostFileEntries().ToList();
            if (!entries.Contains(entry))
                return new ResultMessage
                {
                    Success = false,
                    Message = "Domain not in host file"
                };

            try
            {
                // Get index of old entry
                var idx = entries.IndexOf(entry);
                // Set IsActive to opposite of old entry
                entry.IsActive = !entries[idx].IsActive;
                // Remove old entry
                entries.RemoveAt(idx);
                // Add new entry
                entries.Insert(idx, entry);

                var success = Write(entries);
                var isActive = entry.IsActive ? "active" : "inactive";
                var msg = success ? $"Domain {entry.Domain} is now {isActive}" : "No changes made";
                return new ResultMessage
                {
                    Success = success,
                    Message = msg
                };
            }
            catch (Exception ex)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public bool Write(IEnumerable<HostFileEntry> hostFileEntries)
        {
            if (!File.Exists(_hostfilePath))
                return false;
            try
            {
                var bakPath = $"{_hostfilePath}.bak";
                if (File.Exists(bakPath))
                    File.Delete(bakPath);
                File.Copy(_hostfilePath, $"{_hostfilePath}.bak");
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return false;
            }


            var sb = new StringBuilder($"#\tGenerated by SidSoft Host Helper: {DateTime.Now}");
            sb.AppendLine(Environment.NewLine);
            foreach (var item in hostFileEntries)
            {
                if (item is null || string.IsNullOrWhiteSpace(item.Domain))
                    continue;

                sb.AppendLine($"{(item.IsActive ? "" : "#")}{item.IPString}\t\t{item.Domain}");
            }

            try
            {
                File.WriteAllText(_hostfilePath, sb.ToString());
            }
            catch(Exception ex)
            {
                Debug.Print(ex.Message);
                return false;
            }

            return true;
        }
    }
}
