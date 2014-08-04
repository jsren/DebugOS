using System;
using System.Collections.Generic;

namespace DebugOS
{
    public abstract class PathComparer : IComparer<string>, IEqualityComparer<string>
    {
        private static WindowsPathComparer windows;
        private static WindowsPathComparer windowsIgnoreFolder;
        private static OSIndependentPathComparer osIndependent;
        private static OSIndependentPathComparer osIndependentIgnoreFolder;

        static PathComparer()
        {
            windows                   = new WindowsPathComparer(false);
            windowsIgnoreFolder       = new WindowsPathComparer(true);
            osIndependent             = new OSIndependentPathComparer(false);
            osIndependentIgnoreFolder = new OSIndependentPathComparer(true);
        }


        protected PathComparer() { }


        public static PathComparer WindowsPath { get { return windows; } }
        public static PathComparer WindowsFilename { get { return windowsIgnoreFolder; } }

        /* TODO: CURRENT OS COMPARER */

        public static PathComparer OSIndependentPath { get { return osIndependent; } }
        public static PathComparer OSIndependentFilename { get { return osIndependentIgnoreFolder; } }


        public abstract int Compare(string x, string y);
        public abstract bool Equals(string x, string y);
        public abstract int GetHashCode(string obj);

    }

    sealed class WindowsPathComparer : PathComparer
    {
        bool ignoreDirectory;

        public WindowsPathComparer(bool ignoreDirectory)
        {
            this.ignoreDirectory = ignoreDirectory;
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

                return StringComparer.InvariantCultureIgnoreCase.
                    Compare(path1, path2);
            }
            else
            {
                string[] parts1 = path1.Split('\\');
                string[] parts2 = path2.Split('\\');

                int min = Math.Min(parts1.Length, parts2.Length);
                for (int i = 0; i < min; i++)
                {
                    int res = StringComparer.InvariantCultureIgnoreCase.
                        Compare(parts1[i], parts2[i]);

                    if (res == 0) continue;
                    else return res;
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
                return PathComparer.WindowsFilename.Compare(
                    Utils.GetWindowsPath(x), Utils.GetWindowsPath(y));
            }
            else
            {
                return PathComparer.WindowsPath.Compare(
                    Utils.GetWindowsPath(x), Utils.GetWindowsPath(y));
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
                return PathComparer.WindowsFilename.GetHashCode(
                    Utils.GetWindowsPath(path));
            }
            else
            {
                return PathComparer.WindowsPath.GetHashCode(
                    Utils.GetWindowsPath(path));
            }
        }
    }
}
