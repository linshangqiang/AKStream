using System;
using LibCommon;

namespace AKStreamWeb.WebResponse
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