using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildMenu
{
    private const string BuildRoot = "Builds";
    private const string ProductName = "Roguelike";

    [MenuItem("Build/Build PC App")]
    public static void BuildPCApp()
    {
        string platformFolder = Application.platform == RuntimePlatform.OSXEditor ? "macOS" : "PC";
        string buildPath = Path.Combine(BuildRoot, platformFolder);

        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            buildPath = Path.Combine(buildPath, ProductName + ".app");
        }
        else
        {
            buildPath = Path.Combine(buildPath, ProductName + ".exe");
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = buildPath,
            target = Application.platform == RuntimePlatform.OSXEditor ? BuildTarget.StandaloneOSX : BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build finished with result: " + report.summary.result + ", output: " + buildPath);
    }

    private static string[] GetEnabledScenes()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        System.Collections.Generic.List<string> enabledScenes = new System.Collections.Generic.List<string>();
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].enabled)
            {
                enabledScenes.Add(scenes[i].path);
            }
        }

        return enabledScenes.ToArray();
    }
}
