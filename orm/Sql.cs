using System;
using System.Collections.Generic;

using Orm.Models;
using Orm.Datastructures;
using Orm.Filters;
using Orm.Relations;


namespace Orm.Sql
{
	using Pair = KeyValuePair<string, object>;


	public interface IQuery
	{
		List<Pair> GetData();
		string ToString();
	}


	public class SelectQuery: IQuery
	{
		Type source_model;
		Node<SelectLeaf> select_node;
		List<JoinLeaf> joins;
		Node<WhereLeaf> where_node;


		public SelectQuery(Type source_model)
		{
			this.source_model = source_model;
		}

		public void Filter(Node<WherePredicate> predicates, bool and=true)
		{
			Parser p = new Parser(source_model, select_node, joins, where_node);
			p.ParseWhereNode(predicates);
		}

		public override string ToString()
		{
			if (select_node.Children.Count > 0)
			{
				
			}
			else
			{
				if (select_node.Payload != null)
				{
					// uppermost node has one selector
				}
				else
				{
					// no selectors yet. set default selectors.
				}
			}
			return "SelectQuery";
		}

		public List<Pair> GetData()
		{
			return new List<Pair>();
		}

		public List<Pair> Data
		{
			get { return GetData(); }
		}

		public Type Model
		{
			get { return source_model; }
		}
	}


	public interface IRenderLeaf
	{
		string ToString();
	}


	public abstract class RenderLeaf : IRenderLeaf
	{
		public override string ToString()
		{
			throw new NotImplementedException();
		}
	}


	public class SelectLeaf : RenderLeaf
	{
		public override string ToString()
		{
			return "SelectLeaf";
		}
	}


	public class JoinLeaf : RenderLeaf
	{
		public Relation Relation;


		public JoinLeaf(Relation relation)
		{
			this.Relation = relation;
		}

		public override string ToString()
		{
			return "FromLeaf";
		}
	}


	public class WhereLeaf : RenderLeaf
	{
		WherePredicate pred;
		string db_field;
		string db_table;


		public WhereLeaf(WherePredicate pred, string db_field, string db_table)
		{
			this.pred = pred;
			this.db_field = db_field;
			this.db_table = db_table;
		}

		public override string ToString()
		{
			return "WhereLeaf";
		}
	}
}
