using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IgnitionLauncher;

public static class BuildHandler
{
    public static string BuildRoot => "C:\\Users\\Jaiden\\Documents\\Visual Studio 2022 Projects\\IgnitionLauncher\\builds";
    public static string PackagedContentRoot => "C:\\Users\\Jaiden\\Desktop\\Rogue";
    public static string ClientContentRoot => "C:\\Users\\Jaiden\\Desktop\\RogueClient";
    /*public static string PackagedContentRoot => "C:\\Users\\Jaiden\\Documents\\Visual Studio 2022 Projects\\TestAppForLauncher\\bin\\Debug\\net7.0";
    public static string ClientContentRoot => "C:\\Users\\Jaiden\\Documents\\Visual Studio 2022 Projects\\TestAppForLauncher\\ClientContent";*/

    public static string MostRecentBuildRoot => $"{BuildRoot}/{MostRecentBuildId}";

    public static int MostRecentBuildId = -1;

    public static void CheckForDiffsAgainstPackagedContent( int buildId = 0 )
    {
        var result = CheckPathForDiffs( $"{BuildRoot}/{buildId}", PackagedContentRoot );

        if ( result.AddedFiles.Count > 0 || result.RemovedFiles.Count > 0 || result.ChangedFiles.Count > 0 || result.AddedDirectories.Count > 0 || result.RemovedDirectories.Count > 0 )
            ConstructBuild( ++MostRecentBuildId, result.RemovedFiles, result.RemovedDirectories );
    }

    public static FolderContentsDiffInfo CheckPathForDiffs( string localPath, string comparePath )
    {
        FolderContentsDiffInfo result = new();

        Parallel.ForEach( Directory.GetDirectories( localPath, "*", SearchOption.AllDirectories ), ( string folder ) =>
        {
            if ( !Directory.Exists( folder.Replace( localPath, comparePath ) ) )
                result.RemovedDirectories.TryAdd( folder, true );
        } );

        Parallel.ForEach( Directory.GetDirectories( comparePath, "*", SearchOption.AllDirectories ), ( string folder ) =>
        {
            if ( !Directory.Exists( folder.Replace( comparePath, localPath ) ) )
                result.AddedDirectories.TryAdd( folder, true );
        } );

        Parallel.ForEach( Directory.GetFiles( localPath, "*", SearchOption.AllDirectories ), ( string file ) =>
        {
            if ( !File.Exists( file.Replace( localPath, comparePath ) ) )
            {
                result.RemovedFiles.TryAdd( file, true );
                return;
            }

            var localFile = File.ReadAllBytes( file );
            var mymd5 = GetMD5String( GetMD5Hash( localFile ) );

            var compareFile = File.ReadAllBytes( file.Replace( localPath, comparePath ) );
            var comparemd5 = GetMD5String( GetMD5Hash( compareFile ) );

            if ( mymd5 != comparemd5 )
                result.ChangedFiles.TryAdd( file.Replace( localPath, string.Empty ), true );
        } );

        Parallel.ForEach( Directory.GetFiles( comparePath, "*", SearchOption.AllDirectories ), ( string file ) =>
        {
            if ( !File.Exists( file.Replace( comparePath, localPath ) ) )
                result.AddedFiles.TryAdd( file, true );
        } );

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

    public static void ConstructBuild( int buildId = 0, ConcurrentDictionary<string, bool>? ignoredFiles = null, ConcurrentDictionary<string, bool>? ignoredDirectories = null )
    {
        string path = PackagedContentRoot;

        Directory.CreateDirectory( path.Replace( path, $"{BuildRoot}/{buildId}" ) );

        Parallel.ForEach( Directory.GetDirectories( path, "*", SearchOption.AllDirectories ), ( string item ) =>
        {
            bool value;
            if ( ignoredDirectories != null && ignoredDirectories.TryGetValue( item, out value ) ) return;
            Directory.CreateDirectory( item.Replace( path, $"{BuildRoot}/{buildId}" ) );
        } );

        Parallel.ForEach( Directory.GetFiles( path, "*", SearchOption.AllDirectories ), ( string item ) =>
        {
            bool value;
            if ( ignoredFiles != null && ignoredFiles.TryGetValue( item, out value ) ) return;
            File.Copy( item, item.Replace( path, $"{BuildRoot}/{buildId}" ), true );
        } );

        if ( Directory.Exists( $"{BuildRoot}/{buildId - 1}" ) )
            Directory.Delete( $"{BuildRoot}/{buildId - 1}", true );
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
