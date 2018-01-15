rem Build Old Docs
..\bin\Python27\Scripts\python build.py
rem Build New Docs
rd /s/q _build
cmd /c make html
rem Assembly Full Docs
rd /s/q publish_html
mkdir publish_html
xcopy /e _build\html publish_html
mkdir publish_html\olddocs
xcopy /e out\html_documentation publish_html\olddocs