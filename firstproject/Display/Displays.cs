// Displays/Display.cs
using System;
using Crestron.SimplSharp; // for TCP/IP in SIMPL#
using Crestron.SimplSharp.CrestronSockets;


namespace firstproject.Display
{
  public class Display : DisplayInterface
  {
    public string Name { get; private set; }
    public string Address { get; private set; }
    public int Port { get; private set; }

    private bool _powerOn;
    public DisplayPowerState State { get; private set; } = DisplayPowerState.Off;

    public event Action<DisplayPowerState> OnPowerStateChange;

    private TCPClient _client;
    private readonly byte[] _onCmd = System.Text.Encoding.ASCII.GetBytes("POWER ON\r");
    private readonly byte[] _offCmd = System.Text.Encoding.ASCII.GetBytes("POWER OFF\r");

    public Display(DisplayConfig config)
    
    {
      
      Name = config.Name;
      Address = config.Address;
      Port = config.Port;
      _client = new TCPClient(Address, Port, 2000);
      _client.SocketStatusChange += (o, a) =>
      {
        // optional: log or surface connection changes
      };
      //       _client.ConnectToServerAsync((TCPClient c, SocketStatus s) =>
      // {
      //     if (s == SocketStatus.SOCKET_STATUS_CONNECTED)
      //         CrestronConsole.PrintLine($"{Name} connected to {Address}:{Port}");
      //     else
      //         CrestronConsole.PrintLine($"{Name} connect failed: {s}");
      // });
    }

    public bool PowerOn
    {
      get { return _powerOn; }
      set
      {
        if (value == _powerOn) return;

        try
        {
          var bytes = value ? _onCmd : _offCmd;
          if (_client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
            _client.SendData(bytes, bytes.Length);

          _powerOn = value;
          State = _powerOn ? DisplayPowerState.On : DisplayPowerState.Off;
          OnPowerStateChange?.Invoke(State);
        }
        catch (Exception)
        {
          // handle and maybe set State = Cooling/Warming as needed
        }
      }
    }

    private void ConnectCallback(TCPClient client, SocketStatus status)
    {

    }


public void Connect()
        {
      _client.ConnectToServerAsync((Crestron.SimplSharp.CrestronSockets.TCPClient client) =>
      {
        var status = client.ClientStatus;
        if (status == SocketStatus.SOCKET_STATUS_CONNECTED)
        {
          CrestronConsole.PrintLine($"{Name} connected to {Address}:{Port}");
        }
        else
        {
          CrestronConsole.PrintLine($"{Name} failed to connect: {status}");
        }
      });
        }

  }
}
