using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IgnitionLauncher;

public class Program
{
    static Client? Client;
    static Server? Server;

    //static string PackagedContentRoot => $"C:\\Users\\Jaiden\\Documents\\Visual Studio 2022 Projects\\TestAppForLauncher\\bin\\Debug\\net7.0";
    //static string ClientContentRoot => $"C:\\Users\\Jaiden\\Documents\\Visual Studio 2022 Projects\\TestAppForLauncher\\ClientContent";
    static string PackagedContentRoot => $"{Environment.GetFolderPath( Environment.SpecialFolder.Desktop )}/Rogue";
    static string ClientContentRoot => $"{Environment.GetFolderPath( Environment.SpecialFolder.Desktop )}/Rogue";

    public static void Main( string[] args )
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        string type = string.Empty;

        if ( ( type = Console.ReadLine() ) == "1" )
        {
            TcpListener listener = new( IPAddress.Any, 11000 );
            listener.Start();

            TcpClient client;
            while ( true )
            {
                client = null;

                try
                {
                    client = listener.AcceptTcpClient();
                    var stream = client.GetStream();

                    byte[] buffer = new byte[ client.ReceiveBufferSize ];

                    int byteCount = stream.Read( buffer, 0, buffer.Length );

                    var readableData = Encoding.UTF8.GetString( buffer, 0, byteCount ).Split( "@" );

                    foreach ( var item in Directory.GetFiles( PackagedContentRoot, "*", SearchOption.AllDirectories ) )
                    {
                        if ( !item.Contains( readableData[ 0 ] ) ) continue;

                        var file = File.ReadAllBytes( item );
                        var filemd5 = BuildHandler.GetMD5String( BuildHandler.GetMD5Hash( file ) );

                        if ( filemd5 != readableData[ 1 ] )
                        {
                            byte[] fileName = Encoding.UTF8.GetBytes( $"{item.Replace( $"{PackagedContentRoot}\\", string.Empty )}@" );
                            byte[] combinedData = fileName.Concat( file ).ToArray();

                            stream.Write( combinedData, 0, file.Length );
                            break;
                        }
                    }

                    stream.Close();
                    client.Close();

                    Console.WriteLine( $"data: {readableData[ 0 ]}, {readableData[ 1 ]}" );
                }
                catch ( Exception ex ) { }
            }
        }
        else
        {
            foreach ( var item in Directory.GetFiles( ClientContentRoot, "*", SearchOption.AllDirectories ) )
            {
                var file = File.ReadAllBytes( item );
                var filemd5 = BuildHandler.GetMD5String( BuildHandler.GetMD5Hash( file ) );

                try
                {
                    TcpClient client = new();
                    client.Connect( "192.168.1.246", 11000 );

                    var stream = client.GetStream();

                    byte[] data = Encoding.UTF8.GetBytes( $"{item.Replace( $"{ClientContentRoot}\\", string.Empty )}@{filemd5}" );

                    stream.Write( data, 0, data.Length );

                    byte[] buffer = new byte[ client.ReceiveBufferSize ];
                    int byteCount = stream.Read( buffer, 0, buffer.Length );
                    if ( byteCount <= 0 ) continue;

                    var dataAsString = Encoding.UTF8.GetString( buffer, 0, byteCount );

                    int splitIndex = 0;
                    string fileName = string.Empty;
                    for ( int i = 0; i < dataAsString.Length; i++ )
                    {
                        splitIndex++;

                        char c = dataAsString[ i ];
                        if ( c == '@' ) break;

                        fileName += dataAsString[ i ];
                    }

                    byte[] dataWithoutFileName = buffer.Skip( splitIndex ).ToArray();

                    File.WriteAllBytes( $"{ClientContentRoot}/{fileName}", dataWithoutFileName );

                    //Console.WriteLine( fileName );

                    //Console.WriteLine( $"data: {Encoding.UTF8.GetString( buffer, 0, byteCount )}" );

                    client.Close();
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( ex.Message );
                }
            }

            Console.ReadKey();
        }

        /*BuildHandler.DetermineMostRecentBuild();

        if ( BuildHandler.MostRecentBuildId != -1 )
            BuildHandler.CheckForDiffsAgainstPackagedContent( BuildHandler.MostRecentBuildId );
        else
        {
            Directory.CreateDirectory( $"{BuildHandler.BuildRoot}/0" );
            BuildHandler.ConstructBuild();
        }*/
    }
}