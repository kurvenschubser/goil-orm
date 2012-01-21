using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using Orm.Connections;
using Orm.Fields;
using Orm.Sql;


namespace Orm.Backends
{
	public class BackendError : ApplicationException
	{
		public BackendError(string message) : base(message) { }
		public BackendError(string message, Exception innerException) : base(message, innerException) { }
	}


	public class TypeConversionError : ApplicationException
	{
		public TypeConversionError(string message) : base(message) { }
		public TypeConversionError(string message, Exception innerException) : base(message, innerException) { }
	}


	/// <summary>
	/// Convert a value pulled from the db to the appropriate C-Sharp type.
	/// </summary>
	public interface ITypeConverter
	{
		object ToCSharp(object val);
	}
	
	
	public interface IBackend
	{
		IConnection Connection { get; set; }
		Table[] Tables { get; }
		Table GetTable(string table_name);
		ITypeConverter GetTypeConverterForTypeName(string db_type_name);
	}


	public class JetBackend : IBackend
	{
		IConnection connection; 
		List<Table> tables;
		static Dictionary<string, ITypeConverter> converters = 
			new Dictionary<string,ITypeConverter>();


		public IConnection Connection
		{
			get
			{
				return connection;
			}
			set
			{
				connection = value;
			}
		}

		public Table[] Tables
		{
			get
			{
				if (tables == null)
					tables = GetTables();
				return tables.ToArray();
			}
		}

		public Table GetTable(string table_name)
		{
			foreach (Table t in Tables)
			{
				if (t.Name == table_name)
					return t;
			}
			throw new KeyNotFoundException(string.Format(
				"Could not find table '{0}'.", table_name
			));
		}

		public List<Table> GetTables()
		{
			List<Table> ret = new List<Table>();

			// XXX smelly statement ahead.
			object[][] tablerows = Connection.Execute(@"				
				SELECT MSysObjects.Name 
				FROM MSysObjects 
				WHERE (((MSysObjects.Type) In (1,4,6)) 
					AND ((Left([Name],4))<>'MSYS') 
					AND ((Left([Name],1))<>'~'));"
			);
			if (tablerows.Length == 0)
				throw new BackendError(
					"Could not find any user tables in database '" 
					+ Settings.Settings.Get("dbms.name") + "'.");

			object[][] columnrows;
			string tablename;
			List<Column> columns = new List<Column>();
			foreach (object tableval in tablerows[0])
			{
				tablename = System.Convert.ToString(tableval);
				columnrows = Connection.Execute("SELECT * FROM " + tablename);
				columns.Add(new Column(
					Convert.ToString(columnrows[0][0]),		// name
					Convert.ToString(columnrows[0][1]),		// type
					Convert.ToInt32(columnrows[0][2])		// max-length
				));
				ret.Add(new Table(tablename, columns.ToArray()));
			}
			return ret;
		}

		public ITypeConverter GetTypeConverterForTypeName(string db_type_name)
		{
			if (converters.ContainsKey(db_type_name))
				return converters[db_type_name];
			switch (db_type_name)
			{
				case "varchar":
					converters[db_type_name] = new TypeConverter(typeof(string));
					break;
			}
			if (converters.ContainsKey(db_type_name))
				return converters[db_type_name];
			else
				throw new KeyNotFoundException(string.Format(
					"Could not find converter for db type name '{0}'.",
					db_type_name
				));
		}
	}


	public class Backend
	{
		private static Dictionary<string, IBackend> backends;


		public static IBackend GetBackend()
		{
			string engine = System.Convert.ToString(Orm.Settings.Settings.Get("engine"));
			if (!backends.ContainsKey(engine))
			{
				switch (engine)
				{
					case "Jet":
						backends[engine] = new JetBackend();
						break;
				}
			}
			if (!backends.ContainsKey(engine))
				throw new BackendError("Unknown backend '" + engine + "'.");
			return backends[engine];
		}
	}


	public class TypeConverter : ITypeConverter
	{
		private Type db_type;


		public TypeConverter(Type db_type)
		{
			this.db_type = db_type;
		}

		public object ToCSharp(object val)
		{
			try
			{
				return System.Convert.ChangeType(val, db_type);
			}
			catch (InvalidCastException ex)
			{
				throw new TypeConversionError(string.Format(
					"Could not convert to type '{0}': {1}.", db_type, ex.Message));
			}
			catch (FormatException ex)
			{
				throw new TypeConversionError(string.Format(
					"Could not convert to type '{0}': {1}.", db_type, ex.Message));
			}
			catch (OverflowException ex)
			{
				throw new TypeConversionError(string.Format(
					"Could not convert to type '{0}': {1}.", db_type, ex.Message));
			}
			catch (ArgumentException ex)
			{
				throw new TypeConversionError(string.Format(
					"Could not convert to type '{0}': {1}.", db_type, ex.Message));
			}
		}
	}


	/// <summary>
	/// Represents a db table.
	/// </summary>
	public class Table
	{
		string name;
		Column[] columns;


		public Table(string name, Column[] columns)
		{
			this.name = name;
			this.columns = columns;
		}

		public Column GetColumn(string column_name)
		{
			foreach (Column col in columns)
			{
				if (col.Name == column_name)
				{
					return col;
				}
			}
			throw new KeyNotFoundException(string.Format(
				"Could not find column '{0}'.", column_name
			));
		}

		public string Name
		{
			get { return name; }
		}

		public Column[] Columns
		{
			get { return columns; }
		}
	}


	/// <summary>
	/// Represents a column in a db table.
	/// </summary>
	public class Column
	{
		public string name;
		public string db_type_name;
		public int max_length;

		public Column(string name, string db_type_name, int max_length = 0)
		{
			this.name = name;
			this.db_type_name = db_type_name;
			this.max_length = max_length;
		}

		public string Name
		{
			get { return name; }
		}

		public ITypeConverter TypeConverter
		{
			get { return Backend.GetBackend()
							.GetTypeConverterForTypeName(db_type_name); }
		}

	}	
}
