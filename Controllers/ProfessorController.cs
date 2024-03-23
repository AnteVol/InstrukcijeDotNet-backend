using InstrukcijeDotNet.Data;
using InstrukcijeDotNet.Models;
using InstrukcijeDotNet.ViewModels;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace InstrukcijeDotNet.Controllers
{

    [ApiController]
    [Route("api")]

    public class ProfessorController : Controller
    {
        private readonly AppContextHandler context;
        private readonly IConfiguration configuration;

        public ProfessorController(AppContextHandler context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }
        private string GenerateJwtToken(Professor profesor)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, profesor.name),
                    new Claim(JwtRegisteredClaimNames.Email, profesor.surname),
                    new Claim("id", profesor.id.ToString()),
                };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("register/professor")]
        public async Task<IActionResult> Register(RegisterProfessorModel model)
        {
            if (ModelState.IsValid & model.password == model.confirmPassword)
            {
                var professor = new Professor
                {
                    name = model.name,
                    surname = model.surname,
                    email = model.email,
                    password = model.password,
                    profilePictureUrl = model.profilePictureUrl,
                    instructionsCount = 0
                };
                context.Professors.Add(professor);
                await context.SaveChangesAsync();
                if (model.subjects != null)
                {
                    var newProfessorId = await context.Professors.OrderByDescending(p => p.id).FirstOrDefaultAsync();
                    //dodaj is null provjeru
                    //pronađi profesorid i subjectiD
                    foreach (var url in model.subjects)
                    {
                        var subject = await context.Subjects.FirstOrDefaultAsync(s => s.url == url);
                        if (subject != null)
                        {
                            var professorSubject = new ProfessorSubject
                            {
                                professorId = newProfessorId.id,
                                subjectId = subject.id
                            };
                            context.ProfessorSubjects.Add(professorSubject);
                        }
                    }
                    await context.SaveChangesAsync();
                }
                var response = new { success = true, professor };
                return Ok(response);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost("login/professor")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Professor? profesor = null;
            if (!string.IsNullOrWhiteSpace(model.email))
            {
                // if (EmailCheck.IsValidEmail(model.email))
                profesor = await context.Professors.FirstOrDefaultAsync(p => p.email == model.email);
            }
            if (profesor == null)
            {
                return Unauthorized(new { success = false, message = "Authentication failed. Profesor not found." });
            }
            var passwordIsValid = model.password == profesor.password;
            if (!passwordIsValid)
            {
                return Unauthorized(new { success = false, message = "Authentication failed. Incorrect password." });
            }

            var token = GenerateJwtToken(profesor);

            var response = new
            {
                success = true,
                message = "Login successful",
                user = new { profesor.id, profesor.name, profesor.surname, profesor.email, profesor.profilePictureUrl },
                token
            };

            return Ok(response);
        }

        [HttpGet("professors")]
        public async Task<IActionResult> GetAllProfesors()
        {
            var professors = await context.Professors.Select(professor => new
            {
                professor.id,
                professor.name,
                professor.surname,
                professor.email,
                professor.profilePictureUrl,
                professor.instructionsCount

            }).ToListAsync();
            var response = new
            {
                success = true,
                professors
            };

            return Ok(response);
        }

        [HttpGet("newprofessors")]
        public async Task<IActionResult> GetNewProfesors()
        {
            var professors = await context.Professors
                .OrderByDescending(professor => professor.id)
                .Select(professor => new
                {
                    professor.id,
                    professor.name,
                    professor.surname,
                    professor.email,
                    professor.profilePictureUrl,
                    professor.instructionsCount
                })
                .Take(4)
                .ToListAsync();
            var response = new
            {
                success = true,
                professors
            };

            return Ok(response);
        }

        [HttpPost("professor/addSubject")]
        public async Task<IActionResult> AddSubjectForProfessor(Help model)
        {
            var professorSubject = new ProfessorSubject
                    { 
                        professorId = model.professorId,
                        subjectId = model.subjectId
                    };
            context.ProfessorSubjects.Add(professorSubject);
            await context.SaveChangesAsync();
            var response = new { success = true, professorSubject = professorSubject };
            return Ok(response);
        }

        [HttpGet("professor/{email}")]
        public async Task<IActionResult> GetProfesorByEmail(string email)
        {
            var professor =  context.Professors.FirstOrDefault(p => p.email == email);

            if (professor == null) return NotFound(new { success = false, message = "User not found." });

            var subjectsForProfessor = await context.ProfessorSubjects.Where(ps => ps.professorId == professor.id).
                Select(professorSubject => new {
                    professorSubject.professorId,
                    professorSubject.subjectId
                }).ToListAsync();

            List<string> subjects = new List<string>();

            if (subjectsForProfessor != null)
            {
                foreach (var subjectId in subjectsForProfessor)
                {
                    var subject = await context.Subjects.FirstOrDefaultAsync(s => s.id == subjectId.subjectId);
                    subjects.Add(subject.title);
                }
            }
                var response = new
                {
                success = true,
                professor = new { professor.id, professor.name, professor.surname, professor.email, professor.profilePictureUrl }
                //,subjects = JsonConvert.SerializeObject(data)
                };

            return Ok(response);
        }

        [HttpGet("professors/instructions")]
        public async Task<IActionResult> GetProfessorsAndCountInstructions()
        {
            var professorsInstructions = await context.InstructionDates
                .GroupBy(i => i.professorId)
                .Select(g => new ProfessorsInstructions
                {
                    id = g.Key,
                    countInstructions = g.Count()
                })
                .OrderByDescending(pi => pi.countInstructions)
                .Take(5)
                .ToListAsync();

            var lista = new List<object>();

            foreach (var professorInstruction in professorsInstructions)
            {
                var professor = await context.Professors.FirstOrDefaultAsync(p => p.id == professorInstruction.id);

                if (professor != null)
                {
                    var toDisplay = new { professor.name, professor.surname, professorInstruction.countInstructions };
                    lista.Add(toDisplay);
                }
            }

            var response = new
            {
                success = true,
                professorsInstructions = lista
            };

            return Ok(response);
        }


        [HttpGet("professors/myinstructions/{id}")]
        public async Task<ActionResult<List<InstructionDate>>> GetInstructions(int id)
        {
            try
            {
                var currentTimeUtc = DateTime.UtcNow;

                var upcomingInstructions = await context.InstructionDates
                    .Where(i => i.professorId == id && i.dateTime > currentTimeUtc && i.status == "odobrene")
                    .ToListAsync();

                var pending = await context.InstructionDates
                    .Where(i => i.professorId == id && i.dateTime > currentTimeUtc && i.status == "u čekanju")
                    .ToListAsync();
                var passed = await context.InstructionDates
                    .Where(i => i.professorId == id && i.dateTime < currentTimeUtc)
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
                        var toDisplay = new InstructionDisplay { name = student.name, professorId = professor.id, time = instruction.dateTime, surname = student.surname, profilePictureUrl = student.profilePictureUrl };
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
                        var toDisplay = new InstructionDisplay { name = student.name, professorId = professor.id, time = instruction.dateTime, surname = student.surname, profilePictureUrl = student.profilePictureUrl };
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
                        var toDisplay = new InstructionDisplay { name = student.name, professorId = professor.id, time = instruction.dateTime, surname = student.surname, profilePictureUrl = student.profilePictureUrl };
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


        [HttpPut("professor/edit/{id}")]
        public async Task<IActionResult> UpdateProfessor(int id, EditModel model)
        {
            var professor = await context.Professors.FindAsync(id);

            if (professor == null)
            {
                return NotFound(new { success = false, message = "Profesor nije pronađen." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            professor.name = model.name;
            professor.surname = model.surname;
            professor.email = model.email;
            professor.password = model.password;
            professor.profilePictureUrl = model.profilePictureUrl;

            try
            {
                context.Professors.Update(professor);
                await context.SaveChangesAsync();
                return Ok(new { success = true, message = "Informacije o profesoru su uspješno ažurirane." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Došlo je do pogreške prilikom ažuriranja informacija o profesoru." });
            }
        }

        [HttpGet("professorsforsubject/{url}")]
        public async Task<IActionResult> GetProfessorsForSubject(string url)
        {
            var subject = await context.Subjects.FirstOrDefaultAsync(s => s.url == url);
            var id = subject.id;

            var professorsWithSubject = await context.ProfessorSubjects
                .Where(ps => ps.subjectId == id)
                .Select(ps => ps.professorId)
                .Distinct()
                .ToListAsync();

            var professors1 = await context.Professors
                .Where(p => professorsWithSubject.Contains(p.id))
                .ToListAsync();


            var response = new
            {
                success = true,
                professors = professors1
            };

            return Ok(response);
        }

        [HttpGet("professorsforsubjects")]
        public async Task<IActionResult> GetProfessorSubjects()
        {
            var professorsubject = await context.ProfessorSubjects
                .Select(professor => new
                {
                    professor.id,
                    professor.professorId,
                    professor.subjectId,
                })
                .ToListAsync();

            var response = new
            {
                success = true,
                professorsubject = professorsubject
            };

            return Ok(response);
        }

        [HttpDelete("professor/delete/{professorId}")]
        public async Task<IActionResult> DeleteProfessor(int professorId)
        {
            var professor = await context.Professors.FindAsync(professorId);
            if (professor == null)
            {
                return NotFound(new { success = false, message = "Professor not found." });
            }

            try
            {
                context.Professors.Remove(professor);
                await context.SaveChangesAsync();
                return Ok(new { success = true, message = "Professor deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the professor." });
            }
        }

        [HttpDelete("professor/delete/all")]
        public async Task<IActionResult> DeleteAllProfessors()
        {
            try
            {
                var professors = await context.Professors.ToListAsync();
                if (professors == null || professors.Count == 0)
                {
                    return NotFound(new { success = false, message = "No professors found." });
                }

                context.Professors.RemoveRange(professors);
                await context.SaveChangesAsync();
                return Ok(new { success = true, message = "All professors deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while deleting professors.", error = ex.Message });
            }
        }


    }
}
