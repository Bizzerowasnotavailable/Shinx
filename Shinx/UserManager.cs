using Shinx.Commands;
using System;
using System.Collections.Generic;
using System.IO;

namespace Shinx
{
    public static class UserManager
    {
        private static Dictionary<string, string> users = new Dictionary<string, string>();
        public static string currentUser = "";
        private static string usersFile = @"0:\users.txt";

        public static void Init()
        {
            if (!File.Exists(usersFile))
            {
                // create default root user
                users.Add("root", core_sha256.Hash("root"));
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
        }

        private static void Save()
        {
            List<string> lines = new List<string>();
            foreach (var user in users)
                lines.Add(user.Key + ":" + user.Value);
            File.WriteAllLines(usersFile, lines.ToArray());
        }

        public static bool Login(string username, string password)
        {
            if (users.ContainsKey(username) && users[username] == core_sha256.Hash(password))
            {
                currentUser = username;
                return true;
            }
            return false;
        }

        public static void AddUser(string username, string password)
        {
            users.Add(username, core_sha256.Hash(password));
            Save();
        }

        public static void RemoveUser(string username)
        {
            users.Remove(username);
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
            currentUser = "";
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
    }
}