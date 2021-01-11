using Microsoft.Azure.Cosmos.Table;
using System;

namespace ImageResizeWebApp.Helpers
{
    public static class TableService
    {
        public static CloudTable GetTableReference(string storageConnString, string tableName, bool createIfNotExists = false)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(storageConnString);
            CloudTableClient client = account.CreateCloudTableClient();

            var table = client.GetTableReference(tableName);

            if (createIfNotExists)
            {
                table.CreateIfNotExists();
            }

            return table;
        }

        public static void AddObject<T>(CloudTable table, T value) where T : ITableEntity
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            TableOperation operation = TableOperation.InsertOrReplace(value);
            table.Execute(operation);
        }

        public static string StorageConnectionString
        {
            get { return Environment.GetEnvironmentVariable(TableService.StorageConnectionStringVariableName); }
        }

        /// <summary>
        /// Environment variable name for the storage connection string.
        /// </summary>
        public const string StorageConnectionStringVariableName = "StorageConnectionString";
    }
}
