using System;
using System.Collections.Generic;
using System.Text;

namespace GSockets
{

    public class GMessageAttribute : Attribute
    {
        static uint MSG_INDEX = uint.MinValue;

        public uint msgId = uint.MinValue;

        public GMessageAttribute(uint msgId)
        {
            if (msgId == 0) throw new Exception("msg id not 0");
            this.msgId = msgId;
        }

        public GMessageAttribute()
        {
            ++MSG_INDEX;
            msgId = MSG_INDEX;
        }
    }

    public class GRPCMessageAttribute : Attribute
    {
    }

    public class GRPCAttribute : Attribute
    {
        /// <summary>
        /// rpc int key
        /// </summary>
        public uint rpcId = uint.MinValue;

        /// <summary>
        /// rpc string key
        /// </summary>
        public String rpcKey;

        /// <summary>
        /// construct rpc attribute
        /// </summary>
        /// <param name="key"></param>
        public GRPCAttribute(string key)
        {
            rpcKey = key;
        }

        /// <summary>
        /// construct rpc attribute
        /// </summary>
        /// <param name="key"></param>
        /// <param name="id"></param>
        public GRPCAttribute(string key, uint id)
        {
            rpcKey = key;
            rpcId = id;
        }
    }
}
