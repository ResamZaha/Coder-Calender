using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CoderCalender.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace CoderCalender.Controllers;

public class HomeController : Controller
{
    static List<ContestModel> allContests = new List<ContestModel>();
    UserModel? userNow = null;
    private HashSet<string> platformNamesSet = new HashSet<string>();
    private readonly ContestContext _contest_context;
    private readonly ContestBookmarkContext _contest_bookmark_context;
    //inject db context
    public HomeController(ContestContext contest_context, ContestBookmarkContext contest_bookmark_context)
    {
        _contest_context = contest_context;
        _contest_bookmark_context = contest_bookmark_context;
    }

    public IActionResult Refresh()
    {
        userNow = GetCurrentUser();
        getAllContest();
        initPlatforms();
        initViewData();
        return RedirectToAction("Index");
    }
    void initViewData()
    {
        ViewData["contests"] = allContests;
        ViewData["contests_count"] = allContests.Count;
        ViewData["platforms"] = platformNamesSet.ToList();
        ViewData["user"] = userNow;
        ViewData["ContestBookmarkContext"] = _contest_bookmark_context;

    }

    public IActionResult Index()
    {
        userNow = GetCurrentUser();
        if (allContests.Count() == 0)
        {
            getAllContest();
        }
        initPlatforms();
        initViewData();
        return View();
    }
    private void initPlatforms()
    {
        platformNamesSet.Clear();
        for (int i = 0; i < allContests.Count; i++)
        {
            platformNamesSet.Add(allContests[i].platform!);
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
    private void getAllContest()
    {
        Console.WriteLine("called");
        HttpClient client = new HttpClient();
        HttpResponseMessage response = client.GetAsync("https://kontests.net/api/v1/all").Result;
        string json = response.Content.ReadAsStringAsync().Result;
        allContests.Clear();
        if (json != null)
        {
            dynamic stuff = JsonConvert.DeserializeObject(json)!;
            foreach (var item in stuff)
            {
                Console.WriteLine(item);
                ContestModel cm = new ContestModel();
                cm.title = item.name;
                //2021-01-29T16:35:00
                cm.start = cm.parseTime(Convert.ToString(item.start_time));
                cm.end = cm.parseTime(Convert.ToString(item.end_time));

                cm.duration = (long)Convert.ToDouble(Convert.ToString(item.duration));
                cm.link = item.url;
                cm.status = item.status;
                cm.platform = item.site;
                allContests.Add(cm);
            }
        }
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

    [HttpPost]
    public async Task<Int32> BookmarkToggle(string link)
    {
        Console.WriteLine("called " + link);
        if (link == null || link.Count() == 0)
        {
            return -1;
        }
        userNow = GetCurrentUser();
        if (userNow == null)
        {
            return -1;
        }
        else
        {
            var contestListModel = await _contest_bookmark_context.ContestListModel.FirstOrDefaultAsync(x => x.userId == userNow.Id && x.contestId == link);
            if (contestListModel == null)
            {
                contestListModel = new ContestListModel();
                contestListModel.userId = userNow.Id;
                contestListModel.contestId = link;
                _contest_bookmark_context?.ContestListModel?.Add(contestListModel);
                
                ContestModel? contestModel = allContests.FirstOrDefault(x => x.id == link);
                if(contestModel!=null && _contest_context.ContestModel.Contains(contestModel)==false){
                    _contest_context.ContestModel?.Add(contestModel!);
                    await _contest_context.SaveChangesAsync();
                }
                await _contest_bookmark_context.SaveChangesAsync();
                return 1;
            }
            else
            {
                Console.WriteLine(contestListModel.contestId, contestListModel.userId, contestListModel.Id);
                _contest_bookmark_context?.ContestListModel?.Remove(contestListModel);
                await _contest_bookmark_context?.SaveChangesAsync();
                return 0;
            }
        }
    }
}
