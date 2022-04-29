:: 使用说明：执行_PublishToUnity.bat后再执行此bat即可
md __UnityTemp
xcopy /S /Y ..\..\Publish\Project\Assets\StreamingAssets\*.* __UnityTemp\
del /S /Q __UnityTemp\*.meta
del __UnityTemp\__filelist.txt
del __UnityTemp\__version.txt
del __UnityTemp\web.config
del /Q __UnityTemp\*.bytes
..\..\EntryBuilder BuildEncrypt __UnityTemp ..\..\Publish\Project\Assets\StreamingAssets
rd /S /Q __UnityTemp