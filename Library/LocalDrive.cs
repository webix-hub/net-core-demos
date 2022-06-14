using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Webix.WFS.Local {

	// LocalDrive  represents the file folder on local drive
	// due to ForceRootPolicy all operations outside of the root folder will be blocked
	public class LocalDrive{
		private readonly ILogger _logger;
		private string root;
		private IPolicy policy;
		private ListConfig list;
		private OperationConfig operation;
		private DateTime _epoch;
		
		public LocalDrive(string path, DriveConfig config = null) {
			_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			root = Path.GetFullPath(path);

			var p = new CombinedPolicy();
			p.Add(new ForceRootPolicy(root));

			if (config != null){
				_logger = config.Logger;
				list = config.List;
				operation = config.Operation;

				if (config.Policy != null) {
					p.Add(config.Policy);
				}
			}
		
			if (list == null)
				list = new ListConfig{};
			if (operation == null)
				operation = new OperationConfig{};

			policy = p;
		}

		public LocalDrive(string root, ListConfig list, OperationConfig operation, IPolicy policy, ILogger logger){
			this.root = root;
			this.list = list;
			this.operation = operation;
			this.policy = policy;
			_logger = logger;
			_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		}

		// List method returns array of files from the target folder
		public FsObject[] List(string path, ListConfig config = null){
			string fullpath = idToPath(path);

			if (_logger != null)
				_logger.LogDebug($"List {fullpath}");

			if (!policy.Comply(fullpath, Operation.Read))
				throw new System.InvalidOperationException("Access Denied");

			if (config == null){
				config = list;
			}

			return listFolder(fullpath, config).ToArray();
		}

		private bool isDir(string path){
			return (File.GetAttributes(path) & FileAttributes.Directory)
	                 == FileAttributes.Directory; 
		}

		// Remove deletes a file or a folder
		public void Remove(string path) {

			path = idToPath(path);


			if (_logger != null)
				_logger.LogDebug($"Remove {path}");
		
			if (!policy.Comply(path, Operation.Write)) {
				throw new System.InvalidOperationException("Access Denied");
			}

			if (isDir(path)){
				Directory.Delete(path, true);
			} else { 
				File.Delete(path);
			}
		}

		// Read returns content of a file
		public FileStream Read(string path) {
			if (_logger != null)
				_logger.LogDebug($"Read {path}");

			path = idToPath(path);;
			if (!policy.Comply(path, Operation.Read)) {
				throw new System.InvalidOperationException("Access Denied");
			}

			return new FileStream(path, FileMode.Open);
		}

		// Write saves content to a file
		public string Write(string path, Stream data) {
				
			if (_logger != null)
				_logger.LogDebug($"Write {path}");

			path = idToPath(path);
			if (!policy.Comply(path, Operation.Write)) {
				throw new System.InvalidOperationException("Access Denied");
			}

			if (operation.PreventNameCollision) {
				path = checkName(path);
			}

			var target = File.Create(path);
			data.CopyTo(target);
			target.Close();

			return pathToID(path);
		}

		public bool Exists(string path) {
			path = idToPath(path);
			return File.Exists(path) || Directory.Exists(path);
		}

	// Info returns info about a single file
		public FsObject Info(string id) {
			var path = idToPath(id);
			if (!policy.Comply(path, Operation.Read)) {
				throw new System.InvalidOperationException("Access Denied");
			}

			if (isFolder(path)){
				var info = new DirectoryInfo(path);
				return new FsObject{
					Value = info.Name,
					ID = id,
					Size = 0,
					Date = (long)(info.LastWriteTimeUtc-_epoch).TotalSeconds,
					Type = "folder"
				};
			} else {
				var file = new FileInfo(path);
				return new FsObject{
					Value = file.Name,
					ID = id,
					Size = file.Length,
					Date = (long)(file.LastWriteTimeUtc-_epoch).TotalSeconds,
					Type = FileType.GetType(file.Extension)
				};
			}
		}

		//Mkdir creates a new folder
		public string Mkdir(string path) {
			if (_logger != null)
				_logger.LogDebug($"Make folder %s", path);

			path = idToPath(path);
			if (!policy.Comply(path, Operation.Write)) {
				throw new System.InvalidOperationException("Access Denied");
			}

			if (operation.PreventNameCollision)
				path = checkName(path);
			

			Directory.CreateDirectory(path);
			return pathToID(path);
		}

		// Copy makes a copy of file or a folder
		public string Copy(string source, string target) {
			if (_logger != null)
				_logger.LogDebug($"Copy {source} to {target}");

			bool ok = Exists(target);
			source = idToPath(source);
			target = idToPath(target);

			if (!policy.Comply(source, Operation.Read) || !policy.Comply(target, Operation.Write)) {
				throw new System.InvalidOperationException("Access Denied");
			}

			bool st = isFolder(source);
			bool et = isFolder(target);

			//file to folder
			if (et) {
				target = Path.Combine(target, Path.GetFileName(source));
			} else if (st && ok) {
				throw new System.InvalidOperationException("Can't copy folder to file");
			}

			if (target.IndexOf(source + Path.DirectorySeparatorChar)  == 0){
				throw new System.InvalidOperationException("Can't copy folder into self");
			}

			if (operation.PreventNameCollision) {
				target = checkName(target);
			}

			//folder to folder
			if (st) {
				CopyDirectory(source, target);
			} else {
				File.Copy(source, target);
			}

			return pathToID(target);
		}

		// Move renames(moves) a file or a folder
		public string Move(string source, string target) {
			if (_logger != null)
				_logger.LogDebug($"Move {source} to {target}");

			bool ok = Exists(target);		
			source = idToPath(source);
			target = idToPath(target);

			if (!policy.Comply(source, Operation.Write) || !policy.Comply(target, Operation.Write)) {
				throw new System.InvalidOperationException("Access Denied");
			}

			bool st = isFolder(source);
			bool et = isFolder(target);

			//file to folder
			if (et) {
				target = Path.Combine(target, Path.GetFileName(source));
			} else if (st && ok) {
				throw new System.InvalidOperationException("Can't copy folder to file");
			}

			if (target.IndexOf(source + Path.DirectorySeparatorChar)  == 0){
				throw new System.InvalidOperationException("Can't move folder into self");
			}

			if (operation.PreventNameCollision) {
				target = checkName(target);
			}

			//folder to folder
			if (st) {
				Directory.Move(source, target);
			} else {
				File.Move(source, target);
			}

			return pathToID(target);

		}

		// WithOperationConfig makes a copy of drive with new operation config
		public LocalDrive WithOperationConfig(OperationConfig config) {
			return new LocalDrive(root, list, config, policy, _logger);
		}

		private bool isFolder(string path) {
			return Directory.Exists(path);
		}

		private string idToPath(string id) {
			if (id.Substring(0,1).Equals("/"))
				id = id.Substring(1);

			return Path.Combine(root, id);
		}

		private string pathToID(string path) {
			int pos = path.IndexOf(root);
			if (pos >= 0)
				path = path.Substring(0, pos) + path.Substring(pos + root.Length);

			if (!path.Substring(0,1).Equals("/"))
				path = "/"+path;

			return path;
		}

		private List<FsObject> listFolder(string path, ListConfig config, List<FsObject> res = null) {
			var files = Directory.GetFileSystemEntries(path);
			
			bool needSortData  = false;
			if (config.Nested || res == null) {
				res = new List<FsObject>();
				needSortData = true;
			}

			foreach (var file in files) {
				bool skipFile = false;
				var id = pathToID(file);
				var fs = Info(id);

				if (config.Exclude != null && config.Exclude(fs.Value)) {
					skipFile = true;
				}
				if (config.Include != null && !config.Include(fs.Value)) {
					skipFile = true;
				}

				if (fs.Type != "folder" && (config.SkipFiles || skipFile))
					continue;

				if (fs.Type == "folder" && config.SubFolders) {
					var sub = listFolder(
						Path.Combine(path, file),
						config, res);

					fs.Type = "folder";
					
					if (config.Nested && sub.Count > 0) {
						fs.Data = sub.ToArray();
					}
				}

				if (!skipFile) {
					res.Add(fs);
				}
			}

			// sort files and folders by name, folders first
			if (needSortData) {
				res.Sort(delegate(FsObject x, FsObject y){
					bool afolder = x.Type == "folder";
					bool bfolder = y.Type == "folder";

					if ((afolder || bfolder) && x.Type != y.Type){
						return afolder ? -1 : 1;
					}

					return x.Value.ToUpper().CompareTo(y.Value.ToUpper());
				});
			}

			return res;
		}

		private string checkName(string path) {
			while (File.Exists(path) || Directory.Exists(path)) {
				path = path + ".new";
			}
			return path;
		}

		private void CopyDirectory(string source, string target){
	        DirectoryInfo dir = new DirectoryInfo(source);
	        DirectoryInfo[] dirs = dir.GetDirectories();

	        if (!Directory.Exists(target))
	            Directory.CreateDirectory(target);
	        
	        FileInfo[] files = dir.GetFiles();
	        foreach (FileInfo file in files) {
	            string temppath = Path.Combine(target, file.Name);
	            file.CopyTo(temppath, false);
	        }

	        // If copying subdirectories, copy them and their contents to new location.
	        foreach (DirectoryInfo subdir in dirs) {
				string temppath = Path.Combine(target, subdir.Name);
				CopyDirectory(subdir.FullName, temppath);
			}
		}
	}
}