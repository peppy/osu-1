namespace Unosquare.Labs.LiteLib
{
    internal class DefinitionCacheItem
    {
        public string TableName { get; set; }

        public string TableDefinition { get; set; }

        public string SelectDefinition { get; set; }

        public string InsertDefinition { get; set; }

        public string UpdateDefinition { get; set; }

        public string DeleteDefinition { get; set; }

        public string DeleteDefinitionWhere { get; set; }

        public string AnyDefinition { get; set; }

        public string[] PropertyNames { get; set; }
    }
}