using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Chat.Models
{
    public class ChatRoom
    {
        [Key]
        public int id { get; set; }

        public bool isActiveRoom { get; set; }
        public bool isDefaultRoom { get; set; }
        public string name { get; set; }
        public string school { get; set; }
        public int schoolId { get; set; }
        public int location { get; set; }
        public string url { get; set; }
        public int UserCount { get; set; }
        public List<ChatRoomUser> Users { get; set; }
        public List<ChatMessage> messages { get; set; }
        public bool isCurrent { get; set; }
        public int messageCount { get; set; }
        public int maxMessages { get; set; }

        public ChatRoom()
        {
            maxMessages = 15;
            messages = new List<ChatMessage>();
            Users = new List<ChatRoomUser>();
        }
    }
}