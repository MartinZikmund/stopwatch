using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stopwatch.Models;

namespace Stopwatch.Services.Data;

public interface IStopwatchRepository : IRepository<StopwatchModel>
{
	StopwatchModel GetOrCreateFirst();
}
