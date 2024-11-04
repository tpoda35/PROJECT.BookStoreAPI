namespace BookStoreAPI.Helpers
{
    public static class CacheKeys
    {
        public static string GetBookListKey(int page, int pageSize, string? searchTerm)
            => $"books_p{page}_s{pageSize}_{searchTerm ?? "all"}";
    }
}
