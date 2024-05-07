namespace bms.Leaf.Initializer
{
    public class IDGenInitializer : IIDGenInitializer
    {
        private static bool _initialized;
        private readonly IEnumerable<IIDGen> _idGens;
        public IDGenInitializer(IEnumerable<IIDGen> idGens)
        {
            _idGens = idGens;
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            _initialized = true;
            foreach (var item in _idGens)
            {
                await item.InitAsync();
            }
        }
    }
}
