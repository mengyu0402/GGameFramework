using System;
using System.Collections.Generic;
using System.Text;

namespace GSockets
{
    public class GRPCAttribute : Attribute
    {
        /// <summary>
        /// rpc int key
        /// </summary>
        public int rpcId = int.MinValue;

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
        public GRPCAttribute(string key, int id)
        {
            rpcKey = key;
            rpcId = id;
        }
    }
}
