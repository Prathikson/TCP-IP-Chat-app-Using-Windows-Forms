namespace PacketProject280
{
    public enum MessageType
    {
        Broadcast,
        Connected,
        Disconnected,
        File,
        FileName,
        List,
        FileReq,
        SendFile,
        ServerOnlyMessage,
        Name
    }
    public class Packet280
    {
        public MessageType ContentType { get; set; }

        public string Payload { get; set; }
    }
}