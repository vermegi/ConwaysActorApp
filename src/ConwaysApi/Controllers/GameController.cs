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
            var gameActor = new GameActorOrchestrator();
            gameActor.Initiate(name, rows, columns);
        }

        public void Put(string name, string message)
        {
            switch (message)
            {
                case "doTurn":
                    var gameOrchestrator = new GameActorOrchestrator();
                    gameOrchestrator.DoTurn(name);
                    break;

            }
        }
    }
}
