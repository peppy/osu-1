using System.ComponentModel.DataAnnotations.Schema;

namespace Unosquare.Labs.LiteLib
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Swan.Validators;

    internal class TypeDefinition
    {
        private const int max_string_length = 4096;
        private const BindingFlags public_instance_flags = BindingFlags.Instance | BindingFlags.Public;

        private static readonly ConcurrentDictionary<Type, DefinitionCacheItem> definition_cache =
            new ConcurrentDictionary<Type, DefinitionCacheItem>();

        private readonly StringBuilder _createBuilder = new StringBuilder();
        private readonly List<string> _propertyNames = new List<string>();

        public TypeDefinition(Type type)
        {
            if (definition_cache.ContainsKey(type))
            {
                Definition = definition_cache[type];
                return;
            }

            Definition = new DefinitionCacheItem();
            parseType(type);

            definition_cache[type] = Definition;
        }

        public DefinitionCacheItem Definition { get; }

        private void parseType(Type type)
        {
            var indexBuilder = new List<string>();

            var properties = type.GetProperties(public_instance_flags);

            // Start off with the table name
            Definition.TableName = type.Name;
            var tableAttribute = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
                Definition.TableName = tableAttribute.Name;

            _createBuilder.AppendLine($"CREATE TABLE IF NOT EXISTS [{Definition.TableName}] (");
            _createBuilder.AppendLine($"    [{nameof(ILiteModel.ID)}] INTEGER PRIMARY KEY AUTOINCREMENT,");

            foreach (var property in properties)
            {
                parseProperty(property, indexBuilder);
            }

            if (_propertyNames.Any() == false)
                throw new InvalidOperationException("Invalid DbSet, you need at least one property to bind");

            // trim out the extra comma
            _createBuilder.Remove(_createBuilder.Length - Environment.NewLine.Length - 1, Environment.NewLine.Length + 1);

            _createBuilder.AppendLine();
            _createBuilder.AppendLine(");");

            foreach (var indexDdl in indexBuilder)
            {
                _createBuilder.AppendLine(indexDdl);
            }

            var escapedColumnNames = string.Join(", ", _propertyNames.Select(p => $"[{p}]").ToArray());
            var parameterColumnNames = string.Join(", ", _propertyNames.Select(p => $"@{p}").ToArray());
            var keyValueColumnNames = string.Join(", ", _propertyNames.Select(p => $"[{p}] = @{p}").ToArray());

            Definition.TableDefinition = _createBuilder.ToString();
            Definition.PropertyNames = _propertyNames.ToArray();

            Definition.SelectDefinition =
                $"SELECT [{nameof(ILiteModel.ID)}], {escapedColumnNames} FROM [{Definition.TableName}]";
            Definition.InsertDefinition =
                $"INSERT INTO [{Definition.TableName}] ({escapedColumnNames}) VALUES ({parameterColumnNames}); SELECT last_insert_ID();";
            Definition.UpdateDefinition =
                $"UPDATE [{Definition.TableName}] SET {keyValueColumnNames}  WHERE [{nameof(ILiteModel.ID)}] = @{nameof(ILiteModel.ID)}";
            Definition.DeleteDefinition =
                $"DELETE FROM [{Definition.TableName}] WHERE [{nameof(ILiteModel.ID)}] = @{nameof(ILiteModel.ID)}";
            Definition.DeleteDefinitionWhere = $"DELETE FROM [{Definition.TableName}]";
            Definition.AnyDefinition = $"SELECT EXISTS(SELECT 1 FROM '{Definition.TableName}')";
        }

        private void parseProperty(PropertyInfo property, List<string> indexBuilder)
        {
            string propName = getPropertyName(property);

            if (propName == nameof(ILiteModel.ID) || property.CanWrite == false)
                return;

            // Skip if not mapped
            var notMappedAttribute = property.GetCustomAttribute<NotMappedAttribute>();
            if (notMappedAttribute != null)
                return;

            // Add to indexes if indexed attribute is ON
            var indexedAttribute = property.GetCustomAttribute<LiteIndexAttribute>();

            if (indexedAttribute != null)
            {
                indexBuilder.Add(
                    $"CREATE INDEX IF NOT EXISTS [IX_{Definition.TableName}_{propName}] ON [{Definition.TableName}] ([{propName}]);");
            }

            // Add to unique indexes if indexed attribute is ON
            var uniqueIndexAttribute = property.GetCustomAttribute<LiteUniqueAttribute>();

            if (uniqueIndexAttribute != null)
            {
                indexBuilder.Add(
                    $"CREATE UNIQUE INDEX IF NOT EXISTS [IX_{Definition.TableName}_{propName}] ON [{Definition.TableName}] ([{propName}]);");
            }

            generatePropertyInfo(property);
        }

        private string getPropertyName(PropertyInfo property)
        {
            return property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name;
        }

        private void generatePropertyInfo(PropertyInfo property)
        {
            string propName = getPropertyName(property);

            var isValidProperty = false;
            var propertyType = property.PropertyType;
            var isNullable = propertyType.GetTypeInfo().IsGenericType &&
                             propertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable) propertyType = Nullable.GetUnderlyingType(propertyType);
            var nullStatement = isNullable ? "NULL" : "NOT NULL";

            if (propertyType == typeof(string))
            {
                isValidProperty = true;

                var stringLength = property.GetCustomAttribute<StringLengthAttribute>()?.MaximumLength ?? max_string_length;

                isNullable = property.GetCustomAttribute<NotNullAttribute>() == null;

                nullStatement = isNullable ? "NULL" : "NOT NULL";

                if (stringLength != max_string_length)
                {
                    var checkLength = $"length({propName})<={stringLength}";
                    _createBuilder.AppendLine(
                        $"    [{propName}] NVARCHAR({stringLength}) {nullStatement} CHECK({checkLength}),");
                }
                else
                {
                    _createBuilder.AppendLine($"    [{propName}] NVARCHAR({stringLength}) {nullStatement},");
                }
            }
            else if (propertyType.GetTypeInfo().IsValueType)
            {
                isValidProperty = true;
                _createBuilder.AppendLine(
                    $"    [{propName}] {propertyType.GetTypeMapping()} {nullStatement},");
            }
            else if (propertyType == typeof(byte[]))
            {
                isValidProperty = true;
                _createBuilder.AppendLine($"    [{propName}] BLOB {nullStatement},");
            }

            if (isValidProperty)
            {
                _propertyNames.Add(propName);
            }
        }
    }
}
