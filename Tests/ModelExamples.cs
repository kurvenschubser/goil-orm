using System.Collections.Generic;

using Orm.Models;
using Orm.Fields;


namespace Orm.Tests
{
	public class Egg : Model
	{
		public new ModelMeta Meta
		{
			get
			{
				if (__meta__ == null)
				{
					__meta__ = new ModelMeta(
						"eggs",
						new List<IField>(new Field[] {
							new IntegerField("id", is_pk: true),
							new StringField("serial"),
							new FloatField("weight")
					}));
				}
				return __meta__;
			}
		}
	}


	public class Car : Model
	{
		//private static ModelMeta __meta__ = new ModelMeta(
		//    "cars",
		//    new List<IField>(new Field[] { 
		//        new IntegerField("id", is_pk: true),
		//        new StringField("brand"),
		//        new FloatField("tank_fill_level", "gas")
		//}));

		private static new ModelMeta __meta__;
		public new ModelMeta Meta
		{
			get
			{
				if (__meta__ == null)
				{
					__meta__ = new ModelMeta(
						"cars",
						new List<IField>(new Field[] {
								new IntegerField("id", is_pk: true),
								new StringField("brand"),
								new FloatField("tank_fill_level", "gas")
							}
						)
					);
				}
				return __meta__;
			}
		}

		public Car(Dictionary<string, object> dict) : base(dict) { }
	}
}