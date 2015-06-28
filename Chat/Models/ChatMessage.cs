using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Chat.Models
{
    public class ChatMessage
    {
        public string AuthorName { get; set; }

        public ChatRoomUser Author { get; set; }
        public int AuthorId { get; set; }
        
        [Key]
        public int Id { get; set; }

        public string Message { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime MessageDate { get; set; }
        public ChatRoom Room { get; set; }
        public int RoomId { get; set; }



        public ChatMessage()
        {
            Author = new ChatRoomUser();
            Message = "";
            MessageDate = DateTime.Now;
            Room = new ChatRoom();

        }
    }
}