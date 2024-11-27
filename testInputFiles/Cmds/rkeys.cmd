
if exist *.xml del *.xml
if exist *.trk del *.trk
if exist *.out del *.out

C:\Projects\sst.NET\sst.DOS\Debug\sst.DOS.exe /keys=keys%1.sks /strokes=keys%2.sks
