using System;
using System.Data;
using System.Collections.Generic;
using System.Data.OleDb;

using Orm.Settings;
using Orm.Models;
using Orm.Sql;


namespace Orm.Connections
{
	public class ConnectionError : ApplicationException
	{
		public ConnectionError(string msg) : base(msg) { }
	}


	public interface IConnection
	{
		void Close();
		void Commit();
		object[][] Execute(string stmt, KeyValuePair<string, object>[] data);
		object[][] Execute(string stmt);
		object[][] Execute(IQuery qry);
		object[][] ExecuteMany(string stmt, IEnumerable<KeyValuePair<string, object>[]> data);
		IConnection Open(); 
		IConnection Setup();
		// ITransaction StartTransaction();
	}


	public class Connection : IConnection
	{
		private IConnection conn;

		public Connection()
		{
			Setup();
		}

		public void Close()
		{
			this.conn.Close();
			this.conn = null;
		}

		public void Commit()
		{
			this.conn.Commit();
		}

		public virtual object[][] Execute(string stmt, KeyValuePair<string, object>[] data)
		{
			return ExecuteMany(stmt, new KeyValuePair<string, object>[][] { data });
		}

		public virtual object[][] Execute(string stmt)
		{
			return Execute(stmt, new KeyValuePair<string, object>[] { });
		}

		public virtual object[][] Execute(IQuery qry)
		{
			return Execute(qry.ToString(), qry.GetData().ToArray());
		}

		public virtual object[][] ExecuteMany(string stmt, IEnumerable<KeyValuePair<string, object>[]> data)
		{
			return this.conn.ExecuteMany(stmt, data);
		}

		public IConnection Setup()
		{
			this.conn = Backends.Backend.GetBackend().Connection;
			return this;
		}

		public IConnection Open()
		{
			if (this.conn == null)
				throw new ConnectionError("Connection non-existent.");
			return this.conn.Open();
		}
	}


	public class JetConnection : IConnection
	{
		private OleDbConnection conn;

		public JetConnection()
		{
			this.Setup();
		}

		public void Close()
		{
			this.conn.Close();
		}

		public void Commit()
		{
			// pass
		}

		public object[][] Execute(string stmt, KeyValuePair<string, object>[] data)
		{
			return ExecuteMany(stmt, new KeyValuePair<string, object>[][] { data });
		}

		public object[][] Execute(string stmt)
		{
			return Execute(stmt, new KeyValuePair<string, object>[] { });
		}

		public object[][] Execute(IQuery qry)
		{
			return Execute(qry.ToString(), qry.GetData().ToArray());
		}

		public object[][] ExecuteMany(string stmt, IEnumerable<KeyValuePair<string, object>[]> data)
		{
			OleDbCommand cmd = new OleDbCommand(stmt, this.conn);
			IEnumerator<KeyValuePair<string, object>[]> enumerator = data.GetEnumerator();
			if (enumerator.MoveNext())
			{
				cmd.Parameters.AddRange(ConvertToParameters(enumerator.Current));
			}
			List<object[]> ret = new List<object[]>();
			if (stmt.TrimStart(' ').ToLower().StartsWith("select"))
			{
				if (enumerator.MoveNext())
					throw new ConnectionError("Can't have more than one value row in 'data' argument when doing a SELECT.");

				DataSet ds = new DataSet();
				OleDbDataAdapter adapter = new OleDbDataAdapter();
				adapter.SelectCommand = cmd;
				adapter.Fill(ds);

				List<object> rowtuple;
				int col_count = ds.Tables[0].Columns.Count;
				foreach (DataRow row in ds.Tables[0].Rows)
				{
					rowtuple = new List<object>();
					for (int i_col = 0; i_col < col_count; i_col++)
					{
						rowtuple.Add(row[i_col]);
					}
					ret.Add(rowtuple.ToArray());
				}
			}
			else
			{
				int rowcount = cmd.ExecuteNonQuery();
				ret.Add(new object[] { rowcount });
				while (enumerator.MoveNext())
				{
					foreach (KeyValuePair<string, object> kvs in enumerator.Current)
					{
						cmd.Parameters[kvs.Key].Value = kvs.Value;
					}
					rowcount = cmd.ExecuteNonQuery();
					ret.Add(new object[] { rowcount });
				}
			}

			return ret.ToArray();
		}

		public IConnection Open()
		{
			this.conn.Open();
			return this;
		}

		private OleDbParameter[] ConvertToParameters(KeyValuePair<string, object>[] data)
		{
			List<OleDbParameter> parameters = new List<OleDbParameter>();
			foreach (KeyValuePair<string, object> kv in data)
			{
				parameters.Add(new OleDbParameter(kv.Key, kv.Value));
			}
			return parameters.ToArray();
		}

		public IConnection Setup()
		{
			this.conn = new OleDbConnection();
			this.conn.ConnectionString = string.Format(
				@"Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0};
						User Id={1};Password={2};",
				Settings.Settings.Get("name"),
				Settings.Settings.Get("user"),
				Settings.Settings.Get("password")
			);
			return this;
		}
	}
}
