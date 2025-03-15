using Microsoft.EntityFrameworkCore;
using Stopwatch.Models;

namespace Stopwatch.Services.Data;

public class StopwatchDbContext : DbContext
{
	public StopwatchDbContext(DbContextOptions<StopwatchDbContext> options) : base(options) { }

	public DbSet<StopwatchModel> Stopwatches { get; set; } = null!;
	public DbSet<HistoryEntryModel> HistoryEntries { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		// Configure entity properties and relationships here
	}
}