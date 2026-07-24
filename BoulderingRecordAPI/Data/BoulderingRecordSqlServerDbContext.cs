using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Data;

public class BoulderingRecordSqlServerDbContext(DbContextOptions<BoulderingRecordSqlServerDbContext> options)
    : BoulderingRecordDbContext(options);
