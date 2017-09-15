Making changes to the metamodel in Tonka
========================================

 1. Make sure upstream `meta-core` is configured as a remote:
        git remote add meta-core git@github.com:metamorph-inc/meta-core.git

 2. Fetch `meta-core/master` (so it's up to date):
        git fetch meta-core master

 3. Make your metamodel changes in `CyPhyML-tonka.xme`.  Make sure you export (back to the same file, `CyPhyML-tonka.xme`) when done.

 4. Run `merge.py` using the tonka-provided Python (in `tonka\bin\Python27\Scripts`) to merge changes into `CyPhyML.xme`:
        cd meta\CyPhyML
        python merge.py --version [version number] --upstream_rev meta-core/master

 5. Build using `build_both.cmd` from an administrator VS2015 Developer Command Prompt.
