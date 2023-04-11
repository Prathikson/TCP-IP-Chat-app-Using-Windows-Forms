using Newtonsoft.Json;
using PacketProject280;
namespace TCPServerProject
{
    public partial class ServerForm : Form
    {
        string file = "";
        public ServerForm()
        {
            InitializeComponent();
            
        }



        private void S_ServerMessageEvent(Packet280 msg)
        {
            if(msg.ContentType == MessageType.Connected || msg.ContentType == MessageType.Broadcast)
            {
                this.Invoke(() => this.lstMyMessages.Items.Add(msg.Payload));
            }
            else if (msg.ContentType == MessageType.FileName)
            {
                file = msg.Payload;
            }
            else if (msg.ContentType == MessageType.File)
            {
                var fto = JsonConvert.DeserializeObject<FileTransferObject>(msg.Payload);
                //Write it out to disk
                File.WriteAllBytes(file+".bin", fto.FileBytes);
            }
            else if (msg.ContentType == MessageType.Disconnected)
            {
                this.Invoke(() => this.lstMyMessages.Items.Add(msg.Payload));
            }
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Server s = new Server(int.Parse(comboBox1.Text));
                s.ServerMessageEvent += S_ServerMessageEvent;
                s.Start();
            }
            catch
            {
                MessageBox.Show("Select a Port to connect");
            }
            
        }
    }
}