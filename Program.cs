using System;
using System.Threading.Tasks;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure.IO.GraphSON;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;

namespace GremlinApp
{
    class Program
    {
        static void Main(string[] args)
        {
        try
        {
            if (args.Length!=1)
            {
                Console.WriteLine("Please enter a Gremlin/Graph Query.");
            }
            else
            {
                var azureConfig = new ConfigurationBuilder()
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build()
                    .GetSection("AzureConfig");
                var hostname = azureConfig["HostName"];
                var port = Convert.ToInt32(azureConfig["Port"]);
                var authKey = azureConfig["AuthKey"];
                var database = azureConfig["Database"];
                var collection = azureConfig["Collection"];
                var gremlinServer = new GremlinServer(
                    hostname, port, enableSsl: true,
                    username: $"/dbs/" + database + "/colls/" + collection,
                    password: authKey);
                using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
                {
                    var resultSet = AzureAsync(gremlinClient, args[0]);
                    Console.WriteLine("\n{{\"Returned\": \"{0}\"}}", resultSet.Result.Count);
                    foreach (var result in resultSet.Result)
                    {
                        string jsonOutput = JsonConvert.SerializeObject(result);
                        Console.WriteLine("{0}",jsonOutput);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("EXCEPTION: {0}", ex.Message);
        }
        }


        private static Task<ResultSet<dynamic>> AzureAsync(GremlinClient gremlinClient, string query)
        {
            try
            {
                return gremlinClient.SubmitAsync<dynamic>(query);
            }
            catch (ResponseException ex)
            {
                Console.WriteLine("EXCEPTION: {0}", ex.StatusCode);
                throw;
            }
        }
    }
}
