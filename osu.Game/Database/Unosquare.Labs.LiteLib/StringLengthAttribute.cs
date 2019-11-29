namespace Unosquare.Labs.LiteLib
{
    using System;

    /// <summary>
    /// Attribute for string length.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class StringLengthAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringLengthAttribute"/> class.
        /// </summary>
        /// <param name="maximumLength">The maximum length.</param>
        public StringLengthAttribute(int maximumLength)
        {
            MaximumLength = maximumLength;
        }

        /// <summary>
        /// Gets the maximum length.
        /// </summary>
        /// <value>
        /// The maximum length.
        /// </value>
        public int MaximumLength { get; }
    }
}
