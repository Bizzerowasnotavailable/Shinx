using System;
using System.Collections.Generic;
using System.IO;

namespace Shinx
{
    public static class PermissionManager
    {
        private static Dictionary<string, HashSet<string>> permissions = new Dictionary<string, HashSet<string>>();
        private static Dictionary<string, string> ownerMap = new Dictionary<string, string>();
        private static string permFile = @"0:\sys\permissions.txt";
        private static string ownersFile = @"0:\sys\owners.txt";

        public static void Init()
        {

            if (!File.Exists(permFile))
            {
                SetPermission(@"0:\sys", new HashSet<string> { "root" });
                SetPermission(@"0:\home", new HashSet<string> { "root", "user" });
                SetPermission(@"0:\", new HashSet<string> { "root" });
                Save();
            }
            else
            {
                Load();
            }
        }

        public static bool CanAccess(string path, string username)
        {
            if (UserManager.IsRoot(username))
                return true;

            string current = path.TrimEnd('\\');
            while (!string.IsNullOrEmpty(current))
            {
                if (ownerMap.ContainsKey(current) && ownerMap[current] == username)
                    return true;
                string parent = Path.GetDirectoryName(current);
                if (parent == current || string.IsNullOrEmpty(parent))
                    break;
                current = parent;
            }

            string match = FindBestMatch(path);
            if (match != null)
            {
                foreach (var group in permissions[match])
                {
                    if (UserManager.IsInGroup(username, group))
                        return true;
                }
                return false;
            }

            return false;
        }

        public static void SetDefault(string path, string username)
        {
            path = path.TrimEnd('\\');
            ownerMap[path] = username;
            SaveOwners();
        }

        public static void SetPermission(string path, HashSet<string> allowedGroups)
        {
            permissions[path] = allowedGroups;
            Save();
        }

        private static string FindBestMatch(string path)
        {
            string current = path;
            while (!string.IsNullOrEmpty(current))
            {
                if (permissions.ContainsKey(current))
                    return current;
                string parent = Path.GetDirectoryName(current);
                if (parent == current || string.IsNullOrEmpty(parent))
                    break;
                current = parent;
            }
            return null;
        }

        private static void Save()
        {
            List<string> lines = new List<string>();
            foreach (var entry in permissions)
                lines.Add(entry.Key + ":" + string.Join(",", entry.Value));
            File.WriteAllLines(permFile, lines.ToArray());
        }

        private static void SaveOwners()
        {
            List<string> lines = new List<string>();
            foreach (var entry in ownerMap)
                lines.Add(entry.Key + ":" + entry.Value);
            File.WriteAllLines(ownersFile, lines.ToArray());
        }

        private static void Load()
        {
            permissions.Clear();
            ownerMap.Clear();

            string[] permLines = File.ReadAllLines(permFile);
            foreach (string line in permLines)
            {
                int lastColon = line.LastIndexOf(':');
                if (lastColon > 0)
                {
                    string path = line.Substring(0, lastColon);
                    string groupsPart = line.Substring(lastColon + 1);
                    HashSet<string> groups = new HashSet<string>();
                    foreach (string g in groupsPart.Split(','))
                        if (!string.IsNullOrEmpty(g))
                            groups.Add(g);
                    permissions[path] = groups;
                }
            }

            if (File.Exists(ownersFile))
            {
                string[] ownerLines = File.ReadAllLines(ownersFile);
                foreach (string line in ownerLines)
                {
                    int lastColon = line.LastIndexOf(':');
                    if (lastColon > 0)
                    {
                        string path = line.Substring(0, lastColon);
                        string owner = line.Substring(lastColon + 1);
                        ownerMap[path] = owner;
                    }
                }
            }
        }
        public static string GetOwner(string path)
        {
            path = path.TrimEnd('\\');
            if (ownerMap.ContainsKey(path))
                return ownerMap[path];
            return "root";
        }
        public static string GetPermissionGroups(string path)
        {
            path = path.TrimEnd('\\');
            if (permissions.ContainsKey(path))
                return string.Join(",", permissions[path]);

            string match = FindBestMatch(path);
            if (match != null)
                return string.Join(",", permissions[match]);

            return "none";
        }
        public static void TransferOwnership(string username, string newOwner)
        {
            List<string> keys = new List<string>(ownerMap.Keys);
            foreach (string path in keys)
            {
                if (ownerMap[path] == username)
                    ownerMap[path] = newOwner;
            }
            SaveOwners();
        }
        public static void RemoveGroupFromPermissions(string group)
        {
            foreach (var entry in permissions)
                entry.Value.Remove(group);
            Save();
        }
    }
}