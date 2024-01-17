using ModularPipelines.Context;
using ModularPipelines.DotNet;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Enums;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Modules;

namespace TomLonghurst.Selenium.BrowserRequestsWaitingWebDriver.Pipeline.Modules;

public class RunUnitTestsModule : Module<List<DotNetTestResult>>
{
    protected override async Task<List<DotNetTestResult>?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        var results = new List<DotNetTestResult>();

        foreach (var unitTestProjectFile in context
                     .Git().RootDirectory!
                     .GetFiles(file => file.Path.EndsWith("Tests.csproj", StringComparison.OrdinalIgnoreCase)))
        {
            results.Add(await context.DotNet().Test(new DotNetTestOptions
            {
                TargetPath = unitTestProjectFile.Path,
                CommandLogging = CommandLogging.Input | CommandLogging.Error,
            }, cancellationToken));
        }

        return results;
    }
}