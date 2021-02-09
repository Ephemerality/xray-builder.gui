namespace XRayBuilder.Core.Database.Logic
{
    public sealed class BookRepository
    {
        private readonly IDatabaseConnection _connection;

        public BookRepository(IDatabaseConnection connection)
        {
            _connection = connection;
        }
    }
}