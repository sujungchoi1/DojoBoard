using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TheWall.Models;

namespace TheWall.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyContext dbContext;

        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        // helper method to get the current user id
        public User GetCurrentUser()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return null;
            }
            return dbContext
                .Users
                .First(u => u.UserId == userId);
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User userToRegister)
        {
            if (dbContext.Users.Any(u => u.Email == userToRegister.Email))
            {
                ModelState.AddModelError("Email", "Please use a different email.");
            }

            if (ModelState.IsValid)
            {
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                userToRegister.Password = Hasher.HashPassword(userToRegister, userToRegister.Password);
                dbContext.Add(userToRegister);
                dbContext.SaveChanges(); 
                HttpContext.Session.SetInt32("UserId", userToRegister.UserId);
                return RedirectToAction("Dashboard");
            }

            return View("Index"); 
        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser userToLogin)
        {
            // look in the DB              if we don't find the user at all, the default is null
            var foundUser = dbContext.Users.FirstOrDefault(u => u.Email == userToLogin.LoginEmail);

            if (foundUser == null)
            {
                ModelState.AddModelError("LoginEmail", "Please check your email and password");
                return View("Index");
            }
            var hasher = new PasswordHasher<LoginUser>();
            var result = hasher.VerifyHashedPassword(userToLogin, foundUser.Password, userToLogin.LoginPassword);

            if (result == 0)
            {
                ModelState.AddModelError("LoginPassword", "Please check your email and password");
                return View("Index");
            }

            // set ID in session
            HttpContext.Session.SetInt32("UserId", foundUser.UserId);
            return RedirectToAction("Dashboard");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.CurrentUser = currentUser;
            ViewBag.AllMessages = dbContext.Messages
                .Include(m => m.PostedBy)
                .Include(m => m.Comments)
                    .ThenInclude(c => c.CommentedBy)
                .OrderByDescending(d => d.CreatedAt)
                .ToList();
            
            return View();
        }

        [HttpPost("create-message")]
        public IActionResult CreateMessage(Message messageFromForm)
        {   
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                messageFromForm.UserId = (int)HttpContext.Session.GetInt32("UserId"); // so the post created has the UserId linked to it

                dbContext.Messages.Add(messageFromForm);
                dbContext.SaveChanges();

                return Redirect("Dashboard");
            }

            return View("Dashboard");
        }
        [HttpPost("create-comment")]
        public IActionResult CreateComment(int messageId, string commentBody)
        {   
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction("Index");
            }

            // creating a new object for the form w/ partial component 
            var newComment = new Comment {
                MessageId = messageId,
                Content = commentBody,
                UserId = currentUser.UserId
            };

            dbContext.Comments.Add(newComment);
            dbContext.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        [HttpPost("delete/{msgId}")]
        public IActionResult DeleteMessage(int msgId) // this movieId should match the one from url
        {
            // find it
            var msgToDelete = dbContext.Messages
                .First(m => m.MessageId == msgId);
            
            // delete it
            dbContext.Remove(msgToDelete);
            dbContext.SaveChanges();

            // persist changes to the db
            return RedirectToAction("Dashboard");

        }

    }
}
