using System.Diagnostics;
using System.Security.Principal;
using VRC_AntiFBTHeaven.Wrappers;

namespace VRC_AntiFBTHeaven
{
    internal class Boot
    {
        public static void Main()
        {
            Console.Title = Utils.RandomString(20);

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
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

            while (true)
            {
                ToggleHostBlock(false);
                Console.ReadLine();
                ToggleHostBlock(true);
                Console.ReadLine();
            }
        }

        private static void ToggleHostBlock(bool State)
        {
            List<string> targetDomains = new()
            {
                "pastebin.com",
                "github.io",
                "gist.githubusercontent.com"
            };

            string hostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
            List<string> savedFile = File.ReadAllLines(hostsFilePath).ToList();

            if (State)
            {
                foreach (string domain in targetDomains)
                {
                    if (!savedFile.Contains(domain)) savedFile.Add(domain);
                }

                Logger.LogSuccess("UdonBlocker ENABLED [PRESS ENTER TO TOGGLE]");
            }
            else
            {

                foreach (string domain in targetDomains)
                {
                    if (savedFile.Contains(domain)) savedFile.Remove(domain);
                }

                Logger.LogError("UdonBlocker DISABLED [PRESS ENTER TO TOGGLE]");
            }

            File.WriteAllLines(hostsFilePath, savedFile);
        }
    }
}
