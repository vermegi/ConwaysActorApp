using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using GameCell.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.ServiceFabric.Actors.Client;
using GameActor.Interfaces;

namespace GameCell
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
    internal class GameCell : Actor, IGameCell
    {
        private int column;
        private int row;
        private string name;
        private CellState state;
        private CellState nextState;

        /// <summary>
        /// Initializes a new instance of GameCell
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public GameCell(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task CalculateNextState(int rows, int columns)
        {
            var aliveNeighbourCount = 0;

            var neighbourIds = GetNeighbourIds(rows, columns);

            foreach (var neighbourId in neighbourIds)
            {
                var actorId = new ActorId($"{name}{neighbourId}");
                var neighbour = ActorProxy.Create<IGameCell>(actorId, new Uri("fabric:/ConwaysActorApp/GameCellActorService"));
                var state = await neighbour.GetState();
                if (state == CellState.Alive)
                    aliveNeighbourCount++;
            }

            if (state == CellState.Alive && aliveNeighbourCount < 2) nextState = CellState.Dead;
            else if (state == CellState.Alive && (aliveNeighbourCount == 2 || aliveNeighbourCount == 3)) nextState = CellState.Alive;
            else if (state == CellState.Alive && aliveNeighbourCount > 3) nextState = CellState.Dead;
            else if (state == CellState.Dead && aliveNeighbourCount == 3) nextState = CellState.Alive;
        }

        private IEnumerable<string> GetNeighbourIds(int rows, int columns)
        {
            var west = column--;
            var north = row--;
            var east = column++;
            var south = row++;

            // w
            if (west >= 0) yield return $"{row}{west}";
            // nw
            if (west >= 0 && north >= 0) yield return $"{north}{west}";
            // n
            if (north >= 0) yield return $"{north}{column}";
            // ne
            if (north >= 0 && east < columns) yield return $"{north}{east}";
            // e
            if (east < columns) yield return $"{row}{east}";
            // se
            if (east < columns && south < rows) yield return $"{south}{east}";
            // s
            if (south < rows) yield return $"{south}{column}";
            // sw
            if (south < rows && west >= 0) yield return $"{south}{west}";
        }

        private string GetNextCellId(int rows, int columns)
        {
            var nextCol = column++;
            var nextRow = row++;
            if (nextCol < columns) return $"{row}{nextCol}";
            if (nextRow < rows) return $"{nextRow}{0}";
            else return string.Empty;
        }

        public Task<string> Initiate(string name, int row, int column)
        {
            var rnd = new Random(System.DateTime.Now.Millisecond);
            this.name = name;
            this.row = row;
            this.column = column;
            state = rnd.Next() % 2 == 0 ? CellState.Alive : CellState.Dead;
            var stateString = state == CellState.Alive ? "Alive" : "Dead";
            ActorEventSource.Current.ActorMessage(this, $"created cell at {row}-{column}: {stateString}");
            return Task.FromResult("done");
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
        Task<int> IGameCell.GetCountAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task IGameCell.SetCountAsync(int count, CancellationToken cancellationToken)
        {
            // Requests are not guaranteed to be processed in order nor at most once.
            // The update function here verifies that the incoming count is greater than the current count to preserve order.
            return this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, cancellationToken);
        }

        public Task<CellState> GetState()
        {
            return Task.FromResult(state);
        }

        public async Task GotoNextState()
        {
            state = nextState;
        }
    }
}
