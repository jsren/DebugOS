/* DynamicSessionProperties.cs - (c) James S Renwick 2014
 * ------------------------------------------------------
 * Version 1.0.0
 */
using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DebugOS
{
    /// <summary>
    /// Allows the session properties to be accessed dynamically as members
    /// rather than through the indexer. 
    /// </summary>
    internal class DynamicSessionProperties : DynamicMetaObject
    {
        private PropertyInfo indexer;
        private SessionProperties sessionProps;

        internal DynamicSessionProperties(Expression exp, SessionProperties sessionProps)
            : base(exp, BindingRestrictions.Empty, sessionProps)
        {
            this.sessionProps = sessionProps;

            // Get the property info for the indexer:
            foreach (PropertyInfo prop in typeof(SessionProperties).GetProperties())
            {
                var indexParams = prop.GetIndexParameters();
                if (indexParams.Length == 1 && indexParams[0].ParameterType == typeof(String))
                {
                    this.indexer = prop;
                    break;
                }
            }
            if (this.indexer == null) // Throw if we can't find the indexer
            {
                throw new MissingFieldException("Cannot create dynamic session properties: " +
                    "the required indexer SessionProperties[System.String^] is missing.");
            }
        }

        /// <summary>
        /// Performs a "get member" expression by getting the session property of 
        /// the requsted member name.
        /// </summary>
        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            String property = binder.Name;

            // Accessing via members restricts keys from containing symbols.
            // Search for a matching key whose symbols are replaced with '_'.
            if (!this.sessionProps.ContainsKey(property))
            {
                StringBuilder builder = new StringBuilder();
                foreach (string key in this.sessionProps.Keys)
                {
                    // Replace bad chars with '_'
                    foreach (char c in key)
                    {
                        if (Char.IsLetterOrDigit(c)) { builder.Append(c); }
                        else builder.Append('_');
                    }

                    // Compare - if equal, use this key
                    if (StringComparer.InvariantCulture.Equals(
                        builder.ToString(), property))
                    {
                        property = key;
                        break;
                    }
                    else builder.Clear();
                }
            }

            return new DynamicMetaObject
            (
                // Read the session property
                Expression.Constant(this.sessionProps[property]),
                // Restrict to the SessionProperties object
                BindingRestrictions.GetTypeRestriction(this.Expression, this.RuntimeType)
             );
        }
    }
}
