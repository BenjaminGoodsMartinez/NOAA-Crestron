using System;
using System.Text;
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
using Crestron.SimplSharpPro.AudioDistribution;
using Crestron.SimplSharpPro.EthernetCommunication;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Diagnostics;


namespace firstCrestronProject
{
    public class ControlSystem : CrestronControlSystem
    {
        private EthernetIntersystemCommunications myEISC;
        private Tsw1070 _touchpanel;
        private UserInterface _userInterface;
        private const uint touchpanelID = 0x03;

        private bool isTouchPanelRegistered = default;
        private bool isTouchPanelOnline = default;

        private DmNax16ain MCR_Amplifier;
        string JSONDataAsAString = "";
        string configfilepath = "./Config.json";

        private List<Displays> displays = new List<Displays>();
        //Display Commands
        private string Bravia_DisplayOn = "8C 00 00 02 01 8F";
        private string Bravia_DisplayOff = "8C 00 00 02 01 8E";



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



        private void initializeUI ()
        {

            //Check if registered and online
            _touchpanel = new Tsw1070(touchpanelID, this);

            if (_touchpanel.Register() == eDeviceRegistrationUnRegistrationResponse.Success && _touchpanel.IsOnline)
            {
                isTouchPanelRegistered = true;
                isTouchPanelOnline = true;
                _touchpanel.Description = "Mission Control Room Touch Panel";

                Console.WriteLine("Mission Control Room Touch Panel is registered and online!\nInitializing Touch Panel Functions...");
            } else
            {
                Console.WriteLine("There was an issue initializing touch panel");
            }

            //Begin to create button/function mappings
            
            if (isTouchPanelOnline && isTouchPanelOnline)
            {
                try
                {
                    //Displays
                    for (int i = 0; i < this.displays.Count; i++)
                    {
                        var index = i;
                        var display = this.displays[index];
                        display.OnPowerStateChange += (e) => {
                            
                        } 
                        
                       
                    }
                

                } catch (Exception e)
                {
                    Console.WriteLine("Touch Panel is not registered and/or online");
                }
            }

                

        

                
                


                _touchpanel.SigChange += _userInterface.InterfaceSigChange;
        }


        public SystemConfig LoadJsonConfig(string args)
        {
            SystemConfig config = default;
            try
            {


                using (StreamReader reader = new StreamReader(configfilepath))
                {
                    config = JsonConvert.DeserializeObject<SystemConfig>(reader.ReadToEnd());
                }

            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Error reading config file. Error {0}", e.Message);
            }

            return config;
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
        public override void InitializeSystem()
        {


            try
            {
                CrestronConsole.PrintLine("NASA AV System Initializing...");
                // this.RelayPorts[1]
                //Change IP address
                



                myEISC = new EthernetIntersystemCommunications(0x12, "172.16.0.0", this);
                if (myEISC.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    CrestronConsole.PrintLine("Error message");
                }
                else
                {
                    myEISC.SigChange += myEISC_SigChange;
                    myEISC.OnlineStatusChange += MyEISCOnOnlineStatusChange;


                    myEISC.Register();
                }

            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        private void myEISC_SigChange(BasicTriList currentdevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                    myEISC.BooleanInput[args.Sig.Number].BoolValue = args.Sig.BoolValue;
                    break;
                case eSigType.UShort:
                    myEISC.UShortInput[args.Sig.Number].UShortValue = args.Sig.UShortValue;
                    break;
                case eSigType.String:
                    myEISC.StringInput[args.Sig.Number].StringValue = args.Sig.StringValue;
                    break;
                case eSigType.NA:
                    break;
            }
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