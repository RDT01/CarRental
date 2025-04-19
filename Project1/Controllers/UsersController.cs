using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Project1.Filters;
using Project1.Models;

namespace Project1.Controllers
{
    public class UsersController : Controller
    {
        private UsersContext db = new UsersContext();

        // Display all users
        public ActionResult Index()
        {
            using (var db = new UsersContext())
            {
                var users = db.Users.ToList();
                return View(users);
            }
        }

        // Show details for a specific user
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            return View(users);
        }

        // GET form to create a user (Admin only)
        [AdminAuthorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Add new user to the database (Admin only)
        [HttpPost]
        [AdminAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Email,Address,Phone")] Users users)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(users);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(users);
        }

        // GET: Load user edit form (Admin only)
        [AdminAuthorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            return View(users);
        }

        // POST: Save updated user info to the database (Admin only)
        [HttpPost]
        [AdminAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Email,Password,Address,Phone")] Users users)
        {
            var existingUser = db.Users.Find(users.Id);
            if (existingUser == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                // Update basic user fields
                existingUser.Name = users.Name;
                existingUser.Email = users.Email;
                existingUser.Address = users.Address;
                existingUser.Phone = users.Phone;

                // Only update password if one was provided
                if (!string.IsNullOrWhiteSpace(users.PasswordHash))
                {
                    existingUser.PasswordHash = HashPassword(users.PasswordHash);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(existingUser);
        }

        // GET: Confirm delete user (Admin only)
        [AdminAuthorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            return View(users);
        }

        // POST: Actually delete user (Admin only)
        [HttpPost, ActionName("Delete")]
        [AdminAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Users users = db.Users.Find(id);
            db.Users.Remove(users);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Dispose database resources
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // Load registration page
        public ActionResult Register()
        {
            return View();
        }

        // POST: Register a new user with hashed password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Users model, string Password, string Role)
        {
            if (ModelState.IsValid)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Password));
                    var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

                    model.PasswordHash = hash;
                    model.Role = Role; // Role selected from dropdown
                }

                db.Users.Add(model);
                db.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(model);
        }

        // Utility method to hash a password string
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        // Load login page
        public ActionResult Login()
        {
            return View();
        }

        // POST: Validate login credentials and assign session
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Login(string email, string password)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                Session["UserId"] = user.Id;
                Session["Username"] = user.Name;
                Session["Role"] = user.Role;

                if (user.Role == "Admin")
                {
                    return Json(new { success = true, redirectTo = Url.Action("Index", "Admin") });
                }
                else
                {
                    return Json(new { success = true, redirectTo = Url.Action("Index", "Home") });
                }
            }
            else
            {
                return Json(new { success = false, message = "Invalid credentials" });
            }
        }

        // Compare entered password against stored hash
        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            string enteredPasswordHash = HashPassword(enteredPassword);
            return enteredPasswordHash == storedPasswordHash;
        }

        // Clear all session data (logout)
        public ActionResult Logout()
        {
            Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        // POST: Admin can update a user's role via AJAX
        [HttpPost]
        [AdminAuthorize]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateUserRole(int userId, string newRole)
        {
            var user = db.Users.Find(userId);
            if (user != null)
            {
                user.Role = newRole;
                db.SaveChanges();
                return Json(new { success = true, message = "Role updated successfully." });
            }
            return Json(new { success = false, message = "User not found." });
        }
    }
}
