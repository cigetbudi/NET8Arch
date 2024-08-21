namespace Core.Entities
{
    public class APIResponse<T>
    {
        public string StatusCode { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
        public string[]? ErrorDetails { get; set; }
        public T? Data { get; set; }
    }
}