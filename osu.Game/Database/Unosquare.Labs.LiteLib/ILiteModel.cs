namespace Unosquare.Labs.LiteLib
{
    using System;

    /// <summary>
    /// Provides minimum contract on which a class can be considered a model for
    /// a SQLite entity set.
    /// </summary>
    public interface ILiteModel
    {
        /// <summary>
        /// Gets or sets the native SQLite row identifier.
        /// </summary>
        long ID { get; set; }
    }

    /// <summary>
    /// Hints the DDL generator that an index needs to be created.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class LiteIndexAttribute : Attribute
    {
    }

    /// <summary>
    /// Hints the DDL generator that a UNIQUE index needs to be created.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class LiteUniqueAttribute : Attribute
    {
    }
}
