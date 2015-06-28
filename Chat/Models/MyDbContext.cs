using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;


namespace Chat.Models
{
    public class ContextModelChangeInitializer : DropCreateDatabaseIfModelChanges<MyDbContext>
    {
        [Key]
        public int id { get; set; }

        protected override void Seed(MyDbContext context)
        {
            base.Seed(context);

            ChatRoomUser admin = new ChatRoomUser();
            admin.Email = "Admin@Chat.com";
            admin.FirstName = "Admin";
            admin.LastName = "Chat";
            admin.Password = "password";
            admin.isAdmin = true;


            context.users.Add(admin);
            context.SaveChanges();
        }
    }


    public class MyDbContext : DbContext
    {
        private void init()
        {
            //calling this will tell EF to automatically delete the 
            //actual DB whenever a change is detected.  Very handy for debugging, but not so
            //much for production.
            Database.SetInitializer<MyDbContext>(new ContextModelChangeInitializer());
        }
        public MyDbContext()
        {
            init();
            
        }

        public MyDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            init();
            
        }


        public DbSet<ChatMessage> messages { get; set; }
        public DbSet<ChatRoom> chatRooms { get; set; }
        public DbSet<ChatRoomUser> users { get; set; }
        public ChatRoom currentRoom { get; set; }
        public ChatRoomUser currentUser { get; set; }
        
    }
}