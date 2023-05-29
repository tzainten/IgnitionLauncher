using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IgnitionLauncher;

public class Client
{
    private string _hostname;
    private int _port;

    public Client( string hostname, int port )
    {
        _hostname = hostname;
        _port = port;

        IgnitionSocket socket = new( hostname, port );

        Console.WriteLine( Encoding.UTF8.GetString( socket.Read().Data ) );
        socket.Close();
    }
}