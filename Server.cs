using NHibernate.Cache;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SysProjekat
{
    internal class AsyncServer
    {
        static readonly string RootFolder = Path.Combine(Directory.GetCurrentDirectory());
        static int index = 0;
        private static MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        public static async Task StartWebServer()
        {
            using var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            Console.WriteLine("Accepting requests...");

            while (true)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting request: {ex.Message}");
                }
            }
        }

        static async Task HandleRequestAsync(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if (request.Url == null)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close();
                return;
            }

            string requestUrl = request.Url.LocalPath;
            byte[] cachedFile = cache.Get(requestUrl) as byte[];

            if (cachedFile != null)
            {
                ServeCachedFile(response, requestUrl, cachedFile);
            }
            else
            {
                await ServeFileFromDiskAsync(response, requestUrl);
            }
        }

        static void ServeCachedFile(HttpListenerResponse response, string requestUrl, byte[] cachedFile)
        {
            response.ContentType = GetContentType(requestUrl);
            response.ContentLength64 = cachedFile.Length;
            response.OutputStream.Write(cachedFile, 0, cachedFile.Length);
            Console.WriteLine($"Cached content found for request: {requestUrl}");
            response.Close();
        }

        static async Task ServeFileFromDiskAsync(HttpListenerResponse response, string requestUrl)
        {
            Console.WriteLine($"Received the following request: {requestUrl}");

            Stopwatch stopwatch = Stopwatch.StartNew();

            string filePath = Path.Combine(RootFolder, requestUrl.TrimStart('/'));

            if (File.Exists(filePath))
            {
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);

                cache.Set(requestUrl, fileBytes, DateTimeOffset.Now.AddMinutes(10));

                _ = Task.Run(() => ConvertClass.ConvertToGif(filePath, ++index));

                response.ContentType = GetContentType(filePath);
                response.ContentLength64 = fileBytes.Length;
                await response.OutputStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                Console.WriteLine("Created a .gif file.");
            }
            else
            {
                RespondWithNotFound(response, requestUrl);
            }

            response.Close();
            stopwatch.Stop();
            Console.WriteLine($"Processed the request in {stopwatch.ElapsedMilliseconds} milliseconds.");
        }

        static void RespondWithNotFound(HttpListenerResponse response, string requestUrl)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            string errorMessage = $"File not found: {requestUrl}";
            byte[] errorBytes = Encoding.UTF8.GetBytes(errorMessage);
            response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
            response.Close();
        }

        static string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
