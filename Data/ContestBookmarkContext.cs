using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoderCalender.Models;

    public class ContestBookmarkContext : DbContext
    {
        public ContestBookmarkContext (DbContextOptions<ContestBookmarkContext> options)
            : base(options)
        {
        }

        public DbSet<CoderCalender.Models.ContestListModel>? ContestListModel { get; set; }
    }
