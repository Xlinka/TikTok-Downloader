using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TiktokDownloader
{   

    class Program
    {
        static HttpClient client = new HttpClient();
        static Random rand = new Random();

        static async Task Main(string[] args)
        {
            do
            {
                Console.WriteLine("TikTok Downloader");
                Console.WriteLine("==============================");

                Console.Write("Enter the URL of the TikTok video you want to download: ");
                string url = Console.ReadLine();

                if (!Regex.IsMatch(url, @"https:\/\/www\.tiktok\.com\/.+"))
                {
                    Console.WriteLine("Invalid URL. Please enter a valid TikTok video URL.");
                    continue; 
                }

                string videoId = ExtractVideoId(url);
                if (string.IsNullOrEmpty(videoId))
                {
                    Console.WriteLine("Failed to extract video ID.");
                    continue; 
                }

                SetRandomUserAgent();
                await DownloadVideoAsync(videoId);

                Console.WriteLine("Do you want to download another video? (yes/no)");
            }
            while (Console.ReadLine().Trim().ToLower() == "yes");
        }

        static string ExtractVideoId(string url)
        {
            var match = Regex.Match(url, @"https:\/\/www\.tiktok\.com\/@[^/]+\/video\/(\d+)");
            if (!match.Success)
            {
                Console.WriteLine("Failed to extract Video ID from URL.");
                return null;
            }

            return match.Groups[1].Value;
        }

        static void SetRandomUserAgent()
        {
            string[] userAgents = new string[]
            {
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0.3 Safari/605.1.15",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:87.0) Gecko/20100101 Firefox/87.0",
                // Add more as needed
            };

            string randomAgent = userAgents[rand.Next(userAgents.Length)];
            client.DefaultRequestHeaders.UserAgent.ParseAdd(randomAgent);
        }

        static async Task DownloadVideoAsync(string videoId)
        {
            string apiUrl = $"https://api22-normal-c-alisg.tiktokv.com/aweme/v1/feed/?aweme_id={videoId}&iid=7318518857994389254&device_id=7318517321748022790&channel=googleplay&app_name=musical_ly&version_code=300904&device_platform=android&device_type=ASUS_Z01QD&version=9";
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                string content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Logger.Log($"Failed to fetch video for Video ID: {videoId}. Status code: {response.StatusCode}");
                    return;
                }

                string downloadUrl = ExtractDownloadUrl(content);
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    Console.WriteLine("Failed to extract the download URL from the API response.");
                    return;
                }

                await SaveVideoAsync(downloadUrl, videoId);
            }
            catch (Exception ex)
            {
                Logger.Log($"Exception occurred: {ex.Message}");
            }
        }

        static string ExtractDownloadUrl(string apiResponse)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(apiResponse);
                var videoUrl = doc.RootElement
                                .GetProperty("aweme_list")[0]
                                .GetProperty("video")
                                .GetProperty("play_addr")
                                .GetProperty("url_list")[0].GetString();

                return videoUrl?.Replace("\\u0026", "&");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error parsing JSON response: {ex.Message}");
                return null;
            }
        }

        static async Task SaveVideoAsync(string downloadUrl, string videoId)
        {
            HttpResponseMessage response = await client.GetAsync(downloadUrl);
            if (response.IsSuccessStatusCode)
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "videos");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = Path.Combine(folderPath, $"{videoId}.mp4");
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    await response.Content.CopyToAsync(fs);
                    Console.WriteLine($"Video downloaded successfully: {fileName}");
                }
            }
            else
            {
                Logger.Log($"Failed to download the video. Status code: {response.StatusCode}");
            }
        }
    }
}
