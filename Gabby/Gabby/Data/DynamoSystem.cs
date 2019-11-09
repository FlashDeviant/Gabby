namespace Gabby.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.DataModel;
    using Amazon.DynamoDBv2.DocumentModel;
    using JetBrains.Annotations;

    internal static class DynamoSystem
    {
        private static readonly AmazonDynamoDBClient Client = new AmazonDynamoDBClient();
        private static readonly DynamoDBContext Context = new DynamoDBContext(Client);

        #region GET

        [ItemCanBeNull]
        public static Task<T> GetItemAsync<T>([NotNull] object hash)
        {
            return Context.LoadAsync<T>(hash.ToString());
        }

        #endregion

        #region PUT

        public static async Task PutItemAsync<T>(T item)
        {
            await Context.SaveAsync(item).ConfigureAwait(false);
        }

        #endregion

        #region SCAN

        public static Task<List<T>> ScanItemAsync<T>()
        {
            var search = Context.FromScanAsync<T>(new ScanOperationConfig {ConsistentRead = true});

            return search.GetRemainingAsync();
        }

        #endregion

        #region QUERY

        public static Task<List<T>> QueryItemAsync<T>(string columnName, QueryOperator queryOperator, string queryValue)
        {
            string indexName = null;
            foreach (var property in typeof(T).GetProperties())
            {
                if (property.CustomAttributes.Any(attribute =>
                    attribute.AttributeType == typeof(DynamoDBHashKeyAttribute)))
                    indexName = property.Name;

                if (!string.IsNullOrEmpty(indexName)) break;
            }

            Console.WriteLine();
            var search = Context.FromQueryAsync<T>(new QueryOperationConfig
            {
                IndexName = indexName,
                Filter = new QueryFilter(columnName, queryOperator, queryValue)
            });

            return search.GetRemainingAsync();
        }

        #endregion

        #region UPDATE

        public static async Task UpdateItemAsync<T>(T item)
        {
            await Context.SaveAsync(item).ConfigureAwait(false);
        }

        #endregion

        #region DELETE

        public static async Task DeleteItemAsync<T>(T item)
        {
            await Context.DeleteAsync(item).ConfigureAwait(false);
        }

        #endregion
    }
}