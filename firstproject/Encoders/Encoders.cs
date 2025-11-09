using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using firstproject.Common;
namespace firstproject.Encoders
{
    public class Encoders : EncoderInterface
    {
        public string Address { get; set; }
        public string Name { get; set; }

        public string Model { get; set; }
        
        public string AviSplTag { get; set; }

        public int Port { get; set; }

        public uint IPID { get; set; }




        public event Action<StreamChangeStatus> OnStreamChange;
    }
}