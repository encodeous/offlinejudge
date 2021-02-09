dotnet publish -c Release -p:PublishSingleFile=true -r win-x64 --no-self-contained
dotnet publish -c Release -p:PublishSingleFile=true -r linux-x64 --no-self-contained
dotnet publish -c Release -p:PublishSingleFile=true -r osx-x64 --no-self-contained