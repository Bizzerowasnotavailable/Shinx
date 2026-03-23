using Shinx.Commands;
using System;
using System.Collections.Generic;
using System.IO;

namespace Shinx
{
    public static class UserManager
    {
        private static Dictionary<string, HashSet<string>> groups = new Dictionary<string, HashSet<string>>();
        private static HashSet<string> registeredGroups = new HashSet<string> { "root", "user" };
        private static string groupsFile = @"0:\sys\groups.txt";
        private static string registeredGroupsFile = @"0:\sys\grouplist.txt";
        private static Dictionary<string, string> users = new Dictionary<string, string>();
        public static string currentUser = "";
        private static string savedUser = "";
        private static string usersFile = @"0:\sys\users.txt";

        public static void Init()
        {
            if (!File.Exists(usersFile))
            {
                // create default root user
                users.Add("root", core_sha256.Hash("root"));
                groups.Add("root", new HashSet<string> { "root" });
                Save();
            }
            else
            {
                Load();
            }
        }

        private static void Load()
        {
            users.Clear();
            string[] lines = File.ReadAllLines(usersFile);
            foreach (string line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2)
                    users[parts[0]] = parts[1];
            }

            if (File.Exists(groupsFile))
            {
                string[] groupLines = File.ReadAllLines(groupsFile);
                foreach (string line in groupLines)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        HashSet<string> userGroups = new HashSet<string>();
                        foreach (string g in parts[1].Split(','))
                            if (!string.IsNullOrEmpty(g))
                                userGroups.Add(g);
                        groups[parts[0]] = userGroups;
                    }
                }
            }

            if (File.Exists(registeredGroupsFile))
            {
                registeredGroups.Clear();
                foreach (string line in File.ReadAllLines(registeredGroupsFile))
                    if (!string.IsNullOrEmpty(line))
                        registeredGroups.Add(line);
            }
        }

        private static void Save()
        {
            List<string> lines = new List<string>();
            foreach (var user in users)
                lines.Add(user.Key + ":" + user.Value);
            File.WriteAllLines(usersFile, lines.ToArray());

            List<string> groupLines = new List<string>();
            foreach (var entry in groups)
                groupLines.Add(entry.Key + ":" + string.Join(",", entry.Value));
            File.WriteAllLines(groupsFile, groupLines.ToArray());
            File.WriteAllLines(registeredGroupsFile, new List<string>(registeredGroups).ToArray());
        }

        public static bool Login(string username, string password)
        {
            if (users.ContainsKey(username) && users[username].Equals(core_sha256.Hash(password)))
            {
                currentUser = username;
                if (username == "root")
                    Shell.currentDirectory = @"0:\";
                else
                    Shell.currentDirectory = @"0:\home\" + username + @"\";
                return true;
            }
            return false;
        }

        public static void AddUser(string username, string password, HashSet<string> userGroups = null)
        {
            users.Add(username, core_sha256.Hash(password));
            groups.Add(username, userGroups ?? new HashSet<string> { "user" });

            string homeDir = @"0:\home\" + username;
            if (!Directory.Exists(homeDir))
            {
                Directory.CreateDirectory(homeDir);
                PermissionManager.SetDefault(homeDir, username);
            }

            Save();
        }

        public static void RemoveUser(string username)
        {
            users.Remove(username);
            groups.Remove(username);
            PermissionManager.TransferOwnership(username, "root");
            Save();
        }

        public static void ListUsers()
        {
            foreach (var user in users.Keys)
                Console.WriteLine(user);
        }

        public static bool UserExists(string username)
        {
            return users.ContainsKey(username);
        }

        public static void Logout()
        {
            currentUser = savedUser;
            savedUser = "";
        }

        public static void ChangeUserPass(string username, string password)
        {
            users[username] = core_sha256.Hash(password);
            Save();
        }

        public static string GetPasswordHash(string username)
        {
            if (users.ContainsKey(username))
                return users[username];
            return null;
        }

        public static void SwitchUser(string username, string password)
        {
            savedUser = currentUser;
            Login(username, password);
        }
        public static bool IsInGroup(string username, string group)
        {
            if (groups.ContainsKey(username))
                return groups[username].Contains(group.ToLower());
            return false;
        }

        public static bool IsRoot(string username) => IsInGroup(username, "root");
        public static void AddToGroup(string username, string group)
        {
            if (!GroupExists(group))
            {
                Console.WriteLine("groupmod: group does not exist: " + group);
                return;
            }
            if (groups.ContainsKey(username))
            {
                groups[username].Add(group.ToLower());
                Save();
            }
        }

        public static void RemoveFromGroup(string username, string group)
        {
            if (groups.ContainsKey(username))
            {
                groups[username].Remove(group.ToLower());
                Save();
            }
        }

        public static HashSet<string> GetGroups(string username)
        {
            if (groups.ContainsKey(username))
                return groups[username];
            return new HashSet<string>();
        }

        public static void RegisterGroup(string group)
        {
            group = group.ToLower();
            if (registeredGroups.Contains(group))
            {
                Console.WriteLine("groupadd: group already exists");
                return;
            }
            registeredGroups.Add(group);
            Save();
        }

        public static void UnregisterGroup(string group)
        {
            group = group.ToLower();
            if (!registeredGroups.Contains(group))
            {
                Console.WriteLine("groupdel: group not found");
                return;
            }
            registeredGroups.Remove(group);

            foreach (var user in groups.Keys)
                groups[user].Remove(group);

            PermissionManager.RemoveGroupFromPermissions(group);

            Save();
        }

        public static bool GroupExists(string group)
        {
            return registeredGroups.Contains(group.ToLower());
        }

        public static void ListGroups()
        {
            foreach (var group in registeredGroups)
                Console.WriteLine(group);
        }
    }
}