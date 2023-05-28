using System;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;

namespace IgnitionLauncher;

public class Program
{
    public static void Main( string[] args )
    {
        BuildHandler.DetermineMostRecentBuild();

        if ( BuildHandler.MostRecentBuildId != -1 )
            BuildHandler.CheckForDiffsAgainstPackagedContent( BuildHandler.MostRecentBuildId );
        else
        {
            Directory.CreateDirectory( $"{BuildHandler.BuildRoot}/0" );
            BuildHandler.ConstructBuild();
        }
    }
}