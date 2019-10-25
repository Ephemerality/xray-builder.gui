using System.Collections.Generic;

namespace XRayBuilderGUI.Libraries
{
    public interface IFactory<in TEnum, out TValue>
    {
        TValue Get(TEnum @enum);
    }

    public abstract class Factory<TEnum, TValue> : IFactory<TEnum, TValue>
    {
        protected abstract Dictionary<TEnum, TValue> _dictionary { get; }

        public TValue Get(TEnum @enum) => _dictionary[@enum];
    }
}
