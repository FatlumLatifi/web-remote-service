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
using Microsoft.AspNetCore.Http; // Add this directive

public static class WebRemoteApplication
{
    public static WebApplication CreateWebApplication(WebApplicationBuilder? builder)
    {
        if (builder is null) { builder = WebApplication.CreateBuilder(); }
        var app = builder.Build();
        app.UseStaticFiles();
        app.UseDefaultFiles();
        app.UseWebSockets();
        var lm = new LinuxMedia();

        app.MapGet("/ws", async (HttpContext context, CancellationToken ct) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine("Sending Media audio message");
                 
                    await lm.CheckMediaPlayersAsync();
                    await webSocket.SendTextAsUTF8Async(JsonSerializer.Serialize<AudioRemoteMessage>(lm.GetAudioMessage())); 
                    Console.WriteLine("Sent audio message");
                
                byte[] buffer = new byte[128];
                //using (X11Mouse mouse = new())
                using (IWebRemoteServer server = new X11WebRemoteServer())
                {
                reread:
                    if (webSocket.State is not WebSocketState.Open) { webSocket.Dispose(); return; }
                    try
                    {
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, ct); // System.Threading.CancellationToken.None);
                        bool shouldReRead = webSocket.HandleMessage(buffer, result, server);
                        if (shouldReRead) { goto reread; }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        });

        app.MapGet("/audio", async (HttpContext context) => 
        {
            try{
                int mediaPlayersCount = await lm.CheckMediaPlayersAsync();
                return Results.Json<AudioRemoteMessage>(lm.GetAudioMessage());
            }
            catch(Exception ex){ return Results.Problem(ex.Message);}
            
        });

        app.MapPost("/audio/volume/{newvolume}", (HttpContext context, [FromRoute] int newvolume) => 
        {
            try 
            {
                lm.SetVolume(newvolume);
                return Results.Ok();
            }
            catch (Exception ex) { return Results.Problem(ex.Message); }
        });

        app.MapPost("/audio", async (HttpContext context) => 
        {
            try{
            MediaControlMessage? mcm = await context.Request.ReadFromJsonAsync<MediaControlMessage?>();
            if (mcm is not null)
            {
                if (lm.ChangeSelectedMediaPlayer(mcm.name) is false) { return Results.NotFound($"{mcm.name} not found or might be closed.");}
                switch(mcm.action)
                {
                    case "play":
                    case "pause":
                        lm.PlayPause();
                        break;
                    case "next":
                        lm.Next();
                        break;
                    case "previous":
                        lm.Previous();
                        break;
                    case "stop":
                        lm.Stop();
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

