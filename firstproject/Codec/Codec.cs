using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace firstproject.Codec
{
    public class Codec 
    {
        public Codec (CodecConfig config)
        {
            this.address = config.address;
            this.isSerial = config.isSerial;
            this.name = config.name;
            this.make = config.make;
            this.model = config.model;
        }
    }
}