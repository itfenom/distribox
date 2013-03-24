using Distribox.CommonLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Distribox.FileSystem
{
	// TODO this class would be serialized for deserialized
	/// <summary>
	/// Version list.
	/// </summary>
    public class VersionList
    {
		/// <summary>
		/// Gets or sets all files that ever existed.
		/// </summary>
		/// <value>All files.</value>
        public List<FileItem> AllFiles { get; set; }

		/// <summary>
		/// Maps relative path to file item object
		/// </summary>
        private Dictionary<string, FileItem> _pathToFile = new Dictionary<string, FileItem>();

		/// <summary>
		/// The path of version list.
		/// </summary>
        private string _path;

		/// <summary>
		/// Initializes a new instance of the <see cref="Distribox.CommonLib.VersionList"/> class.
		/// Only for serialization
		/// </summary>
        public VersionList() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Distribox.CommonLib.VersionList"/> class.
		/// </summary>
		/// <param name="path">Path of version list.</param>
        public VersionList(string path)
        {
			this._path = path;

			// Deserialize version list
            AllFiles = CommonHelper.ReadObject<List<FileItem>>(_path);
            foreach (var file in AllFiles.Where(x => x.IsAlive))
            {
                _pathToFile[file.CurrentName] = file;
            }
        }

		/// <summary>
		/// Create a file item
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="isDirectory">If set to <c>true</c> is directory.</param>
		/// <param name="when">When.</param>
        public FileItem Create(string name, bool isDirectory, DateTime when)
        {
            if (_pathToFile.ContainsKey(name))
				return null;

            FileItem item = new FileItem(name, isDirectory);
            item.Create(when);

            AllFiles.Add(item);

            _pathToFile[name] = item;

            return item;
        }

		/// <summary>
		/// Change a file item.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="isDirectory">If set to <c>true</c> is directory.</param>
		/// <param name="SHA1">SH a1.</param>
		/// <param name="when">When.</param>
		public void Change(string name, bool isDirectory, string SHA1, DateTime when)
        {
            if (!_pathToFile.ContainsKey(name))
            {
                Create(name, isDirectory, when);
            }

            if (isDirectory) return;

            FileItem item = _pathToFile[name];
            item.Change(name, SHA1, when);
        }

		/// <summary>
		/// Rename a file item.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="oldName">Old name.</param>
		/// <param name="SHA1">SH a1.</param>
		/// <param name="when">When.</param>
        public void Rename(string name, string oldName, string SHA1, DateTime when)
        {
            FileItem item = _pathToFile[oldName];

            item.Rename(name, when);

            _pathToFile.Remove(oldName);
            _pathToFile[name] = item;
        }

		/// <summary>
		/// Delete a file item.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="when">When.</param>
        public void Delete(string name, DateTime when)
        {
            FileItem item = _pathToFile[name];
            item.Delete(when);
            _pathToFile.Remove(name);
        }

		// TODO use version
		/// <summary>
		/// Gets all version that exist in <paramref name="list"/> but not exist in this
		/// </summary>
		/// <returns>The less than.</returns>
		/// <param name="list">List.</param>
        public List<FileItem> GetLessThan(VersionList list)
        {
            HashSet<string> myFileList = new HashSet<string>();
            foreach (var item in AllFiles)
			{
				foreach (var history in item.History)
				{
					myFileList.Add(item.Id + "@" + history.SHA1);
				}
			}

            Dictionary<string, FileItem> dict = new Dictionary<string, FileItem>();
            foreach (var item in list.AllFiles)
			{
				foreach (var history in item.History)
				{
					string guid = item.Id + "@" + history.SHA1;
					if (myFileList.Contains(guid))
						continue;
					if (!dict.ContainsKey(item.Id))
						dict[item.Id] = new FileItem(item.Id);
					dict[item.Id].NewVersion(history);
				}
			}
            return dict.Values.ToList();
        }

		/// <summary>
		/// Flush version list into disk
		/// </summary>
        public void Flush()
        {
			AllFiles.WriteObject(_path);
        }

		/// <summary>
		/// Gets file by name.
		/// </summary>
		/// <returns>The file.</returns>
		/// <param name="name">Name.</param>
		public FileItem GetFileByName(string name)
		{
			return _pathToFile[name];
		}

		/// <summary>
		/// Sets file by name.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="fileItem">File item.</param>
		public void SetFileByName(string name, FileItem fileItem)
		{
			_pathToFile[name] = fileItem;
		}
    }
}