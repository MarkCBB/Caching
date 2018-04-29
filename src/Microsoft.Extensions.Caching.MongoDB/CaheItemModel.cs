using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Caching.MongoDB
{
    [BsonIgnoreExtraElements]
    public class CaheItemModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
