using System;
using System.Collections.Generic;


namespace Orm.Datastructures
{
	public class Node<T>
	{
		List<Node<T>> children;
		public T payload;
		public bool not;
		public bool and;		// and == true => AND; and == false => OR


		public Node(bool and=true, bool not=false)
		{
			this.and = and;
			this.not = not;
		}

		public Node(T payload, bool and=true, bool not=false)
			: this(and, not)
		{
			this.payload = payload;
		}

		public static implicit operator bool(Node<T> self)
		{
			return self.Children.Count > 0;
		}

		public static Node<T> operator &(Node<T> self, Node<T> other)
		{
			self.AddNode(other, true);
			return self;
		}

		public static Node<T> operator |(Node<T> self, Node<T> other)
		{
			self.AddNode(other, false);
			return self;
		}

		public static Node<T> operator ~(Node<T> self)
		{
			self.not = !self.not;
			return self;
		}

		public static Node<T> operator ^(Node<T> self, Node<T> other)
		{
			return ((self & ~other) | (~self & other));
		}

		/// <summary>
		/// Add a node to this node. Constructs a nested tree structure
		/// if nodes are not of the same type with (AND, OR) being the 
		/// possible choices. To represent AND pass in parameter *and*
		/// as true; for OR pass in false.
		/// </summary>
		/// <param name="n">
		/// A Node instance that is to be added to this Node.
		/// </param>
		/// <param name="and">Pass in true to make the resultant Node an AND
		/// Node or false to make it an OR Node.</param>
		public void AddNode(Node<T> n, bool and)
		{
			if (and)
			{
				if (this.and == n.and)
				{
					if (this)
						Children.Add(n);
					else
						Children.AddRange(new Node<T>[] { this, n });
				}
				else if (this.and && !n.and)
				{

				}
				else if (!this.and && n.and)
				{

				}
			}
			else
			{

			}
		}

		public Node<T> Clone()
		{
			Node<T> n = new Node<T>(this.and, this.not);
			foreach (Node<T> child in this.Children)
				n.children.Add(child.Clone());
			return n;
		}

		public List<Node<T>> Children
		{
			get
			{
				if (children == null)
					children = new List<Node<T>>();
				return children;
			}
		}

		public T Payload
		{
			get { return payload; }
			set { payload = value; }
		}
	}
}
