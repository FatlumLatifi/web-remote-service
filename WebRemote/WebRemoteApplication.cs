using System.Net.WebSockets;
using WebRemote;
using WebRemote.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Runtime.InteropServices;

public static class WebRemoteApplication
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
            reread:
                if (webSocket.State is not WebSocketState.Open) { await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, null, ct); webSocket.Dispose(); return; }
                try
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, ct); // System.Threading.CancellationToken.None);
                    bool shouldReRead = webSocket.HandleWebRemoteMessage(buffer, result, webRemoteServer);
                    if (shouldReRead) { goto reread; }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, ex.Message, ct); webSocket.Dispose(); return; 
                }
            }
           

        });
        app.MapFallbackToFile("{**path:nonfile}", "index.html");
        return app;
    }
}

