using System.Collections.Generic;
using System.Linq;

namespace DebugOS
{
    public class ArgumentSet
    {
        private string[] flags;
        private string[] values;
        private Dictionary<string, string> options;
        
        public IEnumerable<string> Flags
        {
            get { return this.flags.Clone() as IEnumerable<string>; }
        }
        public IEnumerable<string> Values
        {
            get { return this.values.Clone() as IEnumerable<string>; }
        }
        public IEnumerable<KeyValuePair<string, string>> Options
        {
            get { return (from x in this.options select x); }
        }

        public bool HasFlagSet(string flag)
        {
            return this.flags.Contains(flag);
        }

        public string this[string option]
        {
            get
            {
                if (this.options.ContainsKey(option)) {
                    return this.options[option];
                }
                else return null;
            }
        }

        public ArgumentSet(string[] args)
        {
            bool beginValues = false;

            var values = new List<string>();
            var flags  = new List<string>();
            var opts   = new Dictionary<string, string>();

            args.Select((a) => a.Trim());

            for (int i = 0; i < args.Length; i++)
            {
                if (beginValues)
                {
                    values.Add(args[i]);
                    continue;
                }
                else if (!args[i].StartsWith("-"))
                {
                    beginValues = true;
                    values.Add(args[i]);
                }
                else if (i == args.Length - 1 || args[i + 1].StartsWith("-")) {
                    flags.Add(args[i]);
                }
                else {
                    opts.Add(args[i], args[++i]);
                }
            }
            this.values  = values.ToArray();
            this.flags   = flags.ToArray();
            this.options = opts;
        }
    }
}
