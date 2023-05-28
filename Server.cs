using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IgnitionLauncher;

public class Server
{
    static TcpListener Listener { get; set; }

    public Server()
    {
        Listener = new( System.Net.IPAddress.Any, 11000 );
        Listener.Start();
        Listener.Server.ReceiveBufferSize = 1024 * 8;
        Listener.Server.SendBufferSize = 1024 * 20;

        while ( true )
        {
            if ( Listener.Pending() )
            {
                Console.WriteLine( "Processing request!" );
                Socket request = Listener.AcceptSocket();

                Span<byte> buffer = new();
                var stream = new NetworkStream( request );
                stream.Read( buffer );

                string data = Encoding.UTF8.GetString( buffer );
                Console.WriteLine( $"data: {data}" );
            }
        }
    }
}