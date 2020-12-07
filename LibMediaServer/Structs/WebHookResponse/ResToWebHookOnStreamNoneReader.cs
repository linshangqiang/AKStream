using System;
using LibMediaServer.Structs.WebResponse;

namespace LibMediaServer.Structs.WebHookResponse
{
    [Serializable]
    public class ResToWebHookOnStreamNoneReader : ResZLMediaKitResponseBase
    {
        private bool? close;

        public bool? Close
        {
            get => close;
            set => close = value;
        }
    }
}