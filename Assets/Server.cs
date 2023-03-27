using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;

public class Server {
    private static Server instance;
    private TcpListener tcpListener;
    private List<ClientHandler> clients = new List<ClientHandler>();
    private Thread serverThread;
    private int portno;
    
    public int clientsConnected;

    private string msgToSendToClients = "payload";

    // cmd = func dictionary
    Dictionary<string, Action<JObject>> messageHandlers = new Dictionary<string, Action<JObject>>();

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

    public void Start(int portNumber){
        portno = portNumber;
        clientsConnected = 0;
        serverThread = new Thread(new ThreadStart(ServerThread));
        serverThread.Start();

    }

    public void Stop() {
        tcpListener.Stop();
    }

    public void On(string command, Action<JObject> action){
        messageHandlers[command] = action;
    }

    public void Emit(string msg){
        msgToSendToClients = msg;
    }

    private void ServerThread(){
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        tcpListener = new TcpListener(ipAddress, portno);
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
        private StreamWriter writer;
        private Thread receiveThread;
        private bool isRunning = false;
        private Server serverInstance;

        public ClientHandler(TcpClient client, Server instance) {
            this.client = client;
            this.serverInstance = instance;
        }

        public void Run() {
            stream = client.GetStream();
            writer = new StreamWriter(stream);
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

                        string[] tokens = message.Split("$");
                        string command = tokens[0];
                        Debug.Log($"tokens[1]: {tokens[1]}");
                        JObject data = JObject.Parse(tokens[1]);
                        Debug.Log($"data: {data}");

                        if(serverInstance.messageHandlers.ContainsKey(command)){
                            Action<JObject> handler = serverInstance.messageHandlers[command];
                            handler(data);
                        }

                        // Echo the message back to the client
                        writer.WriteLine(serverInstance.msgToSendToClients);
                        writer.Flush();
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