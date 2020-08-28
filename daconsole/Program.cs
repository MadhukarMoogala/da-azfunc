namespace daconsole
{
    using Autodesk.Forge.Core;
    using Autodesk.Forge.DesignAutomation;
    using Autodesk.Forge.DesignAutomation.Model;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="ConsoleHost" />.
    /// </summary>
    class ConsoleHost : IHostedService
    {
        /// <summary>
        /// The StartAsync.
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// The StopAsync.
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Defines the <see cref="App" />.
    /// </summary>
    internal class App
    {
        /// <summary>
        /// Defines the api.
        /// </summary>
        public DesignAutomationClient api;

        /// <summary>
        /// Defines the config.
        /// </summary>
        public ForgeConfiguration config;

        /// <summary>
        /// Defines the TargetEngine.
        /// </summary>
        private static readonly string TargetEngine = "Autodesk.AutoCAD+24";

        /// <summary>
        /// Defines the PackageName.
        /// </summary>
        private static readonly string PackageName = "forgeExtractLength";

        /// <summary>
        /// Defines the Label.
        /// </summary>
        private static readonly string Label = "prod";

        /// <summary>
        /// Defines the Owner.
        /// </summary>
        private static readonly string Owner = "mx";

        /// <summary>
        /// Defines the ActivityName.
        /// </summary>
        private static readonly string ActivityName = "extractlengths";

        /// <summary>
        /// Defines the AzureFunctionUrl.
        /// </summary>
        private static readonly string AzureFunctionUrl = "";

        /// <summary>
        /// Defines the PostResultUrl.
        /// </summary>
        private static readonly string PostResultUrl = $"{AzureFunctionUrl}/api/PutResultJson/$(workitem.id)";

        /// <summary>
        /// Defines the GetResultUrl.
        /// </summary>
        private static readonly string GetResultUrl = $"{AzureFunctionUrl}/api/FetchJson";

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        /// <param name="api">The api<see cref="DesignAutomationClient"/>.</param>
        /// <param name="config">The config<see cref="IOptions{ForgeConfiguration}"/>.</param>
        public App(DesignAutomationClient api, IOptions<ForgeConfiguration> config)
        {
            this.api = api;
            this.config = config.Value;
        }

        /// <summary>
        /// The GetPlotToPdfActivityAsync.
        /// </summary>
        /// <returns>The <see cref="Task{string}"/>.</returns>
        private async Task<string> GetPlotToPdfActivityAsync()
        {
            Console.WriteLine("\nActiviy Start");
            var activitiesApi = api.ActivitiesApi;
            ApiResponse<Page<string>> activitiesResp = await activitiesApi.GetActivitiesAsync();
            List<string> listOfActivities = new List<string>();

            string activityName = null;
            if (activitiesResp.HttpResponse.IsSuccessStatusCode)
            {
                var page = activitiesResp.Content.PaginationToken;
                activitiesResp.Content.Data.ForEach(e => listOfActivities.Add(e));
                while (page != null)
                {
                    activitiesResp = await activitiesApi.GetActivitiesAsync(page);
                    page = activitiesResp.Content.PaginationToken;
                    activitiesResp.Content.Data.ForEach(e => listOfActivities.Add(e));
                }
                var activities = listOfActivities.Where(a => a.Contains("PlotToPDF")).Select(a => a);
                if (activities.Count() > 0)
                {
                    activityName = activities.FirstOrDefault();
                }

            }
            return activityName;
        }

        /// <summary>
        /// The GetListOfEnginesAsync.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task GetListOfEnginesAsync()
        {
            var forgeEnginesApi = api.EnginesApi;
            ApiResponse<Page<string>> engines = await forgeEnginesApi.GetEnginesAsync();
            if (engines.HttpResponse.IsSuccessStatusCode)
            {
                Console.WriteLine(JsonConvert.SerializeObject(engines.Content, Formatting.Indented));
            }
        }

        /// <summary>
        /// The CreateZip.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        static string CreateZip()
        {
            Console.WriteLine("\tGenerating autoloader zip...");
            string zip = Path.Combine(Directory.GetCurrentDirectory(), "package.zip");
            if (!File.Exists(zip))
                throw new FileNotFoundException("Missing package.zip!");
            return zip;
        }

        /// <summary>
        /// The DownloadToDocsAsync.
        /// </summary>
        /// <param name="url">The url<see cref="string"/>.</param>
        /// <param name="localFile">The localFile<see cref="string"/>.</param>
        /// <returns>The <see cref="Task{string}"/>.</returns>
        public async Task<string> DownloadToDocsAsync(string url, string localFile)
        {
            var report = Directory.GetCurrentDirectory();
            var fname = Path.Combine(report, localFile);
            if (File.Exists(fname))
                File.Delete(fname);
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using (var fs = new FileStream(fname, FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs);
            }

            return fname;
        }

        /// <summary>
        /// The GetActivitySpec.
        /// </summary>
        /// <param name="myApp">The myApp<see cref="string"/>.</param>
        /// <param name="ActivityName">The ActivityName<see cref="string"/>.</param>
        /// <param name="settings">The settings<see cref="Dictionary{string, ISetting}"/>.</param>
        /// <param name="parameters">The parameters<see cref="Dictionary{string, Parameter}"/>.</param>
        /// <returns>The <see cref="Activity"/>.</returns>
        private Activity GetActivitySpec(string myApp, string ActivityName, Dictionary<string, ISetting> settings, Dictionary<string, Parameter> parameters)
        {
            var activity = new Activity()
            {
                Appbundles = new List<string>()
                    {
                        myApp
                    },
                CommandLine = new List<string>()
                    {
                       $"$(engine.path)\\accoreconsole.exe /i $(args[HostDwg].path) /al $(appbundles[{PackageName}].path) /s $(settings[script].path)"
                    },
                Settings = settings,
                Engine = TargetEngine,
                Parameters = parameters,
                Id = ActivityName
            };
            return activity;
        }

        /// <summary>
        /// The SetupActivityAsync.
        /// </summary>
        /// <param name="myApp">The myApp<see cref="string"/>.</param>
        /// <param name="ActivityName">The ActivityName<see cref="string"/>.</param>
        /// <returns>The <see cref="Task{string}"/>.</returns>
        private async Task<string> SetupActivityAsync(string myApp, string ActivityName)
        {
            Console.WriteLine("Setting up activity...");
            var myActivity = $"{Owner}.{ActivityName}+{Label}";
            var actResponse = await this.api.ActivitiesApi.GetActivityAsync(myActivity, throwOnError: false);
            var settings = new Dictionary<string, ISetting>()
                        {
                            { "script", new StringSetting() { Value = "ComputeLength\n" } }
                        };

            var parameters = new Dictionary<string, Parameter>()
                        {
                            { "HostDwg", new Parameter() { Verb= Verb.Get, LocalName = "square.dwg",  Required = true}},
                            { "Result", new Parameter() { Verb= Verb.Post, LocalName = "result.json", Required= true}}
                        };
            Activity activity = GetActivitySpec(myApp, ActivityName, settings, parameters);

            if (actResponse.HttpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Creating activity {myActivity}...");
                await api.CreateActivityAsync(activity, Label);
                return myActivity;
            }
            await actResponse.HttpResponse.EnsureSuccessStatusCodeAsync();
            Console.WriteLine("\tFound existing activity...");
            if (!Equals(activity, actResponse.Content))
            {
                Console.WriteLine($"\tUpdating activity {myActivity}...");
                await api.UpdateActivityAsync(activity, Label);
            }
            return myActivity;

            static bool Equals(Autodesk.Forge.DesignAutomation.Model.Activity a, Autodesk.Forge.DesignAutomation.Model.Activity b)
            {
                Console.Write("\tComparing activities...");
                //ignore id and version
                b.Id = a.Id;
                b.Version = a.Version;
                var res = a.ToString() == b.ToString();
                Console.WriteLine(res ? "Same." : "Different");
                return res;
            }
        }

        /// <summary>
        /// The SetupAppBundleAsync.
        /// </summary>
        /// <returns>The <see cref="Task{string}"/>.</returns>
        private async Task<string> SetupAppBundleAsync()
        {
            Console.WriteLine("Setting up appbundle...");
            var myApp = $"{Owner}.{PackageName}+{Label}";
            var appResponse = await this.api.AppBundlesApi.GetAppBundleAsync(myApp, throwOnError: false);
            var app = new AppBundle()
            {
                Engine = TargetEngine,
                Id = PackageName
            };
            var package = CreateZip();
            if (appResponse.HttpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"\tCreating appbundle {myApp}...");
                await api.CreateAppBundleAsync(app, Label, package);
                return myApp;
            }
            await appResponse.HttpResponse.EnsureSuccessStatusCodeAsync();
            Console.WriteLine("\tFound existing appbundle...");
            if (!await EqualsAsync(package, appResponse.Content.Package))
            {
                Console.WriteLine($"\tUpdating appbundle {myApp}...");
                await api.UpdateAppBundleAsync(app, Label, package);
            }
            return myApp;

            async Task<bool> EqualsAsync(string a, string b)
            {
                Console.Write("\tComparing bundles...");
                using var aStream = File.OpenRead(a);
                var bLocal = await DownloadToDocsAsync(b, "das-appbundle.zip");
                using var bStream = File.OpenRead(bLocal);
                using var hasher = SHA256.Create();
                var res = hasher.ComputeHash(aStream).SequenceEqual(hasher.ComputeHash(bStream));
                Console.WriteLine(res ? "Same." : "Different");
                return res;
            }
        }

        /// <summary>
        /// The CreateWorkItem.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task CreateWorkItem()
        {
            //step1:
            Console.WriteLine("\nSetupAppBundleAsync Start...");
            var myApp = await SetupAppBundleAsync();
            //step2:
            Console.WriteLine("\nSetupActivityAsync Start...");
            var myActivity = await SetupActivityAsync(myApp, ActivityName);
            //step3:
            Console.WriteLine("\nWorkItem Start...");
            Console.WriteLine($"\nComputed Lenghths values as JSON will be posted to {PostResultUrl}");
            var workItemsApi = api.WorkItemsApi;
            ApiResponse<WorkItemStatus> workItemStatus = await workItemsApi.CreateWorkItemAsync(new Autodesk.Forge.DesignAutomation.Model.WorkItem()
            {
                ActivityId = myActivity,
                Arguments = new Dictionary<string, IArgument>() {
                              {
                               "HostDwg",
                               new XrefTreeArgument() {
                                Url = "https://fpd-uploads.s3.us-west-2.amazonaws.com/square.dwg",
                                Verb = Verb.Get
                               }
                              }, {
                               "Result",
                               new XrefTreeArgument() {
                                Verb = Verb.Post, Url = PostResultUrl
                               }
                              }
                             }
            });

            Console.Write("\tPolling status");
            while (!workItemStatus.Content.Status.IsDone())
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                workItemStatus = await workItemsApi.GetWorkitemStatusAsync(workItemStatus.Content.Id);

                Console.Write(".");
            }
            Console.WriteLine($"\n{JsonConvert.SerializeObject(workItemStatus.Content, Formatting.Indented)}");
            var reportPath = Path.Combine(Directory.GetCurrentDirectory(), $"{workItemStatus.Content.Id}_report.txt");
            Console.WriteLine($"\nFind Report: {reportPath}");
            await DownloadToDocsAsync(workItemStatus.Content.ReportUrl, $"{workItemStatus.Content.Id}_report.txt");
            if (workItemStatus.Content.Status.Equals(Status.Success))            {
                
                Console.WriteLine($"Launch URL to check posted results.\n\t{GetResultUrl}");
            }
        }

        /// <summary>
        /// The RunAsync.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task RunAsync()
        {
            await CreateWorkItem();
        }
    }

    /// <summary>
    /// Defines the <see cref="Program" />.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The Main.
        /// </summary>
        /// <param name="args">The args<see cref="string[]"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables();
                    builder.AddForgeAlternativeEnvironmentVariables();
                }).ConfigureServices((hostContext, services) =>
                { // add our no-op host (required by the HostBuilder)
                    services.AddHostedService<ConsoleHost>();

                    // our own app where all the real stuff happens
                    services.AddSingleton<App>();

                    // add and configure DESIGN AUTOMATION
                    services.AddDesignAutomation(hostContext.Configuration);
                    services.AddOptions();
                })
                .UseConsoleLifetime()
                .Build();
            using (host)
            {
                await host.StartAsync();
                // Get a reference to our App and run it
                var app = host.Services.GetRequiredService<App>();
                await app.RunAsync();
                await host.StopAsync();
            }
        }
    }
}
