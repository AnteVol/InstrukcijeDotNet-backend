using InstrukcijeDotNet.Data;
using InstrukcijeDotNet.Models;
using InstrukcijeDotNet.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace InstrukcijeDotNet.Controllers
{
    [ApiController]
    [Route("api")]
    public class SubjectsController : Controller
    {
        private readonly AppContextHandler context;
        private readonly IConfiguration configuration;

        public SubjectsController(AppContextHandler context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        [HttpPost("subject/addSubject")]
        public async Task<IActionResult> Register(CreateSubjectModel model)
        {
            if (ModelState.IsValid)
            {
                var subject = new Subject
                {
                    title = model.title,
                    url = model.url,
                    description = model.description
                };

                context.Subjects.Add(subject);
                await context.SaveChangesAsync();

                var response = new { success = true, subject };
                return Ok(response);
            }

            return BadRequest(ModelState);
        }

        [HttpGet("subjects")]
        public async Task<IActionResult> GetAllSubjects()
        {
            var subjects = await context.Subjects
                .OrderByDescending(subject => subject.id)
                .Select(subject => new
                {
                    subject.id,
                    subject.title,
                    subject.url,
                    subject.description
                })
                .ToListAsync();

            var response = new
            {
                success = true,
                subjects = subjects
            };

            return Ok(response);

        }

        [HttpGet("subject/{url}")]
        public async Task<IActionResult> GetSubjectByURL(string url)
        {
            var subject = await context.Subjects
                                        .FirstOrDefaultAsync(s => s.url == url);

            if (subject == null)
                return NotFound(new { success = false, message = "Subject not found." });

            var professorsWithSubject = await context.ProfessorSubjects
               .Where(ps => ps.subjectId == subject.id)
               .Select(ps => ps.professorId)
               .Distinct()
               .ToListAsync();

            var professors1 = await context.Professors
                .Where(p => professorsWithSubject.Contains(p.id)).Select(professor => new
                {
                    professor.id,
                    professor.name,
                    professor.surname,
                    professor.email,
                    professor.profilePictureUrl
                })
                .ToListAsync();

            var response = new
            {
                success = true,
                subject = new { subject.id, subject.title, subject.url, subject.description },
                professors = professors1
            };

            return Ok(response);
        }

        [HttpGet("instructions")]
        public async Task<IActionResult> GetAllInstructions()
        {
            var instructions = await context.InstructionDates
                                            .Select(instruction => new
                                            {
                                                instruction.id,
                                                instruction.studentId,
                                                instruction.professorId,
                                                instruction.dateTime,
                                                instruction.status
                                            })
                                            .ToListAsync();

            var response = new
            {
                success = true,
                instructions
            };

            return Ok(response);
        }

        

        [HttpPost("subject/instruction")]
        public async Task<IActionResult> ScheduleInstructionSession(ScheduleInstructionSessionModel model)
        {
            if (ModelState.IsValid)
            {
                var status = DateTime.UtcNow > model.dateTime ? "prošle" : "u čekanju";
                var instruction = new InstructionDate
                {
                    studentId = model.studentId,
                    professorId = model.professorId,
                    dateTime = model.dateTime,
                    status = status
                };

                context.InstructionDates.Add(instruction);
                await context.SaveChangesAsync();

                var response = new { success = true, instruction };
                return Ok(response);
            }

            return BadRequest(ModelState);
        }

        [HttpPut("subject/instruction/edit/{id}")]
        public async Task<IActionResult> EditeInstructionSession(int id, InstructionDate model)
        {

            var instructionDate = await context.InstructionDates.FindAsync(id);

            if (instructionDate == null)
            {
                return NotFound(new { success = false, message = "Student nije pronađen." });
            }

            
            instructionDate.dateTime = model.dateTime;
            var status = DateTime.UtcNow > model.dateTime ? "prošle" : "u čekanju";

            try
            {
                context.InstructionDates.Update(instructionDate);
                await context.SaveChangesAsync();
                return Ok(new { success = true, message = "Informacije o studentu su uspješno ažurirane." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Došlo je do pogreške prilikom ažuriranja informacija o studentu." });
            }
            
        }



        [HttpGet("subjectsandprofessors")]
        public async Task<IActionResult> GetProfessorsAndSubjects()
        {
            var subjectsAndProfessors = await context.ProfessorSubjects.Select(subject => new
            {
                subject.id,
                subject.professorId,
                subject.subjectId
            }).ToListAsync();
            var response = new
            {
                success = true,
                subjectsAndProfessors = subjectsAndProfessors
            };

            return Ok(response);
        }

        [HttpDelete("subject/delete/all")]
        public async Task<IActionResult> DeleteAllSubjects()
        {
            try
            {
                var subjects = await context.Subjects.ToListAsync();
                if (subjects == null || subjects.Count == 0)
                {
                    return NotFound(new { success = false, message = "No subjects found." });
                }

                context.Subjects.RemoveRange(subjects);
                await context.SaveChangesAsync();
                return Ok(new { success = true, message = "All subjects deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while deleting subjects.", error = ex.Message });
            }
        }

        [HttpDelete("subject/professors/delete/all")]
        public async Task<IActionResult> DeleteAllSubjectsProfessors()
        {
            try
            {
                var subjectprofessors = await context.ProfessorSubjects.ToListAsync();
                if (subjectprofessors == null || subjectprofessors.Count == 0)
                {
                    return NotFound(new { success = false, message = "No subject professors found." });
                }

                context.ProfessorSubjects.RemoveRange(subjectprofessors);
                await context.SaveChangesAsync();
                return Ok(new { success = true, message = "All subject professors deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while deleting subject professors.", error = ex.Message });
            }
        }

        [HttpDelete("instruction/delete/all")]
        public async Task<IActionResult> DeleteAllInstructionDates()
        {
            try
            {
                var instuctionDate = await context.InstructionDates.ToListAsync();
                if (instuctionDate == null || instuctionDate.Count == 0)
                {
                    return NotFound(new { success = false, message = "No subject instuction dates found." });
                }

                context.InstructionDates.RemoveRange(instuctionDate);
                await context.SaveChangesAsync();
                return Ok(new { success = true, message = "All instuction dates deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while deleting instuction dates.", error = ex.Message });
            }
        }
    }
}
