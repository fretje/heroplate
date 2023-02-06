# Heroplate

This code is based on the FullStackHero code, more specifically it's a "merge" of the two repositories [dotnet-webapi-boilerplate](https://github.com/fullstackhero/dotnet-webapi-boilerplate) and [blazor-wasm-boilerplate](https://github.com/fullstackhero/blazor-wasm-boilerplate).

It seems like maintenance over on the fullstackhero project has kind of grinded to a halt, so I took the liberty to release my "updated" version here.

This template is optimized for use with Visual Studio on Windows. It's definitely possible to use it with other IDE's/platforms, but this hasn't been tested.

The following extensions for Visual Studio are recommended:
* [SwitchStartupProject](https://marketplace.visualstudio.com/items?itemName=vs-publisher-141975.SwitchStartupProjectForVS2022)
* [Open Command Line](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.OpenCommandLine)

Changes:

* Update to .NET 7
* Update packages
* Remove different database providers (The code is using sql server now, but changing to another provider shouldn't be that much work. This has the advantage you only have to load the assemblies for the provider you're actually using.)
* Add ValidationBehavior for asynchronous validation using FluentValidation
* Add HangfireMediatorBridge
* Add ProblemDetails middleware
* Add Tests
* Re-structure 
* Rename Client => Admin
* Many other small changes, refactorings and bug fixes
* ...

DISCLAIMER: This code is provided as is and has been used successfully in a real-world application. However, every application has its own unique rules and constraints, and this code may not be the best fit for every situation. Please thoroughly evaluate the specific requirements of your application before using this code.