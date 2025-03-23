using System.Net.WebSockets;
using WebRemote;
using WebRemote.Models;
using LinuxInput.X11;
using LinuxMediaControl;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Text.Json;
// Add these using directives
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.Runtime.InteropServices;
using WindowsInputRemote;
// Add this directive

public static class WebRemoteApplication
{
    public static WebApplication CreateWebApplication(WebApplicationBuilder? builder)
    {
        if (builder is null)
        {
            builder = WebApplication.CreateBuilder();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            builder.Services.AddSingleton<IMultimediaControl, WindowsMedia>();
            builder.Services.AddSingleton<IWebRemoteControl, WindowsControl>();

        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            builder.Services.AddSingleton<IMultimediaControl, LinuxMedia>();
            builder.Services.AddSingleton<IWebRemoteControl, X11WebRemoteServer>();
        }



        var app = builder.Build();
        app.UseStaticFiles();
        app.UseDefaultFiles();
        app.UseWebSockets();

        app.MapGet("/ws", async (HttpContext context, CancellationToken ct, [FromServices] IMultimediaControl webMedia, [FromServices] IWebRemoteControl webRemoteServer) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                await webSocket.SendTextAsUTF8Async(JsonSerializer.Serialize<AudioRemoteMessage>(webMedia.GetAudioMessage()));
                Console.WriteLine("Sent audio message");

                byte[] buffer = new byte[128];
            //using (X11Mouse mouse = new())

            reread:
                if (webSocket.State is not WebSocketState.Open) { webSocket.Dispose(); return; }
                try
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, ct); // System.Threading.CancellationToken.None);
                    bool shouldReRead = webSocket.HandleMessage(buffer, result, webRemoteServer);
                    if (shouldReRead) { goto reread; }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        });

        app.MapGet("/audio", async (HttpContext context, [FromServices] IMultimediaControl multimedia) =>
        {
            try
            {
                await multimedia.RefreshAsync();
                return Results.Json<AudioRemoteMessage>(multimedia.GetAudioMessage());
            }
            catch (Exception ex) { return Results.Problem(ex.Message); }

        });

        app.MapPost("/audio/volume/{newvolume}", (HttpContext context, [FromRoute] int newvolume, [FromServices] IMultimediaControl webMedia) =>
        {
            try
            {
                webMedia.SetVolume(newvolume);
                return Results.Ok();
            }
            catch (Exception ex) { return Results.Problem(ex.Message); }
        });

        app.MapPost("/audio", async (HttpContext context, [FromServices] IMultimediaControl multimedia) =>
        {

            try
            {
                await multimedia.RefreshAsync();
                MediaControlMessage? mcm = await context.Request.ReadFromJsonAsync<MediaControlMessage?>();
                if (mcm is not null)
                {
                    if (multimedia.ChangeSelectedMediaPlayer(mcm.name) is false) { return Results.NotFound($"{mcm.name} not found or might be closed."); }
                    switch (mcm.action)
                    {
                        case "play":
                        case "pause":
                            multimedia.SelectedMediaPlayer?.PlayPause();
                            break;
                        case "next":
                            multimedia.SelectedMediaPlayer?.Next();
                            break;
                        case "previous":
                            multimedia.SelectedMediaPlayer?.Previous();
                            break;
                        case "stop":
                            multimedia.SelectedMediaPlayer?.Stop();
                            break;
                        default:
                            return Results.BadRequest($"Action: \"{mcm.action}\" does not exist");
                    }
                    return Results.Ok();
                }
                else return Results.BadRequest("Audio control not found. Recheck installation of dependecies.");
            }
            catch (Exception ex) { return Results.Problem(ex.Message); }
        });

        app.MapFallbackToFile("{**path:nonfile}", "index.html");
        return app;
    }
}

