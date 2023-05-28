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
    connection:
        try
        {
            TcpClient client = new( "192.168.1.123", 11000 );
            string message = "Hello World!";

            int byteCount = Encoding.ASCII.GetByteCount( message + 1 );
            byte[] sendData = new byte[ byteCount ];
            sendData = Encoding.ASCII.GetBytes( message );

            NetworkStream stream = client.GetStream();
            stream.Write( sendData, 0, sendData.Length );

            StreamReader reader = new( stream );
            string response = reader.ReadLine();
            Console.WriteLine( response );

            stream.Close();
            client.Close();
            Console.ReadKey();
        }
        catch ( Exception ex )
        {
            Console.WriteLine( "Failed to connect, retrying..." );
            goto connection;
        }
    }
}