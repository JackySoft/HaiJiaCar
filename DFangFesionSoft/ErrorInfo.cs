using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DFangFesionSoft
{
    [DataContract]
    public class ErrorInfo
    {
        [DataMember(Order = 0)]
        public string data { get; set; }
        [DataMember(Order = 1)]
        public string code { get; set; }
        [DataMember(Order = 2)]
        public string message { get; set; }


        public ErrorInfo(string data, string code, string message)
        {
            this.data = data;
            this.code = code;
            this.message = message;
        }
    }
}
