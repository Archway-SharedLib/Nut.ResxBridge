REM dotnet tool install -g dotnet-reportgenerator-globaltool

dotnet test ./test/Nut.ResxBridge.Test/Nut.ResxBridge.Test.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=..\..\nut.resxbridge.coverage.xml
reportgenerator "-reports:.\nut.resxbridge.coverage.xml" "-targetdir:coveragereport" -reporttypes:Html
