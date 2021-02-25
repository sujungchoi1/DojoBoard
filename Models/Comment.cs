using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TheWall.Models
{
    public class Comment
    {
        [Key]
        public int CommentId {get;set;}

        [Required(ErrorMessage="Please enter a comment")]
        [Display(Name="Post a comment")]
        public string Content {get;set;}
        
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
        

        public int UserId {get;set;} // foreign key
        public User CommentedBy {get;set;}
        public int MessageId {get;set;} // foreign key
        public Message Messages {get;set;}

    }
}