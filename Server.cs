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

    static string PackagedContentRoot => $"C:\\Users\\Jaiden\\Documents\\Visual Studio 2022 Projects\\TestAppForLauncher\\bin\\Debug\\net7.0";

    static List<string> Files = new();
    static Dictionary<string, string> FileHashes = new();
    static Dictionary<string, bool> AckFiles = new();

    public Server()
    {
        Listener = new( System.Net.IPAddress.Any, 11000 );
        Listener.Start();

        foreach ( var item in Directory.GetFiles( PackagedContentRoot, "*", SearchOption.AllDirectories ) )
        {
            Files.Add( item );
            FileHashes.Add( item.Replace( $"{PackagedContentRoot}\\", string.Empty ), BuildHandler.GetMD5String( BuildHandler.GetMD5Hash( File.ReadAllBytes( item ) ) ) );
            AckFiles.Add( item.Replace( $"{PackagedContentRoot}\\", string.Empty ), true );
        }

        while ( true )
        {
            try
            {
                IgnitionSocket socket = new( Listener.AcceptTcpClient() );

                PacketMetadata metadata = socket.Read();

                switch ( metadata.Type )
                {
                    case PacketType.RequestFullDownload:
                        socket.Write( BitConverter.GetBytes( Directory.GetFiles( PackagedContentRoot, "*", SearchOption.AllDirectories ).Length ) );
                        break;

                    case PacketType.RequestDownloadFile:
                        {
                            int index = BitConverter.ToInt32( metadata.Data );

                            var fileName = Files[ index ];
                            var file = File.ReadAllBytes( fileName );

                            socket.Write( Encoding.UTF8.GetBytes( fileName.Replace( $"{PackagedContentRoot}\\", string.Empty ) + "@" ).Concat( file ).ToArray() );
                            break;
                        }
                    case PacketType.CompareFileHash:
                        {
                            var filePath = Encoding.UTF8.GetString( metadata.Data );

                            string path = string.Empty;
                            foreach ( char item in filePath )
                            {
                                if ( item == '@' ) break;
                                path += item;
                            }

                            string localHash = string.Empty;
                            if ( FileHashes.TryGetValue( path, out localHash ) )
                            {
                                AckFiles.Remove( path );

                                var fileHash = Encoding.UTF8.GetString( metadata.Data.Skip( path.Length + 1 ).ToArray() );
                                if ( fileHash != localHash )
                                {
                                    Console.WriteLine( $"Mismatch detected for {path}!" );

                                    var file = File.ReadAllBytes( $"{PackagedContentRoot}/{path}" );

                                    socket.Write( file, PacketType.FileMismatched );
                                    Console.WriteLine( file.Length );
                                }
                                else
                                    socket.Write( new byte[ 1 ] );
                            }
                            else
                                socket.Write( new byte[ 1 ] );

                            break;
                        }
                    case PacketType.DoneComparingFileHashes:
                        {
                            if ( AckFiles.Count > 0 )
                            {
                                Console.WriteLine( "Client is missing files!" );

                                socket.Write( BitConverter.GetBytes( AckFiles.Count ), PacketType.NotifyOfMissingFiles );
                            }
                            else
                                socket.Write( new byte[ 1 ] );
                            break;
                        }
                    case PacketType.RequestMissingFile:
                        {
                            foreach ( var item in AckFiles )
                            {
                                AckFiles.Remove( item.Key );

                                var file = File.ReadAllBytes( $"{PackagedContentRoot}/{item.Key}" );
                                var path = item.Key + "@";

                                socket.Write( Encoding.UTF8.GetBytes( path ).Concat( file ).ToArray() );

                                break;
                            }
                            break;
                        }
                }

                socket.Close();
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.Message );
            }
        }
    }
}