namespace DesolaServices.Interfaces
{
    public interface ITableBase<T> where T : class
    {
        Task InsertTableEntityAsync(T entity);

        Task<T> GetTableEntityAsync(string partitionKey, string rowKey);

        Task UpdateTableEntityAsync(T entity);
    }
}