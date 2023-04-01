using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class ServerObject : MonoBehaviour
{
    public int tickrate = 200;

    private Server server;
    private GameState _gameState = new GameState();

    void Start()
    {
        server = new Server(tickrate);
        server.On("spawn", OnPlayerSpawn);
        server.On("move", OnPlayerMove);
    }

    void Update()
    {
        if(server.IsListening())
        {
            // Update the game state periodically (assuming you have a method to get the game state as a JObject)
            JObject gameState;
            gameState = GetGameState();
            server.SetState(gameState);
            server.ProcessActions();
        }
    }

    public void StartServer()
    {
        server.Start("192.168.1.3", 8888); // Replace with the server computer's local IP address
    }

    private void OnPlayerSpawn(JObject message){
        string playerId = message["id"].ToString();
        List<float> startPosition = message["position"].ToObject<List<float>>();
        string color = message["color"].ToObject<string>();
        PlayerInfo newPlayer = new PlayerInfo();
        newPlayer.Id = playerId;
        newPlayer.Position = startPosition;
        newPlayer.Color = color;
        _gameState.Players[playerId] = newPlayer;
    }

    private void OnPlayerMove(JObject message){
        string playerId = message["id"].ToString();
        List<float> movementVector = message["Vector"].ToObject<List<float>>();
        _gameState.Players[playerId].Position[0] += movementVector[0];
        _gameState.Players[playerId].Position[1] += movementVector[1];
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
        public string Color { get; set; }
    }

    public class GameState
    {
        public Dictionary<string, PlayerInfo> Players { get; set;} = new Dictionary<string, PlayerInfo>();
    }
}