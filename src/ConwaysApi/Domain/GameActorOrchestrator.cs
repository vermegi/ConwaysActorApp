using GameActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Threading.Tasks;

namespace ConwaysApi.Domain
{
    class GameActorOrchestrator
    {
        internal async Task Initiate(string name, int rows, int columns)
        {
            var actorId = new ActorId(name);
            var gameActor = ActorProxy.Create<IGameActor>(actorId, new Uri("fabric:/ConwaysActorApp/GameActorService"));
            await gameActor.Initiate(name, rows, columns);
        }

        internal async Task<string> Get(string name)
        {
            var actorId = new ActorId(name);
            var gameActor = ActorProxy.Create<IGameActor>(actorId, new Uri("fabric:/ConwaysActorApp/GameActorService"));
            return await gameActor.Get(name);
        }

        internal async Task DoTurn(string name)
        {
            var actorId = new ActorId(name);
            var gameActor = ActorProxy.Create<IGameActor>(actorId, new Uri("fabric:/ConwaysActorApp/GameActorService"));
            await gameActor.DoTurn();
        }
    }
}
