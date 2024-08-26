using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stopwatch.Models;

namespace Stopwatch.Services.Data;

public interface IRepository<T> where T : class, IId
{
	T[] GetAll();

	T? Get(int id);

	void Add(T item);

	void Update(T item);

	void Delete(T item);
}
