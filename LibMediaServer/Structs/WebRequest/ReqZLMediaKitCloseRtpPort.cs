using System;

namespace LibMediaServer.Structs.WebRequest
{
    [Serializable]
    public class ReqZLMediaKitCloseRtpPort : ReqZLMediaKitRequestBase
    {
        private string? _stream_id;

        public string? Stream_Id
        {
            get => _stream_id;
            set => _stream_id = value;
        }
    }
}