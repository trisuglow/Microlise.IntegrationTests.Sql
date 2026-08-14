namespace Microlise.IntegrationTests.Sql
{
    /// <summary>
    /// Regex defining the allowable format of the exclusion.
    /// </summary>
    /// <param name="format"></param>
    [AttributeUsage(AttributeTargets.Class)]
    public class FilterFormatAttribute(string format) : Attribute
    {
        public string Format = format;
    }
}
