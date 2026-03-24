using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    public class core_lico : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            Console.WriteLine("echo: display a message in the terminal");
            Console.WriteLine("lico: list commands");
            Console.WriteLine("clear: clear the terminal");
            Console.WriteLine("ls: list files in a directory");
            Console.WriteLine("cp: copy files");
            Console.WriteLine("mv: move files");
            Console.WriteLine("mkdir: make directory");
            Console.WriteLine("cat: show contents of a file");
            Console.WriteLine("cd: change directory");
            Console.WriteLine("edit: basic text editor");
            Console.WriteLine("rm: delete file/directory");
            Console.WriteLine("sha256sum: calculate sha256 hash of a string");
            Console.WriteLine("useradd: add new user");
            Console.WriteLine("userdel: delete user");
            Console.WriteLine("passwd: change user password");
            Console.WriteLine("whoami: prints current user's username");
            Console.WriteLine("exit: logout");
            Console.WriteLine("su: switch user");
            Console.WriteLine("groupadd: create a new group");
            Console.WriteLine("groupdel: delete a group");
            Console.WriteLine("groupmod: add/remove user from group");
            Console.WriteLine("groups: list groups for a user");
            Console.WriteLine("chown: change owner of a file/directory");
            Console.WriteLine("chgrp: change group of a file/directory");
            Console.WriteLine("lua: lua interpreter");
            foreach (var d in peppe.descriptions)
                Console.WriteLine(d.Key + ": " + d.Value);
        }
    }
}
