namespace Unosquare.Labs.LiteLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides basic DDL and CRUD command definitions and a DI-supplied Context.
    /// </summary>
    public interface ILiteDbSet
    {
        /// <summary>
        /// Gets the name of the data-backing table.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Gets the table definition.
        /// </summary>
        string TableDefinition { get; }

        /// <summary>
        /// Gets the select command definition.
        /// </summary>
        string SelectDefinition { get; }

        /// <summary>
        /// Gets the insert command definition.
        /// </summary>
        string InsertDefinition { get; }

        /// <summary>
        /// Gets the update command definition.
        /// </summary>
        string UpdateDefinition { get; }

        /// <summary>
        /// Gets the delete command definition.
        /// </summary>
        string DeleteDefinition { get; }

        /// <summary>
        /// Gets the delete definition where.
        /// </summary>
        string DeleteDefinitionWhere { get; }

        /// <summary>
        /// Gets any definition.
        /// </summary>
        string AnyDefinition { get; }

        /// <summary>
        /// Gets or sets the parent set context.
        /// </summary>
        LiteDbContext Context { get; set; }

        /// <summary>
        /// Provides and asynchronous counterpart to the Count method.
        /// </summary>
        /// <returns>A Task with the total number of rows.</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Counts the total number of rows in the table.
        /// </summary>
        /// <returns>
        /// The total number of rows.
        /// </returns>
        int Count();

        /// <summary>
        /// Provides and asynchronous counterpart to the Count method.
        /// </summary>
        /// <param name="whereText">The where text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns>
        /// A Task with the total number of rows.
        /// </returns>
        Task<int> CountAsync(string whereText, object whereParams = null);

        /// <summary>
        /// Provides and asynchronous counterpart to the Count method.
        /// </summary>
        /// <param name="whereText">The where text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns>
        /// The total number of rows.
        /// </returns>
        int Count(string whereText, object whereParams = null);

        /// <summary>
        /// Check if the row exist in the table.
        /// </summary>
        /// <returns><c>true</c> if the query contains data, otherwise <c>false</c>.</returns>
        bool Any();

        /// <summary>
        /// Check if the row exist in the table.
        /// </summary>
        /// <param name="whereText">The where text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns><c>true</c> if the query contains data, otherwise <c>false</c>.</returns>
        bool Any(string whereText, object whereParams = null);

        /// <summary>
        /// Check asynchronous if the row exist in the table.
        /// </summary>
        /// <param name="whereText">The where text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> AnyAsync(string whereText, object whereParams = null);

        /// <summary>
        /// Check asynchronous if the table contains data.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> AnyAsync();
    }

    /// <summary>
    /// Provides typed access to querying the database.
    /// </summary>
    /// <typeparam name="T">The type of LiteModel.</typeparam>
    public interface ILiteDbSet<T> : ILiteDbSet
        where T : ILiteModel
    {
        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The number of rows inserted.</returns>
        int Insert(T entity);

        /// <summary>
        /// Deletes the specified entity. ID must be set.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The number of rows deleted.</returns>
        int Delete(T entity);

        /// <summary>
        /// Updates the specified entity in a non optimistic concurrency manner.
        /// ID must be set.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The number of rows updated.</returns>
        int Update(T entity);

        /// <summary>
        /// Selects a set of entities from the database.
        /// Example whereText = "X = @X" and whereParams = new { X = "hello" }.
        /// </summary>
        /// <param name="whereText">The where text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns>A Enumerable with generic type.</returns>
        IEnumerable<T> Select(string whereText, object whereParams);

        /// <summary>
        /// Selects all entities from the database.
        /// </summary>
        /// <returns>A Enumerable with generic type.</returns>
        IEnumerable<T> SelectAll();

        /// <summary>
        /// Selects a single entity from the database given its row id.
        /// </summary>
        /// <param name="id">The row identifier.</param>
        /// <returns>A generic type.</returns>
        T Single(long id);

        /// <summary>
        /// Inserts the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <exception cref="ArgumentNullException">entities.</exception>
        void InsertRange(IEnumerable<T> entities);

        /// <summary>
        /// Provides and asynchronous counterpart to the Insert method.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>A Task with the number of rows inserted.</returns>
        Task<int> InsertAsync(T entity);

        /// <summary>
        /// Provides and asynchronous counterpart to the Delete method.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>A Task with the number of rows deleted.</returns>
        Task<int> DeleteAsync(T entity);

        /// <summary>
        /// Provides and asynchronous counterpart to the Update method.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>A Task with the number of rows updated.</returns>
        Task<int> UpdateAsync(T entity);

        /// <summary>
        /// Deletes the specified where text.
        /// Example whereText = "X = @X" and whereParams = new { X = "hello" }.
        /// </summary>
        /// <param name="whereText">The where text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns>A Enumerable with generic type.</returns>
        int Delete(string whereText, object whereParams);

        /// <summary>
        /// Deletes the asynchronous.
        /// Example whereText = "X = @X" and whereParams = new { X = "hello" }.
        /// </summary>
        /// <param name="whereText">The where text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns>A Task of type Enumerable with a generic type.</returns>
        Task<int> DeleteAsync(string whereText, object whereParams);

        /// <summary>
        /// Provides and asynchronous counterpart to the Select method.
        /// </summary>
        /// <param name="whereText">The where text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns>A Task of type Enumerable with a generic type.</returns>
        Task<IEnumerable<T>> SelectAsync(string whereText, object whereParams);

        /// <summary>
        /// Selects all asynchronous.
        /// </summary>
        /// <returns>A Task of type Enumerable with a generic type.</returns>
        Task<IEnumerable<T>> SelectAllAsync();

        /// <summary>
        /// Provides and asynchronous counterpart to the Single method.
        /// </summary>
        /// <param name="id">The row identifier.</param>
        /// <returns>
        /// A Task with a generic type.
        /// </returns>
        Task<T> SingleAsync(long id);

        /// <summary>
        /// Retrieves the first or default record in the <c>DbSet</c>
        /// filtering by the field and value provided.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>A generic type.</returns>
        T FirstOrDefault(string fieldName, object fieldValue);

        /// <summary>
        /// Retrieves the first or default record in the <c>DbSet</c>
        /// filtering by the field and value provided.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>A Task with a generic type.</returns>
        Task<T> FirstOrDefaultAsync(string fieldName, object fieldValue);

        /// <summary>
        /// Retrieves the first or default record in the <c>DbSet</c>
        /// filtering by the field and value provided.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>A generic type.</returns>
        T FirstOrDefault<TProperty>(Expression<Func<T, TProperty>> field, object fieldValue);

        /// <summary>
        /// Retrieves the first or default record in the <c>DbSet</c>
        /// filtering by the field and value provided.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>A Task with a generic type.</returns>
        Task<T> FirstOrDefaultAsync<TProperty>(Expression<Func<T, TProperty>> field, object fieldValue);
    }
}
