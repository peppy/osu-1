namespace Unosquare.Labs.LiteLib
{
    using System;

    /// <summary>
    /// Represents a Entity EventArg.
    /// </summary>
    /// <typeparam name="T">The type of LiteModel.</typeparam>
    /// <seealso cref="System.EventArgs" />
    public class EntityEventArgs<T> : EventArgs
        where T : ILiteModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityEventArgs{T}"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="liteDbSet">The database set.</param>
        public EntityEventArgs(T entity, ILiteDbSet<T> liteDbSet)
        {
            DbSet = liteDbSet;
            Entity = entity;
            Cancel = false;
        }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public T Entity { get; protected set; }

        /// <summary>
        /// Gets or sets the database set.
        /// </summary>
        /// <value>
        /// The database set.
        /// </value>
        public ILiteDbSet<T> DbSet { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="EntityEventArgs{T}"/> is cancel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancel; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel { get; set; }
    }
}