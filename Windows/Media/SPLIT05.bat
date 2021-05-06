for %%a in ("*.*") do "C:\Program Files\MKVToolNix\mkvmerge.exe" -o "splitMOV\%%a" --split duration:00:05:00.000 "%%a"
for %%a in ("splitMOV\*.*") do ren "%%~a" "%%~na_split05%%~xa"