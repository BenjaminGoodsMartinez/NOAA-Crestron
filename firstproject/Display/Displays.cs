using System;

namespace firstproject.Display
{
  // POCO used to pass configuration (from JSON or elsewhere)
  public class Displays
  {
    public string Name { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
     public bool IsPowerOn { get; private set; }
  



    public Displays(DisplayConfig config)
    {
      if (config == null) throw new ArgumentNullException(nameof(config));
      this.Address = config.Address;
      this.Port = config.Port;
      this.Name = config.Name;
      this.Make = config.Make;
      this.Model = config.Model;
    }

    // Example API — fill in with your transport later
    public void PowerOn()  { IsPowerOn = true;  /* send POWER ON */ }
    public void PowerOff() { IsPowerOn = false; /* send POWER OFF */ }
  }
}
