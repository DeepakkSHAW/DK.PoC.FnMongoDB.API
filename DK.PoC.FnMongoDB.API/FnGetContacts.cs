using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
//using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DK.PoC.FnMongoDB.API.Models;
using DK.PoC.FnMongoDB.API.MongoCollectionSettings;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.Linq;



namespace DK.PoC.FnMongoDB.API;

public class FnGetContacts
{
    private readonly ILogger _logger;
    private readonly IMongoCollection<Contact> _contactCollection;

    //public FnGetContacts(ILoggerFactory loggerFactory, IOptions<ContactSettings> options)
    //{
    //    //MongoClientSettings mongodbSetting = MongoClientSettings.FromConnectionString(Environment.GetEnvironmentVariable("MongoDBAtlasConnectionString"));
    //    //MongoClientSettings mongodbSetting = MongoClientSettings.FromConnectionString(options.Value.MdbConnectionstring);
    //    //mongodbSetting.LinqProvider = LinqProvider.V3;

    //    //var client = new MongoClient(mongodbSetting);
    //    //var client = new MongoClient(options.Value.MongodbClient);
    //    _contactCollection = new MongoClient(options.Value.mongoDBClientSetting)
    //            .GetDatabase("IndOMTechDB")
    //            .GetCollection<Contact>("IOContacts");

    //    _logger = loggerFactory.CreateLogger<FnGetContacts>();

    //}

    public FnGetContacts(ILoggerFactory loggerFactory, IOptions<MongoDBSettings> options)
    {
        _contactCollection = options.Value.mongoClient
                .GetDatabase(options.Value.DatabaseName)
                .GetCollection<Contact>(options.Value.ContactCollection);

        _logger = loggerFactory.CreateLogger<FnGetContacts>();

    }

    [Function("ContactV4")]
    public async Task<HttpResponseData> ContactV4([HttpTrigger(AuthorizationLevel.Anonymous,
        "get", Route = "v4/contact/{id}")] HttpRequestData req, string id)
    {
        var response = req.CreateResponse();
        try
        {
            var contact = await _contactCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (contact != null)
            {
                response.StatusCode = HttpStatusCode.OK;
                response.Headers.Add("Content-Type", "application/json");
                response.WriteString(JsonSerializer.Serialize(contact));
            }
            else
            {
                response.StatusCode = HttpStatusCode.NotFound;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.WriteString(ex.Message);
        }
        return response;
    }

    [Function("ContactsV4")]
    public async Task<HttpResponseData> ContactsV4([HttpTrigger(AuthorizationLevel.Anonymous,
        "get", Route = "v4/contact")] HttpRequestData req)
    {
        var response = req.CreateResponse();
        try
        {
            var contacts = await _contactCollection.Find(_ => true).ToListAsync();
            response.StatusCode = HttpStatusCode.OK;
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(JsonSerializer.Serialize(contacts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.WriteString(ex.Message);
        }
        return response;
    }

    [Function("AddContactV4")]
    public async Task<HttpResponseData> AddContactV4([HttpTrigger(AuthorizationLevel.Anonymous,
        "post", Route = "v4/contact")] HttpRequestData req)
    {
        var response = req.CreateResponse();
        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var contact = JsonSerializer.Deserialize<Contact>(requestBody);
            if (contact != null)
            {
                response.StatusCode = HttpStatusCode.Created;
                response.Headers.Add("Content-Type", "text/plain");
                await _contactCollection.InsertOneAsync(contact);
                response.WriteString(string.Format("{0}/{1}", req.Url.ToString(), contact.Id));
            }
            else
            {
                response.StatusCode = HttpStatusCode.BadRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.WriteString(ex.Message);
        }
        return response;
    }

    [Function("UpdateContactV4")]
    public async Task<HttpResponseData> UpdateContactV4([HttpTrigger(AuthorizationLevel.Anonymous,
        "put", Route = "v4/contact/{id}")] HttpRequestData req, string id)
    {
        var response = req.CreateResponse();
        try
        {
            var contact = await _contactCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (contact == null) { response.StatusCode = HttpStatusCode.NotFound; return response; }
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updateContact = JsonSerializer.Deserialize<Contact>(requestBody);
            if (updateContact != null)
            {
                updateContact.Id = contact.Id;
                await _contactCollection.ReplaceOneAsync(x => x.Id == id, updateContact);
                response.StatusCode = HttpStatusCode.NoContent;
            }
            else
            {
                response.StatusCode = HttpStatusCode.BadRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.WriteString(ex.Message);
        }
        return response;
    }

    [Function("RemoveContactV4")]
    public async Task<HttpResponseData> RemoveContactV4([HttpTrigger(AuthorizationLevel.Anonymous,
        "delete", Route = "v4/contact/{id}")] HttpRequestData req, string id)
    {
        var response = req.CreateResponse();
        try
        {
            var delOperation = await _contactCollection.DeleteOneAsync(x => x.Id == id);

            if (delOperation.DeletedCount > 0)
                response.StatusCode = HttpStatusCode.NoContent;
            else
                response.StatusCode = HttpStatusCode.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.WriteString(ex.Message);
        }
        return response;
    }

    
    
    //---------------------------------//
    [Function("ContactsV1")]
    public HttpResponseData GetContact([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/getcontacts")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json charset=utf-8");

        //response.WriteString("Welcome to Azure Functions!");

        try
        {
            MongoClientSettings mongodbSetting = MongoClientSettings.FromConnectionString(Environment.GetEnvironmentVariable("MongoDBAtlasConnectionString"));
            mongodbSetting.LinqProvider = LinqProvider.V3;

            var client = new MongoClient(mongodbSetting);
            IMongoCollection<Contact> contacts = client
                .GetDatabase("IndOMTechDB")
                .GetCollection<Contact>("IOContacts");

            var result = contacts.AsQueryable().ToList();
            int i = result.Count();
            response.WriteString($"{JsonSerializer.Serialize(result)}");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex.Message);
            //return new BadRequestObjectResult("Error refreshing demo - " + ex.Message);
        }
        return response;
    }

    [Function("ContactsV2")]
    public HttpResponseData GetAllContacts([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v2/getcontacts")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var client = new MongoClient(Environment.GetEnvironmentVariable("MongoDBAtlasConnectionString"));
        var database = client.GetDatabase("IndOMTechDB", null);

        var contactCollection = database.GetCollection<Contact>("IOContacts");
        var filterDefinition = Builders<Contact>.Filter.Empty;
        var contacts = contactCollection.Find(filterDefinition).ToList();


        var response = req.CreateResponse();
        response.StatusCode = HttpStatusCode.OK;
        response.Headers.Add("Content-Type", "application/json");
        response.WriteString(JsonSerializer.Serialize(contacts));
        //response.Body = contacts;
        return response;

        //var myObj = new { name = "thomas", location = "Denver" };
        //var jsonToReturn = JsonSerializer.Serialize(myObj);

        //return new HttpResponseData(HttpStatusCode.OK)
        //{
        //    Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")


        //}
    }
}
