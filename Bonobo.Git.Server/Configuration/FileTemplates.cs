using Bonobo.Git.Server.App_GlobalResources;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Bonobo.Git.Server.Configuration
{
    public static class FileTemplateConfiguration
    {
        private static Dictionary<string, List<string>> _files = new Dictionary<string, List<string>>();
        private static string _basePath = String.Empty;
        static FileTemplateConfiguration()
        {
            string _basePath = ConfigurationManager.AppSettings["FileTemplates"];

            _basePath = Path.IsPathRooted(_basePath)
                   ? _basePath
                   : HostingEnvironment.MapPath(_basePath);
            foreach (string dir in Directory.GetDirectories(_basePath))
            {
                string key = dir.Split('\\').Last();
                _files.Add(key, new List<string>());
                foreach (var file in Directory.GetFiles(dir))
                {
                    _files[key].Add(file);
                }
            }

        }

        private static Stream GetFile(string fileName, string fileTemplate)
        {
            string filePath = string.Empty;
            if (!_files.ContainsKey(fileName) || string.IsNullOrWhiteSpace(filePath = _files[fileName].SingleOrDefault(s => s.Split('\\').Last().Equals(fileTemplate))))
            {
                return null;
            }
            Stream result = new MemoryStream();
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                stream.CopyTo(result);
                result.Seek(0, SeekOrigin.Begin);
            }
            return result;
        }


        public static bool AddFiles(Repository repo, string branchName, Dictionary<string, string> files, string userName, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                email = "@NA";
            }
            if(string.IsNullOrWhiteSpace(branchName))
            {
                branchName = "master";
            }
            
            // Create a blob from the content stream

            // Put the blob in a tree
            TreeDefinition td = new TreeDefinition();
            foreach (var kvp in files)
            {
                if (!kvp.Value.Equals(Resources.Repository_File_Templates_None, StringComparison.InvariantCultureIgnoreCase))
                {
                    Stream stream = GetFile(kvp.Key, kvp.Value);
                    if (stream != null)
                    {
                        Blob newBlob = repo.ObjectDatabase.CreateBlob(stream);
                        td.Add(kvp.Key, newBlob, Mode.NonExecutableFile);
                    }
                    else
                    {
                        return false; ;
                    }
                }
            }

            Tree tree = repo.ObjectDatabase.CreateTree(td);

            // Committer and author
            
            Signature committer = new Signature(userName, email, DateTime.Now);
            Signature author = committer;
            
            // Create binary stream from the text
            Commit commit = repo.ObjectDatabase.CreateCommit(
                author,
                committer,
                Resources.Repository_Seed_Initial_Files_Commit_Message,
                tree,
                repo.Commits,
                true);
            if (repo.Branches.SingleOrDefault(b => b.FriendlyName.Equals(branchName, StringComparison.InvariantCultureIgnoreCase)) == null)
            {
                repo.CreateBranch(branchName, commit);
            }
            return true;
        }
        public static Dictionary<string, List<string>> FileTemplates { get { return _files; } }
    }
}