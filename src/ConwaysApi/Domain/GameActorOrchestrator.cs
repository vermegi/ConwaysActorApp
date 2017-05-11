﻿using GameActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;

namespace ConwaysApi.Domain
{
    class GameActorOrchestrator
    {
        internal async void Initiate(string name, int rows, int columns)
        {
            var actorId = new ActorId(name);
            var gameActor = ActorProxy.Create<IGameActor>(actorId, new Uri("fabric:/ConwaysActorApp/GameActorService"));
            await gameActor.Initiate(name, rows, columns);
        }

        internal async void DoTurn(string name)
        {
            var actorId = new ActorId(name);
            var gameActor = ActorProxy.Create<IGameActor>(actorId, new Uri("fabric:/ConwaysActorApp/GameActorService"));
            await gameActor.DoTurn();
        }
    }
}
