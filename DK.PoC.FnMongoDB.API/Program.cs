using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DK.PoC.FnMongoDB.API.Models;
using Microsoft.Extensions.Http;
using System;
using DK.PoC.FnMongoDB.API.MongoCollectionSettings;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()

    .ConfigureServices(services =>
    {
        services.Configure<MongoDBSettings>(
            option =>
            {
                option.MdbConnectionstring = Environment.GetEnvironmentVariable("MongoDBAtlasConnectionString");
                option.DatabaseName = Environment.GetEnvironmentVariable("DatabaseName");
                option.ContactCollection = Environment.GetEnvironmentVariable("ContactCollection");

                option.mongoDBClientSetting = MongoClientSettings.FromConnectionString(option.MdbConnectionstring);
                option.mongoDBClientSetting.LinqProvider = LinqProvider.V3;
                option.mongoClient = new MongoClient(option.mongoDBClientSetting);
            });

        services.AddHttpClient("DK", configuration => //Just to showcase how to inject HTTP Factory
        {
            configuration.BaseAddress = new Uri(Environment.GetEnvironmentVariable("DKUrl"));
        });
        services.AddTransient<IDemoApp, DemoApp>(); //Just to showcase how to inject Repository pattern to access DB
        //services.AddHttpClient();
        //services.AddScoped();
    })
    .Build();

host.Run();
