using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.IO;

namespace WebixDemos.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class SchedulerController : ControllerBase
    {
        // GET api/events
        [HttpGet]
        public ActionResult Load()
        {
            using (var db = new DemosContext()){
                var events = db.Events.ToList();
                return Ok(events);
            }
        }


        // POST api/events
        [HttpPost]
        public ActionResult Insert([FromForm] Event ev)
        {
            using (var db = new DemosContext()){
                ev.Id = 0;
                db.Events.Add(ev);
                db.SaveChanges();
                return Ok(new {Id=ev.Id});
            }
        }

        // PUT api/events
        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromForm] Event update)
        {
            using (var db = new DemosContext()){
                var ev = db.Events.Where(a => a.Id == id).FirstOrDefault();

                ev.Text = update.Text;
                ev.StartDate = update.StartDate;
                ev.EndDate = update.EndDate;
                
                db.Events.Update(ev);
                db.SaveChanges();
                return Ok(new {Id=ev.Id});
            }
        }

        // DELETE api/events
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            using (var db = new DemosContext()){
                var ev = db.Events.Where(a => a.Id == id).FirstOrDefault();
                db.Events.Remove(ev);  
                db.SaveChanges();
                return Ok(new {Id=id});
            }
        }
    }
}
