using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleFileBrowser;

public class FileBrowserSelectZip : MonoBehaviour
{
    internal void OpenFilePanel(Action<List<string>> onComplete)
    {
        FileBrowser.SetFilters( false, new FileBrowser.Filter( "Zip Archive", ".zip"));
        FileBrowser.SetDefaultFilter( ".zip" );
        FileBrowser.SetExcludedExtensions( ".lnk", ".tmp", ".rar", ".exe" );
        FileBrowser.AddQuickLink( "Users", "C:\\Users", null );
        FileBrowser.AddQuickLink( "Downloads", $"C:\\Users\\{System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)}\\Downloads", null );
        StartCoroutine( ShowLoadDialogCoroutine(onComplete));
    }

    IEnumerator ShowLoadDialogCoroutine(Action<List<string>> onComplete)
    {
        yield return FileBrowser.WaitForLoadDialog( FileBrowser.PickMode.Files, false, null, null, "Select Files", "Load" );
        if( FileBrowser.Success )
            onComplete.Invoke(FileBrowser.Result.ToList());
    }
	
    void OnFilesSelected( string[] filePaths )
    {
        // Print paths of the selected files
        for( int i = 0; i < filePaths.Length; i++ )
            Debug.Log( filePaths[i] );

        // Get the file path of the first selected file
        string filePath = filePaths[0];

        // Read the bytes of the first file via FileBrowserHelpers
        // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
        byte[] bytes = FileBrowserHelpers.ReadBytesFromFile( filePath );

        // Or, copy the first file to persistentDataPath
        string destinationPath = Path.Combine( Application.persistentDataPath, FileBrowserHelpers.GetFilename( filePath ) );
        FileBrowserHelpers.CopyFile( filePath, destinationPath );
    }
}