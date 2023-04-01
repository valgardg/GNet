using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class ServerDemo : MonoBehaviour
{
    private Server server1;
    private Server server2;

    void Start()
    {
        server1 = new Server();
        server1.Start("192.168.1.3", 8888); // Replace with the server computer's local IP address
        server1.On("message", HandleMessage);

        server2 = new Server();
        server2.On("message", HandleMessage);
    }

    void Update()
    {
        // Update the game state periodically (assuming you have a method to get the game state as a JObject)
        JObject gameState = GetGameState();
        server1.SetState(gameState);
        if(Input.GetKeyDown(KeyCode.S)){
            server2.Start("192.168.1.3", 8889); // Replace with the server computer's local IP address
        }
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
        server1.Stop();
        server2.Stop();
    }
}
