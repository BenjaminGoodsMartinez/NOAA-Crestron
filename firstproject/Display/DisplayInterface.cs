using System;


namespace firstproject.Display
{
    public interface DisplayInterface
    {
        

    string Name { get; }
    bool PowerOn { get; set; }        // set = send command; get = last known state
    DisplayPowerState State { get; }   // richer state if you want
    event Action<DisplayPowerState> OnPowerStateChange;

    }



}