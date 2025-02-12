using System.Diagnostics;

namespace Alf
{
    class AccountLockoutFinder
    {
        
        static void PrintUsage()
        {
            Console.WriteLine("usage: alf.exe -u <username> -d <domain controllers>");
            Console.WriteLine("domain controllers can be single hostname or comma separated list of hostnames");
        }

        static List<string> GetLockingHosts(string host, string username)
        {
            Process psGetEvent = new();
            psGetEvent.StartInfo.FileName = "powershell.exe";
            psGetEvent.StartInfo.Arguments = $"Invoke-Command -Computer {host} -ScriptBlock {{ Get-WinEvent -FilterHashtable @{{LogName='Security'; Id=4740}}}}";
            psGetEvent.StartInfo.UseShellExecute = false;
            psGetEvent.StartInfo.RedirectStandardOutput = true;
            psGetEvent.StartInfo.RedirectStandardError = true;
            psGetEvent.Start();
            string psGetEventResults = psGetEvent.StandardOutput.ReadToEnd() + "\n" + psGetEvent.StandardError.ReadToEnd();
            psGetEvent.WaitForExit();

            string[] outputLines = psGetEventResults.Split('\n');
            const int LINES_TO_HOSTNAME = 3;
            const int EVENT_MESSAGE_LENGTH = 14;
            const int FIRST_MESSAGE_INDEX = 2;

            List<string> lockingComputers = new List<string>();
            string tempString;
            int lastSpace;
            for (int i = FIRST_MESSAGE_INDEX; i < outputLines.Length; i++)
            {
                if (outputLines[i].Contains("Message"))
                {
                    for (int j = 0; j < EVENT_MESSAGE_LENGTH; j++)
                    {
                        if (outputLines[i + j].Contains(username))
                        {
                            lastSpace = outputLines[i + j + LINES_TO_HOSTNAME].LastIndexOf("\t");
                            tempString = outputLines[i + j + LINES_TO_HOSTNAME].Substring(lastSpace + 1);
                            lockingComputers.Add(tempString);
                        }
                    }
                }
            }

            return lockingComputers;
        }
        static int Main(string[] args)
        {
            DateTime startTime = DateTime.Now;

            if (args.Length != 4)
            {
                PrintUsage();
                return -1;
            }

            string? username =  null;
            string? domainControllers = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-u") { username = args[i + 1]; }
                if (args[i] == "-d") { domainControllers = args[i + 1]; }
            }

            if (username == null || domainControllers == null)
            {
                PrintUsage();
                return -1;
            }
            
            string[] hosts = domainControllers.Split(',');
            List<string>[] lockingHosts = new List<string>[hosts.Length];
            int totalLockingHosts = 0;

            Console.WriteLine($"Searching for user {username} in event logs on {domainControllers}...");
            Console.WriteLine();

            for (int i = 0; i < hosts.Length; i++)
            {
                lockingHosts[i] = GetLockingHosts(hosts[i], username);
                totalLockingHosts += lockingHosts[i].Count;
            }
            string[] finalHosts = new string[totalLockingHosts];

            int finalHostsIndex = 0;
            for (int i = 0; i < lockingHosts.Length; i++)
            {
                Array.Copy(lockingHosts[i].ToArray(), 0, finalHosts, finalHostsIndex, lockingHosts[i].Count);
                if (i == lockingHosts.Length - 1) { break; }
                finalHostsIndex += lockingHosts[i].Count;
            }

            Console.WriteLine($"Number of lockouts found: {totalLockingHosts}");
            Console.WriteLine();

            Console.WriteLine("Locking hosts:");
            foreach (string host in finalHosts.Distinct())
            {
                Console.WriteLine($" {host}");
            }
            Console.WriteLine();

            DateTime endTime = DateTime.Now;
            TimeSpan timeToRun = endTime - startTime;
            Console.WriteLine($"Time elapsed: {timeToRun.TotalSeconds} sec");
            
            return 0;
        }
    }
}
