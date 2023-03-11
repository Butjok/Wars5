using System.Diagnostics;
using System.IO;
using Butjok.CommandLine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class GetGitInfo {
    
    [Command]
    [DidReloadScripts]
    public static void Test() {
    
#if UNITY_EDITOR_WIN
            //Debug.Log("");
#elif UNITY_EDITOR_OSX

        var path = Path.Combine(Path.Combine(Application.dataPath, ".."), "GetGitInfo.sh");
        var process = new Process {
            StartInfo = new ProcessStartInfo { FileName = path }
        };
        process.Start();
        
        AssetDatabase.Refresh();

#endif

    }
}