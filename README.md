# WebAPIOData

Projeto de estudo para utilização de OData com ASP.Net Core Web API, que utiliza um controller de Games para exemplo. Possui uma [coleção do Postman](https://github.com/allistoncarlos/WebAPIOData/blob/master/OData%20Sample.postman_collection.json) que exemplifica o teste de chamadas OData. Utiliza o pacote NuGet [Microsoft.AspNetCore.OData (versão 7.0.0-beta2)](https://www.nuget.org/packages/Microsoft.AspNetCore.OData/) para implementação do OData.

# Explicação do projeto

[Startup.cs](https://github.com/allistoncarlos/WebAPIOData/blob/master/WebAPIOData/Startup.cs)

```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddOData();
    services.AddSingleton(sp => new ODataUriResolver { EnableCaseInsensitive = true });

    services.AddMvc().AddJsonOptions(opt =>
    {
        if (opt.SerializerSettings.ContractResolver != null)
        {
            var resolver = opt.SerializerSettings.ContractResolver as DefaultContractResolver;
            resolver.NamingStrategy = null;
        }
    });
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseMvc(ConfigureODataRoutes);
}

private static void ConfigureODataRoutes(IRouteBuilder routes)
{
    var model = GetEdmModel();
    routes.MapODataServiceRoute("ODataRoute", "odata", model);
    routes.Filter(QueryOptionSetting.Allowed);
    routes.OrderBy();
    routes.Count();
    routes.Select();
}

private static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    var games = builder.EntitySet<Game>("Games").EntityType;
    games.ComplexProperty(y => y.Platform);
    
    return builder.GetEdmModel();
}
```

No arquivo Startup.cs, registramos o OData logo no começo, seguido do Singleton para ODataUriResolver, definindo que teremos um tratamento Case Insensitive para URLs.

AddJsonOptions é necessário para que o JsonSerializer não altere o nome das propriedades de retorno e o mantenha em Upper Case.

No método Configure, é chamada a Action ConfigureODataRoutes, onde definimos a rota ODataRoute, com prefixo "odata" (o prefixo que será chamado na URL). Essa rota é baseada em um Model pré definido, sendo criado no método GetEdmModel. Tal método informa quais conjuntos de entidade (EntitySet) serão disponibilizados. Caso haja uma propriedade complexa (um subtipo agregado, como Platform, por exemplo), é necessário definir que ela também será retornada.

As outras definições (Filter, OrderBy, Count e Select) são informadas para que as respectivas operações sejam suportadas.

[GamesController.cs](https://github.com/allistoncarlos/WebAPIOData/blob/master/WebAPIOData/Controllers/GamesController.cs)
```C#
[EnableQuery(PageSize = 20)]
[Route("api/[controller]")]
public class GamesController : Controller
{
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
```

No arquivo GamesController.cs temos as actions que serão disponibilizadas pela API. Vale notar o atributo [EnableQuery], definindo o PageSize = 20. Isso permite que a paginação já venha habilitada por padrão.

Os métodos que utilizam Id (GET/Id, PUT e PATCH) possuem uma peculiaridade. É necessário substituir o nome de parâmetro 'id' (padrão do WebAPI) por 'key', caso contrário o OData não reconhece a Action.

# Exemplos de Chamada GET
###### Retornar todos os Games
http://localhost:53760/odata/Games

###### Retornar somente a propriedade Name de todos os Games
http://localhost:53760/odata/Games?$select=Name

###### Retornar a quantidade de Games
http://localhost:53760/odata/Games/$count

###### Retornar todos os Games com nome igual à 'Persona 4 Golden'
http://localhost:53760/odata/Games?$filter=Name eq 'Persona 4 Golden'

###### Retornar todos os Games que o nome contenha a palavra 'Need'
http://localhost:53760/odata/Games?$filter=contains(Name, 'Need')

###### Retornar todos os Games do gênero FPS (utilizando Enum)
http://localhost:53760/odata/Games?$filter=Genre eq WebAPIOData.Genre'FPS'

###### Retornar apenas o nome de todos os Games em que o nome contenha a letra 'a', ordenando de maneira crescente pelo nome da plataforma
http://localhost:53760/odata/Games?$filter=contains(Name, 'a')&$select=Name&$orderby=Platform/Name asc

###### Retornar um jogo pelo ID
http://localhost:53760/odata/Games(2)

Para demais operações (POST, PUT, PATCH e DELETE), vide coleção do Postman