using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreDemo.Migrations
{
    /// <inheritdoc />
    public partial class addView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE VIEW dbo.CoursesWithAuthorsAndCategories
            AS
 
            SELECT	Cources_Title		= c.Title,
		            Courses_Description	= c.[Description],
		            Courses_Level		= c.[Level],
		            Courses_FullPrice	= c.FullPrice,
		            Authors_Name		= a.[Name],
		            Categories_Name		= cat.[Name]
            FROM	Courses				AS c
		            INNER JOIN
		            Authors				AS a
			            ON	a.Id		= c.AuthorId
		            INNER JOIN
		            Categories			AS cat
			ON	cat.Id		= c.CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW dbo.CoursesWithAuthorsAndCategories");
        }
    }
}
