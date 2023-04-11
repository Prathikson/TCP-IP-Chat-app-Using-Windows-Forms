using PacketProject280;
using System.Net;
using System.IO;
using static System.Net.WebRequestMethods;
using Timer = System.Windows.Forms.Timer;
using Newtonsoft.Json;

namespace TCPClientProject
{
    public partial class ClientForm : Form
    {
        Client client;
        string PendingUser;
        string ReqName;
        
        Timer timer = new Timer();
        

        public ClientForm()
        {
            InitializeComponent();
            timer.Enabled = true;
            timer.Interval = 50;
            timer.Tick += Timer_Tick;
            comboBox1.Items.Add("Light");
            comboBox1.Items.Add("Dark");
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            
        }

        public async void btnConnect_Click(object sender, EventArgs e)
        {

            client = new Client("localhost", 25565);
            client.ReceiveMessageEvent += Client_ReceiveMessageEvent;
            PendingUser = client.DefaultUser();
            client.User = PendingUser;

        }
        public async void NameSet()
        {
            UserName u = new UserName();
            u.ShowDialog();
            Packet280 p = new Packet280();
            p.ContentType = MessageType.Name;
            p.Payload = u.sendName();
            PendingUser = u.sendName();
            await client.SendMesssage(p);
        }

        private void Client_ReceiveMessageEvent(Packet280 msg)
        {
            if (msg.ContentType == MessageType.Broadcast)
            {
                this.Invoke(() => lstMessages.Items.Add(msg.Payload));
            }
            else if (msg.ContentType == MessageType.Connected)
            {
                this.Invoke(() => lstMessages.Items.Add(msg.Payload));
            }
            else if (msg.ContentType == MessageType.Disconnected)
            {
                this.Invoke(() => lstMessages.Items.Add(msg.Payload));
            }
            else if (msg.ContentType == MessageType.Name)
            {
                if (msg.Payload == "Confirmed")
                {
                    client.User = PendingUser;
                }
                else
                {
                    MessageBox.Show("The Usename Already Exist Try Again");
                    NameSet();
                }
            }
            else if (msg.ContentType == MessageType.List)
            {
                this.Invoke(() => lstMessages.Items.Add(msg.Payload));
            }
            else if (msg.ContentType == MessageType.FileReq)
            {
                ReqName = msg.Payload;
            }
            else if (msg.ContentType == MessageType.SendFile)
            {
                this.Invoke(() => lstMessages.Items.Add($"File Sent to {PendingUser}"));
                string Create = $"E:\\TCP\\Users\\";
                if (!Directory.Exists(Create))
                {
                    Directory.CreateDirectory(Create);
                }
                var fto = JsonConvert.DeserializeObject<FileTransferObject>(msg.Payload);
                System.IO.File.WriteAllBytes(Create+"\\"+ReqName,fto.FileBytes);
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            Packet280 p = new Packet280();
            p.ContentType = MessageType.Broadcast;
            p.Payload =client.User + " : " + textBox1.Text;

            Packet280 pl = new Packet280();
            pl.ContentType = MessageType.List;
            pl.Payload = textBox1.Text;

            Packet280 pr = new Packet280();
            pr.ContentType = MessageType.SendFile;
            pr.Payload = textBox1.Text;

            if (textBox1.Text == "!list")
            {
                await client.SendMesssage(pl);
            }
            else if (textBox1.Text.Contains("!Get"))
            {
                await client.SendMesssage(pr);
            }
            else
            {
                await client.SendMesssage(p);
            }
            
            
        }

        private async void btnFile_Click(object sender, EventArgs e)
        {
            try {
                string path = "";
                string file = "";
                OpenFileDialog f = new OpenFileDialog();
                f.Filter = "Text files (*.txt)|*.txt";
                if (f.ShowDialog() == DialogResult.OK)
                {
                    path = f.FileName;
                    file = Path.GetFileNameWithoutExtension(path);
                }
                byte[] bytes = System.IO.File.ReadAllBytes(path);

                FileTransferObject o = new FileTransferObject();
                o.FileBytes = bytes;

                Packet280 p = new Packet280();
                p.ContentType = MessageType.File;
                p.Payload = o.JsonSerialized();

                Packet280 pN = new Packet280();
                pN.ContentType = MessageType.FileName;
                pN.Payload = file;

                //send mesage 
                await client.SendMesssage(pN);
                await client.SendMesssage(p);
            }
            catch { 
            }
            
           
        }

        private async void btnDisconnect_Click(object sender, EventArgs e)
        {
            Packet280 p = new Packet280();
            p.ContentType = MessageType.Disconnected;
            client.Disconnect();
        }

        private void btnSetUser_Click(object sender, EventArgs e)
        {
            NameSet();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox1.Text == "Light")
            {
                this.BackColor = Color.White;
                btnConnect.BackColor = Color.White;
                btnFile.BackColor = Color.White;
                btnSend.BackColor = Color.White;
                btnSetUser.BackColor = Color.White;
                btnDisconnect.BackColor = Color.White;
                lstMessages.BackColor = Color.FloralWhite;
            }
            else
            {
                this.BackColor = Color.Gray;
                btnConnect.BackColor = Color.GreenYellow;
                btnFile.BackColor = Color.GreenYellow;
                btnSend.BackColor = Color.GreenYellow;
                btnSetUser.BackColor = Color.GreenYellow;
                btnDisconnect.BackColor = Color.GreenYellow;
                lstMessages.BackColor = Color.Gray;
            }
        }
    }
}