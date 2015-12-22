using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpSvn;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace ProgramVerificationSystems.PlogConverter
{
    class SvnInfo
    {
        static public string GetProperDirectoryCapitalization(string dirname)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dirname);
            DirectoryInfo parentDirInfo = dirInfo.Parent;
            if (null == parentDirInfo)
                return dirInfo.Name;
            return Path.Combine(GetProperDirectoryCapitalization(parentDirInfo.FullName),
                                parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
        }

        static public string GetProperFilePathCapitalization(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            DirectoryInfo dirInfo = fileInfo.Directory;
            return Path.Combine(GetProperDirectoryCapitalization(dirInfo.FullName),
                                dirInfo.GetFiles(fileInfo.Name)[0].Name);
        }

        static private void ReadStringPairsFile(string filename, Dictionary<string, List<string>> dictionary)
        {
            try
            {
                dictionary.Clear();
                if (!File.Exists(filename))
                    return;

                using (StreamReader fs = new StreamReader(filename))
                {
                    while (!fs.EndOfStream)
                    {
                        string[] pair = fs.ReadLine().Split(' ');

                        //Удалить пустые символы. Проверить корректность.
                        if (pair.Length != 2)
                            continue;

                        AddStringPair(dictionary, pair[0], pair[1]);
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        static private void WriteStringPairsFile(string filename, Dictionary<string, List<string>> dictionary)
        {
            try
            {
                List<KeyValuePair<string, List<string>>> list = dictionary.ToList();
                list.Sort((firstPair,nextPair) =>
                    {
                        return firstPair.Key.CompareTo(nextPair.Key);
                    }
                );

                using (StreamWriter fs = new StreamWriter(filename, false))
                {
                    foreach (KeyValuePair<string, List<string>> pair in list)
                    {
                        foreach (var value in pair.Value)
                        {
                            fs.WriteLine("{0} {1}", pair.Key, value);
                        }
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        static private void AddStringPair(Dictionary<string, List<string>> dictionary, string key, string value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = new List<string>() { value };
            }
            else
            {
                if (!dictionary[key].Contains(value))
                    dictionary[key].Add(value);
            }
        }

        string _emailsFile;
        Dictionary<string, List<string>> _emails = new Dictionary<string, List<string>>();

        public Dictionary<string, List<string>> Emails
        {
            get
            {
                return _emails;
            }
        }
        public long Revision { get; set; }
        public string Author { get; set; }
        public string CaseSensFileName { get; set; }
        public bool AutoEmail { get; set; }

        private SvnInfo()
        {
        }
        static SvnInfo()
        {
            Instance = new SvnInfo();
        }
        public static SvnInfo Instance { get; private set; }

        public void ReadConfig(string outputDir)
        {
            string emailsFile = Path.Combine(outputDir, "Emails.lst");
            ReadStringPairsFile(emailsFile, _emails);
        }

        public void WriteConfig(string outputDir)
        {
            string emailsFile = Path.Combine(outputDir, "Emails.lst");
            WriteStringPairsFile(emailsFile, _emails);
        }

        public void ParseBlame(string fileName, int lineNumber)
        {
            CaseSensFileName = GetProperFilePathCapitalization(fileName);
            Revision = 0;
            Author = "unknown";

            try
            {
                using (var client = new SvnClient())
                {
                    client.Authentication.DefaultCredentials = new NetworkCredential("vivabuild", "#%tsargWV45!@^@gvtRSW");
                    SvnTarget target = SvnPathTarget.FromString(CaseSensFileName);
                    Collection<SvnBlameEventArgs> list;
                    client.GetBlame(target, out list);
                    int idx = lineNumber - 1;
                    if (0 <= idx && idx < list.Count)
                    {
                        Revision = list[idx].Revision;
                        Author = list[idx].Author;
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
            AddAuthor(Author);
        }

        public void AddAuthor(string author)
        {
            if (!_emails.ContainsKey(author))
            {
                AddStringPair(_emails, author,
                    (author != "unknown" && author != "all" && AutoEmail) ? author + "@dalet.com" : "none");
            }
        }
    }
}
