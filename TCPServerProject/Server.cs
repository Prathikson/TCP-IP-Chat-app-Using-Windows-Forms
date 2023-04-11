using Newtonsoft.Json;
using PacketProject280;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TCPServerProject
{
    public class Server
    {
        //a class to listen for connnections
        private TcpListener listener;
        //everyincoming connection will get a tcp client
        List<TcpClient> clients = new List<TcpClient>();
        private bool IsRunning;
        public delegate void ServerPacketMessage(Packet280 msg);
        public event ServerPacketMessage ServerMessageEvent;
        public string User;
        public static List<string> users = new List<string>();


        public Server(int port)
        {
            this.listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task Start()
        {
            this.listener.Start();
            this.IsRunning = true;

            while (IsRunning)
            {
                //this will fire when TCP client connects to our IP
                TcpClient client = await this.listener.AcceptTcpClientAsync();

                //Track the User
                clients.Add(client);
                //create a packet
                Packet280 p = new Packet280();
                p.ContentType = MessageType.Connected;
                p.Payload = $"Client connected from {client.Client.RemoteEndPoint}";
                ServerMessageEvent?.Invoke(p);
                Task.Run(() => this.HandleClient(client));
            }

            
        }
        private async void BroadcasttoAllClients(Packet280 message)
        {
            var msg = JsonConvert.SerializeObject(message);
            byte[] broadcast = Encoding.UTF8.GetBytes(msg);
            foreach (TcpClient client in clients)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    await stream.WriteAsync(broadcast, 0, broadcast.Length);
                }
                catch
                {

                }
            }

        }
        private async void SendToClient(TcpClient client,Packet280 message)
        {
            var msg = JsonConvert.SerializeObject(message);
            byte[] broadcast = Encoding.UTF8.GetBytes(msg);
            
                try
                {
                    NetworkStream stream = client.GetStream();
                    await stream.WriteAsync(broadcast, 0, broadcast.Length);
                }
                catch
                {

                }

        }
        private async void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[4096];
                int bytesread = 0;
                while ((bytesread = await stream.ReadAsync(buffer,0,buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer);
                    var packet = JsonConvert.DeserializeObject<Packet280>(message);
                    if(packet.ContentType == MessageType.Connected)
                    {
                        ServerMessageEvent?.Invoke(packet);
                        //broadcast the connected event to all connected Clients
                        this.BroadcasttoAllClients(packet);
                    }
                    else if (packet.ContentType == MessageType.List)
                    {
                        ServerMessageEvent?.Invoke(packet);
                        if (packet.Payload == "!list")
                        {
                            var path = Directory.GetCurrentDirectory();

                            string[] myfiles = Directory.GetFiles(path, "*.bin");
                            foreach (var file in myfiles)
                            {
                                Packet280 pl = new Packet280();
                                pl.ContentType = MessageType.List;
                                string Name = Path.GetFileName(file);
                                pl.Payload = Name;
                                SendToClient(client, pl);
                            }

                        }


                    }
                    else if (packet.ContentType == MessageType.SendFile)
                    {
                        ServerMessageEvent?.Invoke(packet);
                        if (packet.Payload.Contains("!Get"))
                        {
                            var path = Directory.GetCurrentDirectory();

                            string[] myfiles = Directory.GetFiles(path, "*.bin");
                            foreach (var file in myfiles)
                            {
                                string main = Path.GetFileName(file);
                                if (packet.Payload.Contains(main))
                                {
                                    byte[] bytes = File.ReadAllBytes(file);

                                    FileTransferObject o = new FileTransferObject();
                                    o.FileBytes = bytes;

                                    Packet280 pr = new Packet280();
                                    pr.ContentType = MessageType.SendFile;
                                    pr.Payload = o.JsonSerialized();
                                    Packet280 pNa = new Packet280();
                                    pNa.ContentType = MessageType.FileReq;
                                    pNa.Payload = main;
                                    
                                    SendToClient(client, pNa);
                                    SendToClient(client, pr);
                                    return;
                                }
                                else
                                {
                                    MessageBox.Show("File Not Found");
                                }
                            }

                        }


                    }
                    else if (packet.ContentType == MessageType.Broadcast)
                    {
                        ServerMessageEvent?.Invoke(packet);
                        //broadcast the connected event to all connected Clients
                        this.BroadcasttoAllClients(packet);
                    }
                    //for file type
                    else if(packet.ContentType == MessageType.File)
                    {
                        ServerMessageEvent?.Invoke(packet);
                    }
                    else if (packet.ContentType == MessageType.FileName)
                    {
                        ServerMessageEvent?.Invoke(packet);
                    }
                    else if (packet.ContentType == MessageType.Disconnected)
                    {
                        ServerMessageEvent?.Invoke(packet);

                    }
                   
                    else if (packet.ContentType == MessageType.Name)
                    {
                        ServerMessageEvent?.Invoke(packet);
                        bool check = CheckName(packet.Payload);
                        Packet280 pa = new Packet280();
                        if (check)
                        {
                            
                            pa.ContentType = MessageType.Name;
                            pa.Payload = "Confirmed";
                            SendToClient(client, pa);   
                        }
                        else
                        {
                            pa.ContentType = MessageType.Name;
                            pa.Payload = "Decline";
                            SendToClient(client, pa);
                        }

                    }
                }

                Packet280 p = new Packet280();
                p.ContentType = MessageType.Disconnected;
                p.Payload = $"Client disconnected from {client.Client.RemoteEndPoint}";
                client.Close();
                clients.Remove(client);
                ServerMessageEvent?.Invoke(p);
                this.BroadcasttoAllClients(p);  

            }
            catch(Exception ex)
            {
                try {
                    Packet280 p = new Packet280();
                    p.ContentType = MessageType.Disconnected;
                    p.Payload = $"Client disconnected from {client.Client.RemoteEndPoint} :: Please connect and try again";
                    client.Close();
                    clients.Remove(client);
                    ServerMessageEvent?.Invoke(p);
                    this.BroadcasttoAllClients(p);
                }
                catch
                {
                    
                }
                
            }
        }

        public bool CheckName(string name)
        {
            
            if(users.Count == 0)
            {
                users.Add(name);
                return true;
            }
            else
            {
                bool ret = false;
                for (int i = 0; i < users.Count; i++)
                {

                    if (users[i] == name)
                    {
                        ret = false;
                    }
                    else
                    {
                        users.Add(name);
                        User = name; 
                        ret = true;
                        break;
                    }
                    
                }
                return ret;
            }
            
            
        }


    }
}
