using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace firstproject.Config


{
    public class CodecConfig
    {
    public string Hardware { get; set; }
    public string SerialNumber { get; set; }
    public bool IsSerial { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    }
}