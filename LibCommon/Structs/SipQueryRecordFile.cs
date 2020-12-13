using System;
using LibCommon.Enums;

namespace LibCommon.Structs
{
    /// <summary>
    /// 查询sip录像的结构
    /// </summary>
    [Serializable]
    public class SipQueryRecordFile
    {
        private SipRecordFileQueryType _sipRecordFileQueryType;
        private string startTime;
        private string endTime;

        /// <summary>
        /// 查询录像类型，一般为all
        /// </summary>
        public SipRecordFileQueryType SipRecordFileQueryType
        {
            get => _sipRecordFileQueryType;
            set => _sipRecordFileQueryType = value;
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime
        {
            get => startTime;
            set => startTime = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime
        {
            get => endTime;
            set => endTime = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}