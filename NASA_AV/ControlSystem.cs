using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;
using firstproject.Properties;
using Crestron.SimplSharpPro.DM.Streaming;
using Crestron.SimplSharpPro.AudioDistribution;
using Crestron.SimplSharpPro.EthernetCommunication;
using Independentsoft.Exchange.Autodiscover; // For Generic Device Support

namespace firstCrestronProject
{
    public class ControlSystem : CrestronControlSystem
    {
        private EthernetIntersystemCommunications myEISC;
        private Tsw1070 _touchpanel;
        private UserInterface _userInterface;
        private ControlSystem _controlSystem;
        private const uint touchpanelID = 0x03;
        private DmNvxE30 e30Transmitter_MCR_Lecturn; //tx
        private DmNvxE30 e30Transmitter_MCR_PTZ_1; //tx
        private DmNvxE30 e30Transmitter_MCR_PTZ_2; //tx
        private DmNvxE30 e30Transmitter_MCR_PTZ_3; //tx
        private DmNvxE30 e30Transmitter_Briefing_1; //tx
        private DmNvxE30 e30Transmitter_Briefing_Camera; //tx
        
        
        
        private DmNvxE30 e30Receiver_VideoWall_1; //rx
        private DmNvxE30 e30Receiver_VideoWall_2; //rx
        private DmNvxE30 e30Receiver_VideoWall_3; //rx
        private DmNvxE30 e30Receiver_VideoWall_4; //rx
        private DmNvxE30 e30Receiver_MCR_DualDisplay_1; //rx
        private DmNvxE30 e30Receiver_MCR_DualDisplay_2; //rx
        private DmNvxE30 e30Receiver_Briefing_DualDisplay_1; //rx
        private DmNvxE30 e30Receiver_Briefing_DualDisplay_2; //rx
        private DmNvxE30 e30Receiver_Briefing_Confidence_Monitor; //rx

        private DmNax8Zsa NaxAmp;

        
        /// <summary>
        /// ControlSystem Constructor. Starting point for the SIMPL#Pro program.
        /// Use the constructor to:
        /// * Initialize the maximum number of threads (max = 400)
        /// * Register devices
        /// * Register event handlers
        /// * Add Console Commands
        /// 
        /// Please be aware that the constructor needs to exit quickly; if it doesn't
        /// exit in time, the SIMPL#Pro program will exit.
        /// 
        /// You cannot send / receive data in the constructor
        /// </summary>
        public ControlSystem()
            : base()
        {
            try
            {
                
                CrestronConsole.PrintLine("Constructor is working!");
                
                Thread.MaxNumberOfUserThreads = 20;

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(_ControllerEthernetEventHandler);

                _touchpanel = new Tsw1070(touchpanelID, this);
                e30Receiver_VideoWall_1 = new DmNvxE30(0x03, this);
                _touchpanel.OnlineStatusChange += TouchpanelOnOnlineStatusChange;

                void TouchpanelOnOnlineStatusChange(GenericBase currentdevice, OnlineOfflineEventArgs args)
                {
                    if (currentdevice == _touchpanel)
                    {
                        if (args.DeviceOnLine)
                        {
                            ErrorLog.Notice(("Touch panel is online"));
                        }
                        else
                        {
                            ErrorLog.Error("Touch Panel is online");
                        }
                    }
                }

                _touchpanel.Description = "Main 10in touch panel";

                if (_touchpanel.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Error message");
                    CrestronConsole.PrintLine(("There was an error registering the touch panel."));
                }
                
               _touchpanel.SigChange += TouchpanelOnSigChange;

               void TouchpanelOnSigChange(BasicTriList currentdevice, SigEventArgs args)
               {
                   if (currentdevice == _touchpanel)
                   {
                       switch (args.Sig.Type)
                       {
                           case eSigType.NA:
                               break;
                           case eSigType.Bool:
                           {
                               if (args.Sig.Number == 10)
                               {
                                   if (args.Sig.BoolValue == true)
                                   {
                                       _touchpanel.BooleanInput[20].BoolValue = true;
                                   }

                                   if (args.Sig.BoolValue == false)
                                   {
                                       _touchpanel.BooleanInput[20].BoolValue = false;
                                   }
                               }
                           }
                               break;
                           case eSigType.UShort:
                               break;   
                           case eSigType.String:
                               break;
                       }
                   }
               }

                _touchpanel.SigChange += _userInterface.InterfaceSigChange;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
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