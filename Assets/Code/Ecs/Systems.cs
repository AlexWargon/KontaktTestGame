namespace Wargon.TinyEcs {
    internal sealed class Systems {
        private readonly DynamicArray<ISystem> allSystems;
        private readonly DynamicArray<ISystem> updateSystems;
        private readonly DynamicArray<ISystem> fixedSystems;
        private readonly DynamicArray<ISystem> lateSystems;
        private readonly World world;

        public Systems(World world) {
            this.world = world;
            allSystems = new DynamicArray<ISystem>(8);
            updateSystems = new DynamicArray<ISystem>(6);
            fixedSystems = new DynamicArray<ISystem>(2);
            lateSystems = new DynamicArray<ISystem>(2);
        }

        internal void Add<TSystem>() where TSystem : class, ISystem, new() {
            var system = new TSystem();
            allSystems.Add(system);
            if (system is IFixed) {
                fixedSystems.Add(system);
            }
            else
            if (system is ILate) {
                lateSystems.Add(system);
            }
            else {
                updateSystems.Add(system);
            }

            InjectFields(system);
        }

        internal void Init() {
            var destroyEntities = new DestroyEntitiesSystem();
            destroyEntities.OnCreate(world);
            updateSystems.Add(destroyEntities);
            
            for (var i = 0; i < allSystems.Count; i++) allSystems[i].OnCreate(world);
        }

        private void InjectFields(ISystem system) {
            world.GetDI().Build(system);
        }

        internal void OnUpdate(ref WorldData worldData) {
            for (var i = 0; i < updateSystems.Count; i++) {
                updateSystems[i].OnUpdate(world);
                world.UpdateQueries();
            }
        }
        internal void OnFixedUpdate(ref WorldData worldData) {
            for (var i = 0; i < fixedSystems.Count; i++) {
                fixedSystems[i].OnUpdate(world);
                world.UpdateQueries();
            }
        }
        internal void OnLateUpdate(ref WorldData worldData) {
            for (var i = 0; i < lateSystems.Count; i++) {
                lateSystems[i].OnUpdate(world);
                world.UpdateQueries();
            }
        }
    }
    public interface ISystem {
        void OnCreate(World world);
        void OnUpdate(World world);
    }
    public interface IFixed { }
    public interface ILate { }
    
    public sealed class DestroyEntity : IPureComponent{}
    sealed class DestroyEntitiesSystem : ISystem {
        private Query _query;
        public void OnCreate(World world) {
            _query = world.GetQuery().With<DestroyEntity>();
        }

        public void OnUpdate(World world) {
            foreach (var entity in _query) {
                world.DestroyEntity(entity);
            }
        }
    }
}