using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IgnitionLauncher;

public class IgnitionClient
{
    private TcpClient _client;
    private NetworkStream _stream;

    private string _hostname = string.Empty;
    private int _port;

    public IgnitionClient( TcpClient client )
    {
        _client = client;
        _stream = client.GetStream();
    }

    public IgnitionClient( string hostname, int port )
    {
        _hostname = hostname;
        _port = port;

        _client = new( hostname, port );
        _stream = _client.GetStream();
    }

    public void Write( string input ) => Write( Encoding.UTF8.GetBytes( input ) );

    public void Write( byte[] data )
    {
        string base64 = Convert.ToBase64String( data );
        byte[] base64Bytes = Convert.FromBase64String( base64 );
        _stream.Write( base64Bytes, 0, base64Bytes.Length );
    }

    public byte[] Read()
    {
        List<byte> bufferList = new();

        int bytesRead;
        byte[] buffer = new byte[ _client.ReceiveBufferSize ];

        do
        {
            bytesRead = _stream.Read( buffer, 0, buffer.Length );

            for ( int i = 0; i < bytesRead; i++ )
                bufferList.Add( buffer[ i ] );
        } while ( _stream.DataAvailable );

        return Convert.FromBase64String( Convert.ToBase64String( bufferList.ToArray() ) );
    }

    public void Close( bool establishNewConnection = false )
    {
        _stream.Close();
        _client.Close();

        if ( establishNewConnection )
        {
            _client = new( _hostname, _port );
            _stream = _client.GetStream();
        }
    }
}