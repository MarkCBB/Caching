using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;


namespace Microsoft.Extensions.Caching.MongoDB
{
    /*
     Consider:
     * MongoDB does not allow server side update operations based
       in the results of some calculation or condition of some field(s)
       in the same document to be updated.
     * The $inc field update operator only works with numerical types.
     
     Therefore:
     * A Date value can't be updated at the same time that the value
       is fetched executing the command FindOneAndUpdate and
       using the $inc operator.
     * A field with the effective expiration timestamp will be set.
     * Is much more efficient to work only with an absolutely expiration time
       avoiding sliding Time, because no updates will be performed
       wverytime that a value is fetched.
     * We can use the TTL index to remove all elements in server side.    
        
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
        [BsonElement]
        public byte[] Value { get; set; }

        [BsonElement]
        public long SlidingTimeTicks { get; set; }

        [BsonElement("AbsoluteExpirationTimeUtc")]
        private DateTime _absoluteExpirationTimeUtc
        {
            get
            {
                return AbsoluteExpirationTimeUtc.Date;
            }
            set
            {
                AbsoluteExpirationTimeUtc = value;
            }
        }

        [BsonElement("EffectiveExpirationTimeUtc")]
        private DateTime _effectiveExpirationTimeUtc
        {
            get
            {
                return EffectiveExpirationTimeUtc.Date;
            }
            set
            {
                EffectiveExpirationTimeUtc = value;
            }
        }

        [BsonIgnore]
        public DateTimeOffset AbsoluteExpirationTimeUtc;

        [BsonIgnore]
        public DateTimeOffset EffectiveExpirationTimeUtc;
    }
}
