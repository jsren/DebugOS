using System;
using System.Collections.Generic;

namespace DebugOS
{
    /// <summary>
    /// Class allowing the comparison of platform-dependent or platform-independent
    /// path strings.
    /// </summary>
    public abstract class PathComparer : IComparer<string>, IEqualityComparer<string>
    {
        private static PlatformPathComparer      platform;
        private static PlatformPathComparer      platformIgnoreFolder;
        private static OSIndependentPathComparer osIndependent;
        private static OSIndependentPathComparer osIndependentIgnoreFolder;

        static PathComparer()
        {
            platform                  = new PlatformPathComparer(false);
            platformIgnoreFolder      = new PlatformPathComparer(true);
            osIndependent             = new OSIndependentPathComparer(false);
            osIndependentIgnoreFolder = new OSIndependentPathComparer(true);
        }

        protected PathComparer() { }

        /* TODO: CURRENT OS COMPARER */

        public static PathComparer PlatformPath     { get { return platform; } }
        public static PathComparer PlatformFilename { get { return platformIgnoreFolder; } }

        public static PathComparer OSIndependentPath     { get { return osIndependent; } }
        public static PathComparer OSIndependentFilename { get { return osIndependentIgnoreFolder; } }


        public abstract int Compare(string x, string y);
        public abstract bool Equals(string x, string y);
        public abstract int GetHashCode(string obj);

    }

    /// <summary>
    /// Class for comparing paths on the current platform.
    /// </summary>
    sealed class PlatformPathComparer : PathComparer
    {
        bool           ignoreDirectory;
        char[]         separators;
        StringComparer comparer;

        public PlatformPathComparer(bool ignoreDirectory)
        {
            this.ignoreDirectory = ignoreDirectory;

            this.separators = new char[] 
            {
                System.IO.Path.DirectorySeparatorChar,
                System.IO.Path.AltDirectorySeparatorChar,
                System.IO.Path.VolumeSeparatorChar
            };

            PlatformID platform = Environment.OSVersion.Platform;

            // If a windows platform, use the "ignore case" comparer
            if (platform == PlatformID.Win32NT || platform == PlatformID.Win32S
                || platform == PlatformID.Win32Windows || platform == PlatformID.WinCE)
            {
                comparer = StringComparer.InvariantCultureIgnoreCase;
            }
            else comparer = StringComparer.InvariantCulture;
        }

        private string assertValid(string path)
        {
            return new System.IO.FileInfo(path).FullName;
        }

        public override int Compare(string x, string y)
        {
            // Handle empty strings
            if (string.IsNullOrWhiteSpace(x))
            {
                return string.IsNullOrWhiteSpace(y) ? 0 : -1;
            }
            else if (string.IsNullOrWhiteSpace(y))
            {
                return string.IsNullOrWhiteSpace(x) ? 0 : 1;
            }

            string path1 = assertValid(x);
            string path2 = assertValid(y);

            if (this.ignoreDirectory)
            {
                path1 = System.IO.Path.GetFileName(path1);
                path2 = System.IO.Path.GetFileName(path2);

                return comparer.Compare(path1, path2);
            }
            else
            {
                string[] parts1 = path1.Split(separators);
                string[] parts2 = path2.Split(separators);

                int min = Math.Min(parts1.Length, parts2.Length);
                for (int i = 0; i < min; i++)
                {
                    int res = comparer.Compare(parts1[i], parts2[i]);

                    if (res == 0) continue;
                    else          return res;
                }
                return parts1.Length - parts2.Length;
            }
        }

        public override bool Equals(string x, string y)
        {
            return this.Compare(x, y) == 0;
        }

        public override int GetHashCode(string path)
        {
            // Handle empty strings
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty.GetHashCode();
            }
            else
            {
                string input;

                if (this.ignoreDirectory)
                {
                    input = System.IO.Path.GetFileName(path);
                }
                else
                {
                    input = System.IO.Path.GetFullPath(path);
                }

                return input.ToLowerInvariant().GetHashCode();
            }
        }
    }

    sealed class OSIndependentPathComparer : PathComparer
    {
        bool ignoreDirectory;

        private string assertValid(string path)
        {
            return new System.IO.FileInfo(path).FullName;
        }

        public OSIndependentPathComparer(bool ignoreDirectory)
        {
            this.ignoreDirectory = ignoreDirectory;
        }

        public override int Compare(string x, string y)
        {
            if (this.ignoreDirectory)
            {
                return PathComparer.PlatformFilename.Compare(
                    Utils.GetPlatformPath(x), Utils.GetPlatformPath(y));
            }
            else
            {
                return PathComparer.PlatformPath.Compare(
                    Utils.GetPlatformPath(x), Utils.GetPlatformPath(y));
            }
        }

        public override bool Equals(string x, string y)
        {
            return this.Compare(x, y) == 0;
        }

        public override int GetHashCode(string path)
        {
            if (this.ignoreDirectory)
            {
                return PathComparer.PlatformFilename.GetHashCode(
                    Utils.GetPlatformPath(path));
            }
            else
            {
                return PathComparer.PlatformPath.GetHashCode(
                    Utils.GetPlatformPath(path));
            }
        }
    }
}
