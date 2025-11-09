using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace firstproject.Config
{
    public class SystemConfig
    {
        public List<DisplayConfig> Displays { get; set; }
        public List<EncoderConfig> Encoders { get; set; }
        public List<DecoderConfig> Decoders { get; set; }
    }
}