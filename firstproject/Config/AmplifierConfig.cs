using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace firstproject.Config
{
    public class AmplifierConfig
    {
        public string Address { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string AviSplTag { get; set; }

        public int Port { get; set; }

        public uint IPID { get; set; }

        public string Description { get; set; }
                public List<string> AudioDevices { get; set; }

    }
}