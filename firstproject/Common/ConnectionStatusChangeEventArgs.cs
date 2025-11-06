using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace firstproject.Common
{
    public class ConnectionStatusChangeEventArgs
    {
    public ConnectionStatusChangeEventArgs(bool isOnline) {  IsOnline = isOnline; }
    public bool IsOnline { get; set; }
    }
}