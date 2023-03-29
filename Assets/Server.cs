using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Server
{
    private TcpListener _listener;
    private Dictionary<string, Action<JObject>> _eventHandlers;
    private Thread _listenerThread;
    private List<TcpClient> _connectedClients;
    private string _gameState;
    private int _updateInterval;
    private int _broadcastPort;
    private CancellationTokenSource _cancellationTokenSource;

    // lock object for gamestate
    private readonly Queue<Action> _actionsQueue = new Queue<Action>();

    /*
    Initializes the server and all its attributes.
    */
    public Server(int updateInterval = 1000)
    {
        _eventHandlers = new Dictionary<string, Action<JObject>>();
        _connectedClients = new List<TcpClient>();
        _gameState = "";
        _updateInterval = updateInterval;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /*
    Creates server TcpListener on the specified address and port. 
    Sets up all of servers threads and loops responsible for handling 
    incoming clienit connectons and requests as well as broadcasting 
    server _gameState and port to potential clients.
    */
    public void Start(string address, int port = 8888)
    {
        _broadcastPort = port;
        _listener = new TcpListener(IPAddress.Parse(address), port);
        _listener.Start();
        _listenerThread = new Thread(() => ListenForClients(_cancellationTokenSource.Token));
        _listenerThread.Start();
        ThreadPool.QueueUserWorkItem(SendGameStateUpdates, _cancellationTokenSource.Token);
        ThreadPool.QueueUserWorkItem(BroadcastPresence, Tuple.Create(new IPEndPoint(IPAddress.Parse(address), port), _cancellationTokenSource.Token));
    }

    /*
    Thread responsible for listening for incoming client connections and 
    adding a tcp client to the list of connected clients so that it can 
    start receiving messages and sending _gameState.
    */
    private void ListenForClients(CancellationToken cancellationToken)
    {
        try{
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = _listener.AcceptTcpClient();
                lock (_connectedClients)
                {
                    _connectedClients.Add(client);
                }
                Thread clientThread = new Thread(HandleClientComm);
                clientThread.Start(client);
            }
        }catch(Exception ex){
            if(!cancellationToken.IsCancellationRequested){
                Debug.Log($"Exception occured: {ex}");
            }
        }
    }

    /*
    Thread responsible for receiving a clients command while the server is running. 
    One thread is created for each client.
    */
    private void HandleClientComm(object clientObj)
    {
        TcpClient client = (TcpClient)clientObj;
        NetworkStream stream = client.GetStream();

        Debug.Log("Client connected!");

        while (true)
        {
            byte[] buffer = new byte[4096];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead == 0)
                break;

            string jsonString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.Log($"raw jsonstring; {jsonString}");
            jsonString = jsonString.Split(new[] { "$"}, StringSplitOptions.RemoveEmptyEntries)[0];
            Debug.Log($"corrected jsonString: {jsonString}");

            // TODO # Figure out why this is failing randomly. Is this packets getting messed up?
            try
            {
                JObject message = JsonConvert.DeserializeObject<JObject>(jsonString);
                if (message.ContainsKey("event") && _eventHandlers.ContainsKey(message["event"].ToString()))
                {
                    lock(_actionsQueue){
                        _actionsQueue.Enqueue(() => _eventHandlers[message["event"].ToString()](message));
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.Log($"exception: {ex}");
            }
        }
        lock (_connectedClients)
        {
            _connectedClients.Remove(client);
        }
        client.Close();
    }

    public void ProcessActions()
    {
        lock (_actionsQueue)
        {
            while (_actionsQueue.Count > 0)
            {
                Action action = _actionsQueue.Dequeue();
                action();
            }
        }
    }

    /* 
    Adds an <event, handler> pair to the eventHandlers dictionary so that 
    when an event is received from the client, the appropriate code is executed.
    */
    public void On(string eventName, Action<JObject> handler)
    {
        _eventHandlers[eventName] = handler;
    }

    /* 
    A method that sets the servers _gameState to whatever object was pased 
    into the method. This _gameState is whats sent to the client evrey tick.
    */
    public void SetState(JObject state)
    {
        _gameState = state.ToString();
    }

    /* 
    Thread responsible for broadcasting _gameState to all 
    clients that are connected to the server.
    */
    private async void SendGameStateUpdates(object state)
    {
        CancellationToken cancellationToken = (CancellationToken)state;
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_updateInterval, cancellationToken);
            JObject message = new JObject
            {
                ["event"] = "gameStateUpdate",
                ["data"] = _gameState
            };
            byte[] buffer = Encoding.UTF8.GetBytes(message.ToString());
            lock (_connectedClients)
            {
                foreach (TcpClient client in _connectedClients)
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }

    /*
    Thread responsible for broadcasting the servers port to any clients that may be
    listening so they know the server is running at a specified port.
    */
    private async void BroadcastPresence(object state)
    {
        var tuple = (Tuple<IPEndPoint, CancellationToken>)state;
        IPEndPoint serverEndpoint = tuple.Item1;
        CancellationToken cancellationToken = tuple.Item2;

        using (UdpClient udpClient = new UdpClient())
        {
            udpClient.EnableBroadcast = true;
            IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, _broadcastPort);
            while (!cancellationToken.IsCancellationRequested)
            {
                byte[] data = Encoding.UTF8.GetBytes(serverEndpoint.ToString());
                await udpClient.SendAsync(data, data.Length, broadcastEndpoint);
                await Task.Delay(5000);
            }
        }
    }

    /* 
    Called to stop server running.
    */
    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();
    }
}
