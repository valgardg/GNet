using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private Server server;
    private Client client1;
    private Client client2;

    // Start is called before the first frame update
    void Start()
    {
        server = Server.Instance;

        server.On("spawn", (Dictionary<string,string> msgDict) => {
            Debug.Log("client requested spawn");
        });

        server.Start();

        Debug.Log($"clients connected to server: {server.clientsConnected}");

        client1 = new Client();
        client1.Connect("127.0.0.1", 42069);

        client2 = new Client();
        client2.Connect("127.0.0.1", 42069);

        Dictionary<string,string>  testdict = new ();
        testdict["test1"] = "1test";
        testdict["test2"] = "2test";

        string json = JsonUtility.ToJson(testdict);
        Debug.Log(json);
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("testscript is running ig...");
        if (Input.GetKeyDown(KeyCode.Space)) {
            client1.Send("spawn");
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            client2.Send("spawn");
        }

        // string message = client.Receive();
        // if (message != null) {
        //     Debug.Log("Received message: " + message);
        // }
    }

    void OnApplicationQuit() {
        client1.Disconnect();
        client2.Disconnect();
        server.Stop();
    }
}
