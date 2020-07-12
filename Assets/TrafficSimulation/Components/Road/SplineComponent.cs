using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TrafficSimulation.Components.Road
{
	[Serializable]
	public struct SplineComponent : IComponentData
	{
		public float3 a;
		public float3 b;
		public float3 c;
		public float3 d;
		public float Length;

		public float3 Tangent(float t)
		{
			float t1 = 1 - t;
			float t12 = t1 * t1;
			return -3 * t12 * a
			       + (3 * t12 - 6 * t1 * t) * b
			       + (-3 * t * t + 6 * t * t1) * c
			       + 3 * t * t * d;
		}

		public float3 Point(float t)
		{
			float t1 = 1 - t;
			float t12 = t1 * t1;
			float t13 = t1 * t1 * t1;
			return t13 * a
			       + 3 * t12 * t * b
			       + 3 * t1 * t * t * c
			       + t * t * t * d;
		}

		private float RoughLength()
		{
			return math.length(d - a);
		}

		public float TotalLength()
		{
			int roughLen = (int)math.ceil(RoughLength());
			float totalLen = 0;
			var lastPos = Point(0f);
			for (float i = 0.5f; i <= roughLen; i+=0.5f)
			{
				var curPos = Point((float) i / roughLen);
				totalLen += math.length(curPos - lastPos);
				lastPos = curPos;
			}

			return totalLen;
		}

		public static SplineComponent CreateSpline(Transform startNode, Transform endNode, float curveIn)
		{
			SplineComponent ret = new SplineComponent
			{
				a = startNode.position,
				b = startNode.position + startNode.forward * curveIn,
				c = endNode.position - endNode.forward * curveIn,
				d = endNode.position
			};
			ret.Length = ret.TotalLength();
			return ret;
		}
	}
}