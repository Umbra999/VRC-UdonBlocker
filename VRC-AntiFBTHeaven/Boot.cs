using System.Diagnostics;
using System.Security.Principal;
using VRC_AntiFBTHeaven.Wrappers;

namespace VRC_AntiFBTHeaven
{
    internal class Boot
    {
        public static void Main()
        {
            Console.Title = "ANTI FBT HEAVEN BY UMBRA";

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {

                ProcessStartInfo startInfo = new()
                {
                    FileName = Environment.GetCommandLineArgs()[0],
                    UseShellExecute = true,
                    Verb = "runas",
                    Arguments = "/runas"
                };

                Process.Start(startInfo);
                return;
            }

            ToggleHostBlock(false);

            Task ListDumper = Task.Run(APIClient.FetchLists);
            ListDumper.Wait();

            ScanLog();
        }

        private static void ToggleHostBlock(bool State)
        {
            string hostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
            string targetDomain = "0.0.0.0 pastebin.com";
            List<string> savedFile = File.ReadAllLines(hostsFilePath).ToList();

            if (State)
            {
                if (savedFile.Contains(targetDomain)) return;
                savedFile.Add(targetDomain);
                Logger.LogSuccess("Joining FBT Heaven, Anti enabled");
            }
            else
            {
                if (!savedFile.Contains(targetDomain)) return;
                savedFile.Remove(targetDomain);
                Logger.LogSuccess("Leaving FBT Heaven, Anti disabled");
            }

            File.WriteAllLines(hostsFilePath, savedFile);
        }

        private static void ScanLog()
        {
            var directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\VRChat\VRChat");
            if (directory != null && directory.Exists)
            {
                FileInfo target = null;
                foreach (var info in directory.GetFiles("output_log_*.txt", SearchOption.TopDirectoryOnly))
                {
                    if (target == null || info.LastWriteTime.CompareTo(target.LastWriteTime) >= 0) target = info;
                }

                if (target != null)
                {
                    Process[] VRCProcs = Process.GetProcessesByName("VRChat");
                    if (VRCProcs != null && VRCProcs.Length > 0)
                    {
                        Logger.Log($"Watching VRChat Process [{target.Name}]");
                        Process VRChat = VRCProcs[0];

                        ReadNewLines(target.FullName);

                        while (!VRChat.HasExited)
                        {
                            ReadLog(target.FullName);
                            Thread.Sleep(1);
                        }

                        ToggleHostBlock(false);
                    }
                }
            }
        }


        private static void ReadLog(string Path)
        {
            var lines = ReadNewLines(Path);

            foreach (var line in lines)
            {
                if (line.Contains("Destination set: "))
                {
                    ToggleHostBlock(line.Contains("Destination set: wrld_d319c58a-dcec-47de-b5fc-21200116462c"));
                }

                else if (line.Contains("OnPlayerJoined "))
                {
                    string[] parts = line.Split(new[] { "OnPlayerJoined " }, StringSplitOptions.None);
                    string DisplayName = parts[1];

                    if (APIClient.MutedUsers.Contains(DisplayName)) Logger.Log($"Muted User {DisplayName} connected");
                    if (APIClient.AdminUsers.Contains(DisplayName)) Logger.Log($"Admin User {DisplayName} connected");
                    if (APIClient.BannedUsers.Contains(DisplayName)) Logger.Log($"Banned User {DisplayName} connected");
                }
            }
        }

        private static List<string> ReadNewLines(string filePath)
        {
            List<string> lines = new();

            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);

                reader.BaseStream.Seek(LastReadOffset, SeekOrigin.Begin);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }

                LastReadOffset = reader.BaseStream.Position;
            }
            catch (IOException ex)
            {
                Logger.LogError(ex);
            }

            return lines;
        }

        private static long LastReadOffset = 0;
    }
}
