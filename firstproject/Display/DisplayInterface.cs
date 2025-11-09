using System;


namespace firstproject.Display
{
    public interface DisplayInterface
    {
        

    string Name { get; }
    bool PowerOn { get; set; }        
    DisplayPowerState State { get; }  
    event Action<DisplayPowerState> OnPowerStateChange;

    }



}