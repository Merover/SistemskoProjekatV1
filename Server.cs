using Microsoft.Extensions.Caching.Memory;
using SysProjekat;
using System.Diagnostics;
using System.Net;
using System.Text;

internal class Server
{
    static readonly string RootFolder = Path.Combine(Directory.GetCurrentDirectory());
    static int index = 0;
    private static MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

    public static void StartWebServer()
    {
        var listenerThread = new Thread(Listen);
        listenerThread.IsBackground = true;
        listenerThread.Start();
        Console.WriteLine("Accepting requests...");
    }

    static void Listen()
    {
        using var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();

        while (true)
        {
            try
            {
                HttpListenerContext context = listener.GetContext();
                ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting request: {ex.Message}");
            }
        }
    }

    static void HandleRequest(HttpListenerContext context)
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
            ServeFileFromDisk(response, requestUrl);
        }
    }

    static void ServeCachedFile(HttpListenerResponse response, string requestUrl, byte[] cachedFile)
    {
        response.ContentType = "image/gif";
        response.ContentLength64 = cachedFile.Length;
        response.OutputStream.Write(cachedFile, 0, cachedFile.Length);
        Console.WriteLine($"Cached content found for request: {requestUrl}");
        response.Close();
    }

    static void ServeFileFromDisk(HttpListenerResponse response, string requestUrl)
    {
        Console.WriteLine($"Received the following request: {requestUrl}");

        Stopwatch stopwatch = Stopwatch.StartNew();

        string filePath = Path.Combine(RootFolder, requestUrl.TrimStart('/'));

        if (File.Exists(filePath))
        {
            string gifpath = ConvertClass.ConvertToGif(filePath, Interlocked.Increment(ref index));

            byte[] gifBytes = File.ReadAllBytes(gifpath);

            cache.Set(requestUrl, gifBytes, DateTimeOffset.Now.AddMinutes(10));

            response.ContentType = "image/gif";
            response.ContentLength64 = gifBytes.Length;
            response.OutputStream.Write(gifBytes, 0, gifBytes.Length);
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
}

