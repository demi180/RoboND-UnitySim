using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YAMLParser
{
    public class MsgFileLocation
    {
        private static string[] MSG_GEN_FOLDER_NAMES =
        {
            "msg",
            "srv",
            "msgs",
            "srvs",
            "action"
        };

        /// <summary>
        /// Mangles a file's name to find the package name based on the name of the directory containing the file
        /// </summary>
        /// <param name="path">A file</param>
        /// <param name="targetmsgpath">The "package name"/"msg name" for the file at path</param>
        /// <returns>"package name"</returns>
        private static string getPackageName(string path)
        {
            string[] chunks = path.Split(System.IO.Path.DirectorySeparatorChar);
            string foldername = chunks[chunks.Length - 2];
            if (MSG_GEN_FOLDER_NAMES.Contains(foldername))
                foldername = chunks[chunks.Length - 3];
            return foldername;
        }

        private static string getPackagePath(string basedir, string msgpath)
        {
            string p = getPackageName(msgpath);
            return System.IO.Path.Combine(basedir, p);
        }

        public string path { get; private set; }
        public string basename { get; private set; }
        public string extension { get; private set; }
        public string package { get; private set; }
        public string packagedir { get; private set; }
        public string searchroot { get; private set; }

        public string Path
        {
            get { return path; }
        }

        public MsgFileLocation(string path, string root)
        {
            this.path = path;
            searchroot = root;
            packagedir = getPackagePath(root, path);
            package = getPackageName(path);
            basename = System.IO.Path.GetFileNameWithoutExtension(path);
            extension = System.IO.Path.GetExtension(path).TrimStart('.');
        }

        public override bool Equals(object obj)
        {
            MsgFileLocation other = obj as MsgFileLocation;
            return other != null && string.Equals(other.package, package) && string.Equals(other.basename, basename);
        }

        public override int GetHashCode()
        {
            return (package + "/" + basename).GetHashCode();
        }

        public override string  ToString()
        {
            return string.Format("{0}.{1}", System.IO.Path.Combine(package, basename), extension);
        }
    }

    internal static class MsgFileLocator
    {
        /// <summary>
        /// Finds all msgs and srvs below path and adds them to
        /// </summary>
        /// <param name="m"></param>
        /// <param name="s"></param>
        /// <param name="path"></param>
        private static void explode(List<MsgFileLocation> m, List<MsgFileLocation> s, List<MsgFileLocation> actionFiles, string path)
        {
            var msgfiles = Directory.GetFiles(path, "*.msg", SearchOption.AllDirectories).Select(p => new MsgFileLocation(p, path)).ToArray();
            var srvfiles = Directory.GetFiles(path, "*.srv", SearchOption.AllDirectories).Select(p => new MsgFileLocation(p, path)).ToArray();
            var allActionFiles = Directory.GetFiles(path, "*.action", SearchOption.AllDirectories).Select(p => new MsgFileLocation(p, path)).ToArray();

            int mb4 = m.Count, sb4=s.Count;
            foreach (var nm in msgfiles)
            {
                if (!m.Contains(nm))
                    m.Add(nm);
            }
            foreach (var ns in srvfiles)
            {
                if (!s.Contains(ns))
                    s.Add(ns);
            }
            foreach (var actionFile in allActionFiles)
            {
                if (!actionFiles.Contains(actionFile))
                {
                    actionFiles.Add(actionFile);
                }
            }
            Console.WriteLine("Skipped " + (msgfiles.Length - (m.Count - mb4)) + " duplicate msgs and " + (srvfiles.Length - (s.Count - sb4)) + " duplicate srvs");
        }

        public static void findMessages(List<MsgFileLocation> msgs, List<MsgFileLocation> srvs, List<MsgFileLocation> actionFiles,
            params string[] args)
        {
            //solution directory (where the reference to msg_gen is) is passed -- or assumed to be in a file in the same directory as the executable (which would be the case when msg_gen is directly run in the debugger
            if (args.Length == 0)
            {
                Console.WriteLine("MsgGen needs to receive a list of paths to recursively find messages in order to work.");
                Environment.Exit(1);
            }
            foreach (string arg in args)
            {
                explode(msgs, srvs, actionFiles, new DirectoryInfo(arg).FullName);
            }
        }
    }
}
