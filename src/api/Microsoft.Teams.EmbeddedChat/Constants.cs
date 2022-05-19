
namespace Microsoft.Teams.EmbeddedChat
{
    public static class Constants
    {
        public const string Orchestration = "EntityOrchestration";

        // API Routes
        public const string GetEntityStateRoute = "entity";
        public const string CreateEntityStateRoute = "entity/create";

        // API Operations
        public const string GetEntityAPIHttpPost = "GetEntityAPI";
        public const string CreateChatAPIHttpPost = "CreateEntityAPI";

        // Activities
        public const string GetEntityStateActivity = "GetEntityState";
        public const string CreateEntityStateActivity = "CreateEntityState";
        public const string UpdateEntityStateActivity = "UpdateEntityState";
        public const string UpdateParticipantsActivity = "UpdateParticipants";
        public const string CreateOnlineMeetingActivity = "CreateOnlineMeeting";

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
