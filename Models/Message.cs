using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TheWall.Models
{
    public class Message
    {
        [Key]
        public int MessageId {get;set;}

        [Required(ErrorMessage="Please enter a message")]
        [Display(Name="Post a message")]
        public string Content {get;set;}

        
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
        

        public List<Comment> Comments {get;set;}


        public int UserId {get;set;} // foreign key
        public User PostedBy {get;set;}

    }
}