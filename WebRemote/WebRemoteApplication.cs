using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using WebRemote;
using WebRemote.Models;
using WindowsInputRemote;

public class WebRemoteApplication
{
    public static WebApplication CreateWebApplication(WebApplicationBuilder? builder)
    {
        if (builder is null)
        {
            builder = WebApplication.CreateBuilder();
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
                    Console.WriteLine(ex.Message);
                    await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, ex.Message, ct); webSocket.Dispose(); return; 
                }
            }
           

        });
        app.MapFallbackToFile("{**path:nonfile}", "index.html");


#if WINDOWS
        WebRemoteWindowsTray.InitializeTray(exitAction: () => app.StopAsync());
#endif


        return app;
    }
}

