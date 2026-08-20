namespace Microlise.IntegrationTests.Sql
{
    public class IntegrationTestLibraryConfiguration : IIntegrationTestLibraryConfiguration
    {
        private string _connectionString;

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

    }
}
