using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;
using PacketProject280;

namespace TCPClientProject
{
    public class Client
    {
        public TcpClient client;
        public delegate void ReceivePacketMessage(Packet280 msg);
        public event ReceivePacketMessage? ReceiveMessageEvent;
        public string User;

        public Client(string host, int port)
        {
            this.client = new TcpClient(host, port);

            Task.Run(() => Receive());
        }

        public  string DefaultUser()
        {
            string name = client.Client.RemoteEndPoint.ToString();
            return name;
        }

        public async Task SendMesssage(Packet280 msg)
        {
            if (client.Connected)
            {
                NetworkStream stream = client.GetStream();

                var tmp = JsonConvert.SerializeObject(msg);
                byte[] buffer = Encoding.UTF8.GetBytes(tmp);

                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                MessageBox.Show("Connect to the server and try again");
            }
           
        }

        

        public async Task Disconnect()
        {
            this.client.Close();
        }


        //receive

        public async Task<string> Receive()
        {
            NetworkStream stream = this.client.GetStream();
            while (true)
            {
                byte[] buffer = new byte[1024];
                   int bytesread = await stream.ReadAsync(buffer, 0, buffer.Length);
                //decerialize our data into a packet
                var stringMsg = Encoding.UTF8.GetString(buffer);
                //kh
                var packet = JsonConvert.DeserializeObject<Packet280>(stringMsg);
                if(ReceiveMessageEvent != null && packet != null)
                {
                    ReceiveMessageEvent(packet);
                }
                

            }
        }
        
    }

}
