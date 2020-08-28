# Design Automation and Azure Function as Service

![Platforms](https://img.shields.io/badge/platform-Windows-blue.svg)
![.NET](https://img.shields.io/badge/.NET%20Core-3.1-blue.svg)
[![ASP.NET](https://img.shields.io/badge/.NET-4.8-blue.svg)](https://docs.microsoft.com/en-us/dotnet/framework/)
[![License](http://img.shields.io/:license-mit-blue.svg)](http://opensource.org/licenses/MIT)

# Setup

## Prerequisites

1. **Forge Account**: Learn how to create a Forge Account, activate subscription and create an app at [this tutorial](http://learnforge.autodesk.io/#/account/).
2. **.NET Core** basic knowledge with C#
3. [AutoCAD API](https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-C3F3C736-40CF-44A0-9210-55F6A939B6F2) 
4. [7z](https://www.7-zip.org/download.html)
   1. Use for zipping AutoCAD Design Automation Bundle.
   2.  Make sure `7z.exe` is available on `PATH` environment.

## Running Locally
For Visual Studio, go to project `daConsole`properties and specify the environment variables. 

- `FORGE_CLIENT_ID`: your Forge Client ID
- `FORGE_CLIENT_SECRET`: your Forge Client Secret


Compile the solution, Visual Studio should download the NUGET packages ([Autodesk Forge Design Automation](https://www.nuget.org/packages/Autodesk.Forge.DesignAutomation) and [Newtonsoft.Json](https://www.nuget.org/packages/newtonsoft.json/))

```
git clone https://github.com/MadhukarMoogala/da-azfunc.git
cd da-azfunc
devenv ExtractLength.sln
build solution
```
### Expected Compiled Output

```
>------ Build started: Project: ExtractLength, Configuration: Debug x64 ------
2>------ Build started: Project: ReadJson, Configuration: Debug x64 ------
3>------ Build started: Project: daconsole, Configuration: Debug x64 ------
1>  ExtractLength -> D:\Temp\ExtractLength\ExtractLength\bin\x64\Debug\ExtractLength.dll
1>  D:\Temp\ExtractLength\ExtractLength\bin\x64\Debug\ExtractLength.dll
1>  D:\Temp\ExtractLength\ExtractLength\bin\x64\Debug\Newtonsoft.Json.dll
1>  2 File(s) copied
1>  
1>  7-Zip 19.00 (x64) : Copyright (c) 1999-2018 Igor Pavlov : 2019-02-21
1>  
1>  Scanning the drive:
1>  2 folders, 3 files, 709218 bytes (693 KiB)
1>  
1>  Creating archive: D:\Temp\ExtractLength\daconsole\package.zip
1>  
1>  Add new data to archive: 2 folders, 3 files, 709218 bytes (693 KiB)
1>  
1>  
1>  Files read from disk: 3
1>  Archive size: 258074 bytes (253 KiB)
1>  Everything is Ok
1>  
1>  7-Zip 19.00 (x64) : Copyright (c) 1999-2018 Igor Pavlov : 2019-02-21
1>  
1>  Scanning the drive for archives:
1>  1 file, 258074 bytes (253 KiB)
1>  
1>  Listing archive: D:\Temp\ExtractLength\daconsole\package.zip
1>  
1>  --
1>  Path = D:\Temp\ExtractLength\daconsole\package.zip
1>  Type = zip
1>  Physical Size = 258074
1>  
1>     Date      Time    Attr         Size   Compressed  Name
1>  ------------------- ----- ------------ ------------  ------------------------
1>  2020-08-27 21:58:48 D....            0            0  extractLength.bundle
1>  2020-08-27 20:44:45 D....            0            0  extractLength.bundle\Contents
1>  2020-08-28 14:53:54 ....A         7680         3395  extractLength.bundle\Contents\extractLength.dll
1>  2020-08-18 19:27:12 ....A       700336       253170  extractLength.bundle\Contents\Newtonsoft.Json.dll
1>  2020-08-27 21:58:48 ....A         1202          553  extractLength.bundle\PackageContents.xml
1>  ------------------- ----- ------------ ------------  ------------------------
1>  2020-08-28 14:53:54             709218       257118  3 files, 2 folders
2>ReadJson -> D:\Temp\ExtractLength\ReadJson\bin\x64\Debug\netcoreapp3.1\bin\ReadJson.dll
3>daconsole -> D:\Temp\ExtractLength\daconsole\bin\x64\Debug\netcoreapp3.1\daconsole.dll
========== Build: 3 succeeded, 0 failed, 0 up-to-date, 0 skipped ==========
```

# Demo

[![asciicast](https://asciinema.org/a/LMnLrqt7BHPVmgQM5mjjODTFH.svg)](https://asciinema.org/a/LMnLrqt7BHPVmgQM5mjjODTFH)




# Further Reading

Documentation:

- [Design Automation v3](https://forge.autodesk.com/en/docs/design-automation/v3/developers_guide/overview/)
- [Learn Forge Tutorial](https://learnforge.autodesk.io/#/tutorials/modifymodels)

Other APIs:

- [Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-your-first-function-visual-studio)

## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](https://github.com/MadhukarMoogala/design-migration/blob/master/LICENSE) file for full details.

## Written by

Madhukar Moogala, [Forge Partner Development](http://forge.autodesk.com/) @galakar