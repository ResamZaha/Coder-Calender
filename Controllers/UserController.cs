using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoderCalender.Models;

namespace CoderCalender.Controllers
{
    public class UserController : Controller
    {
        private readonly UserDataContext _context;

        public UserController(UserDataContext context)
        {
            _context = context;
        }
        private UserModel? GetCurrentUser()
        {
            string? name = HttpContext.Request.Cookies["user"];
            string? passwordHash = HttpContext.Request.Cookies["passwordHash"];
            string? id = HttpContext.Request.Cookies["id"];
            if (name != null && passwordHash != null && id != null)
            {
                UserModel userModel = new UserModel();
                userModel.name = name;
                userModel.passwordHash = passwordHash;
                userModel.Id = Convert.ToInt32(id);
                return userModel;
            }
            return null;
        }
        const string SessionKeyName = "Admin";
        const string SessionKeyPass = "Password";
        const string SessionKeyId = "Id";
        private bool isAdmin()
        {

            var userNow = GetCurrentUser();
            UserModel? userAdmin = _context?.UserModel?.Where(e => e.name == "admin@_coder").FirstOrDefault();
            if (userNow == null) return false;
            if (userAdmin == null) return false;

            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyName)))
            {
                if (userNow.name.Equals(userAdmin.name) && userNow.Id.Equals(userAdmin.Id) && userNow.passwordHash.Equals(userAdmin.passwordHash))
                {
                    //todo validate in database
                    HttpContext.Session.SetString(SessionKeyName, userNow.name);
                    HttpContext.Session.SetString(SessionKeyPass, userNow.passwordHash);
                    HttpContext.Session.SetString(SessionKeyId, Convert.ToString(userNow.Id));
                    return true;
                }
                return false;
            }
            var adminName = HttpContext.Session.GetString(SessionKeyName);
            var adminPass = HttpContext.Session.GetString(SessionKeyPass);
            var adminId = HttpContext.Session.GetString(SessionKeyId);
            Console.WriteLine(adminId, adminName, adminPass);

            if (userNow.name != adminName)
            {
                return false;
            }
            if (Convert.ToString(userNow.Id) != adminId)
            {
                return false;
            }
            if (userNow.passwordHash != adminPass)
            {
                return false;
            }
            return true;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {

            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            return _context.UserModel != null ?
                        View(await _context.UserModel.ToListAsync()) :
                        Problem("Entity set 'UserDataContext.UserModel'  is null.");
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (id == null || _context.UserModel == null)
            {
                return NotFound();
            }

            var userModel = await _context.UserModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }

        // GET: User/Create
        public IActionResult Create()
        {

            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,name,passwordHash")] UserModel userModel)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (ModelState.IsValid)
            {
                _context.Add(userModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userModel);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (id == null || _context.UserModel == null)
            {
                return NotFound();
            }

            var userModel = await _context.UserModel.FindAsync(id);
            if (userModel == null)
            {
                return NotFound();
            }
            return View(userModel);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,passwordHash")] UserModel userModel)
        {
            if (!isAdmin())
            {
                //sudhu admin i ki password change korbe?? nam ki change korte dibo
                return RedirectToAction("Error", "Home");
            }
            if (id != userModel.Id)
            {
                return NotFound();
            }
            userModel.convertPassword();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserModelExists(userModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(userModel);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (id == null || _context.UserModel == null)
            {
                return NotFound();
            }

            var userModel = await _context.UserModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (_context.UserModel == null)
            {
                return Problem("Entity set 'UserDataContext.UserModel'  is null.");
            }
            var userModel = await _context.UserModel.FindAsync(id);
            if (userModel != null)
            {
                _context.UserModel.Remove(userModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserModelExists(int id)
        {
            return (_context.UserModel?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
