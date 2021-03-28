using System;
using System.Text.RegularExpressions;
using WCNexus.App.Models;
using MongoDB.Driver;

namespace WCNexus.App.Database
{
    public class DBContext : IDBContext
    {
        private readonly DBConfig settings;
        private readonly MongoClient client;
        private readonly IMongoDatabase dbInstance;

        public DBContext(DBConfig settings)
        {
            this.settings = settings;

            var uri = @$"
                mongodb://{ settings.User }
                :{ settings.Password }
                @{ settings.Host }
                /{ settings.DataDB }
                ?authSource={ settings.AuthDB }
            ";

            // remove all white spaces
            uri = Regex.Replace(uri, @"\s+", "");

            client = new MongoClient(uri);

            dbInstance = client.GetDatabase(settings.DataDB);
        }

        public bool Drop()
        {
            try
            {
                this.client.DropDatabase(settings.DataDB);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

        }

        public IMongoDatabase GetDatabase()
        {
            return dbInstance;
        }

    }
}