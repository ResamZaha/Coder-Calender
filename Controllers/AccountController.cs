using Microsoft.AspNetCore.Mvc;
using CoderCalender.Models;
using Microsoft.EntityFrameworkCore;

namespace CoderCalender.Controllers
{
    public class AccountController : Controller
    {
        private CookieOptions cookieOptions = new CookieOptions
        {
            Expires = DateTime.Now.AddDays(10),
        };
        private readonly UserDataContext _context;
        private readonly ContestBookmarkContext _contest_bookmark_context;
        private readonly ContestContext _contest_context;
        public AccountController(UserDataContext context, ContestBookmarkContext contest_bookmark_context, ContestContext contest_context)
        {
            _context = context;
            _contest_bookmark_context = contest_bookmark_context;
            _contest_context = contest_context;
        }
        //
        // GET: /Account/register
        [HttpGet]
        public ViewResult Register()
        {
            return View();
        }

        //
        // GET: /Account/Login
        [HttpGet]
        public ViewResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            Console.WriteLine("called " + model.Username + " " + model.Password);
            if (ModelState.IsValid)
            {
                UserModel userModel = new UserModel(model);
                if (UserModelExists(userModel))
                {
                    //todo show alert
                    return View(model);
                }
                _context.Add(userModel);
                var res = await _context.SaveChangesAsync();
                userModel = GetUserModel(userModel)!;
                SetUser(userModel);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(UserModel model)
        {
            Console.WriteLine("called " + model.name + " " + model.passwordHash);
            if (ModelState.IsValid)
            {
                model.convertPassword();
                UserModel? temp = GetUserModel(model);
                if (temp != null && temp.name == model.name && temp.passwordHash == model.passwordHash)
                {
                    model.Id = temp.Id;
                    SetUser(model);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View(model);
                }
            }
            else
            {
                return View();
            }
        }

        private bool UserModelExists(UserModel user)
        {
            return (_context.UserModel?.Any(e => (e.name == user.name && e.passwordHash == user.passwordHash))).GetValueOrDefault();
        }
        private UserModel? GetUserModel(UserModel userModel)
        {
            return (_context.UserModel?.FirstOrDefault(e =>
            (e.name == userModel.name && e.passwordHash == userModel.passwordHash)));
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
                return GetUserModel(userModel);
            }
            return null;
        }

        //
        // GET: /Account/Details
        [HttpGet]
        public async Task<IActionResult> Details()
        {
            UserModel? currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return NotFound();
            }
            if (currentUser == null || _context.UserModel == null)
            {
                return NotFound();
            }

            var userModel = await _context.UserModel
                .FirstOrDefaultAsync(m => m.Id == currentUser.Id);
            if (userModel == null)
            {
                return NotFound();
            }
            var res1 = await _contest_bookmark_context.ContestListModel.Where(e => e.userId == currentUser.Id).ToListAsync();
            // var contest_list = await _contest_context.ContestModel.Where(e => res1.Any(f => f.contestId == e.id)).ToListAsync();

            var contest_list = new List<ContestModel>();
            for (int i = 0; i < res1.Count; i++)
            {
                var contest = await _contest_context.ContestModel.Where(e => e.id == res1[i].contestId).FirstOrDefaultAsync();
                if (contest != null)
                {
                    contest_list.Add(contest);
                }
            }
            ViewData["contest_list"] = contest_list;
            ViewData["count"] = contest_list.Count;
            return View(userModel);
        }

        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("user");
            HttpContext.Response.Cookies.Delete("passwordHash");
            HttpContext.Response.Cookies.Delete("id");
            return RedirectToAction("Index", "Home");
        }
        private void SetUser(UserModel userModel)
        {
            HttpContext.Response.Cookies.Append("user", userModel.name, cookieOptions);
            HttpContext.Response.Cookies.Append("passwordHash", userModel.passwordHash, cookieOptions);
            HttpContext.Response.Cookies.Append("id", Convert.ToString(userModel.Id), cookieOptions);
            if(userModel.name.Equals("admin@_coder")){
                HttpContext.Session.SetString("Admin" , userModel.name);
                HttpContext.Session.SetString("Password", userModel.passwordHash);
                HttpContext.Session.SetString("Id", Convert.ToString(userModel.Id));
            }
        }

        [HttpPost]
        public async Task<Int32> BookmarkToggle(string link)
        {
            Console.WriteLine("called " + link);
            if (link == null || link.Count() == 0)
            {
                return -1;
            }
            UserModel? userNow = GetCurrentUser();
            if (userNow == null)
            {
                return -1;
            }
            else
            {
                var contestListModel = await _contest_bookmark_context.ContestListModel.FirstOrDefaultAsync(x => x.userId == userNow.Id && x.contestId == link);
                Console.WriteLine(contestListModel.contestId, contestListModel.userId, contestListModel.Id);
                _contest_bookmark_context?.ContestListModel?.Remove(contestListModel);
                await _contest_bookmark_context?.SaveChangesAsync();
                return 0;
            }
        }
    }
}