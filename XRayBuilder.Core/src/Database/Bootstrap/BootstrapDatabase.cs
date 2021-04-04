using SimpleInjector;
using XRayBuilder.Core.Database.Orm.Author;
using XRayBuilder.Core.Database.Orm.BookAuthorMap;
using XRayBuilder.Core.Libraries.Bootstrap.Model;

namespace XRayBuilder.Core.Database.Bootstrap
{
    public sealed class BootstrapDatabase : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            var config = new DatabaseConfig("xraybuilder.db");
            container.RegisterSingleton(() => config);
            container.RegisterSingleton<DatabaseMigrator>();
            container.RegisterSingleton<IDatabaseConnection>(() => new DatabaseConnection(config));
            container.RegisterSingleton<IAuthorOrm, AuthorOrm>();
            container.RegisterSingleton<IBookAuthorMapOrm, BookAuthorMapOrm>();
        }
    }
}