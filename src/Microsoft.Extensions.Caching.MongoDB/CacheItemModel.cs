using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;


namespace Microsoft.Extensions.Caching.MongoDB
{
    /*
     Consider:
     * MongoDB does not allow server side update operations based
       in some calculation of some fields of the same document to be updated.
     * The $inc field update operator only works with numerical types.
     
     Therefore:
     * A Date value can't be updated at the same time that the value
       is fetched executing the command FindOneAndUpdate and
       using the $inc operator.
     * A field with the effective expiration timestamp will be set.
     * Is more performant to work only with an absolutely expiration time
       avoiding sliding Time, because no updates will be performed
       everytime that a value is fetched.
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

        [BsonDateTimeOptions(DateOnly = false)]
        public DateTime AbsoluteExpirationTimeUtc { get; set; }

        [BsonDateTimeOptions(DateOnly = false)]
        public DateTime EffectiveExpirationTimeUtc { get; set; }

        public static CacheItemModel CreateNewItem(
            string key,
            byte[] value,
            TimeSpan? AbsoluteExpirationRelativeToNow,
            DateTimeOffset? AbsoluteExpiration,
            TimeSpan? SlidingExpiration,
            DateTimeOffset utcNow)
        {
            var AbsoluteExpirationTimeUtc = DateTimeOffset.MinValue;
            if (AbsoluteExpiration.HasValue && AbsoluteExpiration.Value > utcNow)
            {
                AbsoluteExpirationTimeUtc = AbsoluteExpiration.Value.UtcDateTime;
            }
            else
            {
                if (AbsoluteExpirationRelativeToNow.HasValue && AbsoluteExpirationRelativeToNow.Value.Ticks > 0)
                {
                    AbsoluteExpirationTimeUtc = (utcNow + AbsoluteExpirationRelativeToNow.Value);
                }
            }

            var newItem = new CacheItemModel()
            {
                Key = key,
                Value = value,
                AbsoluteExpirationTimeUtc = new DateTime(
                    ticks: AbsoluteExpirationTimeUtc.UtcTicks,
                    kind: DateTimeKind.Utc),
                SlidingTimeTicks = (SlidingExpiration.HasValue && SlidingExpiration.Value.Ticks > 0) ? (SlidingExpiration.Value.Ticks) : 0
            };

            newItem.EffectiveExpirationTimeUtc = GetEffectiveExpirationTimeUtc(newItem, utcNow);
            return newItem;
        }

        public static DateTime GetEffectiveExpirationTimeUtc(CacheItemModel item, DateTimeOffset utcNow)
        {
            if (item.SlidingTimeTicks == 0)
            {
                return item.AbsoluteExpirationTimeUtc;
            }

            var SlidingExpirationDate = utcNow.AddTicks(item.SlidingTimeTicks);

            if ((item.AbsoluteExpirationTimeUtc == DateTimeOffset.MinValue) ||
                (SlidingExpirationDate < item.AbsoluteExpirationTimeUtc))
            {
                return new DateTime(
                    ticks: SlidingExpirationDate.Ticks,
                    kind: DateTimeKind.Utc);
            }
            else
            {
                return item.AbsoluteExpirationTimeUtc;
            }
        }
    }
}
