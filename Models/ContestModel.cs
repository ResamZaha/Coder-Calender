using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
namespace CoderCalender.Models;

public class ContestModel
{
    public string id { get; set; }
    public string title { get; set; }

    [DataType(DataType.Date)]
    public DateTime start { get; set; }
    [DataType(DataType.Date)]
    public DateTime end { get; set; }
    public long duration { get; set; }

    SHA256 hash = SHA256.Create();
    private string LINK {get;set;}

    public string link
    {
        get
        {
            return this.LINK;
        }
        set
        {
            this.LINK = value;
            var byteArray = hash.ComputeHash(Encoding.UTF8.GetBytes(this.link));
            this.id = Convert.ToHexString(byteArray).ToLower();
        }
    }
    public string status { get; set; }
    public string platform { get; set; }

    public DateTime parseTime(string timestr)
    {
        //2022-07-17 14:30:00 UTC
        if (timestr == "-")
            return DateTime.Now;
        else
        {
            try
            {
                return DateTime.ParseExact(timestr, "yyyy-MM-dd HH:mm:ss UTC", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return DateTime.Parse(timestr);
            }
        }
    }
    public ContestModel()
    {
        id = "";
        title = "";
        start = DateTime.Now;
        end = DateTime.Now;
        duration = 0;
        link = "";
        status = "";
        platform = "";
    }
    public bool isBookmarked(UserModel? userNow, ContestBookmarkContext? bookmarkContext)
    {
        if (userNow == null || bookmarkContext == null)
        {
            return false;
        }
        var contestList = bookmarkContext.ContestListModel?.FirstOrDefault(x => x.userId == userNow.Id && x.contestId == this.id);
        Console.WriteLine("called " + id + " " + userNow.Id + " " + contestList);

        if (contestList == null)
        {
            return false;
        }
        return true;
    }
}