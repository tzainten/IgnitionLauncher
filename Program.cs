using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace IgnitionLauncher;

public class Program
{
    static Client? Client;
    static Server? Server;

    public static void Main( string[] args )
    {
#if CLIENT
        Client = new();
#else
        Server = new();
#endif

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