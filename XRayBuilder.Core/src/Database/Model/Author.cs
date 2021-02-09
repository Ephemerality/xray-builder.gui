using JetBrains.Annotations;
using SQLite;

namespace XRayBuilder.Core.Database.Model
{
    [Table(nameof(Author))]
    public sealed class Author
    {
        public Author([JetBrains.Annotations.NotNull] string name)
        {
            Name = name;
        }

        [PrimaryKey]
        [Column(nameof(AuthorId))]
        public long AuthorId { get; set; }

        [Column(nameof(Name))]
        [JetBrains.Annotations.NotNull]
        [SQLite.NotNull]
        public string Name { get; }

        [Column(nameof(Asin))]
        [CanBeNull]
        public string Asin { get; set; }

        [Column(nameof(Biography))]
        [CanBeNull]
        public string Biography { get; set; }
    }
}