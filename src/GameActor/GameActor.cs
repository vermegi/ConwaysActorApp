using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using GameActor.Interfaces;
using GameCell.Interfaces;

namespace GameActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class GameActor : Actor, IGameActor
    {
        private int columns;
        private bool initiated;
        private int rows;
        private bool turnBusy;

        /// <summary>
        /// Initializes a new instance of GameActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public GameActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task<string> DoTurn()
        {
            if (!initiated)
                throw new InvalidOperationException("Initiate this game first");
            if (turnBusy)
                throw new InvalidOperationException("I'm still calculating, hold on plz");

            turnBusy = true;

            await CalculateNextStates();
            await GoToNextStates();

            turnBusy = false;
            return await Task.FromResult("running the turn");
        }

        private async Task GoToNextStates()
        {
            Parallel.For(0, rows, i => {
                Parallel.For(0, columns, async j =>
                {
                    var actorId = new ActorId($"{Id}{i}{j}");
                    var theActor = ActorProxy.Create<IGameCell>(actorId, new Uri("fabric:/ConwaysActorApp/GameCellActorService"));
                    await theActor.GotoNextState();
                });
            });
        }

        private async Task CalculateNextStates()
        {
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    var actorId = new ActorId($"{Id}{i}{j}");
                    var theActor = ActorProxy.Create<IGameCell>(actorId, new Uri("fabric:/ConwaysActorApp/GameCellActorService"));
                    await theActor.CalculateNextState(rows, columns);
                }
            }
        }

        public async Task<string> Get(string name)
        {
            var resultArray = new List<char[]>();
            for (var i = 0; i < rows; i++) resultArray.Add(new char[columns]);

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    var actorId = new ActorId($"{Id}{i}{j}");
                    var theActor = ActorProxy.Create<IGameCell>(actorId, new Uri("fabric:/ConwaysActorApp/GameCellActorService"));
                    var state = await theActor.GetState();
                    resultArray[i][j] = state == CellState.Alive ? 'a' : 'b';
                }
            }

            var resultstring = resultArray.Select(a => new string(a)).Aggregate((a, b) => a + "\n" + b);
            return resultstring.ToString();
        }

        public async Task<string> Initiate(string name, int rows, int columns)
        {
            if (initiated)
                return await Task.FromResult("nothing to do here");

            this.rows = rows;
            this.columns = columns;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    var actorId = new ActorId($"{Id}{i}{j}");
                    var gameCell = ActorProxy.Create<IGameCell>(actorId, new Uri("fabric:/ConwaysActorApp/GameCellActorService"));
                    await gameCell.Initiate(name, i, j);
                }
            }
            initiated = true;
            return await Task.FromResult("done");
        }

        public async Task TurnDone()
        {
            turnBusy = false;
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            return this.StateManager.TryAddStateAsync("count", 0);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <returns></returns>
        Task<int> IGameActor.GetCountAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task IGameActor.SetCountAsync(int count, CancellationToken cancellationToken)
        {
            // Requests are not guaranteed to be processed in order nor at most once.
            // The update function here verifies that the incoming count is greater than the current count to preserve order.
            return this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, cancellationToken);
        }

    }
}
