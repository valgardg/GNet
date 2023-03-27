using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class TestScript : MonoBehaviour
{
    private Server server;
    private Client client1;
    private Client client2;

    private JObject myObj;

    // Start is called before the first frame update
    void Start()
    {
        server = Server.Instance;

        server.On("spawn", (JObject data) => {
            Debug.Log($"vaue of id: {data["id"]}");
        });

        server.Start(42069);

        Debug.Log($"clients connected to server: {server.clientsConnected}");

        client1 = new Client();
        client1.Connect("127.0.0.1", 42069);

        client2 = new Client();
        client2.Connect("127.0.0.1", 42069);
        
        myObj = new JObject {
            {"id", "clientId"}
        };

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("testscript is running ig...");
        if (Input.GetKeyDown(KeyCode.Space)) {
            client1.Send("spawn", myObj);
            string message = client1.Receive();
            // Debug.Log("we are now checking if message is null or not");
            if (message != null) {
                Debug.Log("Received message: " + message);
            }
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            client2.Send("spawn", myObj);
            string message = client2.Receive();
            if (message != null) {
                Debug.Log("Received message: " + message);
            }
        }
    }

    void OnApplicationQuit() {
        client1.Disconnect();
        client2.Disconnect();
        server.Stop();
    }
}
