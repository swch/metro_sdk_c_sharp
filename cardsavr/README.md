# Publishing new versions

The **strivve C# sdk** will be infrequently updated to track changes to the API.  Here are the instructions for publishing it to nuget.

1. Make the changes in /cardsavr
1. Make neccessary test chaanges
1. Run "dotnet test" in /cardsavr_e2e and /cardsavr_tests (using valid creds)
1. Modify the version number in cardsavr.csproj and in CardSavrHttpClient.cs.
1. From /cardsavr, run "dotnet pack --configuration Release".  This creates a .nupkg file.
1. Run: dotnet nuget push bin/Release/cardsavr.2.1.2.nupkg --source https://api.nuget.org/v3/index.json -k API_KEY  (The API key can be created from nuget as a valid contributor, the version will change)
1. From /cardsavr_app/, add run "dotnet add cardsavr".  This should upgrade the package in this project.
1. Run "dotnet run", and the test should run.  Verify in the .csproj file that the version is correct. 
1. Make sure all code changes are merged into master.

