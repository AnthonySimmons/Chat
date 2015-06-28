using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace Chat.Models
{
    public class ChatRoomUser
    {
        [DataType(DataType.DateTime)]
        public DateTime LastActivity { get; set; }

        public ChatRoom Room { get; set; }
        //public int RoomId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime messageLimit { get; set; }

        /// <summary>
        /// All EF classes need a key for DB storage.  Keys need to be integers.
        /// We use the [Key] tag to tell EF which property we want to use as the DB key.
        /// </summary>
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter your first name.")]
        [Display(Name = "First Name:")]
        [MaxLength(50, ErrorMessage = "Name is too long (50 chars max).")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter your last name.")]
        [Display(Name = "Last Name:")]
        public string LastName { get; set; }

        //we can even specify a data type using attributes!
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Please enter your email address.")]
        [Display(Name = "Email Address:")]
        public string Email { get; set; }


        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please enter your password.")]
        [Display(Name = "Password:")]
        public string Password { get; set; }
        //1355230

        public string PicUrl { get; set; }

        public bool isAdmin { get; set; }

        
        public int rid { get; set; }
        
        public ChatRoomUser()
        {
            messageLimit = DateTime.Now;
            LastActivity = DateTime.Now;
            isAdmin = false;
            setRandomPic();   
        }

        public void setRandomPic()
        {
            Random rand = new Random();
            
            if (PicUrl == null)
            {
                int ran = (rand.Next() % 5);
                if(ran == 0)
                {

                }
                PicUrl = "/Content/Images/pic" + ran.ToString() + ".png";
            }
        }
    }
}