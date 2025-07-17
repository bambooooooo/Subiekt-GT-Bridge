namespace SGT_BRIDGE.Services
{
    public class SubiektRequest<T>
    {
        public Func<InsERT.Subiekt, T> Request { get; set; } = default!;
        public TaskCompletionSource<T> CompletionSource { get; } = new();
    }
}
