using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;


namespace Microsoft.Extensions.Caching.MongoDB
{
    /*
     For performance reasons the "time to live" (TTL) index can't be used because
     of the following reasons:

     * The TTL index only works with a type of Date BSON.
     * The $inc field update operator only works with numerical types.
     * Therefore, a Date value can't be updated at the same time that the value
       is fetched executing the command FindOneAndUpdate and using the $inc operator.

     The approach will be to store the expiration time as a long number that will 
     represent the Ticks of the DateTime format. Every time that an item will be
     accessed, the specified SlidingExpiration will be incremented with the
     specified time interval in Ticks.

     This can be done in one request to the database using the command
     FindOneAndUpdate and updateing the document at the same time
     that is retreived.
    */


    /// <summary>
    /// Represents the model to store cache items in the database.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class CacheItemModel
    {
        /// <summary>
        /// The key of the item to be stored in the cache. In MongoDB this key
        /// will be represented as the _id field
        /// </summary>
        [BsonId]
        public string Key { get; set; }

        /// <summary>
        /// The value of the cache item.
        /// </summary>
        public string Value { get; set; }

        public long SlidingTimeUtcTicks { get; set; }

        public long ExpirationTimeUtcInTicks { get; set; }
    }
}
