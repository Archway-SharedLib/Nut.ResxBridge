# dotnet tool install -g dotnet-reportgenerator-globaltool

dotnet test ./test/Nut.ResxBridge.Test/Nut.Results.Test.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=../../nut.results.resxbridge.xml
reportgenerator "-reports:./nut.resxbridge.coverage.xml" "-targetdir:coveragereport" -reporttypes:Html
