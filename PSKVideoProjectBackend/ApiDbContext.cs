using Microsoft.EntityFrameworkCore;
using PSKVideoProjectBackend.Models;
using System.Collections.Generic;
using System.Net;

namespace PSKVideoProjectBackend
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<UploadedVideo> UploadedVideos => Set<UploadedVideo>();
        public DbSet<VideoComment> Comments => Set<VideoComment>();
        public DbSet<User> Users => Set<User>();
    }
}

