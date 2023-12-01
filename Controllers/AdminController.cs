using CoderCalender.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoderCalender.Controllers
{
    public class AdminController : Controller
    {
        private readonly ContestContext contestContext;
        private readonly ContestBookmarkContext contestBookmarkContext;
        private readonly UserDataContext userDataContext;

        public AdminController(ContestContext contestContext, ContestBookmarkContext contestBookmarkContext, UserDataContext userDataContext)
        {
            this.contestContext = contestContext;
            this.contestBookmarkContext = contestBookmarkContext;
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

        public async Task<IActionResult> Index()
        {
            if (!isAdmin())
            {
                return RedirectToAction("Error", "Home");
            }
            //get all contests
            List<ContestModel> dataContests = contestContext?.ContestModel?.ToList()!;
            List<ContestListModel> dataContestBookmarks = contestBookmarkContext.ContestListModel.ToList();
            List<UserModel> dataUsers = userDataContext.UserModel.ToList();
            ViewData["contestList"] = dataContests;
            ViewData["contestBookmarkList"] = dataContestBookmarks;
            ViewData["userList"] = dataUsers;
            return View();
        }
    }
}