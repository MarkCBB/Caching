using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Caching.MongoDB
{
    public class BaseMongoDbTests
    {
        protected const string SkipReason = "These tests require MongoDb server, please configure an"
            + " accessible server and adapt the connection string accordingly.";

        protected MongoDBCache _mongoCache = null;

        public BaseMongoDbTests()
        {
            var options = new MongoDBCacheOptions()
            {
                ConnectionString = "mongodb://mongo1:27018,mongo1:27019,mongo1:27020",
                DatabaseName = "CachingTestsDB",
                CollectionName = "CachingEntriesTestCollection"
            };
            _mongoCache = new MongoDBCache(options);

            //Uncomment to clean the test database before to run the test cases.
            //var db = _mongoCache.GetClient();
            //db.DropDatabase(options.DatabaseName);
        }
    }
}
