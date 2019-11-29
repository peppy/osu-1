namespace Unosquare.Labs.LiteLib
{
    /// <summary>
    /// Base model class for ISQLiteEntity.
    /// Inherit from this model if you don't want to implement the ID property.
    /// </summary>
    public abstract class LiteModel : ILiteModel
    {
        /// <inheritdoc />
        public long ID { get; set; }
    }
}
