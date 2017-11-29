Making changes to the metamodel in Tonka
========================================

 1. Make sure upstream `meta-core` is configured as a remote:
        git remote add meta-core git@github.com:metamorph-inc/meta-core.git

 2. Fetch `meta-core/master` (so it's up to date):
        git fetch meta-core master

 3. Make your metamodel changes in `CyPhyML-tonka.xme`.  Make sure you export (back to the same file, `CyPhyML-tonka.xme`) when done.

 4. Run `merge.py` using the tonka-provided Python (in `tonka\bin\Python27\Scripts`) to merge changes into `CyPhyML.xme`:
        cd meta\CyPhyML
        bash merge.py --version [version number] --upstream_rev meta-core/master

 5. Build using `build_both.cmd` from an administrator VS2015 Developer Command Prompt.

If you see an error like the one below, your GME version is probably too old.
```/@.\CyPhyML-core.mga|kind=RootFolder|relpos=0
/@.\CyPhyML-core.mga|kind=RootFolder|relpos=1
Traceback (most recent call last):
  File "merge.py", line 243, in <module>
    update_cyphy(version=args.version, upstream_rev=args.upstream_rev)
  File "merge.py", line 213, in update_cyphy
    metaint.InvokeEx(project, metaint_currentobj, Dispatch("Mga.MgaFCOs"), 128)
  File "c:\Users\Adam\repos\tonka\bin\Python27\lib\site-packages\pywin32-219-py2.7-win32.egg\win32com\client\dynamic.py", line 522, in __getattr__
    raise AttributeError("%s.%s" % (self._username_, attr))
AttributeError: MGA.Interpreter.MetaInterpreter.InvokeEx```