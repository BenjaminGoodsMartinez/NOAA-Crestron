using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using firstproject.Config;

namespace firstproject.Audio
{
    public class Microphones
    {
        public string Name {get; set;}
        public string Model {get; set;}
        public string Address {get; set;}
        public int Port {get; set;}
        public string Make {get;set;}
        public string Description {get; set;}
        public string IPID {get;set;}

        public TcpClient MicrophoneTCPClient {get; set;}

        public NetworkStream MyStream {get; set;}
        public Microphones (MicrophoneConfig config)
        {
            MicrophoneTCPClient = new TcpClient(config.Address, config.Port);
            MyStream = this.MicrophoneTCPClient.GetStream();
            this.Address = config?.Address;
            this.Make = config?.Make;
            this.Model = config?.Model;
            this.Name = config?.Name;
            this.Description = config?.Description;
            this.Port = config.Port;
            this.IPID = config.IPID;
        }

                public void SendCommand (string Command){
                    Byte[] CommandToSend = System.Text.Encoding.ASCII.GetBytes(Command);
                    //Encoding command string
                    MyStream.Write(CommandToSend, 0, CommandToSend.Length);
                    //Read TCP Stream
                    Int32 RxBytes = MyStream.Read(CommandToSend, 0, CommandToSend.Length);
                    //Convert BYTES from TCP stream to string
                    string ResponseData = System.Text.Encoding.ASCII.GetString(CommandToSend);
                    //Send response to Console for troubleshooting
                    Console.WriteLine("Response from TCP Client:\n", ResponseData);
        }

        public bool IsOnline()
        {
            return this.MicrophoneTCPClient.Connected;
        }


        
    }



}