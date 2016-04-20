# HOWTO: make changes to the DesignDataPackage

Open in eclipse-modeling-kepler-SR2-win32-x86_64
Make mods
Right-click > File > Save as image file > meta\DesignDataPackage\doc\ClassDiagrams\same_filename.pdf
avm.genmodel right-click > Reload, click click click click
avm.genmodel right-click > Export Model, XML Schema, click click click click
cd META\meta\DesignDataPackage
cmd /c make.bat
Diff tool, remove some stuff
 git diff -w --no-color . | git apply --cached --ignore-whitespace
 git checkout -- .
 git reset HEAD -- .
 git apply fix_codegen.patch

TODO: gitattributes EOL on xsd in EMF_MetaModel