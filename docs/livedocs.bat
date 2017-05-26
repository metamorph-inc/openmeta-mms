rd /s/q _build
cmd /c make html
_build\html\index.html
nodemon --exec "rebuilddocs" --ext "rst png" --ignore "_build"