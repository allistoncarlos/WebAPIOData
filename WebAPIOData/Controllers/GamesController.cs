using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIOData.Controllers
{
    [EnableQuery(PageSize = 20)]
    [Route("api/[controller]")]
    public class GamesController : Controller
    {
        private static readonly List<Platform> Platforms = new List<Platform>
        {
            new Platform { Id = 1, Name = "PS4" },
            new Platform { Id = 2, Name = "Switch" },
            new Platform { Id = 3, Name = "PSVita" },
            new Platform { Id = 4, Name = "PC" }
        };
        
        private static readonly List<Game> Games = new List<Game>
        {
            new Game { Id = 1, Name = "The Legend of Zelda: Breath of the Wild", Genre = Genre.ActionAdventure, Platform = GetPlatform(2), Year = 2017 },
            new Game { Id = 2, Name = "Persona 4 Golden", Genre = Genre.RPG, Platform = GetPlatform(3), Year = 2013 },
            new Game { Id = 3, Name = "Destiny", Genre = Genre.FPS, Platform = GetPlatform(1), Year = 2014 },
            new Game { Id = 4, Name = "Need for Speed: Most Wanted", Genre = Genre.Sport, Platform = GetPlatform(4), Year = 2005 }
        };

        // GET odata/Games
        [HttpGet]
        public IQueryable<Game> Get()
        {
            return Games.AsQueryable();
        }

        // GET odata/Games(5)
        [HttpGet]
        public Game Get(int key)
        {
            return Games.SingleOrDefault(x => x.Id == key);
        }

        // POST odata/Games
        [HttpPost]
        public IActionResult Post([FromBody]Game value)
        {
            value.Id = Games.Max(g => g.Id) + 1;
            value.Platform = Platforms.SingleOrDefault(p => p.Id == value.Platform.Id);

            Games.Add(value);

            return Created($"?key={value.Id}", value);
        }

        // PUT odata/Games(5)
        [HttpPut]
        public IActionResult Put(int key, [FromBody]Game value)
        {
            var existingGame = Games.SingleOrDefault(x => x.Id == key);
            Games.Remove(existingGame);

            value.Id = key;
            value.Platform = Platforms.SingleOrDefault(p => p.Id == value.Platform.Id);
            Games.Add(value);

            return Ok(existingGame);
        }

        [HttpPatch]
        public IActionResult Patch(int key, [FromBody]JsonPatchDocument<Game> valuePatch)
        {
            var existingGame = Games.SingleOrDefault(x => x.Id == key);
            Games.Remove(existingGame);

            valuePatch.ApplyTo(existingGame);
            Games.Add(existingGame);

            return Ok(existingGame);
        }

        // DELETE odata/Games(5)
        [HttpDelete]
        public IActionResult Delete(int key)
        {
            var existingGame = Games.SingleOrDefault(x => x.Id == key);
            Games.Remove(existingGame);

            return Ok(key);
        }

        private static Platform GetPlatform(int id)
        {
            return Platforms.SingleOrDefault(x => x.Id == id);
        }
    }
}

