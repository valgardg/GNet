using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;

public class Client {
    private TcpClient tcpClient;
    private StreamReader reader;
    private StreamWriter writer;

    public void Connect(string ipAddress, int port) {
        tcpClient = new TcpClient(ipAddress, port);
        NetworkStream stream = tcpClient.GetStream();
        reader = new StreamReader(stream);
        writer = new StreamWriter(stream);
    }

    public void Disconnect() {
        tcpClient.Close();
    }

    public void Send(string message) {
        writer.WriteLine(message);
        writer.Flush();
    }

    public string Receive() {
        return reader.ReadLine();
    }
}
