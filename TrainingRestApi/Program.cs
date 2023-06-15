using System.Data;
using Dapper;
using MySqlConnector;
using TrainingRestApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<GetConnection>(sp =>
    async () =>
    {
        string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? string.Empty;
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException();
        }

        var connection = new MySqlConnection(connectionString);
        Console.WriteLine("DB_CONNECTION: " + connectionString);
        await connection.OpenAsync();
        return connection;
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.MapGet("/", async () => Results.Ok("API Health Check: " + DateTime.Now))
    .WithOpenApi();

app.MapPost("/companies/ledger", async (CompanyLedger request, GetConnection connectionGetter) =>
    {
        using var con = await connectionGetter();

        var sql = "INSERT INTO company_bookkeeping (companyid, month, year, liquidity, profit, workers, wage, sales, stock, lifetime, extinct) " +
                  "VALUES (@CompanyId, @Month, @Year, @Liquidity, @Profit, @Workers, @Wage, @Sales, @Stock, @Lifetime, @Extinct); " +
                  "SELECT companyid, month, year, liquidity, profit, workers, wage, sales, stock, lifetime, extinct, id " +
                  "FROM company_bookkeeping " +
                  "WHERE id = LAST_INSERT_ID();";
        var result = await con.QueryFirstOrDefaultAsync<CompanyLedger>(sql, request);

        return result != null
            ? Results.Created($"/companies/ledger/{result.Id}", result)
            : Results.BadRequest();
    })
    .WithOpenApi();

app.MapPost("/companies/event", async (CompanyEvent request, GetConnection connectionGetter) =>
    {
        using var con = await connectionGetter();

        var sql = "INSERT INTO company_event (companyid, month, year, actprice, actwage, actworker) " +
                  "VALUES (@CompanyId, @Month, @Year, @ActPrice, @ActWage, @ActWorker); " +
                  "SELECT companyid, month, year, actprice, actwage, actworker, id " +
                  "FROM company_event " +
                  "WHERE id = LAST_INSERT_ID();";
        var result = await con.QueryFirstOrDefaultAsync<CompanyEvent>(sql, request);

        return result != null
            ? Results.Created($"/companies/event/{result.Id}", result)
            : Results.BadRequest();
    })
    .WithOpenApi();

app.Run();


public delegate Task<IDbConnection> GetConnection();
