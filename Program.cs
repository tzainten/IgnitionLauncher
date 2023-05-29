using System;
using System.Collections.Concurrent;
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

    static string PackagedContentRoot => $"C:\\Users\\Jaiden\\Documents\\Visual Studio 2022 Projects\\TestAppForLauncher\\bin\\Debug\\net7.0";
    //static string ClientContentRoot => $"C:\\Users\\Jaiden\\Documents\\Visual Studio 2022 Projects\\TestAppForLauncher\\ClientContent";
    //static string PackagedContentRoot => $"{Environment.GetFolderPath( Environment.SpecialFolder.Desktop )}/Rogue";
    static string ClientContentRoot => $"{Environment.GetFolderPath( Environment.SpecialFolder.Desktop )}/TestApp";

    static List<string> Folders = new();
    static List<string> Files = new();

    static ConcurrentDictionary<string, string> FileMetadata = new();

    public static void Main( string[] args )
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

#if CLIENT
        //Client = new( "127.0.0.1", 11000 );

        IgnitionSocket socket = new( "127.0.0.1", 11000 );

        string[] files = Directory.GetFiles( ClientContentRoot, "*", SearchOption.AllDirectories );

        if ( files.Length == 0 )
        {
            socket.Write( new byte[ 1 ], PacketType.RequestFullDownload );

            int fileCount = BitConverter.ToInt32( socket.Read().Data );
            socket.Close( true );

            Console.WriteLine( fileCount );

            for ( int i = 0; i < fileCount; i++ )
            {
                socket.Write( BitConverter.GetBytes( i ), PacketType.RequestDownloadFile );

                var metadata = socket.Read();
                var filePath = Encoding.UTF8.GetString( metadata.Data );

                string path = string.Empty;
                foreach ( char item in filePath )
                {
                    if ( item == '@' ) break;
                    path += item;
                }

                socket.Close( true );

                Console.WriteLine( metadata.Data.Skip( path.Length + 1 ).ToArray().Length );

                File.WriteAllBytes( $"{ClientContentRoot}/{path}", metadata.Data.Skip( path.Length + 1 ).ToArray() );
            }
        }
        else
        {
            Parallel.ForEach( files, ( string item ) =>
            {
                var file = File.ReadAllBytes( item );
                FileMetadata.TryAdd( item.Replace( $"{ClientContentRoot}\\", string.Empty ), BuildHandler.GetMD5String( BuildHandler.GetMD5Hash( file ) ) );
            } );

            for ( int i = 0; i < files.Length; i++ )
            {
                string item = files[ i ];

                string hash;
                if ( !FileMetadata.TryGetValue( item.Replace( $"{ClientContentRoot}\\", string.Empty ), out hash ) )
                    throw new Exception( "Failed to get a hash!" );

                byte[] filePath = Encoding.UTF8.GetBytes( ( item + "@" ).Replace( $"{ClientContentRoot}\\", string.Empty ) );
                byte[] fileHash = Encoding.UTF8.GetBytes( hash );

                socket.Write( filePath.Concat( fileHash ).ToArray(), PacketType.CompareFileHash );

                PacketMetadata metadata = socket.Read();
                if ( metadata.Type == PacketType.FileMismatched )
                {
                    File.WriteAllBytes( item, metadata.Data );
                    Console.WriteLine( metadata.Data.Length );
                }


                socket.Close( true );
            }
        }
#else
        Server = new();
#endif

        Console.ReadKey();
    }
}