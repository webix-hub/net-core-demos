namespace Webix.WFS.Local {

    class FileType {
        public static string GetType(string ext){
            if (ext.Length > 0)
                ext = ext.Substring(1);

            switch(ext){
                case "docx":
                    return "doc";

                case "xls":
                case "xslx":
                    return "doc";

                case "txt":
                case "md":
                    return "text";

                case "html":
                case "htm":
                case "js":
                case "json":
                case "css":
                case "php":
                case "sh":
                    return "code";

                case "mpg":
                case "mp4":
                case "avi":
                case "mkv":
                    return "video";

                case "png":
                case "jpg":
                case "gif":
                    return "image";

                case "mp3":
                case "ogg":
                    return "audio";

                case "zip":
                case "rar":
                case "7z":
                case "tar":
                case "gz":
                    return "archive";

                default:
                    return "text";
            }
    	}
    }

}