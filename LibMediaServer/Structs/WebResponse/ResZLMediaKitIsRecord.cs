using System;

namespace LibMediaServer.Structs.WebResponse
{
    [Serializable]
    public class ResZLMediaKitIsRecord : ResZLMediaKitResponseBase
    {
        private bool? _status;

        public bool? Status
        {
            get => _status;
            set => _status = value;
        }
    }
}