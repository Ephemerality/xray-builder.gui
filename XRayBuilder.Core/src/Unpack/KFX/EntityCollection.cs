using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Amazon.IonDotnet.Tree;

namespace XRayBuilder.Core.Unpack.KFX
{
    public class EntityCollection : IEnumerable<Entity>
    {
        private readonly List<Entity> _entities;

        public EntityCollection()
        {
            _entities = new List<Entity>();
        }

        public void Add(Entity entity)
        {
            _entities.Add(entity);
        }

        public void Remove(Entity entity)
        {
            _entities.Remove(entity);
        }

        public Entity SingleOrDefault(string fragmentType)
        {
            return _entities.SingleOrDefault(entity => entity.FragmentType == fragmentType);
        }

        public T ValueOrDefault<T>(string fragmentType)
        {
            var entity = SingleOrDefault(fragmentType);
            if (entity?.Value is T value)
                return value;

            return default;
        }

        public IIonValue ValueOrDefault(string fragmentType, string fragmentId)
        {
            return _entities.SingleOrDefault(e => e.FragmentType == fragmentType && e.FragmentId == fragmentId)?.Value;
        }

        public IIonValue Value(string fragmentType, string fragmentId)
        {
            return _entities.Single(e => e.FragmentType == fragmentType && e.FragmentId == fragmentId).Value;
        }

        public T ValueOrDefault<T>(string fragmentType, string fragmentId)
        {
            var entity = _entities.SingleOrDefault(e => e.FragmentType == fragmentType && e.FragmentId == fragmentId);
            if (entity?.Value is T value)
                return value;

            return default;
        }

        public IEnumerator<Entity> GetEnumerator() => _entities.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
