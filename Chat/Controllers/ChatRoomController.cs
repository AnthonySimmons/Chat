using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Validation;
using Chat.Models;

namespace Chat.Controllers
{
    public class ChatRoomController : HomeController
    {
        //
        // GET: /ChatRoom/
        public ActionResult ChatIndex(int numMsg = -1)
        {
            int userid = (int)Session["userid"];
            ChatRoomUser user = Db.users.Where(m => m.Id == userid).FirstOrDefault();
            
            if (Db.currentRoom == null)
            {
                
                ChatRoom mRoom = Db.chatRooms.Where(c => c.isCurrent == true).FirstOrDefault();
                //if (mRoom != null)
                //{
                    Db.currentRoom = mRoom;
                //}
                //else
                //{
                    //return View("SideBar", Db);
                    //Db.currentRoom = Db.chatRooms.FirstOrDefault();
                //}
            }
            if (Db.currentRoom != null)
            {
                Db.currentRoom.messageCount = 0;
                Db.currentRoom.isCurrent = true;
                if (!Db.currentRoom.Users.Contains(user))
                {
                    Db.currentRoom.Users.Add(user);

                }
                //Db.currentRoom.messages = Db.messages.ToList();//Db.messages.Where(m => m.Room.id == Db.currentRoom.id).ToList();
            }
            if (numMsg != -1)
            {
                Db.currentRoom.maxMessages = numMsg;

            }

            Db.SaveChanges();
            return View("ChatIndex", Db);
        }

        [HttpGet]
        public ActionResult GetCurrentRoom()
        {
            Db.currentRoom = Db.chatRooms.Where(m => m.isCurrent == true).FirstOrDefault();
            return PartialView("ChatRoom", Db);
        }

        [HttpGet]
        public ActionResult GetSideBar()
        {
            Db.currentRoom = Db.chatRooms.Where(m => m.isCurrent == true).FirstOrDefault();
            
            //Db.currentRoom.Users = Db.users.Where(c => c.Room.id == Db.currentRoom.id).ToList();
            return PartialView("SideBar", Db);
        }

        public ActionResult LoadEarlierMessages(int roomId)
        {
            int userid = (int)Session["userid"];
            ChatRoomUser user = Db.users.Where(m => m.Id == userid).FirstOrDefault();

            
            DateTime hourAgo = user.messageLimit.Subtract(new TimeSpan(1, 0, 0));
            user.messageLimit = hourAgo;



            Db.currentRoom = Db.chatRooms.Where(m => m.id == roomId).FirstOrDefault();
            int max = Db.currentRoom.maxMessages;
            if (Db.currentRoom == null)
            {
                Db.currentRoom = Db.chatRooms.Where(m => m.isCurrent == true).FirstOrDefault();
            }

            foreach (ChatMessage ms in Db.messages)
            {
                if (ms.RoomId == Db.currentRoom.id)
                {
                    if (ms.MessageDate > hourAgo)
                    {
                        max++;
                    }
                }
            }
            Db.currentRoom.maxMessages = Math.Max(Db.currentRoom.maxMessages, max);
            //Db.Entry(user).State = System.Data.Entity.EntityState.Modified;
            //Db.SaveChanges();
            return RedirectToAction("ChatIndex", new { numMsg = Db.currentRoom.maxMessages });
        }

        public ActionResult SendMessage(string msg)
        {
            ChatMessage newMsg = new ChatMessage();
            newMsg.Message = msg;
            newMsg.MessageDate = DateTime.Now;

            if (Db.currentRoom == null)
            {
                Db.currentRoom = Db.chatRooms.Where(c => c.isCurrent == true).FirstOrDefault();
                if (Db.currentRoom == null)
                {
                    Db.currentRoom = Db.chatRooms.FirstOrDefault();
                }
            }

            if (Db.currentRoom != null)
            {
                newMsg.Room = Db.currentRoom;
                newMsg.RoomId = Db.currentRoom.id;
                Db.currentRoom.isCurrent = true;
                
            }
            if (Session["userid"] != null)
            {
                newMsg.AuthorId = (int)Session["userid"];
            }
            
            
            ChatRoomUser author = Db.users.Where(u => u.Id == newMsg.AuthorId).FirstOrDefault();
            
            if (author != null)
            {
                author.LastActivity = DateTime.Now;
                newMsg.Author = author;
                newMsg.AuthorName = author.FirstName + " " + author.LastName;
            }
            
            //Db.currentRoom.messages.Add(newMsg);
            //Db.Entry(Db.currentRoom).State = System.Data.Entity.EntityState.Modified;

            Db.messages.Add(newMsg);
            Db.SaveChanges();
            return RedirectToAction("ChatIndex");
            //return View("ChatIndex", Db);
        }

        public ActionResult CreateRoom(string nm)
        {
            ChatRoom cr = new ChatRoom();
            cr.name = nm;
            Db.chatRooms.Add(cr);
            
            if (Db.currentRoom == null)
            {
                Db.currentRoom = Db.chatRooms.FirstOrDefault();
                if (Db.currentRoom != null)
                {
                    Db.currentRoom.isCurrent = true;
                }
            }

            Db.SaveChanges();

            return RedirectToAction("ChatIndex");
        }

        public ActionResult JoinRoom(int id = -1)
        { 
            int userId = (int)Session["userid"];
            ChatRoomUser user = Db.users.Where(m => m.Id == userId).FirstOrDefault();
            ChatRoom newRoom = Db.chatRooms.Where(c => c.id == id).FirstOrDefault();
            ChatRoom oldRoom = Db.chatRooms.Where(c => c.isCurrent == true).FirstOrDefault();

            
            if (oldRoom != null)
            {

                //RedirectToAction("SendMessage", new { msg = user.FirstName + " " + user.LastName + " Has Left The Room" });
                ChatMessage leaveMsg = new ChatMessage();
                leaveMsg.Author = user;
                leaveMsg.AuthorId = user.Id;
                leaveMsg.AuthorName = user.FirstName + " " + user.LastName;
                leaveMsg.MessageDate = DateTime.Now;
                leaveMsg.Room = oldRoom;
                leaveMsg.RoomId = oldRoom.id;
                leaveMsg.Message = user.FirstName + " " + user.LastName + " Has Left The Room.";

                Db.messages.Add(leaveMsg);

                oldRoom.isCurrent = false;
                Db.Entry(oldRoom).State = System.Data.Entity.EntityState.Modified;
                Db.SaveChanges();
            }

            if(newRoom != null)
            {
                if (user != null)
                {
                    user.rid = newRoom.id;
                    Db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                }
                if (Db.currentRoom != null)
                {
                    Db.currentRoom.isCurrent = false;
                }
                newRoom.isCurrent = true;
                Db.currentRoom = newRoom;
                Db.currentRoom.Users.Add(user);

                Db.SaveChanges();
            }
            return RedirectToAction("SendMessage", new { msg = user.FirstName + " " + user.LastName + " Has Joined The Room"});
            //return RedirectToAction("ChatIndex");
        }

        public ActionResult RemoveRoom(int id = -1)
        {
            ChatRoom room = Db.chatRooms.Where(c => c.id == id).FirstOrDefault();

            if (room != null)
            {
                Db.Entry(room).State = System.Data.Entity.EntityState.Deleted;
                Db.SaveChanges();
            }
            return RedirectToAction("ChatIndex");
        }

        public ActionResult SetScroll(int ht)
        {
            Session["ScrollHeight"] = ht;
            return RedirectToAction("GetCurrentRoom");
        }
	}
}