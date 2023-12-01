using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoderCalender.Models;

namespace CoderCalender.Controllers
{
    public class ContestController : Controller
    {
        private readonly ContestContext _context;
        private readonly UserDataContext userDataContext;

        public ContestController(ContestContext context, UserDataContext userDataContext)
        {
            _context = context;
            this.userDataContext = userDataContext;
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
            UserModel? userAdmin = userDataContext?.UserModel?.Where(e => e.name == "admin@_coder").FirstOrDefault();
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


        // GET: Contest
        public async Task<IActionResult> Index()
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            return _context.ContestModel != null ?
                        View(await _context.ContestModel.ToListAsync()) :
                        Problem("Entity set 'ContestContext.ContestModel'  is null.");
        }

        // GET: Contest/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (id == null || _context.ContestModel == null)
            {
                return NotFound();
            }

            var contestModel = await _context.ContestModel
                .FirstOrDefaultAsync(m => m.id == id);
            if (contestModel == null)
            {
                return NotFound();
            }

            return View(contestModel);
        }

        // GET: Contest/Create
        public IActionResult Create()
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            return View();
        }

        // POST: Contest/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,title,start,end,duration,link,status,platform")] ContestModel contestModel)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (ModelState.IsValid)
            {
                _context.Add(contestModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contestModel);
        }

        // GET: Contest/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (id == null || _context.ContestModel == null)
            {
                return NotFound();
            }

            var contestModel = await _context.ContestModel.FindAsync(id);
            if (contestModel == null)
            {
                return NotFound();
            }
            return View(contestModel);
        }

        // POST: Contest/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("id,title,start,end,duration,link,status,platform")] ContestModel contestModel)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (id != contestModel.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contestModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContestModelExists(contestModel.id))
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
            return View(contestModel);
        }

        // GET: Contest/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (id == null || _context.ContestModel == null)
            {
                return NotFound();
            }

            var contestModel = await _context.ContestModel
                .FirstOrDefaultAsync(m => m.id == id);
            if (contestModel == null)
            {
                return NotFound();
            }

            return View(contestModel);
        }

        // POST: Contest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (_context.ContestModel == null)
            {
                return Problem("Entity set 'ContestContext.ContestModel'  is null.");
            }
            var contestModel = await _context.ContestModel.FindAsync(id);
            if (contestModel != null)
            {
                _context.ContestModel.Remove(contestModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContestModelExists(string id)
        {
            return (_context.ContestModel?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
