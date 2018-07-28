using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Caching.MongoDB
{
    public class BaseMongoDbTests
    {
        //protected const string SkipReason = "These tests require MongoDb server, please configure an"
        //   + " accessible server and adapt the connection string accordingly.";

        // Ucomment the next line and comment the previous one to run the tests related with MongoDB
        protected const string SkipReason = null;

        protected const string ConnectionString = "mongodb://mongo1:27018,mongo1:27019,mongo1:27020";
        protected const string DefaultDatabaseName = "CachingTestsDB";
        protected const string DefaultCollectionName = "CachingEntriesTestCollection";

        protected MongoDBCache mongoCache = null;

        public BaseMongoDbTests()
        {
            var options = new MongoDBCacheOptions()
            {
                ConnectionString = ConnectionString,
                DatabaseName = DefaultDatabaseName,
                CollectionName = DefaultCollectionName
            };
            mongoCache = new MongoDBCache(options);

            //Uncomment to clean the test database before to run the test cases.
            //var db = _mongoCache.GetClient();
            //db.DropDatabase(options.DatabaseName);
        }

        protected static string GetRandomNumber()
        {
            var r = new Random();
            var n = r.Next(1000, 100000);
            return n.ToString();
        }

        protected static bool ExistsIndexInListHelper(List<BsonDocument> indexes, string indexName)
        {
            foreach (var item in indexes)
            {
                if (item["name"] == indexName)
                {
                    return true;
                }
            }
            return false;
        }

        protected static bool EqualsByteArray(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            var max = a.Length;
            var i = 0;
            while (i < max && a[i] == b[i])
            {
                i++;
            }

            return (i == max);
        }

        protected static long ToUnixTimeMilliseconds(DateTime d)
        {
            return ((DateTimeOffset)d).ToUnixTimeMilliseconds();
        }
    }
}
