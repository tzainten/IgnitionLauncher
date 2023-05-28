using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IgnitionLauncher;

public class Client
{
    static TcpClient Tcp { get; set; }

    public Client()
    {
        Tcp = new TcpClient( "192.168.1.246", 11000 );

        var stream = Tcp.GetStream();
        stream.Write( Encoding.UTF8.GetBytes( "Hello Server!" ) );

        Console.ReadLine();
    }
}