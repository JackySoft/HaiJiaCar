using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DFangFesionSoft
{
    [DataContract]
    public class CarInfo
    {
        [DataMember(Order = 0)]
        public string YYRQ { get; set; }
        [DataMember(Order = 1)]
        public string XNSD { get; set; }
        [DataMember(Order = 2)]
        public string CNBH { get; set; }

        public CarInfo(string YYRQ, string XNSD, string CNBH)
        {
            this.YYRQ = YYRQ;
            this.XNSD = XNSD;
            this.CNBH = CNBH;
        }
    }
}
