namespace firstCrestronProject;

using SimplToPro.Config;
public class Displays
{
    public String Name { get; set; }
    public int Port { get;  set;}
    public string Make { get; set; }
    public string Model { get; set; }

    public Displays (DisplayConfig config)
    {
      this.address = config.Address;
      this.port = config.Port;
      this.Name = config.Name;
      this.Make = config.Make;
      this.Model = config.Model;
    }
}