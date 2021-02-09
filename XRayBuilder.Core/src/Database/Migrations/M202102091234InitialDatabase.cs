using FluentMigrator;

namespace XRayBuilder.Core.Database.Migrations
{
    /// <summary>
    /// Create the initial database
    /// </summary>
    [Migration(202102091234)]
    public sealed class M202102091234InitialDatabase : Migration
    {
        public override void Up()
        {
            Create.Table("Book")
                .WithColumn("BookId")
                    .AsInt64()
                    .PrimaryKey()
                    .Identity()
                    .NotNullable()
                .WithColumn("Title")
                    .AsString()
                    .NotNullable()
                .WithColumn("Author")
                    .AsString()
                    .Nullable()
                .WithColumn("Asin")
                    // Ensure the unique constraint applies regardless of case
                    .AsCustom("TEXT COLLATE NOCASE")
                    .Nullable()
                    .Unique()
                // Realistically having a column per data source isn't very iterable, but hopefully we don't add many sources anyway...
                // Possibly nicer than having a separate table for them?
                .WithColumn("GoodreadsId")
                    .AsString()
                    .Nullable()
                .WithColumn("LibraryThingId")
                    .AsString()
                    .Nullable()
                .WithColumn("ShelfariId")
                    .AsString()
                    .Nullable()
                .WithColumn("Isbn")
                    .AsString()
                    .Nullable();

            Create.Table("Author")
                .WithColumn("AuthorId")
                    .AsInt64()
                    .PrimaryKey()
                    .Identity()
                    .NotNullable()
                .WithColumn("Name")
                    .AsString()
                    .NotNullable()
                .WithColumn("Asin")
                    // Ensure the unique constraint applies regardless of case
                    .AsCustom("TEXT COLLATE NOCASE")
                    .Nullable()
                    .Unique()
                .WithColumn("Biography")
                    .AsString()
                    .Nullable();

            Create.Table("BookAuthorMap")
                .WithColumn("BookAuthorMapId")
                    .AsInt64()
                    .PrimaryKey()
                    .Identity()
                    .NotNullable()
                .WithColumn("BookId")
                    .AsInt64()
                    .NotNullable()
                    .ForeignKey("Book", "BookId")
                .WithColumn("AuthorId")
                    .AsInt64()
                    .NotNullable()
                    .ForeignKey("Author", "AuthorId");
        }

        public override void Down()
        {
            Delete.Table("Book");
            Delete.Table("Author");
            Delete.Table("BookAuthorMap");
        }
    }
}