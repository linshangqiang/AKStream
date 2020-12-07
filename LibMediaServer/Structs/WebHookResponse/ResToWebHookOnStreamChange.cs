using System;
using LibMediaServer.Structs.WebResponse;

namespace LibMediaServer.Structs.WebHookResponse
{
    [Serializable]
    public class ResToWebHookOnStreamChange : ResZLMediaKitResponseBase
    {
        private string? _msg;

        public string? Msg
        {
            get => _msg;
            set => _msg = value;
        }
    }
}