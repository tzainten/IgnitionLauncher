using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IgnitionLauncher;

public static class NetworkStreamHelper
{
    public static void WriteData( TcpClient client, byte[] data )
    {
        byte[] eos = Encoding.UTF8.GetBytes( "\0" );
        byte[] dataWithEos = data.Concat( eos ).ToArray();

        var stream = client.GetStream();
        stream.Write( dataWithEos );
    }

    public static bool ReadData( TcpClient client, ref byte[] buffer )
    {
        var stream = client.GetStream();

        if ( !stream.CanRead )
            return false;

        string data = string.Empty;

        try
        {
            int byteCount;
            while ( ( byteCount = stream.Read( buffer, 0, buffer.Length ) ) != 0 )
            {
                Console.WriteLine( byteCount );

                data += Encoding.UTF8.GetString( buffer, 0, byteCount );
                if ( data.EndsWith( "\0" ) )
                    return true;
            }

            return false;
        }
        catch ( Exception ex )
        {
            return false;
        }
    }
}