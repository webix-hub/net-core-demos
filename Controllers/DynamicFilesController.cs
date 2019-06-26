using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Webix.WFS.Local;

public class FsObjectDynFiles : FsObject {
    public bool webix_files;    
}

namespace WebixDemos.Controllers
{
    [Route("api/dynfiles")]
    [ApiController]
    public class DynamicFilesController : ControllerBase
    {

        private LocalDrive _drive;

        public DynamicFilesController(){
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
                Data= MarkFolders(_drive.List("/", new ListConfig{Nested=true, SubFolders=true,SkipFiles=true})),
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
                    data[i] = new FsObjectDynFiles{
                        Value = data[i].Value,
                        ID = data[i].ID,
                        Type = data[i].Type,
                        Size = data[i].Size,
                        Date = data[i].Date,
                        Data = data[i].Data != null ? MarkFolders(data[i].Data) : null,
                        webix_files = true
                    };
                }

            return data;
        }
    }
}
