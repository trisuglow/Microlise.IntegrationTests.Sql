namespace Microlise.IntegrationTests.Sql
{
    public interface IMicroliseIntegrationTestsSqlConfiguration
    {
        public string ConnectionString { get; set; }

        public Dictionary<string, List<string>> TestFilters { get; set; }
    }
}
