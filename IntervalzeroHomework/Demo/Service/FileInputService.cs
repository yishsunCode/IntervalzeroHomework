using Demo.Interface;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace Demo.Service
{
    class FileInputService : IInputService
    {
        Dictionary<string, FileInfo> _fileDict;

        public string GetArticle(string articleName)
        {
            var file = _fileDict[articleName];
            var text = file.OpenText().ReadToEnd();
            return text;
        }

        public IList<string> GetArticleList()
        {
            var dialog = new FolderBrowserDialog();
            switch (dialog.ShowDialog())
            {
                case DialogResult.OK:
                    {
                        var directory = Directory.CreateDirectory(dialog.SelectedPath);
                        var allText = directory.GetFiles("*.txt");
                        _fileDict = allText.ToDictionary(i => i.Name, i => i);
                        return _fileDict.Keys.ToArray();
                    }
                    break;
                default:
                    return new string[] { };
                    break;
            }
        }
    }
}
