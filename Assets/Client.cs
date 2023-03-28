using System;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Client
{
    private TcpClient _client;
    private NetworkStream _stream;
    private JObject _latestMessage;

    public Client()
    {
        _latestMessage = null;
    }

    public void Connect(string address, int port)
    {
        _client = new TcpClient(address, port);
        _stream = _client.GetStream();
        StartReceiving();
    }

    private async void StartReceiving()
    {
        while (true)
        {
            byte[] buffer = new byte[4096];
            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
            string jsonString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            _latestMessage = JsonConvert.DeserializeObject<JObject>(jsonString);
        }
    }

    public JObject Receive()
    {
        return _latestMessage;
    }

    public void Send(JObject message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message.ToString());
        _stream.Write(buffer, 0, buffer.Length);
    }
}
