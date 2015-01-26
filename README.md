# Crop Healer for Umbraco #

![](CropHealer.png)

[![Build status](https://ci.appveyor.com/api/projects/status/roiahbr11qae79k8?svg=true)](https://ci.appveyor.com/project/JeavonLeopold/umbraco-crop-healer)

This package adds and event handler to the save event of a data type to ensure changes to crops are propagated to content, media and members.

Please note that if you have a used a particular cropper data type on a lot of items then saving the data type may take some time to complete. You can check on progress by looking at the log4net log file either directly or by using a viewer such as the excellent [Diplo Trace Log Viewer](http://our.umbraco.org/projects/developer-tools/diplo-trace-log-viewer) (search for "CropHealer" in the Logger field)

![LogViewer](https://raw.githubusercontent.com/Jeavon/Umbraco-Crop-Healer/master/Docs/LogViewer.jpg)

## Downloads ##

### Release ###

- [NuGet](https://www.nuget.org/packages/Our.Umbraco.CropHealer/) - the latest release version for **NuGet**
- [Umbraco Package](http://our.umbraco.org/projects/collaboration/crop-healer-for-umbraco) - the latest release version for **Umbraco**

### Pre-release ###

- [MyGet](https://www.myget.org/gallery/umbraco-crop-healer) for the very latest **NuGet** package built fresh from source code
- [AppVeyor](https://ci.appveyor.com/project/JeavonLeopold/umbraco-crop-healer/build/artifacts) - for the very latest **Umbraco** package built fresh from source code (download the Zip archive)


## Test Site ##

The username and password for the test site are admin/password

## Roadmap ##

- v0.1-beta Attach to data type save event and propagate changes to content, media & members
- v0.2-beta Add dashboard control to allow for a specific data type to be healed on request and make data type save event optional
- v1.0 Release

This project is [MIT](http://opensource.org/licenses/mit-license.php) licensed