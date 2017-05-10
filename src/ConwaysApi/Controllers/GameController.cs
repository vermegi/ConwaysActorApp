using ConwaysApi.Domain;
using System.Web.Http;

namespace ConwaysApi.Controllers
{
    [ServiceRequestActionFilter]
    public class GameController : ApiController 
    {
        public string Get()
        {
            return "the game";
        }

        public void Post(string name, int rows, int columns)
        {
            var gameActor = new GameActorOrchestrator(name, rows, columns);
            gameActor.Initiate();
        }
    }
}
