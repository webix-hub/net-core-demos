using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;


namespace WebixDemos.Controllers
{

    [Route("api/attachments")]
    [ApiController]
    public class UploaderController : ControllerBase
    {

        private readonly IConfiguration _config;
        public UploaderController(IConfiguration config)
        {
            _config = config;
        }


        // GET api/attachments/{name}
        [HttpGet("{name}")]
        public ActionResult LoadImage(string name)
        {
            var path = _config.GetValue<string>("uploads");
            var fullPath = Path.Combine(path, name);

            var data = new FileStream(fullPath, FileMode.Open);
            return File(data, "application/octet-stream");
        }

       

        // POST api/attachments
        [HttpPost]
        [DisableRequestSizeLimit]
        public ActionResult AttachFile(IFormFile upload)
        {
            var path = _config.GetValue<string>("uploads");
            var name = Path.GetRandomFileName()+Path.GetExtension(upload.FileName);
            var fullPath = Path.Combine(path, name);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                upload.CopyTo(stream);
            }
            return Ok(new{ Sname=$"/api/persons/attach/{name}", Status="server" });
        }
    }
}
