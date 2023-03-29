using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using UnityEngine;

public class Client
{
    private TcpClient _client;
    private NetworkStream _stream;
    private JObject _latestMessage;
    private CancellationTokenSource _cancellationTokenSource;

    private bool connected;

    private int _broadcastPort;

    /*
    Initialises the clients attributes
    */
    public Client(int broadcastPort = 8888)
    {
        _latestMessage = null;
        _broadcastPort = broadcastPort;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /*
    Method that attempts to connect to a server with the specified address and port.
    Very much blocking so its important to make sure that there is a server running
    on the specified address and port. Also calls the clients StartReceiving function
    to start receiving messages from the server.
    */
    public void Connect(string address, int port)
    {
        _client = new TcpClient(address, port);
        connected = true;
        _stream = _client.GetStream();
        StartReceiving(_cancellationTokenSource.Token);
    }

    /*
    Thread responsible for receiving messages from the server 
    client is connected to and sets the clients _latestMessage 
    to whatever was recevied from the client.
    */
    private async void StartReceiving(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            byte[] buffer = new byte[4096];
            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
            string jsonString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            
            // TODO # Is this even more lag?? Need to figure out why it fails.
            try
            {
                _latestMessage = JsonConvert.DeserializeObject<JObject>(jsonString);
            }
            catch(Exception)
            {
                _latestMessage = null;
            }
        }
    }

    /*
    Returns the latest message received by the client from the server.
    */
    public JObject Receive()
    {
        return _latestMessage;
    }

    /*
    Is client connected to server?
    */
    public bool IsConnected(){
        return connected;
    }

    /* 
    Receives a JObect and parses it into a string and then sends 
    the string as a message to the server it is connected to.
    */
    public void Send(JObject message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message.ToString() + "$");
        _stream.Write(buffer, 0, buffer.Length);
    }

    /*
    TODO: Call Thread for every port specified by client
    */
    public void DiscoverServers(Action<IPEndPoint> onServerDiscovered, int[] ports)
    {
        foreach(int p in ports){
            ThreadPool.QueueUserWorkItem(ReceiveBroadcasts, Tuple.Create(onServerDiscovered, _cancellationTokenSource.Token, p));
        }
    }

    /*
    Attempts to receive message from server in order to check if a server is 
    running on a certain port. Once a servers broadcast is found it calls the 
    action specified in the state object passed into the function.
    */
    private async void ReceiveBroadcasts(object state)
    {
        var tuple = (Tuple<Action<IPEndPoint>, CancellationToken, int>)state;
        Action<IPEndPoint> onServerDiscovered = tuple.Item1;
        CancellationToken cancellationToken = tuple.Item2;
        int portToCheck = tuple.Item3;

        using (UdpClient udpClient = new UdpClient(portToCheck))
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string serverAddress = Encoding.UTF8.GetString(result.Buffer);
                IPEndPoint serverEndpoint = ParseIPEndPoint(serverAddress);
                onServerDiscovered(serverEndpoint);
            }
        }
    }

    /* 
    Attempts to parse a string into an IPAddress object and returns the object.
    */
    private IPEndPoint ParseIPEndPoint(string endPointString)
    {
        string[] parts = endPointString.Split(':');
        if (parts.Length != 2)
            throw new FormatException("Invalid endpoint format");

        IPAddress address;
        if (!IPAddress.TryParse(parts[0], out address))
            throw new FormatException("Invalid IP address");

        int port;
        if (!int.TryParse(parts[1], out port))
            throw new FormatException("Invalid port number");

        return new IPEndPoint(address, port);
    }

    /*
    Attempts to stop the client, not sure if it works or not tbh..
    */
    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _client?.Close();
    }
}
