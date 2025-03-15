using Microsoft.EntityFrameworkCore;
using Stopwatch.Models;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Stopwatch.Services.Data.SQLite;

internal class SQLiteRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : IRepository<T> where T : class, IId
{
	private readonly StopwatchDbContext _dbContext;

	public SQLiteRepository(StopwatchDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public T[] GetAll()
	{
		return _dbContext.Set<T>().ToArray();
	}

	public T? Get(int id)
	{
		return _dbContext.Set<T>().Find(id);
	}

	public void Add(T item)
	{
		_dbContext.Set<T>().Add(item);
		_dbContext.SaveChanges();
	}

	public void Update(T item)
	{
		_dbContext.Set<T>().Update(item);
		_dbContext.SaveChanges();
	}

	public void Delete(int id)
	{
		var item = _dbContext.Set<T>().Find(id);
		if (item != null)
		{
			_dbContext.Set<T>().Remove(item);
			_dbContext.SaveChanges();
		}
	}

	public void DeleteAll()
	{
		_dbContext.Set<T>().RemoveRange(_dbContext.Set<T>());
		_dbContext.SaveChanges();
	}
}