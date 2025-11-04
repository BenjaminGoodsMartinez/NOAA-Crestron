

using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace firstproject.Properties
{
    public class UserInterface
    {
        public delegate void DigitalChangEventHandler(uint deiceId, SigEventArgs args);
        public event DigitalChangEventHandler DigitalChanged;

        public delegate void AnalogChangEventHandler(uint deiceId, SigEventArgs args);
        public event AnalogChangEventHandler AnalogChanged;
        
        public delegate void SerialChangEventHandler(uint deiceId, SigEventArgs args);
        public event SerialChangEventHandler SerialChanged;
        
        public UserInterface()
        {
           
        }
         public void InterfaceSigChange(BasicTriList currentDevice, SigEventArgs args)
                    {
                        switch (args.Sig.Type)
                        {
                            case eSigType.Bool:
                            {
                                DigitalChanged(currentDevice.ID, args);
                                break;
                            }

                            case eSigType.UShort:
                            {
                                AnalogChanged(currentDevice.ID, args);
                                break;
                            }

                            case eSigType.String:
                            {
                                SerialChanged(currentDevice.ID, args);
                                break;
                            }
                                
                        }
                        
                    }
        
    }
}