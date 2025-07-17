namespace SGT_BRIDGE.Endpoints
{
    public static class IndexEndpoint
    {
        public static void RegisterIndexEndpoint(this WebApplication app)
        {
            app.MapGet("/", () => "Subiekt GT API Bridge")
                .ExcludeFromDescription();

            app.MapGet("/configuration", (IConfiguration config) =>
            {
                return Configuration(config);
            });
        }

        /// <summary>
        /// Retrieves server configuration
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static Dictionary<string, string> Configuration(IConfiguration config)
        {
            var data = new Dictionary<string, string>();

            foreach (var c in config.AsEnumerable())
            {
                data.Add(c.Key, (c.Value != null) ? c.Value.ToString() : "");
            }

            return data;
        }
    }
}
