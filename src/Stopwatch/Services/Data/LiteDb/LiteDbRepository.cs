#if !HAS_UNO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Stopwatch.Models;

namespace Stopwatch.Services.Data.LiteDb;
internal class LiteDbRepository<T> : IRepository<T> where T : class, IId
{
	private readonly LiteDatabase _database;

	public LiteDbRepository(LiteDatabase database)
	{
		_database = database;
	}

	public void Add(T item)
	{
		var collection = _database.GetCollection<T>();
		var id = collection.Insert(item);
		item.Id = id.AsInt32;
	}

	public void Delete(int id)
	{
		var collection = _database.GetCollection<T>();
		collection.Delete(id);
	}

	public T Get(int id)
	{
		var collection = _database.GetCollection<T>();
		return collection.FindById(id);
	}
	public T[] GetAll()
	{
		var collection = _database.GetCollection<T>();
		return collection.FindAll().ToArray();
	}

	public void Update(T item)
	{
		var collection = _database.GetCollection<T>();
		collection.Update(item);
	}

	public void DeleteAll()
	{
		var collection = _database.GetCollection<T>();
		collection.DeleteAll();
	}
}
#endif
