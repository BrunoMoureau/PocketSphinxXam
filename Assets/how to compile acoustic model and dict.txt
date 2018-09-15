- Update your build.gradle file with your Android SDK version
- Put the acoustic model and the associated dictionnary in -> \src\main\assets\sync\
- Run 'gradle build' command
- Check the \src\main\assets\sync\ folder. You should have a new file named 'assets.lst' and a bunch of .md5 files
- Put the \sync folder in the assets directory of your Xamarin.Android project

FYI : 
- 'assets.lst' is used to list all files that will be written on your device
- md5 files is used to check if your file has been changed and, if so, it is replaced by the new one on your device
