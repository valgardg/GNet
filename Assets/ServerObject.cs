using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class ServerObject : MonoBehaviour
{
    private Server server;

    void Start()
    {
        server = new Server();
        server.Start("192.168.1.3", 8888); // Replace with the server computer's local IP address
        server.On("message", HandleMessage);
    }

    void Update()
    {
        // Update the game state periodically (assuming you have a method to get the game state as a JObject)
        JObject gameState = GetGameState();
        server.SetState(gameState);
    }

    private void HandleMessage(JObject message)
    {
        Debug.Log("Received message: " + message["data"]);
    }

    private JObject GetGameState()
    {
        // Replace this with your actual method of getting the game state as a JObject
        JObject gameState = new JObject
        {
            ["exampleProperty"] = "exampleValue"
        };
        return gameState;
    }

    void OnApplicationQuit()
    {
        server.Stop();
    }
}
