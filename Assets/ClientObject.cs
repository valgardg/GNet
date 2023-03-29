using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;

public class ClientObject : MonoBehaviour
{
    private Client client;
    List<int> openPorts = new List<int>();

    void Start()
    {
        client = new Client();
        int[] portsToCheck = {8887, 8888, 8889};
        client.DiscoverServers(OnServerDiscovered, portsToCheck);
        //client.Connect("192.168.1.3", 8888); // Replace with the server computer's local IP address
    }
 
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && client.IsConnected())
        {
            JObject message = new JObject
            {
                ["event"] = "message",
                ["data"] = "Hello from the client!"
            };
            client.Send(message);
        }

        if(Input.GetKeyDown(KeyCode.C) && !client.IsConnected()){
            client.Connect("192.168.1.3", 8888);
        }

        //Debug.Log($"game state from server: {client.Receive()}");
    }

    private void OnServerDiscovered(IPEndPoint serverEndpoint)
    {
        int openPort = serverEndpoint.Port;
        if(!openPorts.Contains(openPort)){
            Debug.Log($"Port is open for connection: {openPort}");
            openPorts.Add(openPort);
            Debug.Log(string.Join(", ", openPorts));
        }
    }

    void OnApplicationQuit()
    {
        client.Stop();
    }
}
