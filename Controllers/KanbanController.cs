using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;

namespace WebixDemos.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class KanbanController : ControllerBase
    {
        // GET api/tasks
        [HttpGet]
        public ActionResult Load()
        {
            using (var db = new DemosContext()){
                var tasks = db.Tasks.OrderBy(a => a.Order).ToList();
                return Ok(tasks);
            }
        }


        // POST api/tasks
        [HttpPost]
        public ActionResult Insert([FromForm] Task task)
        {
            using (var db = new DemosContext()){
                task.Id = 0;
                if (task.Text == null)
                    task.Text = "";
                db.Tasks.Add(task);
                db.SaveChanges();
                reorder(db, task, 0, task.Status);

                return Ok(new {Id=task.Id});
            }
        }

        // PUT api/tasks
        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromForm] Task update, [FromForm] string webix_move_parent, [FromForm] string webix_move_id)
        {
            using (var db = new DemosContext()){
                var ev = db.Tasks.Where(a => a.Id == id).FirstOrDefault();

                if (webix_move_parent != null){
                    int pid = 0;
                    if (webix_move_id != "" && webix_move_id != null)
                        pid = Int32.Parse(webix_move_id);
                    reorder(db, ev, pid, webix_move_parent);
                } else {
                    ev.Text = update.Text;
                    ev.Status = update.Status;

                    db.Tasks.Update(ev);
                    db.SaveChanges();
                }

                return Ok(new {Id=ev.Id});
            }
        }

        // DELETE api/tasks
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            using (var db = new DemosContext()){
                var ev = db.Tasks.Where(a => a.Id == id).FirstOrDefault();
                db.Tasks.Remove(ev);  
                db.SaveChanges();
                return Ok(new {Id=id});
            }
        }

        private void reorder(DemosContext db, Task task, int id, string status){
            if (id == 0){
                var tasks = db.Tasks.Where(a => a.Status == status).ToList();
                int max = 0;
                tasks.ForEach(a => {
                    if (a.Order > max) max = a.Order;
                });

                task.Order = max + 1;
                db.SaveChanges();
            } else {
                var prev = db.Tasks.Find(id);
                var ind = prev.Order;

                var tasks = db.Tasks.Where(a => a.Order >= ind && a.Status == status).ToList();
                tasks.ForEach( a => a.Order+=1 );

                task.Order = ind;
                db.SaveChanges();
            }
        }
    }
}
