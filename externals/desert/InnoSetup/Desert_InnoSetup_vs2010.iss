; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

; Requires Inno Setup Preprocessor by Alex Yackimoff
#pragma option -v+
#pragma verboselevel 9

#define UDMPATH GetEnv('UDM_PATH')
#if UDMPATH == ""
#define UDMPATH "C:\Projects\UDM"
#endif

#define UDMDLL "UdmDll_3_2_VS10.dll"

#define DESERTPATH SourcePath + "\.."

#define DESERTVER 'v'+GetDateTimeString('y.mm.dd', '', '');

[Setup]
AppName=DESERT {#DESERTVER}
AppVerName=ISIS DESERT v{#DESERTVER}
AppVersion={#DESERTVER}
AppPublisher=ISIS, Vanderbilt University
DefaultDirName={pf}\ISIS\DESERT
DefaultGroupName=DESERT
OutputDir={#DESERTPATH}\InnoSetup
OutputBaseFilename=DESERT-{#DESERTVER}
Compression=lzma
SolidCompression=yes
ChangesEnvironment=true
AppCopyright=Copyright (C) 2009-2010 ISIS, Vanderbilt University

[Languages]
Name: english; MessagesFile: compiler:Default.isl

[Files]
Source: {#DESERTPATH}\bin\{#UDMDLL}; DestDir: {app}\bin
Source: {#DESERTPATH}\bin\xerces-c_2_8.dll; DestDir: {app}\bin
Source: {#DESERTPATH}\bin\DesertTool.exe; DestDir: {app}\bin
Source: {#DESERTPATH}\bin\desert.dll; DestDir: {app}\bin
Source: {#DESERTPATH}\bin\desertd.dll; DestDir: {app}\bin
Source: {#DESERTPATH}\lib\desert.lib; DestDir: {app}\lib
Source: {#DESERTPATH}\lib\desertd.lib; DestDir: {app}\lib
Source: {#DESERTPATH}\ScamlaTester\SCAMLA_Example.xme; DestDir: {app}\Samples\ScamlaTester
Source: {#DESERTPATH}\ScamlaTester\CreateMGA.vbs; DestDir: {app}\Samples\ScamlaTester
Source: {#DESERTPATH}\ScamlaTester\ScamlaTester.cpp; DestDir: {app}\Samples\ScamlaTester
Source: {#DESERTPATH}\ScamlaTester\ScamlaTester.vcproj; DestDir: {app}\Samples\ScamlaTester

[Registry]
Root: HKCU; Subkey: Environment; ValueType: string; ValueName: DESERT_PATH; ValueData: {app}; Flags: uninsdeletevalue deletevalue

[Code]
Procedure ModPath(const ValueName, pathdir: String);
var
	oldpath:	String;
	newpath:	String;
	pathArr:	TArrayOfString;
	i:			Integer;
begin
	// Modify WinNT path
	if UsingWinNT() = true then begin

		// Get current path, split into an array
		RegQueryStringValue(HKEY_CURRENT_USER, 'Environment', ValueName, oldpath);
		if oldpath <> '' then
			oldpath := oldpath + ';';
		i := 0;
		while (Pos(';', oldpath) > 0) do begin
			SetArrayLength(pathArr, i+1);
			pathArr[i] := Copy(oldpath, 0, Pos(';', oldpath)-1);
			oldpath := Copy(oldpath, Pos(';', oldpath)+1, Length(oldpath));
			i := i + 1;

			// Check if current directory matches app dir
			if pathdir = pathArr[i-1] then begin
				// if uninstalling, remove dir from path
				if IsUninstaller() = true then begin
					continue;
				// if installing, abort because dir was already in path
				end else begin
					Exit;
				end;
			end;

			// Add current directory to new path
			if i = 1 then begin
				newpath := pathArr[i-1];
			end else begin
				newpath := newpath + ';' + pathArr[i-1];
			end;
		end;

		// Append app dir to path if not already included
		if IsUninstaller() = false then begin
			if newpath <> '' then begin
				newpath := newpath + ';' + pathdir;
			end else begin
				newpath := pathdir;
			end;
		end;

		// Write new path
		RegWriteStringValue(HKEY_CURRENT_USER, 'Environment', ValueName, newpath);
	end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
	path : String;
begin
	if CurStep = ssInstall then begin
		path := ExpandConstant('{app}');
	//	RegWriteStringValue(HKEY_CURRENT_USER, 'Environment', 'DESERT_PATH', path);
		//Append bin, to Path
		ModPath('PATH', path+'\bin');
		//Output message wizzard
	end;
end;
