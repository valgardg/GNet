using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

public class ClientObject2 : MonoBehaviour
{
    [SerializeField]
    private GameObject playerSquarePrefab;

    public string ipAddress = "192.168.1.3";
    public int portno = 8888;
    public int tickrate = 200;
    public string clientColor = "white";

    public string clientId = "1234";
    public float clientSpeed = 1f;

    private Dictionary<string, GameObject> _playerSquares = new Dictionary<string, GameObject>();

    private Client client;
    private GameState _gameState = new GameState();


    void Start()
    {
        client = new Client(tickrate);
        //client.Connect("192.168.1.3", 8888); // Replace with the server computer's local IP address
    }
 
    void Update()
    {
        GetUserInput();

        if(client.IsConnected()){
            // ISSUE # CLIENT MESSAGE STARTS OFF NULL FOR A LITTLE BIT EVEN AFTER CLIENT IS CONNECTED
            var serverMessage = client.Receive();
            if(serverMessage != null){
                var data = serverMessage["data"].ToString();
                _gameState = JsonConvert.DeserializeObject<GameState>(data);
                ApplyGameState(_gameState);
            }
        }

    }

    public void ConnectToServer(string setClientId)
    {
        clientId = setClientId;
        client.Connect(ipAddress, portno);
    }

    private void GetUserInput()
    {
        #region Movement
        if (Input.GetKeyDown(KeyCode.Space) && client.IsConnected())
        {
            var spawnPositionList = new List<float> {0f, 0f};
            JObject message = new JObject
            {   
                ["event"] = "spawn",
                ["id"] = clientId,
                ["position"] = JToken.FromObject(spawnPositionList),
                ["color"] = clientColor
            };
            client.Send(message);
        }

        if(Input.GetKey(KeyCode.W) && client.IsConnected())
        { 
            var movementVector = new List<float> {0f, clientSpeed};
            JObject message = new JObject
            {
                ["event"] = "move",
                ["id"] = clientId,
                ["Vector"] = JToken.FromObject(movementVector),
            };
            client.Send(message);
        }

        if(Input.GetKey(KeyCode.A) && client.IsConnected())
        { 
            var movementVector = new List<float> {-clientSpeed, 0f};
            JObject message = new JObject
            {
                ["event"] = "move",
                ["id"] = clientId,
                ["Vector"] = JToken.FromObject(movementVector),
            };
            client.Send(message);
        }

        if(Input.GetKey(KeyCode.S) && client.IsConnected())
        { 
            var movementVector = new List<float> {0f, -clientSpeed};
            JObject message = new JObject
            {
                ["event"] = "move",
                ["id"] = clientId,
                ["Vector"] = JToken.FromObject(movementVector),
            };
            client.Send(message);
        }

        if(Input.GetKey(KeyCode.D) && client.IsConnected())
        { 
            var movementVector = new List<float> {clientSpeed, 0f};
            JObject message = new JObject
            {
                ["event"] = "move",
                ["id"] = clientId,
                ["Vector"] = JToken.FromObject(movementVector),
            };
            client.Send(message);
        }
        #endregion
    }

    private GameObject InstantiatePlayerSquare(List<float> position, string color)
    {
        Vector2 positionVec = new Vector2(position[0], position[1]);
        GameObject square = Instantiate(playerSquarePrefab, positionVec, Quaternion.identity);
        // very basic color stuff so i dont have to implement a color dict and whatnot
        if(color == "blue"){
            square.GetComponent<SpriteRenderer>().color = Color.blue;
        }else{
            square.GetComponent<SpriteRenderer>().color = Color.red;
        }
        return square;
    }

    private void ApplyGameState(GameState gameState)
    {
        foreach (var player in gameState.Players.ToList())
        {
            string playerId = player.Key;
            List<float> playerPosition = player.Value.Position;
            string color = player.Value.Color;

            if(!_playerSquares.ContainsKey(playerId))
            {
                GameObject newSquare = InstantiatePlayerSquare(playerPosition, color);
                _playerSquares.Add(playerId, newSquare);
            }
            else 
            {
                _playerSquares[playerId].transform.position = new Vector2(playerPosition[0], playerPosition[1]);
            }
        }
    }

    void OnApplicationQuit()
    {
        client.Stop();
    }

    public class PlayerInfo{
        public string Id { get; set; }
        public List<float> Position { get; set; }
        public string Color { get; set; }
    }

    public class GameState
    {
        public Dictionary<string, PlayerInfo> Players { get; set;} = new Dictionary<string, PlayerInfo>();
    }
}