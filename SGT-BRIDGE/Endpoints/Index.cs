namespace SGT_BRIDGE.Endpoints
{
    public static class IndexEndpoint
    {
        public static void RegisterIndexEndpoint(this WebApplication app)
        {
            app.MapGet("/", () => "Subiekt GT REST API")
                .ExcludeFromDescription();
        }
    }
}
