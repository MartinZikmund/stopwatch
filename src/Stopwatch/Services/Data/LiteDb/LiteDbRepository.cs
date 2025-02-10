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
	private readonly string _name;

	public LiteDbRepository(LiteDatabase database, string name)
	{
		_database = database;
		_name = name;
	}

	public void Add(T item)
	{
		var collection = GetCollection();
		var id = collection.Insert(item);
		item.Id = id.AsString;
	}

	public void Delete(string id)
	{
		var collection = GetCollection();
		collection.Delete(id);
	}

	public T Get(string id)
	{
		var collection = GetCollection();
		return collection.FindById(id);
	}
	public T[] GetAll()
	{
		var collection = GetCollection();
		return collection.FindAll().ToArray();
	}

	public void Update(T item)
	{
		var collection = GetCollection();
		collection.Update(item);
	}

	public void DeleteAll()
	{
		var collection = GetCollection();
		collection.DeleteAll();
	}
	
	private ILiteCollection<T> GetCollection() => _database.GetCollection<T>(_name);
}
