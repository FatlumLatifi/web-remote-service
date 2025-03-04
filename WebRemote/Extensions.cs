
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebRemote.Models;

namespace WebRemote
{
    public static class WebSocketExtensions
    {
        public static async Task SendTextAsUTF8Async(this WebSocket ws, string message) =>
            await ws.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, CancellationToken.None);

        public static bool HandleMessage(this WebSocket webSocket, byte[] buffer, WebSocketReceiveResult result, IWebRemoteServer server)
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
                            server.Mouse.SendClick(button);
                            return true;
                           }
                           else
                           {
                            string[] xy = wrm.data.Split(',', 2, StringSplitOptions.None);
                            int x = int.Parse(xy[0]);
                            int y = int.Parse(xy[1]);
                            server.Mouse.MoveBy(x, y);
                            return true;
                           }
                        }
                        else
                        {
                           
                            if (wrm.isSpecial)
                            {
                                    server.Keyboard.SendKey(wrm.data);
                                return true;
                            }
                            else
                            {
                                server.Keyboard.TypeText(wrm.data);
                                return true;
                            }  
                        }
                    }
                }
            }
            return false;
        }
      
    }
}




