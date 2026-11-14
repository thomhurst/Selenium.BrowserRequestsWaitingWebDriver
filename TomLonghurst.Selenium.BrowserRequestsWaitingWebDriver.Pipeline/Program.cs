using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularPipelines;
using TomLonghurst.Selenium.BrowserRequestsWaitingWebDriver.Pipeline.Modules;
using TomLonghurst.Selenium.BrowserRequestsWaitingWebDriver.Pipeline.Modules.LocalMachine;
using TomLonghurst.Selenium.BrowserRequestsWaitingWebDriver.Pipeline.Settings;

var builder = Pipeline.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.Configure<NuGetSettings>(builder.Configuration.GetSection("NuGet"));

if (builder.Environment.IsDevelopment())
{
    builder.AddModule<CreateLocalNugetFolderModule>()
        .AddModule<AddLocalNugetSourceModule>()
        .AddModule<UploadPackagesToLocalNuGetModule>();
}
else
{
    builder.AddModule<UploadPackagesToNugetModule>();
}

builder.AddModule<RunUnitTestsModule>()
    .AddModule<NugetVersionGeneratorModule>()
    .AddModule<PackProjectsModule>()
    .AddModule<PackageFilesRemovalModule>()
    .AddModule<PackagePathsParserModule>();

await builder.RunAsync();
