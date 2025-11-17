using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using firstproject.Config;

namespace firstproject.Cameras
{
    public class Cameras : CamerasInterface
    {
        public string Address { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Port { get; set; }

        public uint IPID { get; set; }

        public TcpClient CameraTCPClient { get; set; }

        public NetworkStream MyStream { get; set; }


        public Cameras(CameraConfig config)
        {

            CameraTCPClient = new TcpClient(config.Address, config.Port);
            MyStream = this.CameraTCPClient.GetStream();
            this.Address = config?.Address;
            this.Make = config?.Make;
            this.Model = config?.Model;
            this.Name = config?.Name;
            this.Description = config?.Description;
            this.Port = config.Port;
            this.IPID = config.IPID;

        }

        public void SendCommand(string command)
        {

            Byte[] CommandToSend = System.Text.Encoding.ASCII.GetBytes(command);
            //SendCommand
            MyStream.Write(CommandToSend, 0, CommandToSend.Length);
            //Read TCP stream
            Int32 RxBytes = MyStream.Read(CommandToSend, 0, CommandToSend.Length);
            //Convert BYTES from TCP stream to string
            string ResponseData = System.Text.Encoding.ASCII.GetString(CommandToSend, 0, RxBytes);
            //Send response to Console for troubleshooting
            Console.WriteLine("Response from TCP Client:\n", ResponseData);
        }

        public bool IsOnline()
        {
            return this.CameraTCPClient.Connected;
        }

    }
    
    



    }