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

        while ( true )
        {
            TcpClient client = Listener.AcceptTcpClient();
            var stream = client.GetStream();
            StreamReader reader = new( stream );
            StreamWriter writer = new( stream );
            
            try
            {
                byte[] buffer = new byte[ 1024 ];
                stream.Read( buffer, 0, buffer.Length );
                int recv = 0;
                foreach ( byte b in buffer )
                {
                    if ( b != 0 )
                        recv++;
                }
                string request = Encoding.UTF8.GetString( buffer, 0, recv );
                Console.WriteLine( request );
                writer.WriteLine( "Sup bro!" );
                writer.Flush();
            }
            catch ( Exception ex ) 
            {
                Console.WriteLine( ex.Message );
            }
        }
    }
}