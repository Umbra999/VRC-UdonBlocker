using System.Net;
using VRC_AntiFBTHeaven.Wrappers;

namespace VRC_AntiFBTHeaven
{
    internal class APIClient
    {
        public static string MutedUsers = "https://pastebin.com/raw/VjXi7YRP";
        public static string AdminUsers = "https://pastebin.com/raw/FVZUy0XG";
        public static string BannedUsers = "https://pastebin.com/raw/vEZVMirL";

        public static async Task<string> DownloadList(string URL)
        {
            HttpClient Client = new(new HttpClientHandler { UseCookies = false, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            Client.DefaultRequestHeaders.Add("User-Agent", $"Mozilla/5.0 ({Utils.RandomString(25)})");
            Client.DefaultRequestHeaders.Host = "pastebin.com";

            HttpRequestMessage payload = new(HttpMethod.Get, URL);

            HttpResponseMessage Resp = await Client.SendAsync(payload);
            if (Resp.IsSuccessStatusCode) return await Resp.Content.ReadAsStringAsync();

            return null;
        }
    }
}
