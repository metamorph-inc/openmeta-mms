@set DIG_INPUT_CSV=%~f1
@set DIG_META_PATH=%~f2
@pushd %~dp0..
"%~dp0..\R\bin\x64\Rscript" --no-save --no-restore -e "shiny::runApp('Dig',display.mode='normal',quiet=TRUE, launch.browser=TRUE)"
@popd
