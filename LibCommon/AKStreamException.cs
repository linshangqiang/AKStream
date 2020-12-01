using System;

namespace LibCommon
{
    public class AKStreamException:Exception
    {
        public ResponseStruct ResponseStruct;

        public AKStreamException(ResponseStruct rs)
        {
            ResponseStruct = rs;
        }
    }
}