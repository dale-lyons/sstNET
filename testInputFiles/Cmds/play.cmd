
if exist *.xml del *.xml
if exist *.trk del *.trk
if exist *.out del *.out

C:\Projects\sst.NET\sst.DOS\Debug\sst.DOS.exe /keys=keys%1.sks > sst.out
C:\Projects\sst.NET\bin\Debug\sst.NET.exe /keys:keys%1.sks > net.out

if not exist thaw%1.sks goto done

C:\Projects\sst.NET\sst.DOS\Debug\sst.DOS.exe /keys=thaw%1.sks > sst2.out
C:\Projects\sst.NET\bin\Debug\sst.NET.exe /keys:thaw%1.sks > net2.out

:done

