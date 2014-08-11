/* SessionProperties.cs - (c) James S Renwick 2014
 * -----------------------------------------------
 * Version 1.2.0
 */
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;

namespace DebugOS
{
    [Serializable]
    public class SessionProperties : Dictionary<string, string>
    {
        public SessionProperties() : base()
        { 

        }
        public SessionProperties(SessionProperties properties) : base(properties)
        {

        }
        public SessionProperties(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public new string this[string key]
        {
            get { return base.ContainsKey(key) ? base[key] : null; }
            set { base[key] = value; }
        }

        /// <summary>
        /// Determines whether the SessionProperties contains the specified key
        /// and one whose value is not null (Nothing in Visual Basic.)
        /// </summary>
        /// <param name="key">The key to locate in the SessionProperties.</param>
        /// <returns>
        /// true if the SessionProperties contains an element with the specified key,
        /// whose value is not null; otherwise, false.
        /// </returns>
        public bool ContainsKeyWithValue(string key)
        {
            return this.ContainsKey(key) && this[key] != null;
        }

        /// <summary>
        /// Creates a dynamic version of the SessionProperties object which routes
        /// requests for properties to the indexer.
        /// </summary>
        [Obsolete]
        public DynamicMetaObject GetMetaObject(System.Linq.Expressions.Expression parameter)
        {
            return new DynamicSessionProperties(parameter, this);
        }
    }
}
