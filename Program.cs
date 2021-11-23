using System.Text;
using Newtonsoft.Json;
using System.Collections;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


app.MapGet("/", () => "Wellcome to the service geolocation Cadena");

// End point recibe json de Sodim
app.MapPost("/items", async (List<Item> req) =>
{ 
    var items =  req;
    //Homologa Ciudades Codigos Dian
    var homologados  = from i in items
                       select new 
                        {
                          id = i.identificador,
                          country= "co",
                          city = i.ciudad, 
                          address = i.direccion
                        };

    var totalHomologados = homologados.Count();
  
    // Particiona el archivo para servicio luppap.
    int finCiclo = 20;
    var t = new ArrayList();
    for (int i = 0; i <= totalHomologados; i+=20)
    {
        var paquete = homologados.Skip(i).Take(finCiclo - i);
        finCiclo = finCiclo + 20;
        // envio datos end point lupap.
        HttpClient client = new HttpClient();

        var byteArray = Encoding.ASCII.GetBytes("b011644b562a93c21a83e1b0398eaa86b358e781:2f3625ae38e1557f5ba771ceb2b79d116c05482d");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        
        var stringContent = new StringContent(JsonConvert.SerializeObject(paquete), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://batch.api.lupap.co/geocode/v2", stringContent);
        var contents = response.Content.ReadAsStringAsync().Result;
        // Almacenamiento respuesta lupap geolocalizada.
        var resp = JsonConvert.DeserializeObject(contents);
        t.Add(resp);
    }
    // Entrega de archivo geolocalizado sodim
    return JsonConvert.SerializeObject(t);
   
});

app.Run();
record Item(string identificador, string  empresa, string registroIndex, string barrio, string ciudad, string direccion);