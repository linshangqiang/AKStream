using System;

namespace LibMediaServer.Structs.WebResponse
{
    [Serializable]
    public class ResZLMediaKitResponseBase
    {
        private int _code;

        public int Code
        {
            get => _code;
            set => _code = value;
        }
    }
}