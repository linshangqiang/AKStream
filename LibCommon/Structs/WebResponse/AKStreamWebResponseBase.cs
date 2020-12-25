using System;
using LibCommon;

namespace LibCommon.Structs.WebResponse
{
    /// <summary>
    /// web回复结构基类
    /// </summary>
    [Serializable]
    public class AKStreamWebResponseBase
    {
        private ResponseStruct _rs;

        public ResponseStruct Rs
        {
            get => _rs;
            set => _rs = value;
        }
    }
}