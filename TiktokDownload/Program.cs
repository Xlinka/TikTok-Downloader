using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TiktokDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("TikTok Downloader");
            Console.WriteLine("==============================");

            Console.Write("Enter the URL of the TikTok video you want to download: ");
            string url = Console.ReadLine();

            // Check if the input is a TikTok video URL
            if (Regex.Match(url, @"https:\/\/www\.tiktok\.com\/.+").Success)
            {
                // Create a Video object and download the video
                Video video = new Video(url);
                Console.Write(video);
                await video.DownloadAsync();
            }
            else
            {
                Console.WriteLine("Invalid URL. Please enter a valid TikTok video URL.");
            }
        }

        class Video
        {
            public string VideoId { get; private set; }
            public string DownloadUrl { get; private set; }

            public Video(string url)
            {
                // Extract the video ID from the URL
                VideoId = GetVideoId(url);

                // Fetch the video metadata
                FetchMetadata().Wait();
            }

            private async Task FetchMetadata()
            {
                string apiUrl = $"https://api16-normal-c-useast1a.tiktokv.com/aweme/v1/feed/?aweme_id={VideoId}";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36");

                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    string content = await response.Content.ReadAsStringAsync();

                    // Extract the download URL from the API response
                    DownloadUrl = ExtractDownloadUrl(content);
                }
            }

            private string GetVideoId(string url)
            {
                // Extract the video ID from the URL
                var match = Regex.Match(url, @"https:\/\/www\.tiktok\.com\/@[^/]+\/video\/(\d+)");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                return null;
            }

            private string ExtractDownloadUrl(string content)
            {
                try
                {
                    dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                    string url = data.aweme_list[0].video.play_addr.url_list[0].Value;
                    return url.Replace("playwm", "play");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error extracting download URL: {ex.Message}");
                    return null;
                }
            }

            public override string ToString()
            {
                if (string.IsNullOrEmpty(DownloadUrl))
                {
                    return $"==============================\nVideo ID: {VideoId}\nUnable to retrieve video info.\n==============================\n";
                }
                else
                {
                    return $"==============================\nVideo ID: {VideoId}\nDownload URL: {DownloadUrl}\n==============================\n";
                }
            }

            public async Task DownloadAsync()
            {
                if (string.IsNullOrEmpty(DownloadUrl))
                {
                    Console.WriteLine("Unable to download the video.");
                    return;
                }

                using (HttpClient client = new HttpClient())
                {
                    string fileName = $"Videos/{VideoId}.mp4";

                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36");
                    client.DefaultRequestHeaders.Add("Referer", "https://www.tiktok.com/");

                    HttpResponseMessage response = await client.GetAsync(DownloadUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to download video. Status code: {response.StatusCode}");
                        return;
                    }

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                           stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                    {
                        await contentStream.CopyToAsync(stream);
                    }

                    Console.WriteLine($"Video downloaded successfully: {fileName}");
                }
            }
        }
    }
}
