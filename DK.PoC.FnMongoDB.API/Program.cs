using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DK.PoC.FnMongoDB.API.Models;
using Microsoft.Extensions.Http;
using System;
using DK.PoC.FnMongoDB.API.MongoCollectionSettings;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DK.PoC.FnMongoDB.API
{

    internal class Program
    {
        static void Main(string[] args)
        {
            FunctionsDebugger.Enable();

            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.Configure<ContactSettings>(
                        option => {
                            option.MdbConnectionstring = Environment.GetEnvironmentVariable("MongoDBAtlasConnectionString");
                            option.DatabaseName = Environment.GetEnvironmentVariable("DatabaseName");
                            option.ContactCollection = Environment.GetEnvironmentVariable("ContactCollection");

                            option.mongoDBClientSetting = MongoClientSettings.FromConnectionString(option.MdbConnectionstring);
                            option.mongoDBClientSetting.LinqProvider = LinqProvider.V3;
                            option.mongoClient = new MongoClient(option.mongoDBClientSetting);
                        });

                    services.AddHttpClient("DK", configuration =>
                    {
                        configuration.BaseAddress = new Uri(Environment.GetEnvironmentVariable("DKUrl"));
                    });
                    services.AddTransient<IDemoApp, DemoApp>();
                    //services.AddHttpClient();
                    //services.AddScoped();
                })
                .Build();

            host.Run();
        }
    }
}