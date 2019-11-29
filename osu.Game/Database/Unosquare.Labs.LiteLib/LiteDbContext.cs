namespace Unosquare.Labs.LiteLib
{
    using Dapper;
    using System.Diagnostics;
    using System.Data;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Swan;
    using Swan.Reflection;
    using Swan.Logging;
#if !NET461
    using Microsoft.Data.Sqlite;
#endif

    /// <summary>
    /// A base class containing all the functionality to perform data operations on Entity Sets.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public abstract class LiteDbContext : IDisposable
    {
        private static readonly PropertyTypeCache property_info_cache = new PropertyTypeCache();
        private static readonly Type generic_lite_db_set_type = typeof(LiteDbSet<>);

        private readonly Dictionary<string, ILiteDbSet> _entitySets = new Dictionary<string, ILiteDbSet>();
        private readonly Type _contextType;

        private bool _isDisposing; // To detect redundant calls

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteDbContext" /> class.
        /// </summary>
        /// <param name="databaseFilePath">The database file path.</param>
        /// <param name="enabledLog">if set to <c>true</c> [enabled log].</param>
        protected LiteDbContext(string databaseFilePath, bool enabledLog = true)
        {
            EnabledLog = enabledLog;
            _contextType = GetType();
            loadEntitySets();

            databaseFilePath = Path.GetFullPath(databaseFilePath);
            var databaseExists = File.Exists(databaseFilePath);

#if NET461
            Connection = new Mono.Data.Sqlite.SqliteConnection($"URI=file:{databaseFilePath}");
#else
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = databaseFilePath,
            };

            Connection = new SqliteConnection(builder.ToString());
#endif
            Connection.Open();

            if (databaseExists == false)
            {
                "DB file does not exist. Creating.".Debug(nameof(LiteDbContext));
                createDatabase();
                $"DB file created: '{databaseFilePath}'".Debug(nameof(LiteDbContext));
            }

            UniqueId = Guid.NewGuid();
        }

        /// <summary>
        /// Occurs when [on database created].
        /// </summary>
        public event EventHandler OnDatabaseCreated = (s, e) => { };

        #region Properties

        /// <summary>
        /// Gets the underlying SQLite connection.
        /// </summary>
        public IDbConnection Connection { get; private set; }

        /// <summary>
        /// Gets the unique identifier of this context.
        /// </summary>
        public Guid UniqueId { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enabled log].
        /// </summary>
        public bool EnabledLog { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Vacuums the database asynchronously.
        /// </summary>
        /// <returns>
        /// A Task that represent the Execution of the Vacuum command.
        /// </returns>
        public async Task VaccuumDatabaseAsync()
        {
            "DB VACUUM command executing.".Debug(nameof(LiteDbContext));
            await Connection.ExecuteAsync("VACUUM").ConfigureAwait(false);
            "DB VACUUM command finished.".Debug(nameof(LiteDbContext));
        }

        /// <summary>
        /// Sets the specified entity type.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>
        /// A non-generic liteDbSet instance for access to entities of the given type in the context and the underlying store.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws an ArgumentOutOfRangeException.</exception>
        public ILiteDbSet Set(Type entityType)
        {
            var set = _entitySets.Values.FirstOrDefault(x =>
                x.GetType().GetTypeInfo().GetGenericArguments().Any(z => z == entityType));

            return set ?? throw new ArgumentOutOfRangeException(nameof(entityType));
        }

        /// <summary>
        /// Sets this instance.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <returns>
        /// A liteDbSet instance for access to entities of the given type in the context and the underlying store.
        /// </returns>
        public ILiteDbSet<TEntity> Set<TEntity>() where TEntity : ILiteModel => (ILiteDbSet<TEntity>)Set(typeof(TEntity));

        /// <summary>
        /// Gets the set names.
        /// </summary>
        /// <returns>An array of strings of the entities.</returns>
        public string[] GetSetNames() => _entitySets.Keys.ToArray();

        /// <summary>
        /// Selects the specified set.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="set">The set.</param>
        /// <param name="whereText">The where text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns>An enumerable of type of the Entity.</returns>
        public IEnumerable<TEntity> Select<TEntity>(ILiteDbSet set, string whereText, object whereParams = null)
            => Query<TEntity>($"{set.SelectDefinition} WHERE {whereText}", whereParams);

        /// <summary>
        /// Deletes the specified set.
        /// </summary>
        /// <param name="set">The set.</param>
        /// <param name="whereText">The where text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns>A count for affected rows.</returns>
        public int Delete(ILiteDbSet set, string whereText, object whereParams = null)
            => DeleteAsync(set, whereText, whereParams).GetAwaiter().GetResult();

        /// <summary>
        /// Deletes the asynchronous.
        /// </summary>
        /// <param name="set">The set.</param>
        /// <param name="whereText">The where text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns>A count for affected rows.</returns>
        public Task<int> DeleteAsync(ILiteDbSet set, string whereText, object whereParams = null)
        {
            LogSqlCommand($"{set.DeleteDefinitionWhere} WHERE {whereText}", whereParams);
            return Connection.ExecuteAsync($"{set.DeleteDefinitionWhere} WHERE {whereText}", whereParams);
        }

        /// <summary>
        /// Selects the asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="set">The set.</param>
        /// <param name="whereText">The where text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns>A Task with a enumerable of type of the entity.</returns>
        public Task<IEnumerable<TEntity>> SelectAsync<TEntity>(
            ILiteDbSet set,
            string whereText,
            object whereParams = null)
        {
            return QueryAsync<TEntity>($"{set.SelectDefinition} WHERE {whereText}", whereParams);
        }

        /// <summary>
        /// Queries the specified set.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="commandText">The command text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns>
        /// An enumerable of the type of the entity.
        /// </returns>
        public IEnumerable<TEntity> Query<TEntity>(string commandText, object whereParams = null)
        {
            LogSqlCommand(commandText, whereParams);
            return Connection.Query<TEntity>(commandText, whereParams);
        }

        /// <summary>
        /// Queries the asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="commandText">The command text.</param>
        /// <param name="whereParams">The where parameters.</param>
        /// <returns>
        /// A Task with an enumerable of the type of the entity.
        /// </returns>
        public Task<IEnumerable<TEntity>> QueryAsync<TEntity>(string commandText, object whereParams = null)
        {
            LogSqlCommand(commandText, whereParams);
            return Connection.QueryAsync<TEntity>(commandText, whereParams);
        }

        /// <summary>
        /// Inserts the specified entity without triggering events.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The number of rows inserted.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">entity - The object type must be registered as ILiteDbSet.</exception>
        public int Insert(object entity) => InsertAsync(entity).GetAwaiter().GetResult();

        /// <summary>
        /// Inserts the asynchronous without triggering events.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>A Task with the total number of rows inserted.</returns>
        public async Task<int> InsertAsync(object entity)
        {
            var set = Set(entity.GetType());

            LogSqlCommand(set.InsertDefinition, entity);

            var result = await Connection.QueryAsync<int>(set.InsertDefinition, entity).ConfigureAwait(false);

            return result.Any() ? 1 : 0;
        }

        /// <summary>
        /// Deletes the specified entity without triggering events.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The affected rows count.</returns>
        public int Delete(object entity) => DeleteAsync(entity).GetAwaiter().GetResult();

        /// <summary>
        /// Deletes the asynchronous without triggering events.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>A Task with the affected rows count.</returns>
        public Task<int> DeleteAsync(object entity)
        {
            var set = Set(entity.GetType());

            LogSqlCommand(set.DeleteDefinition, entity);

            return Connection.ExecuteAsync(set.DeleteDefinition, entity);
        }

        /// <summary>
        /// Updates the specified entity without triggering events.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The affected rows count.</returns>
        public int Update(object entity) => UpdateAsync(entity).GetAwaiter().GetResult();

        /// <summary>
        /// Updates the asynchronous without triggering events.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>A task with he affected rows count.</returns>
        public Task<int> UpdateAsync(object entity)
        {
            var set = Set(entity.GetType());

            LogSqlCommand(set.UpdateDefinition, entity);

            return Connection.ExecuteAsync(set.UpdateDefinition, entity);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal T ExecuteScalar<T>(string commandText, object whereParams = null)
        {
            LogSqlCommand(commandText);
            return Connection.ExecuteScalar<T>(commandText, whereParams);
        }

        internal Task<T> ExecuteScalarAsync<T>(string commandText, object whereParams = null)
        {
            LogSqlCommand(commandText);
            return Connection.ExecuteScalarAsync<T>(commandText, whereParams);
        }

        /// <summary>
        /// Logs the SQL command being executed and its arguments.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="arguments">The arguments.</param>
        internal void LogSqlCommand(string command, object arguments = null)
        {
            if (EnabledLog == false || Debugger.IsAttached == false || Terminal.IsConsolePresent == false) return;

            $"> {command}{arguments.Stringify()}".Debug(nameof(LiteDbContext));
        }

        /// <summary>
        /// Loads the entity sets registered as virtual public properties of the derived class.
        /// </summary>
        private void loadEntitySets()
        {
            var contextDbSetProperties = property_info_cache
                .Retrieve(GetType(), t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                        p.PropertyType.GetTypeInfo().IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == generic_lite_db_set_type));

            foreach (var entitySetProp in contextDbSetProperties)
            {
                var entitySetType = entitySetProp.PropertyType.GetGenericArguments()[0];

                if (!(entitySetProp.GetValue(this) is ILiteDbSet currentValue))
                {
                    var instanceType = generic_lite_db_set_type.MakeGenericType(entitySetType);
                    currentValue = Activator.CreateInstance(instanceType) as ILiteDbSet;
                    entitySetProp.SetValue(this, currentValue);
                }

                if (currentValue == null) continue;

                currentValue.Context = this;
                _entitySets[entitySetProp.Name] = currentValue;
            }

            $"Context instance {_contextType.Name} - {_entitySets.Count} entity sets."
                .Debug(nameof(LiteDbContext));
        }

        /// <summary>
        /// Creates the database schema using the entity set DDL generators.
        /// </summary>
        private void createDatabase()
        {
            var ddlBuilder = new StringBuilder();

            foreach (var entitySet in _entitySets)
            {
                ddlBuilder.AppendLine(entitySet.Value.TableDefinition);
            }

            using (var tran = Connection.BeginTransaction())
            {
                Connection.Execute(ddlBuilder.ToString());
                tran.Commit();
                OnDatabaseCreated(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposing) return;

            if (disposing)
            {
                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }

            _isDisposing = true;
        }

        #endregion Methods
    }
}
