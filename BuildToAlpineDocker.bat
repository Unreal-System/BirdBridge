@Echo off
set base_dir=%~dp0
del /F /S /Q .\app\
dotnet publish -r linux-musl-x64 -c release -o .\app -p:SelfContained=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:PublishSingleFile=true
cd app
del BirdBridge.pdb /Q /F
cd ..
copy .\Dockerfile .\app\
pause.