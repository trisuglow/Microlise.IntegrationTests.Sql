namespace Microlise.IntegrationTests.Sql
{
    public class MicroliseIntegrationTestsSqlConfiguration : IMicroliseIntegrationTestsSqlConfiguration
    {
        public string ConnectionString { get; set; } = "";

        public Dictionary<string, List<string>> TestFilters { get; set; } = [];
    }
}
