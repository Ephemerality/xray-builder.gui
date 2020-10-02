using System.Collections.Generic;

namespace XRayBuilder.Core.Libraries
{
    public interface IFactory<TEnum, TValue>
    {
        TValue Get(TEnum @enum);
        IReadOnlyDictionary<TEnum, TValue> GetAll();
        IEnumerable<TValue> GetValues();
    }

    public abstract class Factory<TEnum, TValue> : IFactory<TEnum, TValue>
    {
        protected abstract IReadOnlyDictionary<TEnum, TValue> Dictionary { get; }

        public TValue Get(TEnum @enum) => Dictionary[@enum];
        public IReadOnlyDictionary<TEnum, TValue> GetAll() => Dictionary;
        public IEnumerable<TValue> GetValues() => Dictionary.Values;
    }
}
