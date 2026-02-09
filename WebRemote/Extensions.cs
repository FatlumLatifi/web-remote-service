
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebRemote.Models;

namespace WebRemote
{
    internal static class WebSocketExtensions
    {
        internal static async Task SendTextAsUTF8Async(this WebSocket ws, string message) =>
            await ws.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, CancellationToken.None);

        internal static bool HandleWebRemoteMessage(this WebSocket webSocket, ReadOnlySpan<byte> buffer, WebSocketReceiveResult result, IWebRemoteControl server)
        {
            if (result.CloseStatus.HasValue is false)
            {
                if (result.Count > 0)
                {
                    WebRemoteMessage? wrm = JsonSerializer.Deserialize<WebRemoteMessage?>(buffer[..result.Count]);
                    if (wrm is not null)
                    {
                        if (wrm.isMouse)
                        {
                          if (wrm.isSpecial)
                           {
                            int button = int.Parse(wrm.data);
                            server.SendClick(button);
                            return true;
                           }
                           else
                           {
                            string[] xy = wrm.data.Split(',', 2, StringSplitOptions.None);
                            int x = int.Parse(xy[0]);
                            int y = int.Parse(xy[1]);
                            server.MoveBy(x, y);
                           // Console.WriteLine($"Moved by {x} and {y}");
                            return true;
                           }
                        }
                        else
                        {
                           
                            if (wrm.isSpecial)
                            {
                                server.SendKey(int.Parse(wrm.data));
                              //  Console.WriteLine($"Handled special key: {wrm.data}");
                                return true;
                            }
                            else
                            {
                                server.TypeText(wrm.data);
                               // Console.WriteLine($"Handled text: {wrm.data}");
                                return true;
                            }  
                        }
                    }
                }
            }
            return false;
        }
      
    }


    /// <summary>
    /// Extension helpers for <see cref="IPEndPoint"/>.
    /// </summary>
    public static class IPEndPointExtensions
    {

        public static string ToHttpUrl(this IPEndPoint value)
        {
             if (value.Port != 80) { return $"http://{value.Address}:{value.Port}/"; }
                else { return $"http://{value.Address}/"; }
        }

    }
    
    
}






