using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PacketProject280
{
    public class FileTransferObject
    {
        public byte[] FileBytes { get; set; }
        public string JsonSerialized()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
