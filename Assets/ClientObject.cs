using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class ClientObject : MonoBehaviour
{
    private Client client;

    void Start()
    {
        client = new Client();
        client.DiscoverServers(OnServerDiscovered);
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
        Debug.Log("Discovered server: " + serverEndpoint);
        Debug.Log("Port is open for connection: " + serverEndpoint.Port);
    }

    void OnApplicationQuit()
    {
        client.Stop();
    }
}
