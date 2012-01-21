using System;
using System.Diagnostics;

using Orm.Models;


namespace Orm.Relations
{
	public enum RelationType { O2O, O2M, M2O, M2M }


	public class Relation
	{
		Type left_model;
		Type right_model;

		// relation in direction from left_model to right_model
		RelationType relation_type;

		string left_identifier;
		string right_identifier;


		public Relation(Type left_model, Type right_model, 
			RelationType relation_type, string left_identifier,
										string right_identifier)
		{
			this.left_model = left_model;
			this.right_model = right_model;
			this.relation_type = relation_type;
			this.left_identifier = left_identifier;
			this.right_identifier = right_identifier;
		}

		public Type GetModelByIdentifier(string ident)
		{
			Debug.Assert(ident == left_identifier || ident == right_identifier);

			return ident == left_identifier ? left_model : right_model;
		}
	}
}