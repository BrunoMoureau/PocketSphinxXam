I built this for Xamarin.Android on Windows 10 and I am not used to Mac or other operating systems.

The first thing you need to do is to download several files and tools to perform the compilation. 
Here are the download links :

IDE
-----------------------------------------------------------------------------
Android Studio - https://developer.android.com/studio/ 

Projects
-----------------------------------------------------------------------------
SphinxBase - https://github.com/cmusphinx/sphinxbase
PocketSphinx - https://github.com/cmusphinx/pocketsphinx 
PocketSphinx-Android - https://github.com/cmusphinx/pocketsphinx-android 

Tools
-----------------------------------------------------------------------------
Gradle - https://gradle.org/install/
Swig - http://www.swig.org/download.html
Java SDK - http://www.oracle.com/technetwork/java/javase/downloads/index.html

- Run Android Studio and download the last Android SDK, CMake and Android NDK using Tools > SDK Manager (you may also need Android SDK Built-Tools).
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

- Add a new text file to pocketsphinx-android-master repository with path to your Android SDK and NDK as follow :
	sdk.dir=C\:\\androidstudio-sdk
	ndk.dir=C\:\\androidstudio-sdk\\ndk-bundle

- Update the pocketsphinx-android-master build.gradle file. Keep this one as reference (please look at comments) :

// Start of the build.gradle content

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

// End of the build.gradle content

- Now, try to run 'gradle build' in a command prompt from your pocketsphinx-android repository 
  (on Windows, type 'cmd' in the file path system to open the command prompt in the current location).

- If it ends up with error about no "windows-x86_64" folder found ( C:\...\ndk-bundle\toolchains\llvm\prebuilt\windows-x86_64\bin\clang.exe), 
  you will need to do some hacky changes to your Android NDK. Look at this link for more information : https://gitlab.kitware.com/cmake/cmake/issues/17253
  If you are facing this problem, I recommand you to change the name of the folder at "C:\...\ndk-bundle\toolchains\llvm\prebuilt\windows" 
  into "C:\...\ndk-bundle\toolchains\llvm\prebuilt\windows-x86_64" and so on if the error appears again (at another location but for the same reason). 

- If your build succeeded, you will find :
	- The C# wrapper classes at C:\...\pocketsphinx-android-master\build\generated-src\csharp
	- The Android native libraries at C:\...\pocketsphinx-android-master\build\intermediates\bundles\default\jni. You will need to rename
          every '.so' library by the name you gave in the build.gradle file (because the C# wrapper classes will attempt to access your library 
	  with the name 'android-pocketsphinx.so' in my case).

- In Visual Studio, add these files in your Xamarin.Android project as follow : 
	- Paste the C# wrapper classes at any location and add missing SphinxBase namespace where it is required
	- Paste the C:\...\pocketsphinx-android-master\build\intermediates\bundles\default\jni folder at the root of the project,
          rename it 'lib' and set the BuildAction property of every library to AndroidNativeLibrary.
	  (if you use Git, update your .gitignore file by running 'git add -f android-pocketsphinx.so' inside 'x86' folder location)

- If you need word of sentence recognition in your app, you will have to compile some other files. Take a look at 'Assets/README.text' file in this repository.
- If you need advanced information, please rely on the PocketSphinx documentation : https://cmusphinx.github.io/