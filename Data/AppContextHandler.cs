using Microsoft.EntityFrameworkCore;
using InstrukcijeDotNet.Models;


namespace InstrukcijeDotNet.Data
{
    public class AppContextHandler : DbContext
    {
        public AppContextHandler(DbContextOptions<AppContextHandler> options)
                : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=dotNetInstrukcije.db");
        }

        public DbSet<Professor> Professors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<InstructionDate> InstructionDates { get; set; }
        public DbSet<ProfessorSubject> ProfessorSubjects { get; set; }
    }
}
