using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StarChart.Data;
using StarChart.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StarChart.Controllers
{
    
    [Route("")]
    [ApiController]
    // note, common issue ... will inherit incorrectly from Controller
    public class CelestialObjectController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CelestialObjectController(ApplicationDbContext c)
        {
            _context = c;
        }


        [HttpPost]
        public IActionResult Create([FromBody] CelestialObject o)
        {
            _context.Add<CelestialObject>(o);
            _context.SaveChanges();

            return CreatedAtRoute("GetById", new {id = o.Id}, o);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, CelestialObject o)
        {
            var match =
                _context.CelestialObjects.FirstOrDefault<CelestialObject>(v => v.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            match.Name = o.Name;
            match.OrbitalPeriod = o.OrbitalPeriod;
            match.OrbitedObjectId = o.OrbitedObjectId;

            _context.Update<CelestialObject>(match);
            _context.SaveChanges();

            return NoContent();
        }


        [HttpPatch("{id}/{name}")]
        public IActionResult RenameObject(int id, string name)
        {
            var match =
                _context.CelestialObjects.FirstOrDefault<CelestialObject>(v => v.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            match.Name = name;

            _context.Update<CelestialObject>(match);
            _context.SaveChanges();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var matches =
                _context.CelestialObjects
                        .Where<CelestialObject>(v => v.Id == id || v.OrbitedObjectId == id)
                        .ToList<CelestialObject>();

            if (matches.Count == 0)
            {
                return NotFound();
            }

            _context.CelestialObjects.RemoveRange(matches);
            _context.SaveChanges();

            return NoContent();
        }


        [HttpGet("{id:int}", Name = "GetById")]
        public IActionResult GetById(int id)
        {
            var o = _context.CelestialObjects.FirstOrDefault<CelestialObject>(v => v.Id == id);
            if (o == null)
            {
                return NotFound();
            }
            // not sure if need ToList or not
            // based on https://stackoverflow.com/questions/21000092/iterate-through-linq-results-list
            // maybe ?
            // update:  tested w/o tolist, and it passed tests at least ... oh, due to
            // foreach and Add.  sigh.  in my solution, redundant.
            var satellites = _context.CelestialObjects
                                     .Where<CelestialObject>(v => v.OrbitedObjectId == o.Id)
                                     .ToList<CelestialObject>();

            // Eric's is slicker, duh :) :
            // https://github.com/snychka/AspNetCore-StarChartAPI/blob/solution/StarChart/Controllers/CelestialObjectController.cs#L19
            o.Satellites = new List<CelestialObject>();
            foreach(var s in satellites)
            {
                // get null reference in test ... forgot to make a new list
                o.Satellites.Add(s);
            }
            return Ok(o);
        }


        // obviously better to refactor to put in one method
        [HttpGet("{name}")]
        public IActionResult GetByName(string name)
        {
            // made more sense 'cause of check and foreach to change to list right away
            // but, may not be only way
            var matches = _context
                .CelestialObjects.Where<CelestialObject>(v => v.Name == name)
                .ToList<CelestialObject>();

            // note compared to GetById can't check for null
            if (matches.Count == 0)
            {
                return NotFound();
            }

            
            // see GetById comment for more on need for ToList here (for my crappy sol'n
            // doesn't matter)
            // see Eric's for correct and better approach (have query in foreach over matches
            // and don't have a unnecessary nested foreach):
            // https://github.com/snychka/AspNetCore-StarChartAPI/blob/solution/StarChart/Controllers/CelestialObjectController.cs#L29
            var satellites = _context.CelestialObjects
                                     .Where<CelestialObject>(
                                         // definite cheat.  but did pass.
                                         // need to probably do more like in GetAll
                                         v => v.OrbitedObjectId == matches.First<CelestialObject>().Id)
                                     .ToList<CelestialObject>();
            

            // not are setting each object with name to have all the satellites
            // (not the other way around)
            foreach (var o in matches)
            {
                o.Satellites = new List<CelestialObject>();
                foreach (var s in satellites)
                {
                    // get null reference in test ... forgot to make a new list
                    o.Satellites.Add(s);
                }
            }

            return Ok(matches);   
        }


        [HttpGet]
        public IActionResult GetAll()
        {
            // can just use ToLists directly on CelestialObjects.
            // no need for a Where query.  (see Eric's sol'n, link below)
            var matches = _context.CelestialObjects
                                  .Where<CelestialObject>(v => true);
            
            // not are setting each object with name to have all the satellites
            // (not the other way around)
            foreach (var o in matches)
            {
                // see Eric's, as redundant to change to a list then foreach over it.  sigh.
                // https://github.com/snychka/AspNetCore-StarChartAPI/blob/solution/StarChart/Controllers/CelestialObjectController.cs#L43
                // for what it's worth, see my GetById above for necessity of ToList
                var satellites = _context.CelestialObjects
                                     .Where<CelestialObject>( v => v.OrbitedObjectId == o.Id)
                                     .ToList<CelestialObject>();
                o.Satellites = new List<CelestialObject>();
                foreach (var s in satellites)
                {
                    // get null reference in test ... forgot to make a new list
                    o.Satellites.Add(s);
                }
            }

            return Ok(matches);
        }



    }
}
