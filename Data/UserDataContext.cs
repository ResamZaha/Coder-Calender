using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoderCalender.Models;

    public class UserDataContext : DbContext
    {
        public UserDataContext (DbContextOptions<UserDataContext> options)
            : base(options)
        {
        }

        public DbSet<CoderCalender.Models.UserModel>? UserModel { get; set; }
    }
