# Umbraco CropHealer #

[![Build status](https://ci.appveyor.com/api/projects/status/roiahbr11qae79k8?svg=true)](https://ci.appveyor.com/project/JeavonLeopold/umbraco-crop-healer)

This package adds and event handler to the save event of a data type to ensure changes to crops are propagated to content, media and members.

Please note that if you have a used a particular cropper data type on a lot of items then saving the data type may take some time to complete. You can check on progress by looking at the log4net log file either directly or by using a viewer such as the excellent [Diplo Trace Log Viewer](http://our.umbraco.org/projects/developer-tools/diplo-trace-log-viewer) (search for "CropHealer" in the Logger field)

![LogViewer](../docs/LogViewer.jpg)

## Test Site ##

The username and password for the test site are admin/password

This project is [MIT](http://opensource.org/licenses/mit-license.php) licensed