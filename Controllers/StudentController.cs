using InstrukcijeDotNet.Data;
using InstrukcijeDotNet.Models;
using InstrukcijeDotNet.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InstrukcijeDotNet.Controllers
{
    [ApiController]
    [Route("api")]
    public class StudentController : Controller
    {
        private readonly AppContextHandler context;
        private readonly IConfiguration configuration;

        public StudentController(AppContextHandler context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        private string GenerateJwtToken(Student student)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, student.name),
                    new Claim(JwtRegisteredClaimNames.Email, student.surname),
                    new Claim("id", student.id.ToString()),
                };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("register/student")]
        public async Task<IActionResult> Register(StudentRegisterModel model)
        {
            if (ModelState.IsValid & model.password == model.confirmPassword)
            {
                var student = new Student
                {
                    name = model.name,
                    surname = model.surname,
                    email = model.email,
                    password = model.password,
                    profilePictureUrl = model.profilePictureUrl
                };
                context.Students.Add(student);
                await context.SaveChangesAsync();
                var response = new { success = true, student };
                return Ok(response);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        [HttpPost("login/student")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Student? student = null;
            if (!string.IsNullOrWhiteSpace(model.email))
            {
                // if (EmailCheck.IsValidEmail(model.email))
                student = await context.Students.FirstOrDefaultAsync(p => p.email == model.email);
            }
            if (student == null)
            {
                return Unauthorized(new { success = false, message = "Authentication failed. Student not found." });
            }
            var passwordIsValid = model.password == student.password;
            if (!passwordIsValid)
            {
                return Unauthorized(new { success = false, message = "Authentication failed. Incorrect password." });
            }

            var token = GenerateJwtToken(student);

            var response = new
            {
                success = true,
                message = "Login successful",
                user = new { student.id, student.name, student.surname, student.email, student.profilePictureUrl },
                token
            };

            return Ok(response);
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await context.Students.Select(s => new
            {
                s.id,
                s.name,
                s.surname,
                s.email,
                s.profilePictureUrl

            }).ToListAsync();
            var response = new
            {
                success = true,
                students
            };

            return Ok(response);
        }

        [HttpGet("student/{email}")]
        public async Task<IActionResult> GetStudentByEmail(string email)
        {
            var student = context.Students.FirstOrDefault(s => s.email == email);

            if (student == null) return NotFound(new { success = false, message = "User not found." });

            var response = new
            {
                success = true,
                student = new { student.id, student.name, student.surname, student.email, student.profilePictureUrl }
            };

            return Ok(response);
        }

        [HttpGet("students/myinstructions/{id}")]
        public async Task<ActionResult<List<InstructionDate>>> GetInstructions(int id)
        {
            try
            {
                var currentTimeUtc = DateTime.UtcNow;

                var upcomingInstructions = await context.InstructionDates
                    .Where(i => i.studentId == id && i.dateTime > currentTimeUtc && i.status == "odobrene")
                    .ToListAsync();

                var pending = await context.InstructionDates
                    .Where(i => i.studentId == id && i.dateTime > currentTimeUtc && i.status == "u čekanju")
                    .ToListAsync();
                var passed = await context.InstructionDates
                    .Where(i => i.studentId == id && i.dateTime < currentTimeUtc)
                    .ToListAsync();

                List<InstructionDisplay> passedList = new List<InstructionDisplay>();
                List<InstructionDisplay> upcomingList = new List<InstructionDisplay>();
                List<InstructionDisplay> pendingList = new List<InstructionDisplay>();

                foreach (var instruction in upcomingInstructions)
                {
                    var professor = await context.Professors.FirstOrDefaultAsync(p => p.id == instruction.professorId);
                    var student = await context.Students.FirstOrDefaultAsync(p => p.id == instruction.studentId);

                    //var subjectTitle = await context.InstructionDates.FirstOrDefaultAsync(p => p.id == instruction.id);
                    if (professor != null)
                    {
                        var toDisplay = new InstructionDisplay { name = professor.name, professorId = professor.id, time = instruction.dateTime, surname = professor.surname, profilePictureUrl = professor.profilePictureUrl };
                        upcomingList.Add(toDisplay);
                    }
                }

                foreach (var instruction in passed)
                {
                    var professor = await context.Professors.FirstOrDefaultAsync(p => p.id == instruction.professorId);
                    var student = await context.Students.FirstOrDefaultAsync(p => p.id == instruction.studentId);
                    //var subjectTitle = await context.InstructionDates.FirstOrDefaultAsync(p => p.id == instruction.id);
                    if (professor != null)
                    {
                        var toDisplay = new InstructionDisplay { name = professor.name, professorId = professor.id, time = instruction.dateTime, surname = professor.surname, profilePictureUrl = professor.profilePictureUrl };
                        passedList.Add(toDisplay);
                    }
                }

                foreach (var instruction in pending)
                {
                    var professor = await context.Professors.FirstOrDefaultAsync(p => p.id == instruction.professorId);
                    var student = await context.Students.FirstOrDefaultAsync(p => p.id == instruction.studentId);
                    //var subjectTitle = await context.InstructionDates.FirstOrDefaultAsync(p => p.id == instruction.id);
                    if (professor != null)
                    {
                        var toDisplay = new InstructionDisplay { name = professor.name, professorId = professor.id, time = instruction.dateTime, surname = professor.surname, profilePictureUrl = professor.profilePictureUrl };
                        pendingList.Add(toDisplay);
                    }
                }
                var response = new
                {
                    success = true,
                    upcoming = upcomingList,
                    pending = pendingList,
                    passed = passedList
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving upcoming instructions.");
            }
        }

        [HttpPut("student/edit/{id}")]
        public async Task<IActionResult> UpdateStudent(int id, EditModel model)
        {
            var student = await context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student nije pronađen." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            student.name = model.name;
            student.surname = model.surname;
            student.email = model.email;
            student.password = model.password;
            student.profilePictureUrl = model.profilePictureUrl;

            try
            {
                context.Students.Update(student);
                await context.SaveChangesAsync();
                return Ok(new { success = true, message = "Informacije o studentu su uspješno ažurirane." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Došlo je do pogreške prilikom ažuriranja informacija o studentu." });
            }
        }

        [HttpDelete("student/delete/all")]
        public async Task<IActionResult> DeleteAllStudents()
        {
            try
            {
                var students = await context.Students.ToListAsync();
                if (students == null || students.Count == 0)
                {
                    return NotFound(new { success = false, message = "No students found." });
                }

                context.Students.RemoveRange(students);
                await context.SaveChangesAsync();
                return Ok(new { success = true, message = "All students deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while deleting students.", error = ex.Message });
            }
        }
    }
}
