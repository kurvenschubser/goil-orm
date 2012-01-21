using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Orm.Fields
{
	public interface IField
	{
		string DbField { get; }
		string CodeField { get; }
		bool IsPrimaryKey { get; }
	}


	public class Field : IField
	{
		private string db_field;
		private string code_field;
		private bool is_primary_key;


		public Field(string db_field, string code_field = null, bool is_pk = false)
		{
			this.db_field = db_field;
			this.code_field = code_field;
			this.is_primary_key = is_pk;
		}

		public string DbField
		{
			get { return db_field; }
		}

		public string CodeField
		{
			get 
			{
				if (this.code_field == null)
					return db_field;
				return code_field; 
			}
		}

		public bool IsPrimaryKey
		{
			get { return is_primary_key; }
		}
	}


	public class IntegerField : Field
	{
		public IntegerField(string db_field, string code_field=null, bool is_pk=false)
			: base(db_field, code_field, is_pk)
		{}
	}


	public class LongField : Field
	{
		public LongField(string db_field, string code_field = null, bool is_pk = false)
			: base(db_field, code_field, is_pk)
		{}
	}


	public class StringField : Field
	{
		public StringField(string db_field, string code_field = null, bool is_pk = false)
			: base(db_field, code_field, is_pk)
		{}
	}


	public class FloatField : Field
	{
		public FloatField(string db_field, string code_field = null, bool is_pk = false)
			: base(db_field, code_field, is_pk)
		{}
	}
}
