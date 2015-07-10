﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nest.Resolvers.Converters;
using Newtonsoft.Json;
using Nest.Resolvers.Converters.Queries;

namespace Nest
{
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	[JsonConverter(typeof(DismaxQueryJsonConverter))]
	public interface IDisMaxQuery : IQuery
	{
		[JsonProperty(PropertyName = "tie_breaker")]
		double? TieBreaker { get; set; }

		[JsonProperty(PropertyName = "queries")]
		IEnumerable<QueryContainer> Queries { get; set; }
	}

	public class DismaxQuery : QueryBase, IDisMaxQuery
	{
		bool IQuery.Conditionless => IsConditionless(this);
		public double? TieBreaker { get; set; }
		public IEnumerable<QueryContainer> Queries { get; set; }

		protected override void WrapInContainer(IQueryContainer c) => c.DisMax = this;
		internal static bool IsConditionless(IDisMaxQuery q) => !q.Queries.HasAny() || q.Queries.All(qq => qq.IsConditionless);
	}

	public class DisMaxQueryDescriptor<T> 
		: QueryDescriptorBase<DisMaxQueryDescriptor<T>, IDisMaxQuery>
		, IDisMaxQuery where T : class
	{
		private IDisMaxQuery Self => this;
		bool IQuery.Conditionless => DismaxQuery.IsConditionless(this);
		double? IDisMaxQuery.TieBreaker { get; set; }
		IEnumerable<QueryContainer> IDisMaxQuery.Queries { get; set; }

		public DisMaxQueryDescriptor<T> Queries(params Func<QueryContainerDescriptor<T>, QueryContainer>[] querySelectors)
		{
			var queries = new List<QueryContainer>();
			foreach (var selector in querySelectors)
			{
				var query = new QueryContainerDescriptor<T>();
				var q = selector(query);
				queries.Add(q);
			}
			Self.Queries = queries;
			return this;
		}

		public DisMaxQueryDescriptor<T> Queries(params QueryContainer[] queries)
		{
			var descriptors = new List<QueryContainer>();
			foreach (var q in queries)
			{
				if (q.IsConditionless)
					continue;
				descriptors.Add(q);
			}
			((IDisMaxQuery)this).Queries = descriptors.HasAny() ? descriptors : null;
			return this;
		}

		public DisMaxQueryDescriptor<T> TieBreaker(double tieBreaker)
		{
			Self.TieBreaker = tieBreaker;
			return this;
		}
	}
}