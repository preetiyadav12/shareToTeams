
namespace Microsoft.Teams.EmbeddedChat
{
    public static class Constants
    {
        public const string Orchestration = "EntityOrchestration";

        public const string GetEntityStateActivity = "GetEntityState";
        public const string GetEntityStateRoute = "entity/mapping";

        public const string CreateEntityStateActivity = "CreateEntityMapping";

        public const string UpdateEntityStateActivity = "UpdateEntityState";
        public const string UpdateEntityStateRoute = "entity/mapping/update";

        public const string EntityMappingAPIHttpPost = "EntityMappingAPI";
        public const string EntityUpdateAPIHttpPost = "EntityUpdateAPI";

        // Diagnostics API
        public const string GetOrchestrationStatus = "GetStatus";
        public const string TerminateOrchestration = "TerminateOrchestration";
        public const string GetAllFlows = "GetAllFlows";
        public const string GetCompletedFlows = "GetCompletedFlows";
        public const string GetNotCompletedFlows = "GetNotCompletedFlows";

        // Execution timeout and retry
        public const int Timeout = 30;
        public const int RetryInterval = 1;
    }
}
