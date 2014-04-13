/////////////////////////////////////////////////////////////////////////////////
//
//	buildMenus.cs
//	© Keiran Lovett. All Rights Reserved.
//	https://twitter.com/keiranlovett
//	http://www.keiranlovett.com
//
/////////////////////////////////////////////////////////////////////////////////

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;

public class BuildMenu : ScriptableObject {
	[MenuItem("Build/Build Web", false, 1000)]
	public static void BuildWebPlayer(){
		BuildTools.BuildWeb();
	}
	[MenuItem("Build/Build Win32", false, 1000)]
	public static void BuildPCPlayer(){
		BuildTools.BuildPC();
	}
	[MenuItem("Build/Build Win32 - Compressed", false, 1000)]
	public static void BuildPCCompressed(){
		BuildTools.BuildPCCompressed();
	}
	[MenuItem("Build/Build OSX", false, 1000)]
	public static void BuildMacPlayer(){
		BuildTools.BuildMac();
	}
	[MenuItem("Build/Build Android", false, 1000)]
	public static void BuildAndroidPlayer(){
		BuildTools.BuildAndroid();
	}
}
#endif