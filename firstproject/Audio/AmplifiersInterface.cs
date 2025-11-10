using System;

namespace firstproject.Audio
{
    public interface AmplifierInterface
    {
        string Name { get; set; }
        string Model { get; set; }

        List<string> AudioDevices { get; set; }

         
        

    }    


}