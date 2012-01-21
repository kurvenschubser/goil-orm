using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Orm.Fields;
using Orm.Backends;


namespace Orm.Models
{
	public interface IModel
	{
		object Get(string field_name);
		void Set(string field_name, object val);
		void Delete(string field_name);
		ModelMeta Meta {get;}
	}


	public class ModelMeta
	{
		private string table_name;
		private List<IField> fields;


		public ModelMeta(string table_name, List<IField> fields)
		{
			this.table_name = table_name;
			this.fields = fields;
		}

		public ModelMeta(string table_name)
		{
			// set fields through db introspection
		}

		public IField GetField(string field_name)
		{
			return fields[GetFieldIndex(field_name)];
		}

		public int GetFieldIndex(string field_name)
		{
			try
			{
				return GetFieldIndexByCondition(f => f.DbField == field_name
											|| f.CodeField == field_name);
			}
			catch (ArgumentOutOfRangeException ex)
			{
				throw new KeyNotFoundException(string.Format(
					"Key '{0}' not in fields. {1}", field_name, ex.Message));
			}
		}

		public int GetFieldIndexByCondition(Func<IField, bool> predicate)
		{
			for (int i = 0; i < fields.Count; i++)
			{
				if (predicate(fields[i]))
				{
					return i;
				}
			}
			throw new ArgumentOutOfRangeException();
		}

		public Dictionary<string, object> PrepareRowForInstance(object[] row)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			int i = 0;
			foreach (IField field in Fields)
			{
				// XXX once relation fields are implemented, be sure to 
				// avoid setting any values for them.
				//if (!RelationFields.Contains(field))
				//{
					dict[field.DbField] = Table.GetColumn(field.DbField)
									.TypeConverter.ToCSharp(row[i++]);
				//}
			}
			return dict;
		}

		public List<IField> Fields
		{
			get { return fields; }
		}

		public IField PrimaryKey
		{
			get
			{
				try
				{
					return fields[GetFieldIndexByCondition(f => f.IsPrimaryKey)];
				}
				catch (ArgumentOutOfRangeException ex)
				{
					throw new KeyNotFoundException(string.Format(
						"Primary key not found: {1}", ex.Message
					));
				}
			}
		}

		public Table Table
		{
			get { return Backend.GetBackend().GetTable(table_name); }
		}
	}


	public class Model : IModel
	{
		public static ModelMeta __meta__;

		// instance data
		private Dictionary<IField, object> __data__;


		public Model(Dictionary<string, object> dict)
		{
			__data__ = new Dictionary<IField, object>();
			foreach (KeyValuePair<string, object> kv in dict)
			{
				this.Set(kv.Key, kv.Value);
			}
		}

		public Model() : this(new Dictionary<string, object>())
		{

		}

		public object this[string field_name]
		{
			get { return Get(field_name); }
			set { Set(field_name, value); }
		}

		public object Get(string field_name)
		{
			return __data__[__meta__.GetField(field_name)];
		}

		public void Set(string field_name, object val)
		{
			if (Meta == null)
				throw new Exception(this.GetType().GetMember("Meta"));

			__data__[Meta.GetField(field_name)] = val;
		}

		public void Delete(string field_name)
		{
			__data__.Remove(Meta.GetField(field_name));
		}

		public virtual ModelMeta Meta
		{
			get
			{
				return __meta__;
			}
		}

		public object Pk
		{
			get { return Get(Meta.PrimaryKey.DbField); }
			set { Set(Meta.PrimaryKey.DbField, value); }
		}
	}
}
