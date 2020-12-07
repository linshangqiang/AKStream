using System;
using System.Collections.Generic;

namespace  LibMediaServer.Structs.WebResponse
{
    [Serializable]
    public class ResZLMediaKitCloseStreams : ResZLMediaKitResponseBase
    {
        private int? _count_hit;
        private int? _count_closed;
        private List<MediaDataItem> _mediaInfo = new List<MediaDataItem>();

        public int? Count_Hit
        {
            get => _count_hit;
            set => _count_hit = value;
        }

        public int? Count_Closed
        {
            get => _count_closed;
            set => _count_closed = value;
        }

        public List<MediaDataItem> MediaInfo
        {
            get => _mediaInfo;
            set => _mediaInfo = value;
        }
    }
}