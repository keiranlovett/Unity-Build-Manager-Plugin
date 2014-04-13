/////////////////////////////////////////////////////////////////////////////////
//
//	logFile.cs
//	© Keiran Lovett. All Rights Reserved.
//	https://twitter.com/keiranlovett
//	http://www.keiranlovett.com
//
/////////////////////////////////////////////////////////////////////////////////

#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.IO;

public class LogFile{
	// A simple log to file system.
	private	StreamWriter	LogStream;
	private bool			Echo;

	public LogFile( string filename, bool echo_to_debug ){
		LogStream = new StreamWriter( filename, true );

		Echo = echo_to_debug;
	}

	public void Message( string msg )
	{
		if ( LogStream != null ){
			LogStream.WriteLine( msg );

			if ( Echo )
				Debug.Log( msg );
		}
	}

	public void Close(){
		if ( LogStream != null ){
			LogStream.Close();
			LogStream = null;
		}
	}
}
#endif