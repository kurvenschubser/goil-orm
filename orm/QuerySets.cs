using System;
using System.Collections.Generic;

using Orm.Models;
using Orm.Filters;
using Orm.Sql;
using Orm.Datastructures;


namespace Orm.QuerySets
{
	public interface IQuerySetStorage
	{
		IModel Get(QuerySetStorageKey key);
		void Set(QuerySetStorageKey key, IModel o);
		void Delete(QuerySetStorageKey key);
		event QuerySetStorageInvalidateEventHandler Invalidate;
	}


	public delegate void QuerySetStorageInvalidateEventHandler(
		object sender,
		QuerySetStorageInvalidateEventArgs e
	);


	public class QuerySetStorageInvalidateEventArgs : EventArgs
	{
		QuerySetStorageKey Key;


		public QuerySetStorageInvalidateEventArgs(QuerySetStorageKey key)
		{
			this.Key = key;
		}
	}


	public class QuerySetStorageKey
	{
		Type model;
		object pk;


		public QuerySetStorageKey(Type model, object pk)
		{
			this.model = model;
			this.pk = pk;
		}

		public bool Equals(QuerySetStorageKey other)
		{
			return this.GetHashCode() == other.GetHashCode();
		}

		public override int GetHashCode()
		{
			return this.model.GetHashCode() + this.pk.ToString().GetHashCode();
		}
	}


	public class DefaultQuerySetStorage : IQuerySetStorage
	{
		public int MAX_ENTRIES;
		private static Dictionary<QuerySetStorageKey, IModel> cache =
			new Dictionary<QuerySetStorageKey, IModel>();

		public event QuerySetStorageInvalidateEventHandler Invalidate;


		public DefaultQuerySetStorage(int max_entries=1000)
		{
			this.MAX_ENTRIES = max_entries;
		}

		public IModel Get(QuerySetStorageKey key)
		{
			return cache[key];
		}

		public void Set(QuerySetStorageKey key, IModel o)
		{
			if (cache.Count >= MAX_ENTRIES)
			{
				foreach (QuerySetStorageKey k in cache.Keys)
				{
					cache.Remove(k);
					this.RaiseInvalidate(k);
					break;
				}
			}
			cache[key] = o;
			this.RaiseInvalidate(key);
		}

		public void Delete(QuerySetStorageKey key)
		{
			cache.Remove(key);
		}

		public void RaiseInvalidate(QuerySetStorageKey key)
		{
			this.Invalidate(
				this, 
				new QuerySetStorageInvalidateEventArgs(key)
			);
		}
	}


	public class QuerySetEnumerator
	{
		SelectQuery query;
		IQuerySetStorage storage;


		public QuerySetEnumerator(SelectQuery query, IQuerySetStorage storage)
		{
			this.query = query;
			this.storage = storage;
			storage.Invalidate += new QuerySetStorageInvalidateEventHandler(OnStorageInvalidate);
		}

		public void OnStorageInvalidate(object sender,
										QuerySetStorageInvalidateEventArgs e)
		{
			throw new InvalidOperationException(string.Format(
				"QuerySetEnumerator received 'Invalidate' event from storage."));
		}

		public IEnumerator<IModel> GetEnumerator()
		{
			Connections.IConnection conn = new Connections.Connection();
			object[][] res = conn.Execute(this.query);
			IModel o;
			Dictionary<string, object> dict;
			QuerySetStorageKey storage_key;
			foreach (object[] row in res)
			{
				// need to instantiate a model here, to get the ModelMeta
				o = (IModel)(Activator.CreateInstance(this.query.Model));

				storage_key = new QuerySetStorageKey(
					this.query.Model, 
					row[o.Meta.GetFieldIndex(o.Meta.PrimaryKey.DbField)]
				);
				try
				{
					o = storage.Get(storage_key);
				}
				catch (KeyNotFoundException)
				{
					dict = o.Meta.PrepareRowForInstance(row);
					foreach (string key in dict.Keys)
					{
						o.Set(key, dict[key]);
					}
					this.storage.Set(storage_key, o);
				}
				yield return o;
			}
		}
	}


	public class QuerySet
	{
		SelectQuery query;
		Type source_model;
		IQuerySetStorage storage;


		public QuerySet(Type source_model, IQuerySetStorage storage=null)
		{
			this.source_model = source_model;
			if (storage == null)
				storage = new DefaultQuerySetStorage();
			this.storage = storage;
		}

		public IModel this[int index]
		{
			get
			{
				int i = 1;
				foreach (IModel o in this)
				{
					if (index == i++)
						return o;
				}
				throw new IndexOutOfRangeException(string.Format(
					"Index '{0}' does not exist on QuerySet."));
			}
		}

		public List<IModel> this[int start, int end = int.MaxValue,
											int step = 1]
		{
			get { return Slice(start, end, step); }
		}

		public List<IModel> Slice(int start, int end = int.MaxValue, 
															int step = 1)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<IModel> GetEnumerator()
		{
			return this.GetQuerySetEnumerator().GetEnumerator();
		}

		protected QuerySetEnumerator GetQuerySetEnumerator()
		{
			return new QuerySetEnumerator(this.Query, this.storage);
		}

		public void SetStorage(IQuerySetStorage storage)
		{
			this.storage = storage;
		}

		public QuerySet Filter(Node<WherePredicate> predicates, bool and=true)
		{
			this.Query.Filter(predicates, and);
			return this;
		}

		public SelectQuery Query
		{
			get
			{
				if (this.query == null)
					this.query = new SelectQuery(this.source_model);
				return this.query; 
			}
		}
	}
}
