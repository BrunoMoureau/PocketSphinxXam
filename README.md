# PocketSphinxXam

## Description
Speech Recognition with PocketSphinx in Xamarin.Forms project (only available for Android)

Here's an example of a Xamarin.Forms speech recognition project using PocketSphinx. I made it with the speech recognition engine PocketSphinx from CMUSphinx (website : https://cmusphinx.github.io/).

I compiled the library and generated the wrapper C#-C following the explanations on their website. I added the libraries (.so) to the project and wrote some logic to use it (inspirated of the java classes distributed on the website).


## How to compile
I built this for Xamarin.Android on Windows 10 and I am not used to other operating systems...

The first thing you need to do is to download several files and tools to perform the compilation. 

### Download

#### IDE
-----------------------------------------------------------------------------
Android Studio - https://developer.android.com/studio/ 

#### Projects
-----------------------------------------------------------------------------
SphinxBase - https://github.com/cmusphinx/sphinxbase
PocketSphinx - https://github.com/cmusphinx/pocketsphinx 
PocketSphinx-Android - https://github.com/cmusphinx/pocketsphinx-android 

#### Tools
-----------------------------------------------------------------------------
Gradle - https://gradle.org/install/
Swig - http://www.swig.org/download.html
Java SDK - http://www.oracle.com/technetwork/java/javase/downloads/index.html

### Installation
- Run Android Studio and download the last Android SDK, CMake and Android NDK using Tools > SDK Manager 
  (you may also need Android SDK Built-Tools).
- Unzip Gradle and Swig archives (e.g. put these folders at C:\Tools) 
- Install Java SDK and check that you have a folder "jre" inside your "jdk" folder.

- Go to your environment variables ('env' in Windows search menu)
- Be careful, at the moment, there is a mistake in build.gradle 'SPHINX_HOME' environment variable name.
  Check the build.gradle file inside pocketsphinx-android-master repository and find something like "$System.env.SPINXBASE_HOME"
  and use this variable name to create your environment variable. I guess it will be corrected one day so be aware of that.

- Add the following variables (note that you will need to use '/' and not '\' as file separator for CMUSphinx projects !!):
	JAVA_HOME 					C:\...\jdk1.8.0_181\jre
	SPINX_HOME (or SPHINX_HOME if corrected)	C:/.../sphinxbase-master
	POCKETSPHINX_HOME				C:/.../pocketsphinx-master

- Update your 'Path' environment variable by adding the following entries :
   	%JAVA_HOME%\bin
	C:\Tools\gradle-4.10\bin	-> depending where you put your gradle folder
	C:\Tools\swigwin-3.0.12		-> depending where you put your swig folder
	%SPINXBASE_HOME%		-> depending of the name you gave to your environment variable
	%POCKETSPHINX_HOME%

- Check that you can run the following commands from any location :
	gradle -v
	swig -v
	java -version

- Add a new text file to pocketsphinx-android-master repository with paths to your Android SDK and NDK as follow :
	sdk.dir=C\:\\androidstudio-sdk
	ndk.dir=C\:\\androidstudio-sdk\\ndk-bundle

- Update the pocketsphinx-android-master build.gradle file. Keep this one as reference (please look at the comments) :

'''
hello markdown
'''

buildscript {
    repositories {
        jcenter()
    }
    dependencies {
        classpath 'com.android.tools.build:gradle:2.3.1'
    }
}

apply plugin: 'com.android.library'

android {
    compileSdkVersion 27					// Your Android SDK version
    buildToolsVersion "27.0.3"					// Your Android SDK Tools version
    
    defaultConfig {
        minSdkVersion 14
        targetSdkVersion 27					// Your Android SDK version
        versionCode 5
        versionName "5prealpha"
        setProperty("archivesBaseName", "pocketsphinx-android-$versionName")
	externalNativeBuild {
	    cmake {
	    }
	}
        ndk {
	    abiFilters 'x86', 'x86_64', 'armeabi-v7a', 'arm64-v8a'
	}
    }
    externalNativeBuild {
	cmake {
    	    path "CMakeLists.txt"
	}
    }
}

task mkdir {
    doLast {
        new File('build/generated-src/csharp').mkdirs()		// New folder called csharp instead of java
		new File('build/generated-src/cpp').mkdirs()
    }
}

def sphinxbase_home = "$System.env.SPINXBASE_HOME"		// The environment variable used to locate sphinxbase-master repository
def pocketsphinx_home = "$System.env.POCKETSPHINX_HOME"		// The environment variable used to locate pocketsphinx-master repository

task swigSb(type: Exec) {					// SphinxBase swig task to build the C# wrapper and the android native library '.so'
    commandLine 'swig',
	"-I$sphinxbase_home/include", "-I$sphinxbase_home/swig",
	"-csharp", "-dllimport", "android-pocketsphinx.so", "-namespace", "SphinxBase",
	"-outdir", "build/generated-src/csharp", "-o", "build/generated-src/cpp/sphinxbase_wrap.c",
	"$sphinxbase_home/swig/sphinxbase.i"
}

task swigPs(type: Exec) {					// PocketSphinx swig task to build the C# wrapper and the android native library '.so'
    commandLine 'swig',
        "-I$sphinxbase_home/swig",
        "-I$pocketsphinx_home/include",
        "-I$pocketsphinx_home/swig",
        "-csharp", "-dllimport", "android-pocketsphinx.so", "-namespace", "PocketSphinx",
        "-outdir", "build/generated-src/csharp", "-o", "build/generated-src/cpp/pocketsphinx_wrap.c",
        "$pocketsphinx_home/swig/pocketsphinx.i"
}

preBuild.dependsOn mkdir, swigSb, swigPs

- Now, try to run 'gradle build' in a command prompt from your pocketsphinx-android repository 
  (on Windows, type 'cmd' in the file path system to open the command prompt at the current location).

- If it ends up with errors about no "windows-x86_64" folder found ( C:\...\ndk-bundle\toolchains\llvm\prebuilt\windows-x86_64\bin\clang.exe), you will need to do some hacky changes to your Android NDK. Look at this link for more information : https://gitlab.kitware.com/cmake/cmake/issues/17253
If you are facing this issue, I recommand you to change the name of the folder at "C:\...\ndk-bundle\toolchains\llvm\prebuilt\windows" to "C:\...\ndk-bundle\toolchains\llvm\prebuilt\windows-x86_64" and so on if the issue appears again (at another location but for the same reason). 

- If your build succeeded, you will find :
	- The C# wrapper classes at C:\...\pocketsphinx-android-master\build\generated-src\csharp
	- The Android native libraries at C:\...\pocketsphinx-android-master\build\intermediates\bundles\default\jni. You will need to rename every '.so' library by the name you gave in the build.gradle file (because the C# wrapper classes will attempt to access your library with the name 'android-pocketsphinx.so' in my case).

- In Visual Studio, add these files in your Xamarin.Android project as follow : 
	- Paste the C# wrapper classes at any location and add missing SphinxBase namespace where it is required
	- Paste the "C:\...\pocketsphinx-android-master\build\intermediates\bundles\default\jni" folder at the root of the project,
          rename it 'lib' (this is important) and set the BuildAction property of every library to AndroidNativeLibrary.
	  (if you use Git, update your .gitignore file by running 'git add -f android-pocketsphinx.so' inside 'x86' folder location)

- If you need word of sentence recognition in your app, you will have to compile some other files. 
  Take a look at : https://github.com/BrunoMoureau/PocketSphinxXam/tree/master/Assets
  
- If you need advanced information, please rely on the PocketSphinx documentation : https://cmusphinx.github.io/

---------------------------------------------------------------------------------------

This speech recognition engine is under license : 

Copyright (c) 1999-2016 Carnegie Mellon University.  All rights
reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer. 

2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in
   the documentation and/or other materials provided with the
   distribution.

This work was supported in part by funding from the Defense Advanced 
Research Projects Agency and the National Science Foundation of the 
United States of America, and the CMU Sphinx Speech Consortium.

THIS SOFTWARE IS PROVIDED BY CARNEGIE MELLON UNIVERSITY ``AS IS'' AND 
ANY EXPRESSED OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL CARNEGIE MELLON UNIVERSITY
NOR ITS EMPLOYEES BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
