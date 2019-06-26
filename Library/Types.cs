using Microsoft.Extensions.Logging;

namespace Webix.WFS.Local {

	// FsObject stores info about single file
	public class FsObject {
		public string Value;
		public string ID;
		public long Size;
		public long Date;
		public string Type;
		public FsObject[] Data;
	}

	// ListConfig contains file listing options
	public class ListConfig {
		public bool SkipFiles; 
		public bool SubFolders;
		public bool Nested;
		public MatcherDelegate Exclude;
		public MatcherDelegate Include;
	}

	// DriveConfig contains drive configuration
	public class DriveConfig{
		public ILogger Logger;
		public ListConfig List;
		public OperationConfig Operation;
		public IPolicy Policy;
	}

	// OperationConfig contains file operation options
	public class OperationConfig{
		public bool PreventNameCollision;
	}

	// MatcherDelegate receives path and returns true if path matches the rule
	public delegate bool MatcherDelegate( string str );

}