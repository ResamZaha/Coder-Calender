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
    public class ContestListController : Controller
    {
        private readonly ContestBookmarkContext _context;
        private readonly UserDataContext userDataContext;

        public ContestListController(ContestBookmarkContext context, UserDataContext userDataContext)
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


        // GET: ContestList
        public async Task<IActionResult> Index()
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            return _context.ContestListModel != null ?
                        View(await _context.ContestListModel.ToListAsync()) :
                        Problem("Entity set 'ContestBookmarkContext.ContestListModel'  is null.");
        }

        // GET: ContestList/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (id == null || _context.ContestListModel == null)
            {
                return NotFound();
            }

            var contestListModel = await _context.ContestListModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contestListModel == null)
            {
                return NotFound();
            }

            return View(contestListModel);
        }

        // GET: ContestList/Create
        public IActionResult Create()
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            return View();
        }

        // POST: ContestLis/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,userId,contestId")] ContestListModel contestListModel)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (ModelState.IsValid)
            {
                _context.Add(contestListModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contestListModel);
        }

        // GET: ContestList/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (id == null || _context.ContestListModel == null)
            {
                return NotFound();
            }

            var contestListModel = await _context.ContestListModel.FindAsync(id);
            if (contestListModel == null)
            {
                return NotFound();
            }
            return View(contestListModel);
        }

        // POST: ContestList/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,userId,contestId")] ContestListModel contestListModel)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (id != contestListModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contestListModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContestListModelExists(contestListModel.Id))
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
            return View(contestListModel);
        }

        // GET: ContestList/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (id == null || _context.ContestListModel == null)
            {
                return NotFound();
            }

            var contestListModel = await _context.ContestListModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contestListModel == null)
            {
                return NotFound();
            }

            return View(contestListModel);
        }

        // POST: ContestList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            if (_context.ContestListModel == null)
            {
                return Problem("Entity set 'ContestBookmarkContext.ContestListModel'  is null.");
            }
            var contestListModel = await _context.ContestListModel.FindAsync(id);
            if (contestListModel != null)
            {
                _context.ContestListModel.Remove(contestListModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContestListModelExists(int id)
        {
            return (_context.ContestListModel?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
