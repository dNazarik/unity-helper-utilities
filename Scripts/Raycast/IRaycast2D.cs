using UnityEngine;

namespace Hutilities.Raycast
{
	public interface IRaycast2D
	{
		RaycastHit2D PerformRaycast(Vector3 origin, Vector3 direction, int layerMask);
	}
}
