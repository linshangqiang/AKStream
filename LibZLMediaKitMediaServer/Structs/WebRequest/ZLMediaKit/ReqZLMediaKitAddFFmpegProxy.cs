using System;

namespace LibZLMediaKitMediaServer.Structs.WebRequest.ZLMediaKit
{
    [Serializable]
    public class ReqZLMediaKitAddFFmpegProxy : ReqZLMediaKitRequestBase
    {
        private string _src_url;
        private string _dst_url;
        private int _timeout_ms;
        private bool _enable_hls;
        private bool _enable_mp4;

        /// <summary>
        /// 源地址
        /// </summary>
        public string SrcUrl
        {
            get => _src_url;
            set => _src_url = value;
        }

        /// <summary>
        /// 推到目标地址，本地推送使用127.0.0.1为ip地址
        /// 如：rtmp://127.0.0.1/live/stream_form_ffmpeg
        /// </summary>
        public string DstUrl
        {
            get => _dst_url;
            set => _dst_url = value;
        }

        /// <summary>
        /// 拉流超时毫秒
        /// </summary>
        public int TimeoutMs
        {
            get => _timeout_ms;
            set => _timeout_ms = value;
        }

        /// <summary>
        /// 是否开起hls录制
        /// </summary>
        public bool EnableHls
        {
            get => _enable_hls;
            set => _enable_hls = value;
        }

        /// <summary>
        /// 是否开起mp4录制
        /// </summary>
        public bool EnableMp4
        {
            get => _enable_mp4;
            set => _enable_mp4 = value;
        }
    }
}