namespace Unosquare.Labs.LiteLib
{
    using Dapper;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a ILiteDbSet implementation.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    /// <seealso cref="LiteLib.ILiteDbSet{T}" />
    public class LiteDbSet<T> : ILiteDbSet<T>
        where T : ILiteModel, new()
    {
        private readonly DefinitionCacheItem _tableDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteDbSet{T}"/> class.
        /// </summary>
        public LiteDbSet()
        {
            _tableDefinition = new TypeDefinition(typeof(T)).Definition;
        }

        #region Events

        /// <summary>
        /// Occurs when [on before insert].
        /// </summary>
        public event EventHandler<EntityEventArgs<T>> OnBeforeInsert = (s, e) => { };

        /// <summary>
        /// Occurs when [on after insert].
        /// </summary>
        public event EventHandler<EntityEventArgs<T>> OnAfterInsert = (s, e) => { };

        /// <summary>
        /// Occurs when [on before update].
        /// </summary>
        public event EventHandler<EntityEventArgs<T>> OnBeforeUpdate = (s, e) => { };

        /// <summary>
        /// Occurs when [on after update].
        /// </summary>
        public event EventHandler<EntityEventArgs<T>> OnAfterUpdate = (s, e) => { };

        /// <summary>
        /// Occurs when [on before delete].
        /// </summary>
        public event EventHandler<EntityEventArgs<T>> OnBeforeDelete = (s, e) => { };

        /// <summary>
        /// Occurs when [on after delete].
        /// </summary>
        public event EventHandler<EntityEventArgs<T>> OnAfterDelete = (s, e) => { };

        #endregion Events

        #region Properties

        /// <inheritdoc />
        public string SelectDefinition => _tableDefinition.SelectDefinition;

        /// <inheritdoc />
        public string InsertDefinition => _tableDefinition.InsertDefinition;

        /// <inheritdoc />
        public string UpdateDefinition => _tableDefinition.UpdateDefinition;

        /// <inheritdoc />
        public string DeleteDefinition => _tableDefinition.DeleteDefinition;

        /// <inheritdoc />
        public string DeleteDefinitionWhere => _tableDefinition.DeleteDefinitionWhere;

        /// <inheritdoc />
        public string AnyDefinition => _tableDefinition.AnyDefinition;

        /// <inheritdoc />
        public string TableDefinition => _tableDefinition.TableDefinition;

        /// <inheritdoc />
        public string TableName => _tableDefinition.TableName;

        /// <inheritdoc />
        public LiteDbContext Context { get; set; }

        /// <summary>
        /// Gets or sets the property names.
        /// </summary>
        public IReadOnlyCollection<string> PropertyNames => _tableDefinition.PropertyNames;

        #endregion Properties

        #region Methods and Data Access

        /// <inheritdoc />
        public int Insert(T entity) => InsertAsync(entity).GetAwaiter().GetResult();

        /// <inheritdoc />
        public async Task<int> InsertAsync(T entity)
        {
            var args = new EntityEventArgs<T>(entity, this);
            OnBeforeInsert(this, args);
            if (args.Cancel) return 0;

            Context.LogSqlCommand(InsertDefinition, entity);
            var result = await Context.Connection.QueryAsync<long>(InsertDefinition, entity).ConfigureAwait(false);

            if (result.Any() == false) return 0;

            entity.ID = result.First();
            OnAfterInsert(this, args);

            return 1;
        }

        /// <inheritdoc />
        public void InsertRange(IEnumerable<T> entities)
        {
            if (entities == null || entities.Any() == false)
                throw new ArgumentNullException(nameof(entities));

            var escapedColumnNames = string.Join(", ", PropertyNames.Select(p => $"[{p}]").ToArray());
            var command = $"INSERT INTO [{TableName}] ({escapedColumnNames})";
            var baseTypeProperties =
                typeof(T).GetTypeInfo().GetProperties().Where(x => PropertyNames.Contains(x.Name)).ToArray();

            command += string.Join("UNION ALL ",
                entities.Select(
                    entity => "SELECT " + string.Join(", ",
                                  baseTypeProperties.Select(p => $"'{p.GetValue(entity)}'"))));

            Context.Connection.ExecuteScalar(command);
        }

        /// <inheritdoc />
        public int Delete(string whereText, object whereParams = null) => Context.Delete(this, whereText, whereParams);

        /// <inheritdoc />
        public Task<int> DeleteAsync(string whereText, object whereParams = null) => Context.DeleteAsync(this, whereText, whereParams);

        /// <inheritdoc />
        public int Delete(T entity) => DeleteAsync(entity).GetAwaiter().GetResult();

        /// <inheritdoc />
        public async Task<int> DeleteAsync(T entity)
        {
            if (entity.ID == default)
                throw new ArgumentException(nameof(entity.ID));

            var args = new EntityEventArgs<T>(entity, this);
            OnBeforeDelete(this, args);
            if (args.Cancel) return 0;

            Context.LogSqlCommand(DeleteDefinition, entity);
            var affected = await Context.Connection.ExecuteAsync(DeleteDefinition, entity).ConfigureAwait(false);
            entity.ID = default;
            OnAfterDelete(this, args);

            return affected;
        }

        /// <inheritdoc />
        public int Update(T entity) => UpdateAsync(entity).GetAwaiter().GetResult();

        /// <inheritdoc />
        public async Task<int> UpdateAsync(T entity)
        {
            var args = new EntityEventArgs<T>(entity, this);
            OnBeforeUpdate(this, args);
            if (args.Cancel) return 0;

            Context.LogSqlCommand(UpdateDefinition, entity);
            var affected = await Context.Connection.ExecuteAsync(UpdateDefinition, entity).ConfigureAwait(false);
            OnAfterUpdate(this, args);

            return affected;
        }

        /// <inheritdoc />
        public IEnumerable<T> Select(string whereText, object whereParams = null) => Context.Select<T>(this, whereText, whereParams);

        /// <inheritdoc />
        public IEnumerable<T> SelectAll() => Select("1 = 1");

        /// <inheritdoc />
        public Task<IEnumerable<T>> SelectAsync(string whereText, object whereParams = null) => Context.SelectAsync<T>(this, whereText, whereParams);

        /// <inheritdoc />
        public Task<IEnumerable<T>> SelectAllAsync() => SelectAsync("1 = 1");

        /// <inheritdoc />
        public T FirstOrDefault(string fieldName, object fieldValue) => Select($"[{fieldName}] = @FieldValue", new { FieldValue = fieldValue }).FirstOrDefault();

        /// <inheritdoc />
        public T FirstOrDefault<TProperty>(Expression<Func<T, TProperty>> field, object fieldValue)
            => FirstOrDefault(getFieldName(field), fieldValue);

        /// <inheritdoc />
        public async Task<T> FirstOrDefaultAsync(string fieldName, object fieldValue)
        {
            var result = await SelectAsync($"[{fieldName}] = @FieldValue", new { FieldValue = fieldValue })
                .ConfigureAwait(false);

            return result.FirstOrDefault();
        }

        /// <inheritdoc />
        public Task<T> FirstOrDefaultAsync<TProperty>(Expression<Func<T, TProperty>> field, object fieldValue)
            => FirstOrDefaultAsync(getFieldName(field), fieldValue);

        /// <inheritdoc />
        public T Single(long id) => Select($"[{nameof(ILiteModel.ID)}] = @{nameof(ILiteModel.ID)}", new { ID = id })
            .Single();

        /// <inheritdoc />
        public async Task<T> SingleAsync(long id)
        {
            var result = await SelectAsync($"[{nameof(ILiteModel.ID)}] = @{nameof(ILiteModel.ID)}", new { ID = id })
                .ConfigureAwait(false);

            return result.Single();
        }

        /// <inheritdoc />
        public int Count(string whereText, object whereParams = null)
            => Context.ExecuteScalar<int>($"SELECT COUNT(*) FROM [{TableName}] WHERE {whereText}", whereParams);

        /// <inheritdoc />
        public int Count()
            => Context.ExecuteScalar<int>($"SELECT COUNT(*) FROM [{TableName}]");

        /// <inheritdoc />
        public Task<int> CountAsync(string whereText, object whereParams = null)
            => Context.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM [{TableName}] WHERE {whereText}", whereParams);

        /// <inheritdoc />
        public Task<int> CountAsync()
            => Context.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM [{TableName}]");

        /// <inheritdoc />
        public bool Any(string whereText, object whereParams = null)
            => Context.ExecuteScalar<bool>($"SELECT EXISTS(SELECT 1 FROM '{TableName}' WHERE {whereText})", whereParams);

        /// <inheritdoc />
        public bool Any()
            => Context.ExecuteScalar<bool>(AnyDefinition);

        /// <inheritdoc />
        public Task<bool> AnyAsync(string whereText, object whereParams = null)
            => Context.ExecuteScalarAsync<bool>($"SELECT EXISTS(SELECT 1 FROM '{TableName}' WHERE {whereText})", whereParams);

        /// <inheritdoc />
        public Task<bool> AnyAsync()
            => Context.ExecuteScalarAsync<bool>(AnyDefinition);

        #endregion Methods and Data Access

        private string getFieldName<TProperty>(Expression<Func<T, TProperty>> field)
        {
            var fieldInfo = (field.Body as MemberExpression)?.Member as PropertyInfo;

            return fieldInfo == null ? throw new ArgumentNullException(nameof(fieldInfo)) : fieldInfo.Name;
        }
    }
}
