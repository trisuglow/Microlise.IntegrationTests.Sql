namespace Microlise.IntegrationTests.Sql
{
    /// <summary>
    /// Regex defining the allowable format of the exclusion.
    /// </summary>
    /// <param name="format"></param>
    [AttributeUsage(AttributeTargets.Method)]
    public class FilterFormatAttribute(string format) : Attribute
    {
        public string Format = format;
    }
}
