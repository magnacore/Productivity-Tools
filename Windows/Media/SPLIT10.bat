for %%a in ("*.*") do "C:\Program Files\MKVToolNix\mkvmerge.exe" -o "splitMOV\%%a" --split duration:00:10:00.000 "%%a"
for %%a in ("splitMOV\*.*") do ren "%%~a" "%%~na_split10%%~xa"