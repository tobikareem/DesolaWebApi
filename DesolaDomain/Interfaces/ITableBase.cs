namespace DesolaDomain.Interfaces
{
    public interface ITableBase<T> where T : class
    {
        Task InsertTableEntityAsync(T entity);

        Task<T> GetTableEntityAsync(string partitionKey, string rowKey);

        Task UpdateTableEntityAsync(T entity);

        Task<(List<T> Items, string ContinuationToken)> GetTableEntitiesByQueryAsync(string query, int pageSize, string decodedToken);
    }
    
}