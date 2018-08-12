using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.Extensions.Caching.MongoDB
{
    public class MongoDBCacheSetAndGetAsyncTestsWithMongoSettings : MongoDBCacheSetAndGetAsyncTests
    {
        public MongoDBCacheSetAndGetAsyncTestsWithMongoSettings()
        {
            mongoCache = GetMongoDbConnectionWithSettings();
        }

        [Fact(Skip = SkipReason)]
        public override void MongoDBCache_AbsoluteExpirationRelativeToNow_Is_After_Sliding_So_Effective_Should_Be_Sliding()
        {
            base.MongoDBCache_AbsoluteExpirationRelativeToNow_Is_After_Sliding_So_Effective_Should_Be_Sliding();
        }

        [Fact(Skip = SkipReason)]
        public override void MongoDBCache_AbsoluteExpirationRelativeToNow_Is_Before_Sliding_So_Effective_Should_Be_AbsoluteExpiration()
        {
            base.MongoDBCache_AbsoluteExpirationRelativeToNow_Is_Before_Sliding_So_Effective_Should_Be_AbsoluteExpiration();
        }

        [Fact(Skip = SkipReason)]
        public override void MongoDBCache_AbsoluteExpiration_Is_After_Sliding_So_Effective_Should_Be_Sliding()
        {
            base.MongoDBCache_AbsoluteExpiration_Is_After_Sliding_So_Effective_Should_Be_Sliding();
        }

        [Fact(Skip = SkipReason)]
        public override void MongoDBCache_AbsoluteExpiration_Is_Before_Sliding_So_Effective_Should_Be_AbsoluteExpiration()
        {
            base.MongoDBCache_AbsoluteExpiration_Is_Before_Sliding_So_Effective_Should_Be_AbsoluteExpiration();
        }

        [Fact(Skip = SkipReason)]
        public override void MongoDBCache_documents_not_old_are_not_deleted_if_Delete_old_values_is_true()
        {
            base.MongoDBCache_documents_not_old_are_not_deleted_if_Delete_old_values_is_true();
        }

        [Fact(Skip = SkipReason)]
        public override void MongoDBCache_Getting_a_non_existing_key_should_be_null()
        {
            base.MongoDBCache_Getting_a_non_existing_key_should_be_null();
        }

        [Fact(Skip = SkipReason)]
        public override void MongoDBCache_old_documents_are_deleted_if_Delete_old_values_is_true()
        {
            base.MongoDBCache_old_documents_are_deleted_if_Delete_old_values_is_true();
        }

        [Fact(Skip = SkipReason)]
        public override void MongoDBCache_Refreshing_a_non_existing_key_should_be_null()
        {
            base.MongoDBCache_Refreshing_a_non_existing_key_should_be_null();
        }

        [Fact(Skip = SkipReason)]
        public override void MongoDBCache_Set_the_same_key_twice_should_get_the_last_value()
        {
            base.MongoDBCache_Set_the_same_key_twice_should_get_the_last_value();
        }
    }
}
