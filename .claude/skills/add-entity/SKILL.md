---
name: add-entity
description: Scaffold a new domain entity for the BoulderingRecord API — model class, EF Core configuration, a repository interface plus implementation that works against both SQLite (test) and MSSQL (prod), DI registration, and an xUnit test stub. Use when the user asks to add a new domain entity/model (e.g. climb, route, session, gym) to the persistence layer, or invoke via `/add-entity <EntityName> [property:type ...]`.
---

Arguments: `$ARGUMENTS` — first token is the entity name in PascalCase (e.g. `Climb`), remaining tokens are `property:type` pairs (e.g. `Grade:string AttemptedAt:DateTime Sends:int`). If no properties are given, ask the user what fields the entity needs before scaffolding.

Follow CLAUDE.md's persistence architecture: repository logic must stay provider-agnostic (SQLite for tests, MSSQL for production), so never write provider-specific SQL or branch repository code by provider.

## Steps

1. **Check whether the persistence infrastructure already exists** before creating anything new:
   - Search the project for a class inheriting `DbContext`.
   - Check `BoulderingRecordAPI.csproj` for `Microsoft.EntityFrameworkCore.Sqlite` / `Microsoft.EntityFrameworkCore.SqlServer` package references.
   - Search for an existing repository interface pattern (e.g. `I*Repository`).

   If this is the **first entity** and none of this exists yet, bootstrap it once:
   - Add the EF Core provider packages (`Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`).
   - Create `Data/AppDbContext.cs`.
   - Wire a provider switch into `Program.cs` (e.g. reading an `appsettings.json` key like `Database:Provider` = `Sqlite` | `SqlServer`, defaulting to SQLite for `Development`/testing) so swapping providers never touches repository code.

   If infrastructure already exists, **reuse it** — don't create a second `DbContext` or a second provider-switch mechanism.

2. **Create the entity model** under `Models/` with the requested properties plus an `Id` key (match the ID type convention of existing entities if any; otherwise use `int` with `[Key]`/auto-increment).

3. **Add EF Core configuration** for the entity as an `IEntityTypeConfiguration<T>` class under `Data/Configurations/` (preferred over inline `OnModelCreating` for scalability — match existing convention if one is already established), and add the corresponding `DbSet<T>` to `AppDbContext`.

4. **Create the repository**:
   - `Repositories/I{Entity}Repository.cs` — interface with `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`.
   - `Repositories/{Entity}Repository.cs` — EF Core–backed implementation. Because it goes through `DbContext`/LINQ rather than raw SQL, it must work unchanged against both SQLite and MSSQL — do not add provider-specific branches.

5. **Register the repository in DI** in `Program.cs`: `builder.Services.AddScoped<I{Entity}Repository, {Entity}Repository>();`

6. **Migrations**: if a `Migrations/` folder already exists (the project uses EF Core migrations), add a migration for the new entity (`dotnet ef migrations add Add{Entity}`). If no `Migrations/` folder exists yet, don't introduce one unprompted — match whatever the project is currently doing (e.g. `EnsureCreated`).

7. **Add an xUnit test stub** under `BoulderingRecordAPI.Tests/` covering the repository's basic CRUD against an in-memory SQLite connection (`Microsoft.Data.Sqlite` with `DataSource=:memory:`, keep the connection open for the test's lifetime). If the test project doesn't exist yet, create it with `dotnet new xunit -o BoulderingRecordAPI.Tests`, add a project reference to `BoulderingRecordAPI`, and add it to `BoulderingRecord.slnx`.

8. **Build to verify**: run `dotnet build` on the solution before reporting the task done. If a test project exists, run `dotnet test` too.
