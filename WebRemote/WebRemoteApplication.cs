using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using WebRemote;
using WebRemote.Models;
using WindowsInputRemote;

namespace WebRemote;

public static class WebRemoteApplication
{

    public static readonly IPAddress? LocalIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a));


   public static IPEndPoint GetDefaultEndPoint(uint port = 80)
    {
        IPAddress ip = LocalIP ?? IPAddress.Any;

        return new IPEndPoint(ip, (int)port);
    }

    public static WebApplication CreateLocalWebRemote(uint port = 80)
    {
        return CreateWebApplication(GetDefaultEndPoint(port), null);
    }



    public static WebApplication CreateWebApplication(IPEndPoint? endPoint,WebApplicationBuilder? builder)
    {
        if (builder is null)
        {
            builder = WebApplication.CreateBuilder();
            
        }
        if (endPoint is not null)
        {
            builder.WebHost.UseKestrel(options =>
            {
                options.Listen(endPoint);
            });
        }
        

#if WINDOWS
builder.Services.AddSingleton<IWebRemoteControl, WindowsInputRemote.WindowsControl>();
#elif LINUX
builder.Services.AddSingleton<IWebRemoteControl, LinuxInput.X11.X11WebRemoteServer>();
#endif

                // if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                // {
                //     builder.Services.AddSingleton<IWebRemoteControl, WindowsInputRemote.WindowsControl>();
                // }
                // else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                // {
                //     builder.Services.AddSingleton<IWebRemoteControl, LinuxInput.X11.X11WebRemoteServer>();
                // }

                WebApplication app = builder.Build();
        app.UseStaticFiles();
        app.UseDefaultFiles();
        app.UseWebSockets();

        app.MapGet("/ws", async (HttpContext context, CancellationToken ct, [FromServices] IWebRemoteControl webRemoteServer) =>
        {
            Console.WriteLine("/ws was hit, WebSocket request received");
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync().WaitAsync(ct);
                Console.WriteLine("Accepted connection");
                byte[] buffer = new byte[128];
                bool shouldReRead;
            reread:
                if (webSocket.State is not WebSocketState.Open) { await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, null, ct); webSocket.Dispose(); return; }
                try
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, ct); // System.Threading.CancellationToken.None);
                    shouldReRead = webSocket.HandleWebRemoteMessage(buffer, result, webRemoteServer);

                    if (shouldReRead) { goto reread; }
                }
                catch (OperationCanceledException)
                {
                    // host is shutting down (or request was aborted) — close the socket
                    try { await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Server shutting down", CancellationToken.None); } catch { }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WebSocket error: " + ex.Message);
                    try { await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Server shutting down", CancellationToken.None); } catch { }
                }
                Console.WriteLine("WebSocket connection closed");


            }
        });
        app.MapFallbackToFile("{**path:nonfile}", "index.html");


//#if WINDOWS
      // WebRemoteWindowsTray.InitializeTray(exitAction: () => app.StopAsync());
//#endif


        return app;
    }
}

