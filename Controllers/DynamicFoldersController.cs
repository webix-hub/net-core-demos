using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Webix.WFS.Local;

public class SubFolderInfo {
    public string Parent;
    public FsObject[] Data;
}

public class FsObjectDynFolder : FsObject {
    public bool webix_branch;    
}

namespace WebixDemos.Controllers
{
    [Route("api/dynfolders")]
    [ApiController]
    public class DynamicFoldersController : ControllerBase
    {

        private LocalDrive _drive;

        public DynamicFoldersController(){
            _drive = new LocalDrive("./", new DriveConfig{
                Operation = new OperationConfig{ PreventNameCollision=true }
            });
        }

        [HttpGet]
        public ActionResult<IEnumerable<FsObjectInfo>> Get()
        {
            var top = new FsObjectInfo{
                Value="Files",
                ID="/",
                Type="folder",
                Data= MarkFolders(_drive.List("/")),
                Open=true
            };

            return Ok(new FsObjectInfo[]{top});
        }


        [HttpPost]
        public ActionResult<IEnumerable<FsObjectInfo>> Get([FromForm] string source)
        {
            return Ok(new SubFolderInfo{
                Data = MarkFolders(_drive.List(source)),
                Parent = source
            });
        }

        private FsObject[] MarkFolders(FsObject[] data){
            for (var i=0; i<data.Length; i++)
                if (data[i].Type == "folder"){
                    data[i] = new FsObjectDynFolder{
                        Value = data[i].Value,
                        ID = data[i].ID,
                        Type = data[i].Type,
                        Size = data[i].Size,
                        Date = data[i].Date,
                        webix_branch = true
                    };
                }

            return data;
        }
    }
}
