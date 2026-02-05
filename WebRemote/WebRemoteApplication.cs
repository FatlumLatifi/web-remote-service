using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using WebRemote;
using WebRemote.Models;

namespace WebRemote;

public static class WebRemoteApplication
{

    internal static IPAddress GetLocalIP()
    {
        foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            IPInterfaceProperties properties = networkInterface.GetIPProperties();

            foreach (UnicastIPAddressInformation address in properties.UnicastAddresses)
            {
                if (address.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    if (address.Address.ToString().StartsWith("192")) { return address.Address; }
                }
            }
        }
        return IPAddress.Any;
    }

    public static IPEndPoint GetDefaultEndPoint(uint port = 80)
    {
        IPAddress ip = GetLocalIP();

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
builder.Services.AddSingleton<IWebRemoteControl, LinuxInput.UinputControl>();
#endif



        

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
                if (webSocket.State is not WebSocketState.Open || ct.IsCancellationRequested) { await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, null, ct); webSocket.Dispose(); return; }
                try
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, ct); // System.Threading.CancellationToken.None);
                    shouldReRead = webSocket.HandleWebRemoteMessage(buffer, result, webRemoteServer);
                    if (shouldReRead) { goto reread; }
                }
                catch (OperationCanceledException)
                {
                    // host is shutting down (or request was aborted) � close the socket
                    try { await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Server shutting down", ct); } catch { }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WebSocket error: " + ex.Message);
                    try { await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Server shutting down", ct); } catch { }
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

