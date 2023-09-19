using System.Net;
using VRC_AntiFBTHeaven.Wrappers;

namespace VRC_AntiFBTHeaven
{
    internal class APIClient
    {
        public static string[] MutedUsers;
        public static string[] AdminUsers;
        public static string[] BannedUsers;

        private static async Task<string> DownloadList(string URL)
        {
            HttpClient Client = new(new HttpClientHandler { UseCookies = false, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            Client.DefaultRequestHeaders.Add("User-Agent", $"Mozilla/5.0 ({Utils.RandomString(25)})");
            Client.DefaultRequestHeaders.Host = "pastebin.com";

            HttpRequestMessage payload = new(HttpMethod.Get, URL);

            HttpResponseMessage Resp = await Client.SendAsync(payload);
            if (Resp.IsSuccessStatusCode) return await Resp.Content.ReadAsStringAsync();

            return null;
        }

        public static async Task FetchLists()
        {
            try
            {
                string MutedUsersList = await DownloadList("https://pastebin.com/raw/YwBVVUCk");
                if (MutedUsersList != null) MutedUsers = MutedUsersList.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                string AdminUsersList = await DownloadList("https://pastebin.com/raw/71DXayVf");
                if (AdminUsersList != null) AdminUsers = AdminUsersList.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                string BannedUsersList = await DownloadList("https://pastebin.com/raw/Jmj7Miin");
                if (BannedUsersList != null) BannedUsers = BannedUsersList.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                Logger.LogDebug("Moderation Dump created");
                Logger.LogDebug($"{MutedUsers.Length} Muted Users");
                Logger.LogDebug($"{AdminUsers.Length} Admin Users");
                Logger.LogDebug($"{BannedUsers.Length} Banned Users");
            }
            catch 
            {
                Logger.LogError("Failed to create Moderation dump, this can happen if the lists are outdated, check my Github for a udpdated version");
            }
        }
    }
}
