using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class ServerObject : MonoBehaviour
{
    private Server server;
    private GameState _gameState = new GameState();

    void Start()
    {
        server = new Server(100);
        server.Start("192.168.1.3", 8888); // Replace with the server computer's local IP address
        // server.On("message", HandleMessage);
        server.On("spawn", OnPlayerSpawn);
    }

    void Update()
    {
        // Update the game state periodically (assuming you have a method to get the game state as a JObject)
        JObject gameState = GetGameState();
        server.SetState(gameState);
    }

    // private void HandleMessage(JObject message)
    // {
    //     Debug.Log("Received message: " + message["data"]);
    // }

    private void OnPlayerSpawn(JObject message){
        string playerId = message["id"].ToString();
        List<float> startPosition = message["position"].ToObject<List<float>>();
        PlayerInfo newPlayer = new PlayerInfo();
        newPlayer.Id = playerId;
        newPlayer.Position = startPosition;
        _gameState.Players[playerId] = newPlayer;
    }

    private void OnPlayerMove(JObject message)
    {
        //string playerId = message["id"].ToString();
        //Vector2 newPosition = message["position"].ToObject<Vector2>();
        //if(_gameState.Players.ContainsKey(playerId));
    }

    private JObject GetGameState()
    {
        // Replace this with your actual method of getting the game state as a JObject
        JObject gameState = JObject.FromObject(_gameState);
        return gameState;
    }

    void OnApplicationQuit()
    {
        server.Stop();
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