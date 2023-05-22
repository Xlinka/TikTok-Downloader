# TikTok Downloader

This is a C# console application that allows you to download TikTok videos by providing the video URL. The application utilizes the TikTok API to fetch the video metadata and download the video file.



## How to Use

1. Clone this repository to your local machine.
2. Open the solution in your preferred C# IDE.
3. Build the solution to restore the dependencies.
4. Run the application.

## Usage

1. When prompted, enter the URL of the TikTok video you want to download.
2. The application will fetch the video metadata using the TikTok API.
3. If the video is found, it will be downloaded and saved in the "Videos" folder in the application directory.
4. The downloaded video file will be named with the video ID.

## Example

TikTok Downloader
Enter the URL of the TikTok video you want to download: https://www.tiktok.com/@username/video/1234567890123456789

Video ID: 1234567890123456789
Download URL: https://api16-normal-c-useast1a.tiktokv.com/aweme/v1/play/?video_id=1234567890123456789
Video downloaded successfully: Videos/1234567890123456789.mp4

## Notes

- This application uses the TikTok API to fetch the video metadata and download the video file.
- The User-Agent header is set to mimic a web browser to avoid any API restrictions.
- The downloaded video files will be saved in the "Videos" folder within the application directory.
