/////////////////////////////////////////////////////////////////////////////////
//
//	buildtools.cs
//	© Keiran Lovett. All Rights Reserved.
//	https://twitter.com/keiranlovett
//	http://www.keiranlovett.com
//
//	description:	Base class for extended monobehaviours. This script manages automatic builds of projects, and creates version numbers and logs.  Basic comments
//
/////////////////////////////////////////////////////////////////////////////////


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Ionic.Zip;
using System.Collections;
using System.Collections.Generic;

public class BuildTools {
	[SerializeField]
	static string projectName ="GameBuild";

	//Build variations, these are simple and can be expaded upon easily. :: Build(Target build platform, directory to deposite, compress if true);
	public static void BuildWeb() {
		Build(BuildTarget.WebPlayer, @"Builds/"+projectName+"(Web)", true);
	}

	public static void BuildPC() {
		Build(BuildTarget.StandaloneWindows, @"Builds/"+projectName+"(Win32)", false);
	}

	public static void BuildPCCompressed() {
		Build(BuildTarget.StandaloneWindows, @"Builds/"+projectName+"(Win32)", true);
	}

	public static void BuildMac() {
		Build(BuildTarget.StandaloneOSXUniversal, @"Builds/"+projectName+"(OSX)", false);
	}

	public static void BuildAndroid(){
		Build(BuildTarget.Android, @"Builds/"+projectName+"(Android)", false);
	}

	//-------------------------------------------------------------------------------------------------------------------------
	public static void Build(BuildTarget target, string output, bool compress) {
		//This helps with getting the version number base to start from
		var settingsPath = Path.GetDirectoryName(Application.dataPath);
        settingsPath = Path.Combine(settingsPath, "ProjectSettings");
        settingsPath = Path.Combine(settingsPath, "ProjectSettings.asset");
 		//Throw error if we can't find anything
        if (!File.Exists(settingsPath)) {
            Debug.LogError("Couldn't find project settings file.");
            return;
        }

        // Make sure the paths exist before building.
		try{
			Directory.CreateDirectory( output );
		}
		catch{
			Debug.LogError("Failed to create directories: " + output );
		}
 
        var lines = File.ReadAllLines(settingsPath);
        if (!lines[0].StartsWith("%YAML")) {
            Debug.LogError("Project settings file needs to be serialized as a text asset. (Check 'Project Settings->Editor')");
            return;
        }
 
        string pattern = @"^(\s*iPhoneBundleVersion:\s*)([\d\.]+)$";
        bool success = false;
 
        System.Version version = null;
        for (int i=0; i<lines.Length; i++) {
            var line = lines[i];
 
            if (!Regex.IsMatch(line, pattern)) {
                continue;
            }
 
            var match = Regex.Match(line, pattern);
            version = new System.Version(match.Groups[2].Value);
            //Break down our version numbers
            var major = version.Major < 0 ? 0 : version.Major;
            var minor = version.Minor < 0 ? 0 : version.Minor;
            var build = version.Build < 0 ? 0 : version.Build;
            var revision = version.Revision < 0 ? 0 : version.Revision;
 
            version = new System.Version(major, minor, build, revision + 1);
 
            line = match.Groups[1].Value + version;
 
            lines[i] = line;
            success = true;
 
            break;
        }
 		//Throw error if we can't find the bundle version.
        if (!success) {
            Debug.LogError("Couldn't find bundle version in ProjectSettings.asset");
            return;
        }

 		//Lets start writing the files to the log and such
        File.WriteAllLines(settingsPath, lines);
		LogFile log = new LogFile(@"Builds/log.log", false);
		log.Message("-----BUILD-----");
		log.Message("Build version: " + version);
		log.Message( "Building Platform: " + target.ToString() );
	
		//These are to speed things up a little bit.
		string projectNameFile = null;

		if(target.ToString() == "WebPlayer") {
			projectNameFile = projectName+"_"+version;
		} else if(target.ToString() == "StandaloneWindows") {
			projectNameFile = projectName+"_"+version+".exe";
		} else if(target.ToString() == "StandaloneOSXUniversal") {
			projectNameFile = projectName+"_"+version+".app";
		} else if(target.ToString() == "Android") {
			projectNameFile = projectName+"_"+version+".apk";
		}
		string[] level_list = FindScenes();
		log.Message("Scenes to be processed: " + level_list.Length );

		foreach( string s in level_list)
		{
			string cutdown_level_name = s.Remove( s.IndexOf(".unity") );
			log.Message("   " + cutdown_level_name );
		}

		string results = BuildPipeline.BuildPlayer( level_list, output+"/"+projectNameFile, target, BuildOptions.None );
		if ( results.Length == 0 )
			log.Message("No Build Errors" );
		else
			log.Message("Build Error:" + results);

		//This is where we decide if we want to zip the files, make sure Ionic.Zip is included to reference this.
		//This could be cleaned up by moving everything to a temp directory, compressing, and then moving to the proper directory - but I'm too lazy at this point so I'll do that later.
		if(compress == true) {
			//For some reason Windows needs a file and a directory, so here's a silly hardcode to deal with it.
			if(target.ToString() == "StandaloneWindows") {
				using (ZipFile zip = new ZipFile()) {
		             // add this file into the project directory in the zip archive
		             zip.AddFile(output+"/"+projectNameFile, "");
		             zip.AddDirectory(output+"/"+projectName+"_"+version+"_Data", projectName+"_"+version+"_Data");
		             // add the file into a different directory in the archive
		             zip.Save(output+"/"+projectName+version+".zip");
		        }
		    //Web apps is shoved into a folder already, we'll just compress it.
		    } else if(target.ToString() == "WebPlayer") {
				using (ZipFile zip = new ZipFile()) {
		             // add this directory in the zip archive
		             zip.AddDirectory(output+"/"+projectName+"_"+version, "");
		             // add the file into a different directory in the archive
		             zip.Save(output+"/"+projectName+version+".zip");
		        }
		    } else {
				using (ZipFile zip = new ZipFile()) {
		             // add this directory in the zip archive
		             zip.AddDirectory(output+"/"+projectNameFile, "");
		             // add the file into a different directory in the archive
		             zip.Save(output+"/"+projectName+version+".zip");
		        }
		    }
		    log.Message("Compressed:"+output+"/"+projectName+version+".zip");
		}

		log.Message( "");
		log.Close();

	}

	//Find the scenes so we can add them to the log
	public static string[] FindScenes(){
		int num_scenes = 0;

		// Count active scenes.
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
			if (scene.enabled)
				num_scenes++;
		}

		// Build the list of scenes.
		string[] scenes = new string[num_scenes];

		int x = 0;
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
			if ( scene.enabled )
				scenes[x++] = scene.path;
		}

		return ( scenes );
	}

}

#endif
