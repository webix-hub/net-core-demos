using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using System.IO;
using Webix.WFS.Local;

public class FsObjectInfo : FsObject {
    public bool Open;
}

namespace WebixDemos.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FileManagerController : ControllerBase
    {
        private LocalDrive _drive;

        public FileManagerController(){
            _drive = new LocalDrive("./", new DriveConfig{
                Operation = new OperationConfig{ PreventNameCollision=true }
            });
        }
        // GET api/files
        [HttpGet]
        public ActionResult<IEnumerable<FsObjectInfo>> Get()
        {
            var top = new FsObjectInfo{
                Value="Files",
                ID="/",
                Type="folder",
                Data= _drive.List("/", new ListConfig{Nested=true, SubFolders=true}),
                Open=true
            };

            return Ok(new FsObjectInfo[]{top});
        }

        // POST api/files
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public ActionResult Post([FromForm] string source, [FromForm] string target, [FromForm] string action, [FromForm] string text)
        {
            switch(action){
                case "download":
                    var data = _drive.Read(source);
                    var info = _drive.Info(source);

                    return File(data, "application/octet-stream", info.Value);
                
                case "remove":
                    Each(source, path => {
                        _drive.Remove(path);
                        return path;
                    });
                    return Ok(null);

                case "create":
                    var fullpath = Path.Combine(target, source);
                    var name = _drive.Mkdir(fullpath);
                    return Ok(_drive.Info(name));

                case "copy":
                    var copiedNames = Each(source, path => _drive.Copy(path, target));
                    if (copiedNames.Length == 1){
                        return Ok(_drive.Info(copiedNames[0]));
                    }
                    return Ok(Info(copiedNames));

                case "move":
                    var movedNames = Each(source, path => _drive.Move(path, target));
                    if (movedNames.Length == 1){
                        return Ok(_drive.Info(movedNames[0]));
                    }
                    return Ok(Info(movedNames));

                case "rename":
                    var newname = Path.Combine(Path.GetDirectoryName(source), target);
                    return Ok(_drive.Info( _drive.Move(source, newname)));

                case "search":
                    var search = text.ToLower();
                    return Ok(_drive.List(source, new ListConfig{
                        Include = test => test.ToLower().Contains(search),
                        SubFolders=true
                    }));
            }

            return Ok(null);
        }

        [HttpPost("upload")]
        [DisableRequestSizeLimit]
        public ActionResult Upload(IFormFile upload, [FromQuery] string target)
        {
            var fullPath = Path.Combine(target, upload.FileName);
            var name = _drive.Write(fullPath, upload.OpenReadStream());
             
            return Ok(_drive.Info(name));
        }

        private string[] Each(string data, EachPathDelegate handler){
            var paths = data.Split(",");

            for(var i = 0; i < paths.Length; i++)
                paths[i] = handler(paths[i]);

            return paths;
        }

        private FsObject[] Info(string[] paths){
            FsObject[] data = new FsObject[paths.Length];
            for (var i=0; i<paths.Length; i++){
                data[i] = _drive.Info(paths[i]);
            }

            return data;

        }

        private delegate string EachPathDelegate( string str );
        
    }
}
