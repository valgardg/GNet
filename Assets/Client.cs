using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Threading;

using Newtonsoft.Json.Linq;

using UnityEngine;

public class Client {
    private TcpClient tcpClient;
    private StreamReader reader;
    private StreamWriter writer;

    private string latestMsg;
    private bool running;
    private Thread receiveThread;

    public void Connect(string ipAddress, int port) {
        tcpClient = new TcpClient(ipAddress, port);
        NetworkStream stream = tcpClient.GetStream();
        reader = new StreamReader(stream);
        writer = new StreamWriter(stream);
        writer.AutoFlush = true;
        running = true;
        receiveThread = new Thread(ReceieveThread);
        receiveThread.Start();
    }

    public void Disconnect() {
        tcpClient.Close();
    }

    public void Send(string command, JObject data) {
        string message = "";
        if(data.GetType() == typeof(string)){
            message = command + "$" + data;
        }else{
            string jsonData = data.ToString();
            message = command + "$" + jsonData;
        }
        writer.WriteLine(message);
    }

    private void ReceieveThread(){
        while(running){
            try{
                string message = reader.ReadLine();
                if(message != null){
                    // Debug.Log("received message from server...");
                    latestMsg = message;
                }
            } catch(Exception ex) {
                Debug.Log("exception occured while listening for server messages");
                Debug.Log($"Exception: {ex}");
                running = false;
            }
        }
    }

    public string Receive(){
        return latestMsg;
    }
}
