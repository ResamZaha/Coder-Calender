using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoderCalender.Models;

    public class ContestContext : DbContext
    {
        public ContestContext (DbContextOptions<ContestContext> options)
            : base(options)
        {
        }

        public DbSet<CoderCalender.Models.ContestModel>? ContestModel { get; set; }
    }
