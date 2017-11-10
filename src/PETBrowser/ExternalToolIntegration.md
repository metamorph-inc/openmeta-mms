Results Browser External Analysis Tool Integration
==================================================

External PET analysis tools are registered with the Results Browser by creating a new registry key (with a name corresponding to your tool's name) under `HKEY_LOCAL_MACHINE\SOFTWARE\META\PETBrowser\PETTools` (if the tool should be displayed for all users), or under `HKEY_CURRENT_USER\SOFTWARE\META\PETBrowser\PETTools` (if the tool should be visible only to the current user).

Note that the PET Browser, on 64-bit machines, looks for analysis tools in the 32-bit registry; edit tool registry entries using `regedit` from `C:\Windows\SysWow64` or pass the `/reg:32` flag to the `reg add` command line tool.

Your tool's registry key should contain the following entries:

  * `(default)` - (REG_SZ) - A human-readable name for your tool.  (*Required*)
  * `ActionName` - (REG_SZ) - An action name, to be displayed in the Results Browser's "Analyze Selected with Tool" menu. (*Required*)
  * `ExecutableFilePath` - (REG_SZ) - Path to the program to be run when your tool is invoked by the user. (*Required*)
  * `ProcessArguments` - (REG_SZ) - Command-line arguments to be passed to your tool when it is executed.
  * `WorkingDirectory` - (REG_SZ) - The working directory to be passed to your tool.
  * `ShowConsoleWindow` - (REG_DWORD) - Set to 1 if a console window should be displayed when your tool is running; set to 0 if one should not be displayed.

When running tools, the following placeholders will be expanded in `ExecutableFilePath`, `ProcessArguments`, and `WorkingDirectory`:

  * `%1` - The path to the visualizer_config.json for the selected merged PET.
  * `%2` - The path to the current project directory.
  * `%4` - The path to the META install directory.
