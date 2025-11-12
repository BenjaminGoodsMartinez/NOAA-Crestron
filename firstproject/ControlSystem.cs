using System;
using System.Text;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharp.CrestronSockets;	    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;
using firstproject.Properties;
using firstproject;
using firstCrestronProject;
using firstproject.Config;
using firstproject.Display;
using Crestron.SimplSharpPro.DM.Streaming;
using firstproject.Audio;
using Crestron.SimplSharpPro.AudioDistribution;
using Crestron.SimplSharpPro.EthernetCommunication;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reflection;
using firstproject.Cameras;


namespace firstCrestronProject
{
    public class ControlSystem : CrestronControlSystem
    {
        private EthernetIntersystemCommunications myEISC;
        private Tsw1070 _touchpanel;
        private const uint touchpanelID = 0x03;


        private List<DmNvxE30> DecoderArray = new List<DmNvxE30>();
        private List<DmNvxE30> EncoderArray = new List<DmNvxE30>();
        private List<DmNax8Zsa> AmplifierArray = new List<DmNax8Zsa>();

        private List<Display> DisplayArray = new List<Display>();
        private List<Cameras> CameraArray = new List<Cameras>();
       
        
        private SystemConfig config = default;
        string configfilepath = "Config.json";



        public ControlSystem()
            : base()
        {

            try
            {


                Crestron.SimplSharpPro.CrestronThread.Thread.MaxNumberOfUserThreads = 20;

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(_ControllerEthernetEventHandler);


            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        public void InitializeDisplays()
        {
            //Displays
            foreach (var displayInfo in config.Displays)
            {

                var display = new Display(displayInfo);
                DisplayArray.Add(display);
                display.Connect();
            }

        }

        public void InitializeCameras ()
        {
            try
            {
                foreach(var cameraInfo in config.Cameras)
                {
                    var camera = new Cameras(cameraInfo);
                    if (camera.IsOnline())
                    {
                        CameraArray.Add(camera);
                    }
                    else
                    {
                        Console.WriteLine("Camera is not online");
                    }
                }               
            } catch (Exception e)
            {
                Console.WriteLine("Error initializing cameras: \n", e);
            }
        }


        public void InitializeEncoders()
        {
            try
            {
                foreach (var encoderInfo in config.Encoders)
                { 
                    if (encoderInfo.Model == "E30" && encoderInfo.AviSplTag.StartsWith("TX"))
                    {
                        var encoder_tx = new DmNvxE30(encoderInfo.IPID, this);

                        if (encoder_tx.IsOnline)
                        {
                            Console.WriteLine("Encoder ", encoderInfo.Name, " for ", encoderInfo.Description, "is online..");

                            encoder_tx.Description = encoderInfo.Description;

                            encoder_tx.Control.DeviceMode = eDeviceMode.Transmitter;

                            encoder_tx.Control.MulticastAddress.StringValue = encoderInfo.MulticastAddress;

                            encoder_tx.Control.EnableAutomaticInitiation();

                            encoder_tx.Control.EnableAutomaticInputRouting();
                        }
                        EncoderArray.Add(encoder_tx);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading config\n", e);
            }
        }

        public void InitializeDecoders()
        {
            
            try
            {
                foreach (var DecoderInfo in config.Decoders)
                {
                    if (DecoderInfo.Model == "E30" && DecoderInfo.AviSplTag.Substring(0, 2) == "RX")
                    {
                        var Decoder_rx = new DmNvxE30(DecoderInfo.IPID, this);

                        if (Decoder_rx.IsOnline && Decoder_rx.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
                        {
                            Console.WriteLine("Decoder ", DecoderInfo.Name, " for ", DecoderInfo.Description, "is online..");
                            Decoder_rx.Description = DecoderInfo.Description;

                            Decoder_rx.Control.EnableAutomaticInitiation();
                            Decoder_rx.Control.EnableAutomaticInputRouting();

                            Decoder_rx.Control.DeviceMode = eDeviceMode.Receiver;


                        }

                        DecoderArray.Add(Decoder_rx);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading config\n", e);
            }
        }

        



        public SystemConfig LoadJsonConfig(string path)
        {
            try
            {
                if (!Crestron.SimplSharp.CrestronIO.File.Exists(path))
                {
                    CrestronConsole.PrintLine("Config file not found");
                }
                else
                {
                    var json = Crestron.SimplSharp.CrestronIO.File.ReadToEnd(path, Encoding.Default);
                    config = JsonConvert.DeserializeObject<SystemConfig>(json);
                }



            }
            catch (Crestron.SimplSharp.CrestronIO.FileNotFoundException e)
            {
                Console.WriteLine("Error reading config file. Error {0}", e.Message);
            }

            return config;
        }

        
        public void InitializeUIActions (BasicTriList currentDevice,SigEventArgs args)
        {

            if (currentDevice == _touchpanel)
            {

                if (args.Sig.Type == eSigType.Bool && args.Sig.BoolValue == true)
                {
                    switch (args.Sig.Number)
                    {
                        //Display Power Control
                        case 1:
                            DisplayArray[0].PowerOn = true;
                            break;
                        case 2:
                            DisplayArray[0].PowerOn = false;
                            break;
                        case 3:
                            DisplayArray[1].PowerOn = true;
                            break;
                        case 4:
                            DisplayArray[1].PowerOn = false;
                            break;
                        case 5:
                            DisplayArray[2].PowerOn = true;
                            break;
                        case 6:
                            DisplayArray[2].PowerOn = false;
                            break;
                        case 7:
                            DisplayArray[3].PowerOn = true;
                            break;
                        case 8:
                            DisplayArray[3].PowerOn = false;
                            break;

                        //Audio Mute Function
                        case 9:
                            //Program audio
                            EncoderArray[0].Control.AudioMute();
                            break;
                        case 10:
                            EncoderArray[0].Control.AudioUnmute();
                            break;
                            //PC Audio
                        case 11:
                            EncoderArray[6].Control.AudioMute();
                            break;
                        case 12:
                            EncoderArray[6].Control.AudioUnmute();
                            break;
                        //Mute All Speakers
                        case 17:
                            for (uint Index = 0; Index < AmplifierArray[0].Zones.Count; Index++)
                            {
                                AmplifierArray[0].Zones[Index].MuteOn();
                            }
                            break;
                            
                        
                    }
                }
                else if (args.Sig.Type == eSigType.String)
                {

                    //Here is the switch/case logic for setting the multicast address of the encoder to the multicast address of the decoder
                    switch (args.Sig.Number)
                    {
                        case 30:
                            //4 Display Videowall
                            DecoderArray[0].Control.MulticastAddress.StringValue = args.Sig.StringValue;
                            DecoderArray[1].Control.MulticastAddress.StringValue = args.Sig.StringValue;
                            DecoderArray[2].Control.MulticastAddress.StringValue = args.Sig.StringValue;
                            DecoderArray[3].Control.MulticastAddress.StringValue = args.Sig.StringValue;
                            break;
                        case 31:
                            DecoderArray[4].Control.MulticastAddress.StringValue = args.Sig.StringValue;
                            break;
                        case 32:
                            DecoderArray[5].Control.MulticastAddress.StringValue = args.Sig.StringValue;
                            break;
                    }
                }
            } else if (args.Sig.Type == eSigType.UShort)
            {
                switch (args.Sig.Number)
                {
                    case 20:
                        EncoderArray[0].Control.AnalogAudioOutputVolume.UShortValue = args.Sig.UShortValue;
                        break;
                    case 21:
                        EncoderArray[6].Control.AnalogAudioOutputVolume.UShortValue = args.Sig.UShortValue;
                        break;
                }
            }
        }




        public void InitializeAmplifiers (SystemConfig config){
            

            foreach (var amp in config.Amplifiers)
            {
                if (amp.Model == "Nax8Zsa")
                {
                    DmNax8Zsa amplifier = new DmNax8Zsa(amp.IPID, this);
                    if(amplifier.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
                    {
                        AmplifierArray.Add(amplifier);
                    }
                }
            }
        }

        /// <summary>
        /// InitializeSystem - this method gets called after the constructor 
        /// has finished. 
        /// 
        /// Use InitializeSystem to:
        /// * Start threads
        /// * Configure ports, such as serial and verisports
        /// * Start and initialize socket connections
        /// Send initial device configurations
        /// 
        /// Please be aware that InitializeSystem needs to exit quickly also; 
        /// if it doesn't exit in time, the SIMPL#Pro program will exit.
        /// </summary>
        public void InitializeSystem()
        {
            Task.Run(() =>
            {
                try
                {

                    this.config = this.LoadJsonConfig(this.configfilepath);
                    this.InitializeEncoders();
                    this.InitializeCameras();
                    this.InitializeDecoders();
                    this.InitializeDisplays();
                    this.InitializeAmplifiers(config);

            //Check if registered and online
            _touchpanel = new Tsw1070(touchpanelID, this);

            if (_touchpanel.Register() == eDeviceRegistrationUnRegistrationResponse.Success && _touchpanel.IsOnline)
            {
                _touchpanel.SigChange += InitializeUIActions;
                _touchpanel.Description = "Mission Control Room Touch Panel";

                Console.WriteLine("Mission Control Room Touch Panel is registered and online!\nInitializing Touch Panel Functions...");
            }
            else
            {
                Console.WriteLine("There was an issue initializing touch panel");
            }

            //Begin to create button/function mappings

            if (_touchpanel.IsOnline)
            {
                try
                {

                }
                catch (Exception e)
                {
                    Console.WriteLine("Touch Panel is not registered and/or online");
                }
            }

                    CrestronConsole.PrintLine("NASA AV System Initializing...");
                 

                    myEISC = new EthernetIntersystemCommunications(0x12, "172.16.0.0", this);
                    if (myEISC.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                    {
                        CrestronConsole.PrintLine("Error message");
                    }
                    else
                    {
                        myEISC.OnlineStatusChange += MyEISCOnOnlineStatusChange;


                        myEISC.Register();
                    }

                }
                catch (Exception e)
                {
                    ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
                }
            });

        }



        private void MyEISCOnOnlineStatusChange(GenericBase currentdevice, OnlineOfflineEventArgs args)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Event Handler for Ethernet events: Link Up and Link Down. 
        /// Use these events to close / re-open sockets, etc. 
        /// </summary>
        /// <param name="ethernetEventArgs">This parameter holds the values 
        /// such as whether it's a Link Up or Link Down event. It will also indicate 
        /// wich Ethernet adapter this event belongs to.
        /// </param>
        void _ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }

        /// <summary>
        /// Event Handler for Programmatic events: Stop, Pause, Resume.
        /// Use this event to clean up when a program is stopping, pausing, and resuming.
        /// This event only applies to this SIMPL#Pro program, it doesn't receive events
        /// for other programs stopping
        /// </summary>
        /// <param name="programStatusEventType"></param>
        void _ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    //The program has been stopped.
                    //Close all threads. 
                    //Shutdown all Client/Servers in the system.
                    //General cleanup.
                    //Unsubscribe to all System Monitor events
                    break;
            }

        }

        /// <summary>
        /// Event Handler for system events, Disk Inserted/Ejected, and Reboot
        /// Use this event to clean up when someone types in reboot, or when your SD /USB
        /// removable media is ejected / re-inserted.
        /// </summary>
        /// <param name="systemEventType"></param>
        void _ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }

        }
    }
}