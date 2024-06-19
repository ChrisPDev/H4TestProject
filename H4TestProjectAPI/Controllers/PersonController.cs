using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using H4TestProjectAPI.Data;
using H4TestProjectAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace H4TestProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _danishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        private static readonly Random _random = new Random();

        public PersonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Person
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetPersons()
        {
            // Retrieve all persons from the database
            return await _context.Persons.ToListAsync();
        }

        // GET: api/Person/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> GetPerson(int id)
        {
            // Find a person by their Id
            var person = await _context.Persons.FindAsync(id);

            if (person == null)
            {
                return NotFound(); // Return 404 Not Found if person is not found
            }

            return person; // Return the found person
        }

        // GET: api/Person/PersonalId/{personalId}
        [HttpGet("PersonalId/{personalId}")]
        public async Task<ActionResult<Person>> GetPersonByPersonalId(string personalId)
        {
            // Find a person by their PersonalId
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.PersonalId == personalId);

            if (person == null)
            {
                return NotFound(); // Return 404 Not Found if person is not found
            }

            return person; // Return the found person
        }

        // POST: api/Person
        [HttpPost]
        public async Task<ActionResult<Person>> PostPerson(Person person)
        {
            // Generate PersonalId, set CreatedAt and UpdatedAt timestamps
            person.PersonalId = GeneratePersonalId(person.Gender);
            //person.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _danishTimeZone);
            //person.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _danishTimeZone);
            person.CreatedAt = DateTime.UtcNow.AddHours(2);
            person.UpdatedAt = DateTime.UtcNow.AddHours(2);

            // Add the person to the database and save changes
            _context.Persons.Add(person);
            await _context.SaveChangesAsync();

            // Return a 201 Created response with the created person
            return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, person);
        }

        // PUT: api/Person/PersonalId/{personalId}
        [HttpPut("PersonalId/{personalId}")]
        public async Task<IActionResult> PutPerson(string personalId, Person person)
        {
            if (personalId != person.PersonalId)
            {
                return BadRequest("PersonalId cannot be changed"); // Return bad request if trying to change PersonalId
            }

            // Find the existing person by PersonalId
            var existingPerson = await _context.Persons
                .FirstOrDefaultAsync(p => p.PersonalId == personalId);

            if (existingPerson == null)
            {
                return NotFound(); // Return 404 Not Found if person is not found
            }

            // Update the person's FirstName, LastName, and UpdatedAt timestamp
            existingPerson.FirstName = person.FirstName;
            existingPerson.LastName = person.LastName;
            //existingPerson.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _danishTimeZone);
            existingPerson.UpdatedAt = DateTime.UtcNow.AddHours(2);

            // Set the entity state to modified and save changes
            _context.Entry(existingPerson).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(); // Save changes to the database
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(personalId))
                {
                    return NotFound(); // Return 404 Not Found if person is not found
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Return 204 No Content on successful update
        }

        // DELETE: api/Person/PersonalId/{personalId}
        [HttpDelete("PersonalId/{personalId}")]
        public async Task<IActionResult> DeletePerson(string personalId)
        {
            // Find a person by their PersonalId
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.PersonalId == personalId);

            if (person == null)
            {
                return NotFound(); // Return 404 Not Found if person is not found
            }

            // Remove the person from the database and save changes
            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content on successful deletion
        }

        // Helper method to check if a person exists by PersonalId
        private bool PersonExists(string personalId)
        {
            return _context.Persons.Any(e => e.PersonalId == personalId);
        }

        // Generate PersonalId based on gender and current date
        private string GeneratePersonalId(string gender)
        {
            // Generating date part in ddMMyy format
            string datePart = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _danishTimeZone).ToString("ddMMyy");
            // Generating gender part
            string genderPart = gender.ToLower() == "male" ? GenerateGenderPart(true) : GenerateGenderPart(false);

            return datePart + genderPart; // Combine date and gender parts to form PersonalId
        }

        // Generate gender-specific part of PersonalId
        private string GenerateGenderPart(bool isMale)
        {
            // Generating two pairs of digits based on gender
            int firstPair = isMale ? _random.Next(0, 50) * 2 + 1 : _random.Next(0, 50) * 2; // Male: odd, Female: even
            int secondPair = isMale ? _random.Next(0, 50) * 2 + 1 : _random.Next(0, 50) * 2; // Male: odd, Female: even

            // Returning the gender part as a string
            return firstPair.ToString("D2") + secondPair.ToString("D2");
        }
    }
}
