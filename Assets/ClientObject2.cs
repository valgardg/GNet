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

    private Dictionary<string, GameObject> _playerSquares = new Dictionary<string, GameObject>();

    private Client client;
    private GameState _gameState = new GameState();


    void Start()
    {
        client = new Client();
        //client.Connect("192.168.1.3", 8888); // Replace with the server computer's local IP address
    }
 
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && client.IsConnected())
        {
            Vector2 spawnPosition = new Vector2(0,0);
            var spawnPositionList = new List<float> {spawnPosition.x, spawnPosition.y};
            JObject message = new JObject
            {   
                ["event"] = "spawn",
                ["id"] = "1",
                ["position"] = JToken.FromObject(spawnPositionList)
            };
            client.Send(message);
        }

        if(Input.GetKeyDown(KeyCode.L) && !client.IsConnected()){
            client.Connect("192.168.1.3", 8888);
        }

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

    private GameObject InstantiatePlayerSquare(List<float> position)
    {
        Vector2 positionVec = new Vector2(position[0], position[1]);
        GameObject square = Instantiate(playerSquarePrefab, positionVec, Quaternion.identity);
        return square;
    }

    private void ApplyGameState(GameState gameState)
    {
        foreach (var player in gameState.Players)
        {
            string playerId = player.Key;
            List<float> playerPosition = player.Value.Position;

            if(!_playerSquares.ContainsKey(playerId))
            {
                GameObject newSquare = InstantiatePlayerSquare(playerPosition);
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
    }

    public class GameState
    {
        public Dictionary<string, PlayerInfo> Players { get; set;} = new Dictionary<string, PlayerInfo>();
    }
}