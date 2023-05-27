using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IgnitionLauncher;

public class Program
{
    public static string BuildRoot => "C:\\Users\\Jaiden\\Documents\\Visual Studio 2022 Projects\\IgnitionLauncher\\builds";
    public static string PackagedContentRoot => "C:\\Users\\Jaiden\\Desktop\\Rogue";
    public static string ClientContentRoot => "C:\\Users\\Jaiden\\Desktop\\RogueClient";

    /*public static string PackagedContentRoot => "C:\\Users\\Jaiden\\Documents\\Visual Studio 2022 Projects\\TestAppForLauncher\\bin\\Debug\\net7.0";
    public static string ClientContentRoot => "C:\\Users\\Jaiden\\Documents\\Visual Studio 2022 Projects\\TestAppForLauncher\\ClientContent";*/

    public static string MostRecentBuildRoot => $"{BuildRoot}/{MostRecentBuildId}";

    public static int MostRecentBuildId = 0;

    public static void Main( string[] args )
    {
        DetermineMostRecentBuild();

        if ( BuildExists( 0 ) )
        {
            CheckForDiffsAgainstPackagedContent( MostRecentBuildId );
        }
        else
        {
            Directory.CreateDirectory( $"{BuildRoot}/0" );
            ConstructBuild();
        }
    }

    public static void CheckForDiffsAgainstPackagedContent( int buildId = 0 )
    {
        var result = CheckPathForDiffs( $"{BuildRoot}/{buildId}", PackagedContentRoot );

        if ( result.AddedFiles.Count > 0 || result.RemovedFiles.Count > 0 || result.ChangedFiles.Count > 0 || result.AddedDirectories.Count > 0 || result.RemovedDirectories.Count > 0 )
            ConstructBuild( ++MostRecentBuildId, result.RemovedFiles, result.RemovedDirectories );
    }

    public static FolderContentsDiffInfo CheckPathForDiffs( string localPath, string comparePath )
    {
        FolderContentsDiffInfo result = new();

        foreach ( string folder in Directory.GetDirectories( localPath, "*", SearchOption.AllDirectories ) )
        {
            if ( !Directory.Exists( folder.Replace( localPath, comparePath ) ) )
                result.RemovedDirectories.Add( folder );
        }

        foreach ( string folder in Directory.GetDirectories( comparePath, "*", SearchOption.AllDirectories ) )
        {
            if ( !Directory.Exists( folder.Replace( comparePath, localPath ) ) )
                result.AddedDirectories.Add( folder );
        }

        foreach ( string file in Directory.GetFiles( localPath, "*", SearchOption.AllDirectories ) )
        {
            if ( !File.Exists( file.Replace( localPath, comparePath ) ) )
            {
                result.RemovedFiles.Add( file );
                continue;
            }

            var localFile = File.ReadAllBytes( file );
            var mymd5 = GetMD5String( GetMD5Hash( localFile ) );

            var compareFile = File.ReadAllBytes( file.Replace( localPath, comparePath ) );
            var comparemd5 = GetMD5String( GetMD5Hash( compareFile ) );

            if ( mymd5 != comparemd5 )
                result.ChangedFiles.Add( file.Replace( localPath, string.Empty ) );
        }

        foreach ( string file in Directory.GetFiles( comparePath, "*", SearchOption.AllDirectories ) )
        {
            if ( !File.Exists( file.Replace( comparePath, localPath ) ) )
            {
                result.AddedFiles.Add( file );
                continue;
            }
        }

        if ( result.ChangedFiles.Count > 0 )
            Console.WriteLine( $"Changed {result.ChangedFiles.Count} files" );

        if ( result.AddedFiles.Count > 0 )
            Console.WriteLine( $"Added {result.AddedFiles.Count} files" );
        
        if ( result.RemovedFiles.Count > 0 )
            Console.WriteLine( $"Removed {result.RemovedFiles.Count} files" );

        return result;
    }

    public static bool BuildExists( int buildId )
    {
        return Directory.Exists( $"{BuildRoot}/{buildId}" );
    }

    public static void DetermineMostRecentBuild()
    {
        foreach ( string item in Directory.GetDirectories( BuildRoot, "*", SearchOption.TopDirectoryOnly ) )
        {
            int buildId = int.Parse( item.Replace( $"{BuildRoot}\\", "" ) );
            if ( buildId > MostRecentBuildId )
                MostRecentBuildId = buildId;
        }
    }

    public static void ConstructBuild( int buildId = 0, List<string>? ignoredFiles = null, List<string>? ignoredDirectories = null )
    {
        string path = PackagedContentRoot;

        Directory.CreateDirectory( path.Replace( path, $"{BuildRoot}/{buildId}" ) );

        foreach ( string item in Directory.GetDirectories( path, "*", SearchOption.AllDirectories ) )
        {
            if ( ignoredDirectories != null && ignoredDirectories.Contains( item ) ) continue;
            Directory.CreateDirectory( item.Replace( path, $"{BuildRoot}/{buildId}" ) );
        }

        foreach ( string item in Directory.GetFiles( path, "*", SearchOption.AllDirectories ) )
        {
            if ( ignoredFiles != null && ignoredFiles.Contains( item ) ) continue;
            File.Copy( item, item.Replace( path, $"{BuildRoot}/{buildId}" ), true );
        }
    }

    public static byte[] GetMD5Hash( byte[] data )
    {
        return MD5.Create().ComputeHash( data );
    }

    public static string GetMD5String( byte[] md5 )
    {
        return BitConverter.ToString( md5 ).Replace( "-", String.Empty ).ToLowerInvariant();
    }
}