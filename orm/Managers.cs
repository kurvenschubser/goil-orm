using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Orm.Managers
{
	public class Manager<M>
	{
		M model;


		public Manager()
		{
		}

		public M Get(int id)
		{
			throw new NotImplementedException();
		}

		public M Get(object pk)
		{
			throw new NotImplementedException();
		}

		public List<M> Filter(Dictionary<string, object> kwargs)
		{
			throw new NotImplementedException();
		}
	}
}