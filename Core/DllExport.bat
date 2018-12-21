@echo off
:: Copyright (c) 2016-2018  Denis Kuzmin [ entry.reg@gmail.com ]
:: https://github.com/3F/DllExport
if "%~1"=="/?" goto bk
set "aa=%~dpnx0"
set ab=%*
set ac=%*
if defined ab (
if defined __p_call (
set ac=%ac:^^=^%
) else (
set ab=%ab:^=^^%
)
)
set wMgrArgs=%ac%
set ad=%ab:!=^!%
setlocal enableDelayedExpansion
set "ae=^"
set "ad=!ad:%%=%%%%!"
set "ad=!ad:&=%%ae%%&!"
set "af=1.6.1"
set "wAction="
set "ag=DllExport"
set "ah=tools/net.r_eg.DllExport.Wizard.targets"
set "ai=packages"
set "aj=https://www.nuget.org/api/v2/package/"
set "ak=build_info.txt"
set "al=!aa!"
set "wRootPath=!cd!"
set "am="
set "an="
set "ao="
set "ap="
set "aq="
set "ar="
set "as="
set "at="
set "au="
set /a av=0
if not defined ab (
if defined wAction goto bl
goto bk
)
call :bm bf !ad! bg
goto bn
:bk
echo.
@echo DllExport - v1.6.1.38530 [ 2488b0c ]
@echo Copyright (c) 2009-2015  Robert Giesecke
@echo Copyright (c) 2016-2018  Denis Kuzmin [ entry.reg@gmail.com ] :: github.com/3F
echo.
echo Distributed under the MIT license
@echo https://github.com/3F/DllExport
echo.
echo Based on hMSBuild and includes GetNuTool core: https://github.com/3F
echo.
@echo.
@echo Usage: DllExport [args to DllExport] [args to GetNuTool core]
echo ------
echo.
echo Arguments:
echo ----------
echo  -action {type} - Specified action for Wizard. Where {type}:
echo       * Configure - To configure DllExport for specific projects.
echo       * Update    - To update pkg reference for already configured projects.
echo       * Restore   - To restore configured DllExport.
echo       * Export    - To export configured projects data.
echo       * Recover   - To re-configure projects via predefined/exported data.
echo       * Unset     - To unset all data from specified projects.
echo       * Upgrade   - Aggregates an Update action with additions for upgrading.
echo.
echo  -sln-dir {path}    - Path to directory with .sln files to be processed.
echo  -sln-file {path}   - Optional predefined .sln file to be processed.
echo  -metalib {path}    - Relative path from PkgPath to DllExport meta library.
echo  -dxp-target {path} - Relative path to entrypoint wrapper of the main core.
echo  -dxp-version {num} - Specific version of DllExport. Where {num}:
echo       * Versions: 1.6.0 ...
echo       * Keywords:
echo         `actual` - Unspecified local/latest remote version;
echo                    ( Only if you know what you are doing )
echo.
echo  -msb {path}           - Full path to specific msbuild.
echo  -packages {path}      - A common directory for packages.
echo  -server {url}         - Url for searching remote packages.
echo  -proxy {cfg}          - To use proxy. The format: [usr[:pwd]@]host[:port]
echo  -pkg-link {uri}       - Direct link to package from the source via specified URI.
echo  -force                - Aggressive behavior, e.g. like removing pkg when updating.
echo  -mgr-up               - Updates this manager to version from '-dxp-version'.
echo  -wz-target {path}     - Relative path to entrypoint wrapper of the main wizard.
echo  -pe-exp-list {module} - To list all available exports from PE32/PE32+ module.
echo  -eng                  - Try to use english language for all build messages.
echo  -GetNuTool {args}     - Access to GetNuTool core. https://github.com/3F/GetNuTool
echo  -debug                - To show additional information.
echo  -version              - Displays version for which (together with) it was compiled.
echo  -build-info           - Displays actual build information from selected DllExport.
echo  -help                 - Displays this help. Aliases: -help -h
echo.
echo ------
echo Flags:
echo ------
echo  __p_call - To use the call-type logic when invoking %~nx0
echo.
echo --------
echo Samples:
echo --------
echo  DllExport -action Configure
echo  DllExport -action Restore -sln-file "Conari.sln"
echo  DllExport -proxy guest:1234@10.0.2.15:7428 -action Configure
echo  DllExport -action Configure -force -pkg-link http://host/v1.6.1.nupkg
echo.
echo  DllExport -build-info
echo  DllExport -debug -restore -sln-dir ..\
echo  DllExport -mgr-up -dxp-version 1.6.1
echo  DllExport -action Upgrade -dxp-version 1.6.1
echo.
echo  DllExport -GetNuTool -unpack
echo  DllExport -GetNuTool /p:ngpackages="Conari;regXwild"
echo  DllExport -pe-exp-list bin\Debug\regXwild.dll
goto bo
:bn
set /a aw=0
:bp
set ax=!bf[%aw%]!
if [!ax!]==[-help] ( goto bk ) else if [!ax!]==[-h] ( goto bk ) else if [!ax!]==[-?] ( goto bk )
if [!ax!]==[-debug] (
set am=1
goto bq
) else if [!ax!]==[-action] ( set /a "aw+=1" & call :br bf[!aw!] v
set wAction=!v!
for %%g in (Restore, Configure, Update, Export, Recover, Unset, Upgrade, Default) do (
if "!v!"=="%%g" goto bq
)
echo Unknown -action !v!
exit/B 1
) else if [!ax!]==[-sln-dir] ( set /a "aw+=1" & call :br bf[!aw!] v
set wSlnDir=!v!
goto bq
) else if [!ax!]==[-sln-file] ( set /a "aw+=1" & call :br bf[!aw!] v
set wSlnFile=!v!
goto bq
) else if [!ax!]==[-metalib] ( set /a "aw+=1" & call :br bf[!aw!] v
set wMetaLib=!v!
goto bq
) else if [!ax!]==[-dxp-target] ( set /a "aw+=1" & call :br bf[!aw!] v
set wDxpTarget=!v!
goto bq
) else if [!ax!]==[-dxp-version] ( set /a "aw+=1" & call :br bf[!aw!] v
set af=!v!
goto bq
) else if [!ax!]==[-msb] ( set /a "aw+=1" & call :br bf[!aw!] v
set ao=!v!
goto bq
) else if [!ax!]==[-packages] ( set /a "aw+=1" & call :br bf[!aw!] v
set ai=!v!
goto bq
) else if [!ax!]==[-server] ( set /a "aw+=1" & call :br bf[!aw!] v
set aj=!v!
goto bq
) else if [!ax!]==[-proxy] ( set /a "aw+=1" & call :br bf[!aw!] v
set at=!v!
goto bq
) else if [!ax!]==[-pkg-link] ( set /a "aw+=1" & call :br bf[!aw!] v
set ap=!v!
goto bq
) else if [!ax!]==[-force] (
set ar=1
goto bq
) else if [!ax!]==[-mgr-up] (
set as=1
goto bq
) else if [!ax!]==[-wz-target] ( set /a "aw+=1" & call :br bf[!aw!] v
set ah=!v!
goto bq
) else if [!ax!]==[-pe-exp-list] ( set /a "aw+=1" & call :br bf[!aw!] v
set aq=!v!
goto bq
) else if [!ax!]==[-eng] (
chcp 437 >nul
goto bq
) else if [!ax!]==[-GetNuTool] (
call :bs "accessing to GetNuTool ..."
for /L %%p IN (0,1,8181) DO (
if "!ay:~%%p,10!"=="-GetNuTool" (
set az=!ay:~%%p!
call :bt !az:~10!
set /a av=%ERRORLEVEL%
goto bo
)
)
call :bs "!ax! is corrupted: !ay!"
set /a av=1
goto bo
) else if [!ax!]==[-version] (
@echo v1.6.1.38530 [ 2488b0c ]
goto bo
) else if [!ax!]==[-build-info] (
set an=1
goto bq
) else if [!ax!]==[-tests] ( set /a "aw+=1" & call :br bf[!aw!] v
set au=!v!
goto bq
) else (
echo Incorrect key: !ax!
set /a av=1
goto bo
)
:bq
set /a "aw+=1" & if %aw% LSS !bg! goto bp
:bl
call :bs "dxpName = " ag
call :bs "dxpVersion = " af
call :bs "-sln-dir = " wSlnDir
call :bs "-sln-file = " wSlnFile
call :bs "-metalib = " wMetaLib
call :bs "-dxp-target = " wDxpTarget
call :bs "-wz-target = " ah
if defined af (
if "!af!"=="actual" (
set "af="
)
)
if z%wAction%==zUpgrade (
call :bs "Upgrade is on"
set as=1
set ar=1
)
call :bu ai
set "ai=!ai!\\"
set "a0=!ag!"
set "wPkgPath=!ai!!ag!"
if defined af (
set "a0=!a0!/!af!"
set "wPkgPath=!wPkgPath!.!af!"
)
if defined ar (
if exist "!wPkgPath!" (
call :bs "Removing old version before continue. '-force' key rule. " wPkgPath
rmdir /S/Q "!wPkgPath!"
)
)
set a1="!wPkgPath!\\!ah!"
call :bs "wPkgPath = " wPkgPath
if not exist !a1! (
if exist "!wPkgPath!" (
call :bs "Trying to replace obsolete version ... " wPkgPath
rmdir /S/Q "!wPkgPath!"
)
call :bs "-pkg-link = " ap
call :bs "-server = " aj
if defined ap (
set aj=!ap!
if "!aj::=!"=="!aj!" (
set aj=!cd!/!aj!
)
if "!wPkgPath::=!"=="!wPkgPath!" (
set "a2=../"
)
set "a0=:!a2!!wPkgPath!|"
)
if defined ao (
set a3=-msbuild "!ao!"
)
set a4=!a3! /p:ngserver="!aj!" /p:ngpackages="!a0!" /p:ngpath="!ai!" /p:proxycfg="!at!"
call :bs "GetNuTool call: " a4
if defined am (
call :bt !a4!
) else (
call :bt !a4! >nul
)
)
if defined aq (
"!wPkgPath!\\tools\\PeViewer.exe" -list -pemodule "!aq!"
set /a av=%ERRORLEVEL%
goto bo
)
if defined an (
call :bs "buildInfo = " wPkgPath ak
if not exist "!wPkgPath!\\!ak!" (
echo information about build is not available.
set /a av=2
goto bo
)
type "!wPkgPath!\\!ak!"
goto bo
)
if not exist !a1! (
echo Something went wrong. Try to use another keys.
set /a av=2
goto bo
)
call :bs "wRootPath = " wRootPath
call :bs "wAction = " wAction
call :bs "wMgrArgs = " wMgrArgs
if defined ao (
call :bs "Use specific MSBuild tools: " ao
set a5="!ao!"
goto bv
)
call :bw bh & set a5="!bh!"
if "!ERRORLEVEL!"=="0" goto bv
echo MSBuild tools was not found. Try with `-msb` key.
set /a av=2
goto bo
:bv
if not defined a5 (
echo Something went wrong. Use `-debug` key for details.
set /a av=2
goto bo
)
if not defined au (
call :bs "Target: " a5 a1
!a5! /nologo /v:m /m:4 !a1!
)
:bo
if defined au (
echo Running Tests ... "!au!"
call :bw bi
"!bi!" /nologo /v:m /m:4 "!au!"
exit/B 0
)
if defined as (
(copy /B/Y "!wPkgPath!\\DllExport.bat" "!al!" > nul) && ( echo Manager has been updated. & exit/B !av! ) || ( echo -mgr-up failed. & exit/B %ERRORLEVEL% )
)
exit/B !av!
:bw
call :bs "trying via MSBuild tools from .NET Framework - .net 4.0, ..."
for %%v in (4.0, 3.5, 2.0) do (
call :bx %%v Y & if defined Y (
set %1=!Y!
exit/B 0
)
)
call :bs "msbnetf: unfortunately we didn't find anything."
exit/B 2
:bx
call :bs "checking of version: %1"
for /F "usebackq tokens=2* skip=2" %%a in (
`reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\%1" /v MSBuildToolsPath 2^> nul`
) do if exist %%b (
call :bs "found: %%~b"
call :by "%%~b" bj
set %2=!bj!
exit/B 0
)
exit/B 0
:by
set %2=%~1\MSBuild.exe
exit/B 0
:bs
if defined am (
set a6=%1
set a6=!a6:~0,-1!
set a6=!a6:~1!
echo.[%TIME% ] !a6! !%2! !%3!
)
exit/B 0
:bu
call :bz %1
call :b0 %1
exit/B 0
:bz
call :b1 %1 "-=1"
exit/B 0
:b0
call :b1 %1 "+=1"
exit/B 0
:b1
set a7=z!%1!z
if "%~2"=="-=1" (set "a8=1") else (set "a8=")
if defined a8 (
set /a "i=-2"
) else (
set /a "i=1"
)
:b2
if "!a7:~%i%,1!"==" " (
set /a "i%~2"
goto b2
)
if defined a8 set /a "i+=1"
if defined a8 (
set "%1=!a7:~1,%i%!"
) else (
set "%1=!a7:~%i%,-1!"
)
exit/B 0
:bm
set "a9=%~1"
set /a aw=-1
:b3
set /a aw+=1
set %a9%[!aw!]=%~2
shift & if not "%~3"=="" goto b3
set /a aw-=1
set %1=!aw!
exit/B 0
:br
set %2=!%1!
exit/B 0
:bt
setlocal disableDelayedExpansion
@echo off
:: GetNuTool - Executable version
:: Copyright (c) 2015-2018  Denis Kuzmin [ entry.reg@gmail.com ]
:: https://github.com/3F/GetNuTool
set a_=gnt.core
set ba="%temp%\%random%%random%%a_%"
if "%~1"=="-unpack" goto b4
set bb=%*
if defined __p_call if defined bb set bb=%bb:^^=^%
set bc=%__p_msb%
if defined bc goto b5
if "%~1"=="-msbuild" goto b6
for %%v in (4.0, 14.0, 12.0, 3.5, 2.0) do (
for /F "usebackq tokens=2* skip=2" %%a in (
`reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\%%v" /v MSBuildToolsPath 2^> nul`
) do if exist %%b (
set bc="%%~b\MSBuild.exe"
goto b5
)
)
echo MSBuild was not found. Try -msbuild "fullpath" args 1>&2
exit/B 2
:b6
shift
set bc=%1
shift
set bd=%bb:!= #__b_ECL## %
setlocal enableDelayedExpansion
set bd=!bd:%%=%%%%!
:b7
for /F "tokens=1* delims==" %%a in ("!bd!") do (
if "%%~b"=="" (
call :b8 !bd!
exit/B %ERRORLEVEL%
)
set bd=%%a #__b_EQ## %%b
)
goto b7
:b8
shift & shift
set "bb="
:b9
set bb=!bb! %1
shift & if not "%~2"=="" goto b9
set bb=!bb: #__b_EQ## ==!
setlocal disableDelayedExpansion
set bb=%bb: #__b_ECL## =!%
:b5
call :b_
%bc% %ba% /nologo /p:wpath="%~dp0/" /v:m /m:4 %bb%
set "bc="
set be=%ERRORLEVEL%
del /Q/F %ba%
exit/B %be%
:b4
set ba="%~dp0\%a_%"
echo Generating minified version in %ba% ...
:b_
<nul set /P ="">%ba%
set a=PropertyGroup&set b=Condition&set c=ngpackages&set d=Target&set e=DependsOnTargets&set f=TaskCoreDllPath&set g=MSBuildToolsPath&set h=UsingTask&set i=CodeTaskFactory&set j=ParameterGroup&set k=Reference&set l=Include&set m=System&set n=Using&set o=Namespace&set p=IsNullOrEmpty&set q=return&set r=string&set s=delegate&set t=foreach&set u=WriteLine&set v=Combine&set w=Console.WriteLine&set x=Directory&set y=GetNuTool&set z=StringComparison&set _=EXT_NUSPEC
<nul set /P =^<!-- GetNuTool - github.com/3F/GetNuTool --^>^<!-- Copyright (c) 2015-2018  Denis Kuzmin [ entry.reg@gmail.com ] --^>^<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003"^>^<%a%^>^<ngconfig %b%="'$(ngconfig)'==''"^>packages.config^</ngconfig^>^<ngserver %b%="'$(ngserver)'==''"^>https://www.nuget.org/api/v2/package/^</ngserver^>^<%c% %b%="'$(%c%)'==''"^>^</%c%^>^<ngpath %b%="'$(ngpath)'==''"^>packages^</ngpath^>^</%a%^>^<%d% Name="get" BeforeTargets="Build" %e%="header"^>^<a^>^<Output PropertyName="plist" TaskParameter="Result"/^>^</a^>^<b plist="$(plist)"/^>^</%d%^>^<%d% Name="pack" %e%="header"^>^<c/^>^</%d%^>^<%a%^>^<%f% %b%="Exists('$(%g%)\Microsoft.Build.Tasks.v$(MSBuildToolsVersion).dll')"^>$(%g%)\Microsoft.Build.Tasks.v$(MSBuildToolsVersion).dll^</%f%^>^<%f% %b%="'$(%f%)'=='' and Exists('$(%g%)\Microsoft.Build.Tasks.Core.dll')"^>$(%g%)\Microsoft.Build.Tasks.Core.dll^</%f%^>^</%a%^>^<%h% TaskName="a" TaskFactory="%i%" AssemblyFile="$(%f%)"^>^<%j%^>^<Result Output="true"/^>^</%j%^>^<Task^>^<%k% %l%="%m%.Xml"/^>^<%k% %l%="%m%.Xml.Linq"/^>^<%n% %o%="%m%"/^>^<%n% %o%="%m%.Collections.Generic"/^>^<%n% %o%="%m%.IO"/^>^<%n% %o%="%m%.Xml.Linq"/^>^<Code Type="Fragment" Language="cs"^>^<![CDATA[var a=@"$(ngconfig)";var b=@"$(%c%)";var c=@"$(wpath)";if(!String.%p%(b)){Result=b;%q% true;}var d=Console.Error;Action^<%r%,Queue^<%r%^>^>e=%s%(%r% f,Queue^<%r%^>g){%t%(var h in XDocument.Load(f).Descendants("package")){var i=h.Attribute("id");var j=h.Attribute("version");var k=h.Attribute("output");if(i==null){d.%u%("'id' does not exist in '{0}'",f);%q%;}var l=i.Value;if(j!=null){l+="/"+j.Value;}if(k!=null){g.Enqueue(l+":"+k.Value);continue;}g.Enqueue(l);}};var m=new Queue^<%r%^>();%t%(var f in a.Split(new char[]{a.IndexOf('^|')!=-1?'^|':';'},(StringSplitOptions)1)){>>%ba%
<nul set /P =var n=Path.%v%(c,f);if(File.Exists(n)){e(n,m);}else{d.%u%(".config '{0}' was not found.",n);}}if(m.Count^<1){d.%u%("Empty list. Use .config or /p:%c%=\"...\"\n");}else{Result=%r%.Join("|",m.ToArray());}]]^>^</Code^>^</Task^>^</%h%^>^<%h% TaskName="b" TaskFactory="%i%" AssemblyFile="$(%f%)"^>^<%j%^>^<plist/^>^</%j%^>^<Task^>^<%k% %l%="WindowsBase"/^>^<%n% %o%="%m%"/^>^<%n% %o%="%m%.IO"/^>^<%n% %o%="%m%.IO.Packaging"/^>^<%n% %o%="%m%.Net"/^>^<Code Type="Fragment" Language="cs"^>^<![CDATA[var a=@"$(ngserver)";var b=@"$(wpath)";var c=@"$(ngpath)";var d=@"$(proxycfg)";var e=@"$(debug)"=="true";if(plist==null){%q% false;}ServicePointManager.SecurityProtocol^|=SecurityProtocolType.Tls11^|SecurityProtocolType.Tls12;var f=new %r%[]{"/_rels/","/package/","/[Content_Types].xml"};Action^<%r%,object^>g=%s%(%r% h,object i){if(e){%w%(h,i);}};Func^<%r%,WebProxy^>j=%s%(%r% k){var l=k.Split('@');if(l.Length^<=1){%q% new WebProxy(l[0],false);}var m=l[0].Split(':');%q% new WebProxy(l[1],false){Credentials=new NetworkCredential(m[0],(m.Length^>1)?m[1]:null)};};Func^<%r%,%r%^>n=%s%(%r% i){%q% Path.%v%(b,i??"");};Action^<%r%,%r%,%r%^>o=%s%(%r% p,%r% q,%r% r){var s=Path.GetFullPath(n(r??q));if(%x%.Exists(s)){%w%("`{0}` is already exists: \"{1}\"",q,s);%q%;}Console.Write("Getting `{0}` ... ",p);var t=Path.%v%(Path.GetTempPath(),Guid.NewGuid().ToString());using(var u=new WebClient()){try{if(!String.%p%(d)){u.Proxy=j(d);}u.Headers.Add("User-Agent","%y% $(%y%)");u.UseDefaultCredentials=true;u.DownloadFile(a+p,t);}catch(Exception v){Console.Error.%u%(v.Message);%q%;}}%w%("Extracting into \"{0}\"",s);using(var w=ZipPackage.Open(t,FileMode.Open,FileAccess.Read)){%t%(var x in w.GetParts()){var y=Uri.UnescapeDataString(x.Uri.OriginalString);if(f.Any(z=^>y.StartsWith(z,%z%.Ordinal))){continue;}var _=Path.%v%(s,y.TrimStart(>>%ba%
<nul set /P ='/'));g("- `{0}`",y);var aa=Path.GetDirectoryName(_);if(!%x%.Exists(aa)){%x%.CreateDirectory(aa);}using(Stream ab=x.GetStream(FileMode.Open,FileAccess.Read))using(var ac=File.OpenWrite(_)){try{ab.CopyTo(ac);}catch(FileFormatException v){g("[x]?crc: {0}",_);}}}}File.Delete(t);};%t%(var w in plist.Split(new char[]{plist.IndexOf('^|')!=-1?'^|':';'},(StringSplitOptions)1)){var ad=w.Split(new char[]{':'},2);var p=ad[0];var r=(ad.Length^>1)?ad[1]:null;var q=p.Replace('/','.');if(!String.%p%(c)){r=Path.%v%(c,r??q);}o(p,q,r);}]]^>^</Code^>^</Task^>^</%h%^>^<%h% TaskName="c" TaskFactory="%i%" AssemblyFile="$(%f%)"^>^<Task^>^<%k% %l%="%m%.Xml"/^>^<%k% %l%="%m%.Xml.Linq"/^>^<%k% %l%="WindowsBase"/^>^<%n% %o%="%m%"/^>^<%n% %o%="%m%.Collections.Generic"/^>^<%n% %o%="%m%.IO"/^>^<%n% %o%="%m%.Linq"/^>^<%n% %o%="%m%.IO.Packaging"/^>^<%n% %o%="%m%.Xml.Linq"/^>^<%n% %o%="%m%.Text.RegularExpressions"/^>^<Code Type="Fragment" Language="cs"^>^<![CDATA[var a=@"$(ngin)";var b=@"$(ngout)";var c=@"$(wpath)";var d=@"$(debug)"=="true";var %_%=".nuspec";var EXT_NUPKG=".nupkg";var TAG_META="metadata";var DEF_CONTENT_TYPE="application/octet";var MANIFEST_URL="http://schemas.microsoft.com/packaging/2010/07/manifest";var ID="id";var VER="version";Action^<%r%,object^>e=%s%(%r% f,object g){if(d){%w%(f,g);}};var h=Console.Error;a=Path.%v%(c,a);if(!%x%.Exists(a)){h.%u%("`{0}` was not found.",a);%q% false;}b=Path.%v%(c,b);var i=%x%.GetFiles(a,"*"+%_%,SearchOption.TopDirectoryOnly).FirstOrDefault();if(i==null){h.%u%("{0} was not found in `{1}`",%_%,a);%q% false;}%w%("Found {0}: `{1}`",%_%,i);var j=XDocument.Load(i).Root.Elements().FirstOrDefault(k=^>k.Name.LocalName==TAG_META);if(j==null){h.%u%("{0} does not contain {1}.",i,TAG_META);%q% false;}var l=new Dictionary^<%r%,%r%^>();%t%(var m in j.Elements()){l[m.Name.LocalName.ToL>>%ba%
<nul set /P =ower()]=m.Value;}if(l[ID].Length^>100^|^|!Regex.IsMatch(l[ID],@"^\w+([_.-]\w+)*$",RegexOptions.IgnoreCase^|RegexOptions.ExplicitCapture)){h.%u%("The format of `{0}` is not correct.",ID);%q% false;}var n=new %r%[]{Path.%v%(a,"_rels"),Path.%v%(a,"package"),Path.%v%(a,"[Content_Types].xml")};var o=%r%.Format("{0}.{1}{2}",l[ID],l[VER],EXT_NUPKG);if(!String.IsNullOrWhiteSpace(b)){if(!%x%.Exists(b)){%x%.CreateDirectory(b);}o=Path.%v%(b,o);}%w%("Started packing `{0}` ...",o);using(var p=Package.Open(o,FileMode.Create)){Uri q=new Uri(String.Format("/{0}{1}",l[ID],%_%),UriKind.Relative);p.CreateRelationship(q,TargetMode.Internal,MANIFEST_URL);%t%(var r in %x%.GetFiles(a,"*.*",SearchOption.AllDirectories)){if(n.Any(k=^>r.StartsWith(k,%z%.Ordinal))){continue;}%r% s;if(r.StartsWith(a,%z%.OrdinalIgnoreCase)){s=r.Substring(a.Length).TrimStart(Path.DirectorySeparatorChar);}else{s=r;}e("- `{0}`",s);var t=%r%.Join("/",s.Split('\\','/').Select(g=^>Uri.EscapeDataString(g)));Uri u=PackUriHelper.CreatePartUri(new Uri(t,UriKind.Relative));var v=p.CreatePart(u,DEF_CONTENT_TYPE,CompressionOption.Maximum);using(Stream w=v.GetStream())using(var x=new FileStream(r,FileMode.Open,FileAccess.Read)){x.CopyTo(w);}}Func^<%r%,%r%^>y=%s%(%r% z){%q%(l.ContainsKey(z))?l[z]:"";};var _=p.PackageProperties;_.Creator=y("authors");_.Description=y("description");_.Identifier=l[ID];_.Version=l[VER];_.Keywords=y("tags");_.Title=y("title");_.LastModifiedBy="%y% $(%y%)";}]]^>^</Code^>^</Task^>^</%h%^>^<%d% Name="Build" %e%="get"/^>^<%a%^>^<%y%^>1.6.2.52508_a200982^</%y%^>^<wpath %b%="'$(wpath)'==''"^>$(MSBuildProjectDirectory)^</wpath^>^</%a%^>^<%d% Name="header"^>^<Message Text="%%0D%%0A%y% $(%y%) - github.com/3F%%0D%%0A=========%%0D%%0A" Importance="high"/^>^</%d%^>^</Project^>>>%ba%
exit/B 0