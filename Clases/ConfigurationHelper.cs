namespace Api.Web.Dynamics365.Clases
{
    public class ConfigurationHelper
    {
        private static IConfigurationRoot Configuration;

        static ConfigurationHelper()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }

        public static string GetConnectionString(string name)
        {
            return Configuration.GetConnectionString(name);
        }
    }
}
