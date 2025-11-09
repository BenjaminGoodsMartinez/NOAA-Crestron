using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace firstproject.Common
{
    public class StreamChangeStatus
    {
        public StreamChangeStatus(bool isStream) { isStream = isStream; }
        
         bool isStatusChanged { get; set; }
    }
}