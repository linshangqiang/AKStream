using System;

namespace LibMediaServer.Structs.WebResponse
{
    [Serializable]
    public class ResZLMediaKitOpenRtpPort : ResZLMediaKitResponseBase
    {
        private ushort? _port;


        public ushort? Port
        {
            get => _port;
            set => _port = value;
        }
    }
}