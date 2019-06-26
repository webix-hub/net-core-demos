using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.IO;

public class SheetData {
    public Newtonsoft.Json.Linq.JArray Data;
}

namespace WebixDemos.Controllers
{
    [Route("api/sheets")]
    [ApiController]
    public class SpreadsheetController : ControllerBase
    {
        private readonly IConfiguration _config;
        public SpreadsheetController(IConfiguration config)
        {
            _config = config;
        }

        // GET api/sheets/{id}
        [HttpGet("{id}")]
        public ActionResult Load(int id)
        {
            using (var db = new DemosContext()){
                var sheet = db.Sheets.Where(a => a.Id == id).SingleOrDefault();
                var json = sheet == null ? "{}" : sheet.Data;
                return Content(json, "application/json");
            }
        }

        // GET api/sheets/{id}/image
        [HttpGet("{id}/images/{name}")]
        public ActionResult LoadImage(int id, string name)
        {
            var path = _config.GetValue<string>("uploads");
            var fullPath = Path.Combine(path, name);

            var data = new FileStream(fullPath, FileMode.Open);
            return File(data, "application/octet-stream");
        }

        // POST api/sheets/{id}/image
        [HttpPost("{id}/images")]
        [DisableRequestSizeLimit]
        public ActionResult SaveImage(IFormFile upload, int id)
        {
            var path = _config.GetValue<string>("uploads");
            var name = Path.GetRandomFileName()+Path.GetExtension(upload.FileName);
            var fullPath = Path.Combine(path, name);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                upload.CopyTo(stream);
            }
            return Ok(new{ imageURL=$"/api/sheets/{id}/images/{name}", Status="server" });
        }

        // POST api/sheets/{id}
        [HttpPost("{id}")]
        public ActionResult Save(int id, [FromBody] SheetData data)
        {
            string body = data.Data.ToString();
            using (var db = new DemosContext()){
                var sheet = db.Sheets.Where(a => a.Id == id).SingleOrDefault();
                if (sheet == null){
                    db.Sheets.Add(new Sheet{Data=body, Id=id});
                } else {
                    sheet.Data = body;
                }

                db.SaveChanges();
                return Content("{}", "application/json");
            }
        }
    }
}
