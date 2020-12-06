using System;

namespace LibCommon
{
    public class AkStreamException:Exception
    {
        public ResponseStruct ResponseStruct;

        public AkStreamException(ResponseStruct rs)
        {
            ResponseStruct = rs;
        }
    }
}