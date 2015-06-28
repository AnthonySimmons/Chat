using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Chat.Models;

namespace Chat.Controllers
{
    public class HomeController : Controller
    {
        public Chat.Models.MyDbContext Db;
        int numLoggedIn = 0;
        
        public HomeController()
            : this(null)  //note that calling this(null) will also call 
        //code inside HomeController(MyDbContext)
        {

        }

        public HomeController(MyDbContext someDb = null)
        {
            //If we were given a null context, just use the default context
            if (someDb == null)
            {
                Db = new MyDbContext("MyDbContext");
            }
            else
            {
                //otherwise, use the supplied context
                Db = someDb;
            }
         
        }


        public ActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        public ActionResult Login(ChatRoomUser model)
        {
            int uId = 0;
            bool found = false;
            Session["userid"] = null;
            Session["canwrite"] = false;
            Session["isadmin"] = false;


            foreach (ChatRoomUser p in Db.users)
            {
                if (model.Email == p.Email && model.Password == p.Password)
                {
                    uId = p.Id;
                    
                    Session["userid"] = p.Id;
                    Session["name"] = p.FirstName + " " + p.LastName;
                    Session["login"] = true;
                    Session["ScrollHeight"] = 0;
                    if (p.Email == "Admin@Chat.com")
                    {
                        Session["isadmin"] = true;
                    }
                    Db.currentUser = p;
                    found = true;
                    //break;
                    foreach (ChatRoom cr in Db.chatRooms)
                    {
                        cr.isCurrent = false;
                        Db.Entry(cr).State = System.Data.Entity.EntityState.Modified;
                    }
                    Db.SaveChanges();
                }
            }
            if (!found)
            {
                foreach (string key in ModelState.Keys)
                {
                    ModelState[key].Errors.Clear();
                }

                ModelState.AddModelError("", "Invalid User Name / Password");
                return View("Login");
            }
            Db.SaveChanges();
            //return RedirectToAction("Login");
            //return View("MyBlogs", new { model = db.CurrentBlogger });
            return RedirectToAction("ChatIndex", "ChatRoom");

        }

        public ActionResult DeleteAccount(int id)
        {
            ChatRoomUser p = Db.users.Where(c => c.Id == id).FirstOrDefault();
            if (p != null)
            {
                Db.Entry(p).State = System.Data.Entity.EntityState.Deleted;
                Db.SaveChanges();
            }
            return RedirectToAction("Login");
        }


        /// <summary>
        /// View method to allow the creation of a new Person in the DB
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            Session["isadmin"] = false;
            Session["login"] = false;
            //return the view with a newly created (and empty) Person object.
            return View("Create", new ChatRoomUser());
        }


        /// <summary>
        /// Postback version of the Create action.  Note that this method will only be
        /// called on HTTP POST messages (as indicated by the [HttpPost] attribute.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(ChatRoomUser model)
        {
            //MVC uses ModelState to track the correctness of the supplied Person "model."
            //If the model is valid, it means that MVC didn't detect any errors as defined
            //in the attributes in the Person class.
            if (ModelState.IsValid)
            {
                /*if(CurrentPicUrl != "")
                {
                    model.PicUrl = CurrentPicUrl;
                    CurrentPicUrl = "";
                }*/

                //Add the person to the DB.  This queues them for insertion but doesn't
                //actually insert the value into the DB.  For that, we need the next command.

                //model.setRandomPic();
                Db.users.Add(model);

                //tell the DB to save any queued changes.
                Db.SaveChanges();

                //Once we're done, redirect to the Index action of HomeController.
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError("", "One or more issues were found with your submission. Please try again.");
            }
            //If we got here, it means that the model's state is invalid.  Simply return
            //to the create page and display any errors.
            return View("Login", model);
        }

        public ActionResult Logout()
        {
            Session["isadmin"] = false;
            Session["login"] = false;
            int uid = (int)Session["userid"];
            ChatRoomUser user = Db.users.Where(c => c.Id == uid).FirstOrDefault();
            ChatRoom oldRoom = Db.chatRooms.Where(c => c.isCurrent == true).FirstOrDefault();
            if (user != null)
            {
                user.Room = null;
                user.rid = -1;

                if (oldRoom != null)
                {
                    ChatMessage leaveMsg = new ChatMessage();
                    leaveMsg.Author = user;
                    leaveMsg.AuthorId = user.Id;
                    leaveMsg.AuthorName = user.FirstName + " " + user.LastName;
                    leaveMsg.MessageDate = DateTime.Now;
                    leaveMsg.Room = oldRoom;
                    leaveMsg.RoomId = oldRoom.id;
                    leaveMsg.Message = user.FirstName + " " + user.LastName + "Has Left The Room.";

                    Db.messages.Add(leaveMsg);
                }

                if (Db.currentRoom != null)
                {
                    Db.currentRoom.Users.Remove(user);
                    Db.Entry(Db.currentRoom).State = System.Data.Entity.EntityState.Modified;
                }

                foreach (ChatRoom cr in Db.chatRooms)
                {
                    if (cr.Users.Contains(user))
                    {
                        cr.Users.Remove(user);
                        Db.Entry(cr).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                Db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                Db.SaveChanges();
            }
            Db.currentRoom = null;

            return RedirectToAction("Login");
        }

        public ActionResult Edit(int id = -1)
        {
            if (id < 0)
            {
                id = (int)Session["userid"];
            }
            //Find the person in the DB.  Use the supplied "id" integer as a reference.
            ChatRoomUser somePerson = Db.users
                .Where(p => p.Id == id)     //this line says to find the person whose ID matches our parameter
                .FirstOrDefault();          //FirstOrDefault() returns either a singluar Person object or NULL

            //If we got NULL, it must mean that we were supplied an incorrect ID.  
            //In this case, redirect to HomeController's Index action.
            if (somePerson == null)
            {
                return RedirectToAction("Index");
            }

            //If we're here, then we must have a valid person.  Send to the "Create" view because
            //create and edit are kind of the same thing.  The 2nd parameter is the model that
            //we will be sending to the Create view.
            return View("Create", somePerson);
        }

        /// <summary>
        /// Postback version of the Edit action.  Will be called when the browser sends us information
        /// back to the server.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(ChatRoomUser model)
        {
            //again, check modelstate to make sure everything went okay
            if (ModelState.IsValid)
            {
                //Because the item already exists in the DB, we want to tell EF that
                //one of its models has been changed.  We use this somewhat strange syntax to
                //accomplish this task.
                Db.Entry(model).State = System.Data.Entity.EntityState.Modified;

                //Again, the above command adds the request to a queue.  To execute the queue,
                //we need to call SaveChanges()
                Db.SaveChanges();

                //when complete, redirect to Index
                return RedirectToAction("Index");
            }

            //Things must've went bad, so send back to the Create view.
            return View("Create", model);
        }


        public ActionResult Index()
        {
            return View(Db.users.ToList());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }



    }
}