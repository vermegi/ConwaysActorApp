using ConwaysApi.Domain;
using System.Web.Http;

namespace ConwaysApi.Controllers
{
    [ServiceRequestActionFilter]
    class GameController : ApiController 
    {
        public string Get()
        {
            return "the game";
        }

        public void Post([FromBody]string gameName)
        {
            var gameActor = new GameActorOrchestrator(gameName);
            gameActor.Initiate();
        }
    }
}
