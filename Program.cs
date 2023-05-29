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

    //public static string PackagedContentRoot => $"C:\\Users\\Jaiden\\Documents\\Visual Studio 2022 Projects\\TestAppForLauncher\\bin\\Debug\\net7.0";
    //static string ClientContentRoot => $"C:\\Users\\Jaiden\\Documents\\Visual Studio 2022 Projects\\TestAppForLauncher\\ClientContent";
    public static string PackagedContentRoot => @$"{Environment.GetFolderPath( Environment.SpecialFolder.Desktop )}\Rogue";
    public static string ClientContentRoot => @$"{Environment.GetFolderPath( Environment.SpecialFolder.Desktop )}\TestApp";

    static List<string> Folders = new();
    static List<string> Files = new();

    static ConcurrentDictionary<string, string> FileMetadata = new();

    public static void CreateAllFoldersForPath( string path )
    {
        int i = 0;
        string folderPath = string.Empty;
        var folders = path.Split( Path.DirectorySeparatorChar );
        foreach ( string folder in folders )
        {
            if ( i == 0 )
                folderPath += folder;
            else
                folderPath += $"{Path.DirectorySeparatorChar}{folder}";

            if ( !Directory.Exists( folderPath ) )
                Directory.CreateDirectory( folderPath );

            i++;
        }
    }

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

            int folderCount = BitConverter.ToInt32( socket.Read().Data );
            socket.Close( true );

            for ( int i = 0; i < folderCount; i++ )
            {
                socket.Write( BitConverter.GetBytes( i ), PacketType.RequestDownloadFolder );

                var metadata = socket.Read();
                socket.Close( true );

                var folderName = $"{ClientContentRoot}\\{Encoding.UTF8.GetString( metadata.Data )}";
                if ( Directory.Exists( folderName ) ) continue;

                Directory.CreateDirectory( folderName );
            }

            socket.Write( new byte[ 1 ], PacketType.RequestFileCount );

            int fileCount = BitConverter.ToInt32( socket.Read().Data );
            socket.Close( true );

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

                CreateAllFoldersForPath( $"{ClientContentRoot}\\{path}" );
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

            PacketMetadata metadata;
            for ( int i = 0; i < files.Length; i++ )
            {
                string item = files[ i ];

                string hash;
                if ( !FileMetadata.TryGetValue( item.Replace( $"{ClientContentRoot}\\", string.Empty ), out hash ) )
                    throw new Exception( "Failed to get a hash!" );

                byte[] filePath = Encoding.UTF8.GetBytes( ( item + "@" ).Replace( $"{ClientContentRoot}\\", string.Empty ) );
                byte[] fileHash = Encoding.UTF8.GetBytes( hash );

                socket.Write( filePath.Concat( fileHash ).ToArray(), PacketType.CompareFileHash );

                metadata = socket.Read();
                if ( metadata.Type == PacketType.FileMismatched )
                {
                    File.WriteAllBytes( item, metadata.Data );
                    Console.WriteLine( metadata.Data.Length );
                }

                socket.Close( true );
            }

            socket.Write( new byte[ 1 ], PacketType.DoneComparingFileHashes );

            metadata = socket.Read();
            socket.Close( true );

            if ( metadata.Type == PacketType.NotifyOfMissingFiles )
            {
                int missingFileCount = BitConverter.ToInt32( metadata.Data );
                for ( int i = 0; i < missingFileCount; i++ )
                {
                    socket.Write( new byte[ 1 ], PacketType.RequestMissingFile );

                    metadata = socket.Read();
                    var filePath = Encoding.UTF8.GetString( metadata.Data );

                    string path = string.Empty;
                    foreach ( char item in filePath )
                    {
                        if ( item == '@' ) break;
                        path += item;
                    }

                    CreateAllFoldersForPath( $"{ClientContentRoot}\\{path}" );
                    File.WriteAllBytes( $"{ClientContentRoot}/{path}", metadata.Data.Skip( path.Length + 1 ).ToArray() );
                    socket.Close( true );
                }
            }

            socket.Close();
        }
#else
        Server = new();
#endif

        Console.ReadKey();
    }
}