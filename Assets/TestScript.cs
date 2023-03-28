using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class TestScript : MonoBehaviour
{
    private Server server;
    private Client client;

    void Start()
    {
        server = new Server();
        server.Start("192.168.1.3", 8888); // Replace with the server computer's local IP address
        server.On("message", HandleMessage);
        client = new Client();
        client.Connect("192.168.1.3", 8888); // Replace with the server computer's local IP address
    }
 
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            JObject message = new JObject
            {
                ["event"] = "message",
                ["data"] = "Hello from the client!"
            };
            client.Send(message);
        }
    }

    private void HandleMessage(JObject message)
    {
        Debug.Log("Received message: " + message["data"]);
    }
}
