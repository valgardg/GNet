using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class Server
{
    private TcpListener _listener;
    private Dictionary<string, Action<JObject>> _eventHandlers;
    private Thread _listenerThread;

    public Server()
    {
        _eventHandlers = new Dictionary<string, Action<JObject>>();
    }

    public void Start(string address, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(address), port);
        _listener.Start();
        _listenerThread = new Thread(ListenForClients);
        _listenerThread.Start();
    }

    private void ListenForClients()
    {
        while (true)
        {
            TcpClient client = _listener.AcceptTcpClient();
            Thread clientThread = new Thread(HandleClientComm);
            clientThread.Start(client);
        }
    }

    private void HandleClientComm(object clientObj)
    {
        TcpClient client = (TcpClient)clientObj;
        NetworkStream stream = client.GetStream();

        while (true)
        {
            byte[] buffer = new byte[4096];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead == 0)
                break;

            string jsonString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            JObject message = JsonConvert.DeserializeObject<JObject>(jsonString);

            if (message.ContainsKey("event") && _eventHandlers.ContainsKey(message["event"].ToString()))
            {
                _eventHandlers[message["event"].ToString()](message);
            }
        }
        client.Close();
    }

    public void On(string eventName, Action<JObject> handler)
    {
        _eventHandlers[eventName] = handler;
    }
}
