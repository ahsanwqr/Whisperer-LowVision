/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class BuildFlavors
{
    private const string ApkAppName = "Whisperer";
    private const string buildFolderName = "builds";

    private static readonly string[] projectScenes;

    static BuildFlavors()
    {
        var scenePaths = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            scenePaths.Add(scene.path);
        }

        projectScenes = scenePaths.ToArray();
    }

    public static void BuildWin()
    {
        var buildPath = Path.Combine(Path.GetFullPath("."), buildFolderName);
        BuildGeneric("MyScenes.exe",
          projectScenes,
          BuildOptions.ShowBuiltPlayer,
          buildPath,
          BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Whisperer/Build/Android 64")]
    public static void BuildAndroid64()
    {
        Android(AndroidArchitecture.ARM64);
    }

    [MenuItem("Whisperer/Build/Android 32")]
    public static void BuildAndroid32()
    {
        Android(AndroidArchitecture.ARMv7);
    }

    public static void Android(AndroidArchitecture architecture)
    {
        string previousAppIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
        PlayerSettings.Android.targetArchitectures = architecture;
        var friendlyPrint = architecture == AndroidArchitecture.ARMv7 ? "32" : "64";
        var implementation = architecture == AndroidArchitecture.ARM64 ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, implementation);
        BuildPlayerOptions buildOptions = new BuildPlayerOptions()
        {
            locationPathName = $"{buildFolderName}/{ApkAppName}_{friendlyPrint}.apk",
            scenes = projectScenes,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
        };
        buildOptions.options = new BuildOptions();
        try
        {
            var error = BuildPipeline.BuildPlayer(buildOptions);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, previousAppIdentifier);
            HandleBuildError.Check(error);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log($"Build exception: {e}");
            UnityEngine.Debug.Log("Exception while building: exiting with exit code 2.");
            EditorApplication.Exit(2);
        }
    }

    private static void BuildGeneric(string buildName,
      string[] scenes,
      BuildOptions buildOptions,
      string path,
      BuildTarget target)
    {

        if (!string.IsNullOrEmpty(buildName) && null != scenes && scenes.Length > 0)
        {
            var fullPath = Path.Combine(path, buildName);
            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(scenes, fullPath, target, buildOptions);
            }
            else
            {
                UnityEngine.Debug.Log("Invalid build path!");
            }
        }
        else
        {
            UnityEngine.Debug.Log("Invalid build configuration!");
        }
    }
}

public class HandleBuildError
{
    public static void Check(UnityEditor.Build.Reporting.BuildReport buildReport)
    {
        bool buildSucceeded =
            buildReport.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
        if (buildReport.summary.platform == BuildTarget.Android)
        {
            // Android can fail to produce the output even if the build is marked as succeeded in some rare
            // scenarios, notably if the Unity directory is read-only... Annoying, but needs to be handled!
            buildSucceeded = buildSucceeded && File.Exists(buildReport.summary.outputPath);
        }
        if (buildSucceeded)
        {
            UnityEngine.Debug.Log("Exiting with exit code 0");
            EditorApplication.Exit(0);
        }
        else
        {
            UnityEngine.Debug.Log("Exiting with exit code 1");
            EditorApplication.Exit(1);
        }
    }
}
