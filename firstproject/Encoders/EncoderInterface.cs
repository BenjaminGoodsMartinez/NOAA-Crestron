using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using firstproject.Common;
namespace firstproject.Encoders
{
    public interface EncoderInterface
    {
         string Address { get; set; }

         string Name { get; set; }

         string Model { get; set; }

         string AviSplTag { get; set; } 

         int Port { get; set; }

         uint IPID { get; set; }



        event Action<StreamChangeStatus> OnStreamChange;
    }
}