using Azure;
using Azure.Data.Tables;
using System;

namespace Microsoft.Teams.EmbeddedChat.Models
{
    public class IBaseTableEntity: ITableEntity
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
