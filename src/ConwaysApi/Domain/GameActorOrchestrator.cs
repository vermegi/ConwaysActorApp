using GameActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;

namespace ConwaysApi.Domain
{
    class GameActorOrchestrator
    {
        private int columns;
        private int rows;
        private string name;

        public GameActorOrchestrator(string name, int rows, int columns)
        {
            this.name = name;
            this.rows = rows;
            this.columns = columns;
        }

        internal async void Initiate()
        {
            var actorId = new ActorId(name);
            var gameActor = ActorProxy.Create<IGameActor>(actorId, new Uri("fabric:/ConwaysActorApp/GameActorService"));
            await gameActor.Initiate(rows, columns);
        }
    }
}
