using System.Net;
using System.Net.WebSockets;
using System.Text;

async Task EchoWebSocket(HttpContext context, WebSocket ws)
{
    var buffer = new byte[1024];
    WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    while (!result.CloseStatus.HasValue)
    {
        // Nhận dữ liệu từ client
        string clientMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

        // Gửi lại thông điệp kèm nội dung mà client đã gửi
        string responseMessage = "Đã nhận: " + clientMessage;
        byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
        await ws.SendAsync(new ArraySegment<byte>(responseBytes), result.MessageType, result.EndOfMessage, CancellationToken.None);

        // Nhận tiếp dữ liệu từ client
        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }
    await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
}

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:6969");
var app = builder.Build();
app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        await EchoWebSocket(context, ws); // Gọi hàm xử lý kết nối WebSocket
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

await app.RunAsync();