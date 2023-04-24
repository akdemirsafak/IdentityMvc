using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Repository.Models;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
//Burada string seçtiğimizde kullanıcılar için guid değerler oluşturulur.
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}