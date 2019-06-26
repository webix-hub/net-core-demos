using System.IO;
using System.Collections.Generic;

namespace Webix.WFS.Local {

	public interface IPolicy {
		bool Comply(string path, Operation operation);
	}

	public enum Operation {
		Read = 1,
		Write
	}


	public class CombinedPolicy : IPolicy {
		private List<IPolicy> policies;
		public CombinedPolicy(){
			policies = new List<IPolicy>();
		}
		public void Add(IPolicy p){
			policies.Add(p);
		}
		public bool Comply(string path, Operation operation){
			foreach (IPolicy p in policies){
				if (!p.Comply(path, operation)) {
					return false;
				}
			}

			return true;
		}
	}

	public class ReadOnlyPolicy : IPolicy {
		public bool Comply(string path, Operation operation){
			if (operation == Operation.Read){
				return true;
			}

			return false;
		}
	}

	public class ForceRootPolicy : IPolicy {
		private string _root;
		public ForceRootPolicy(string root){
			this._root = root;
		}
		public bool Comply(string path, Operation operation){
			path = Path.GetFullPath(path);
			if (path.IndexOf(this._root) == 0){
				return true;
			}
			return false;
		}
	}

	public class AllowPolicy : IPolicy {
		public bool Comply(string path, Operation operation){ return true; }
	}


	public class DenyPolicy : IPolicy {
		public bool Comply(string path, Operation operation){ return false; }
	}

}