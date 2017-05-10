using GameActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;

namespace ConwaysApi.Domain
{
    class GameActorOrchestrator
    {
        private string gameName;

        public GameActorOrchestrator(string gameName)
        {
            this.gameName = gameName;
        }

        internal async void Initiate()
        {
            var actorId = ActorId.CreateRandom();
            var gameActor = ActorProxy.Create<IGameActor>(actorId, new Uri("fabric:/ConwaysActorApp/GameActor"));
            await gameActor.Initiate();
        }
    }
}
