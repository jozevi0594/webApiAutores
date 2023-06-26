using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Filters;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Controllers.V2
{
    [ApiController]
    [Route("api/[controller]")]
    //[Route("api/v2/[controller]")]
    [CabeceraEstaPresenteAttribute("x-version", "2")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,Policy ="EsAdmin")]
    //[Authorize] se puede proteger a nivel de controlador
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAuthorizationService authorizationService;

        public AutoresController(ApplicationDbContext context,
            IMapper mapper, IAuthorizationService authorizationService
            ) { 
            this.context = context;
            this.mapper = mapper;
            this.authorizationService = authorizationService;
        }


        //[HttpGet("configuraciones")]
        //public ActionResult<string> ObtenerConfiguracion()
        //{
        //    return configuration["apellido"];
        //}

        [HttpGet(Name ="obtenerAutoresV2")]
        [AllowAnonymous]//usuarios no autenticados pueden consumir
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]//Filtro creado a medida para la visualizacion de enlaces
        public async Task<ActionResult<List<AutorDTO>>> Get()
        {
            var autores = await context.Autores.ToListAsync();
            autores.ForEach(autor => autor.Nombre = autor.Nombre.ToUpper());
            return mapper.Map<List<AutorDTO>>(autores);


        }


        [HttpGet("{id:int}",Name ="obtenerAutorV2")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]//Filtro creado a medida para la visualizacion de enlaces
        public async Task<ActionResult<AutorDTOconLibros>> Get(int id)
        {
            var autor = await context.Autores
                .Include(autorDB=>autorDB.AutoresLibros)
                .ThenInclude(autorLibroDB=>autorLibroDB.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (autor == null)
            {
                return NotFound();
            }

            var dto= mapper.Map<AutorDTOconLibros>(autor);

            return dto;
        }



        [HttpGet("{nombre}",Name ="obtenerAutorPorNombreV2")]
        public async Task<ActionResult<List<AutorDTO>>> GetPorNombre([FromRoute] string nombre)
        {
            var autores = await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();
            if (autores == null)
            {
                return NotFound();
            }
            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost(Name ="crearAutorV2")]
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorCreacionDTO)
        {
            var existAutor = await context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTO.Nombre);//validaciones a nivel de controlador
            if (existAutor)
            {
                return BadRequest($"Existe un autor con el nombre {autorCreacionDTO.Nombre}");
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            
            context.Add(autor);
            await context.SaveChangesAsync();

            var autorDTO = mapper.Map<AutorDTO>(autor);

            return CreatedAtRoute("obtenerAutorV2",new { id=autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}",Name ="actualizarAutorV2")]
        public async Task<IActionResult> Put(AutorCreacionDTO autorCreacionDTO,int id)
        {
            
            var exist = await context.Autores.AnyAsync(x => x.Id == id);
            if (!exist)
            {
                return NotFound();
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;  

            context.Update(autor);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}",Name ="borrarAutorV2")]
        public async Task<ActionResult> Delete(int id)
        {
            var exist = await context.Autores.AnyAsync(x=>x.Id == id);
            if (!exist)
            {
                return NotFound();
            }
            context.Remove(new Autor() { Id = id});
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
