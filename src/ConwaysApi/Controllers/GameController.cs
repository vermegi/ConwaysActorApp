using ConwaysApi.Domain;
using System.Threading.Tasks;
using System.Web.Http;

namespace ConwaysApi.Controllers
{
    [ServiceRequestActionFilter]
    public class GameController : ApiController 
    {
        public async Task<string> Get(string name)
        {
            var gameActor = new GameActorOrchestrator();
            return await gameActor.Get(name);
        }

        public async void Post(string name, int rows, int columns)
        {
            var gameActor = new GameActorOrchestrator();
            await gameActor.Initiate(name, rows, columns);
        }

        public async void Put(string name, string message)
        {
            switch (message)
            {
                case "doTurn":
                    var gameOrchestrator = new GameActorOrchestrator();
                    await gameOrchestrator.DoTurn(name);
                    break;

            }
        }
    }
}
