using System;
using System.Collections.Generic;

using Orm.Datastructures;
using Orm.Models;
using Orm.Sql;
using Orm.Relations;


namespace Orm.Filters
{
	public enum WhereFilter {
		Exact, 
		Startswith, 
		Endswith, 
		Contains, 
		Gt, 
		Ge, 
		Lt,
		Le, 
		In, 
		RegEx, 
		Between
	}


	public enum Aggregation
	{
		None, 
		Min,
		Max,
		Count,
		Avg,
		Sum
	}


	public class Parser
	{
		public char RELATION_SPLITTER = '.';

		private Type source_model;
		private Node<SelectLeaf> select_node;
		private List<JoinLeaf> joins;
		private Node<WhereLeaf> where_node;


		public Parser(Type source_model, Node<SelectLeaf> select_node,
						List<JoinLeaf> joins, Node<WhereLeaf> where_node)
		{
			this.source_model = source_model;
			this.select_node = select_node;
			this.joins = joins;
			this.where_node = where_node;
		}

		public string[] Lex(string s)
		{
			return s.Split(RELATION_SPLITTER);
		}

		public void ParseWhereNode(Node<WherePredicate> node)
		{

			//string[] tokens;
			//Relation[] relations;
			//Type current_model;
			//string db_field;
			//string db_table;

			Node<WherePredicate> current_node = node;
			List<int> path = new List<int>();
			while (true)
			{
				if (current_node.Payload != null)
				{
					ParsePredicate(current_node.Payload);
				}

				if (current_node.Children.Count > 0)
				{
					// move to next sub-level (depth-first traversal).
					path.Add(0);
				}
				else
				{
					// move up one level and to next child, if any
					path.RemoveAt(path.Count - 1);
					current_node = GetWhereNodeByPath<WherePredicate>(node, path);
					if (current_node.Children.Count - 1 > path[path.Count - 1])
					{
						path[path.Count - 1]++;
					}
				}
				current_node = GetWhereNodeByPath<WherePredicate>(node, path);
				if (current_node == node)
					break;
			}
		}

		protected void ParsePredicate(WherePredicate predicate)
		{
			predicate.DbField = Traverse(predicate.FieldsQuery);
		}

		protected string Traverse(string fields_query)
		{
			// currently, same model field lookup only, no automatic 
			// resolution of related models' fields.
			// IRelationField relation_field = source_model.GetMethod("GetField").Invoke(fieldname);
			
			return Lex(fields_query)[0];
		}

		private Node<P> GetWhereNodeByPath<P>(Node<P> node, List<int> path)
		{
			Node<P> current_node = node;
			int i = 0;
			while (i < path.Count)
			{
				if (current_node.Children.Count > 0)
				{
					current_node = current_node.Children[path[i++]];
				}
				else
				{
					throw new ArgumentOutOfRangeException();
				}
			}
			return current_node;
		}
	}


	/// <summary>
	/// Represents a where predicate.
	/// </summary>
	public class WherePredicate
	{
		WhereFilter filter;
		string fields_query;
		string db_field;
		string db_table;

		object val;
		bool case_sensitive;


		public WherePredicate(WhereFilter filter, string fields_query, object val, 
													bool case_sensitive=true)
		{
			this.filter = filter;
			this.fields_query = fields_query;
			this.val = val;
			this.case_sensitive = case_sensitive;
		}

		public WherePredicate Clone()
		{
			return new WherePredicate(this.filter, this.fields_query, this.val, 
														this.case_sensitive);
		}

		public string FieldsQuery
		{
			get { return fields_query; }
			set { fields_query = value; }
		}

		public string DbField
		{
			get { return db_field; }
			set { db_field = value; }
		}

		public string DbTable
		{
			get { return db_table; }
			set { db_table = value; }
		}

		public WhereFilter Filter
		{
			get { return filter; }
			set { filter = value; }
		}

		public object Value
		{
			get { return val; }
			set { val = value; }
		}
	}


	/// <summary>
	/// Factory class for WhereFilters.
	/// </summary>
	public class Filters
	{
		public static Node<WherePredicate> Exact(string fields, object val, bool case_sensitive=true)
		{
			return new Node<WherePredicate>(new WherePredicate(WhereFilter.Exact, fields, val, case_sensitive));
		}

		public static Node<WherePredicate> Contains(string fields, object val, bool case_sensitive = true)
		{
			return new Node<WherePredicate>(new WherePredicate(WhereFilter.Contains, fields, val, case_sensitive));
		}

		public static Node<WherePredicate> StartsWith(string fields, object val, bool case_sensitive = true)
		{
			return new Node<WherePredicate>(new WherePredicate(WhereFilter.Contains, fields, val, case_sensitive));
		}

		public static Node<WherePredicate> EndsWith(string fields, object val, bool case_sensitive = true)
		{
			return new Node<WherePredicate>(new WherePredicate(WhereFilter.Contains, fields, val, case_sensitive));
		}

		public static Node<WherePredicate> RegEx(string fields, object val, bool case_sensitive = true)
		{
			return new Node<WherePredicate>(new WherePredicate(WhereFilter.RegEx, fields, val, case_sensitive));
		}

		public static Node<WherePredicate> Gt(string fields, object val)
		{
			return new Node<WherePredicate>(new WherePredicate(WhereFilter.Contains, fields, val));
		}

		public static Node<WherePredicate> Ge(string fields, object val)
		{
			return new Node<WherePredicate>(new WherePredicate(WhereFilter.Contains, fields, val));
		}

		public static Node<WherePredicate> Lt(string fields, object val)
		{
			return new Node<WherePredicate>(new WherePredicate(WhereFilter.Contains, fields, val));
		}

		public static Node<WherePredicate> Le(string fields, object val)
		{
			return new Node<WherePredicate>(new WherePredicate(WhereFilter.Contains, fields, val));
		}

		public static Node<WherePredicate> Between(string fields, object val)
		{
			return new Node<WherePredicate>(new WherePredicate(WhereFilter.Contains, fields, val));
		}

		public static Node<WherePredicate> In(string fields, object val)
		{
			return new Node<WherePredicate>(new WherePredicate(WhereFilter.Contains, fields, val));
		}
	}


	public class SelectPredicate
	{
		Aggregation aggregation;
		string field;


		public SelectPredicate(Aggregation aggregation, string field)
		{
			this.aggregation = aggregation;
			this.field = field;
		}
	}


	public class Aggregators
	{
		public static Node<SelectPredicate> Select(string field)
		{
			return new Node<SelectPredicate>(new SelectPredicate(Aggregation.None, field));
		}

		public static Node<SelectPredicate> Min(string field)
		{
			return new Node<SelectPredicate>(new SelectPredicate(Aggregation.Min, field));
		}

		public static Node<SelectPredicate> Max(string field)
		{
			return new Node<SelectPredicate>(new SelectPredicate(Aggregation.Max, field));
		}

		public static Node<SelectPredicate> Count(string field)
		{
			return new Node<SelectPredicate>(new SelectPredicate(Aggregation.Count, field));
		}

		public static Node<SelectPredicate> Avg(string field)
		{
			return new Node<SelectPredicate>(new SelectPredicate(Aggregation.Avg, field));
		}

		public static Node<SelectPredicate> Sum(string field)
		{
			return new Node<SelectPredicate>(new SelectPredicate(Aggregation.Sum, field));
		}
	}


	public class Join
	{
		Relation relation;


		public Join(Relation relation)
		{
			this.relation = relation;
		}
	}
}
