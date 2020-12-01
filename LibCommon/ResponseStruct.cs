using System;

namespace LibCommon
{
    /// <summary>
    /// 返回状态结构
    /// </summary>
    [Serializable]
    public class ResponseStruct
    {
        private ErrorNumber _code;
        private string _message = null!;
        private string? _exceptMessage = null!;
        private string? _exceptStackTrace = null!;
       

        /// <summary>
        /// 返回结构体构造
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public ResponseStruct(ErrorNumber code, string message)
        {
            Code = code;
            Message = message;
          
        }

     

        public ResponseStruct()
        {
        
        }

        /// <summary>
        /// 状态代码
        /// </summary>
        public ErrorNumber Code
        {
            get => _code;
            set => _code = value;
        }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string Message
        {
            get => _message;
            set => _message = value;
        }

        /// <summary>
        /// 异常的Message
        /// </summary>
        public string? ExceptMessage
        {
            get => _exceptMessage;
            set => _exceptMessage = value;
        }

        /// <summary>
        /// 异常的StackTrace
        /// </summary>
        public string? ExceptStackTrace
        {
            get => _exceptStackTrace;
            set => _exceptStackTrace = value;
        }

      
    }
}