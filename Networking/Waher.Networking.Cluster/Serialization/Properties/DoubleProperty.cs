﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Double precision floating-point property
	/// </summary>
	public class DoubleProperty : Property
	{
		/// <summary>
		/// Double precision floating-point property
		/// </summary>
		/// <param name="PI">Property information</param>
		public DoubleProperty(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(double);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteDouble((double)this.pi.GetValue(Object));
		}
	}
}
