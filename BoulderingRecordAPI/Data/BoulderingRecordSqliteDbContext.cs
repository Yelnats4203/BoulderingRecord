using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Data;

public class BoulderingRecordSqliteDbContext(DbContextOptions<BoulderingRecordSqliteDbContext> options)
    : BoulderingRecordDbContext(options);
