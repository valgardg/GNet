using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;

using System.Threading;

using UnityEngine;

public class Server {
    private static Server instance;
    private TcpListener tcpListener;
    private List<ClientHandler> clients = new List<ClientHandler>();
    private Thread serverThread;
    
    public int clientsConnected;

    // cmd = func dictionary
    Dictionary<string, Action<Dictionary<string,string>>> messageHandlers = new Dictionary<string, Action<Dictionary<string,string>>>();

    private Server() {
    }

    public static Server Instance{
        get {
            if (instance == null){
                instance = new Server();
            }
            return instance;
        }
    }

    public void Start(){
        clientsConnected = 0;
        serverThread = new Thread(new ThreadStart(ServerThread));
        serverThread.Start();

    }

    public void Stop() {
        tcpListener.Stop();
    }

    public void On(string command, Action<Dictionary<string,string>> action){
        messageHandlers[command] = action;
    }

    private void ServerThread(){
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        tcpListener = new TcpListener(ipAddress, 42069);
        tcpListener.Start();
        Debug.Log("Server started");
        
        while(true){
            TcpClient client = tcpListener.AcceptTcpClient();
            Debug.Log("Client connected.");

            ClientHandler clientHandler = new ClientHandler(client, instance);
            Thread clientThread = new Thread(new ThreadStart(clientHandler.Run));
            clientThread.Start();

            clients.Add(clientHandler);

        }
    }

    private void OnClientDisconnected(ClientHandler clientHandler) {
        clients.Remove(clientHandler);
    }

    private class ClientHandler {
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        private bool isRunning = false;
        private Server serverInstance;

        public ClientHandler(TcpClient client, Server instance) {
            this.client = client;
            this.serverInstance = instance;
        }

        public void Run() {
            stream = client.GetStream();
            isRunning = true;

            receiveThread = new Thread(new ThreadStart(Receive));
            receiveThread.Start();
        }

        private void Receive() {
            byte[] buffer = new byte[1024];

            while (isRunning) {
                try {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0) {
                        string message = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Debug.Log("Received message from client: " + message);

                        // convert json encoded message into dictoinary to be used
                        //Dictionary<string, string> msgDict = JsonSerializer.Deserialize<Dictionary<string, string>>(message);
                        //string command = msgDict["action"];
                        // first 
                        if(serverInstance.messageHandlers.ContainsKey(message)){
                            Action<Dictionary<string,string>> handler = serverInstance.messageHandlers[message];
                            handler(null);
                        }

                        // Echo the message back to the client
                        // byte[] response = System.Text.Encoding.ASCII.GetBytes(message);
                        // stream.Write(response, 0, response.Length);
                    }
                } catch (Exception ex) {
                    Debug.Log("Error receiving message from client: " + ex.Message);
                    isRunning = false;
                }
            }

            // Client disconnected
            Debug.Log("Client disconnected.");
            stream.Close();
            client.Close();
            OnDisconnected();
        }

        private void OnDisconnected() {
            isRunning = false;
            Server.Instance.OnClientDisconnected(this);
        }
    }
}