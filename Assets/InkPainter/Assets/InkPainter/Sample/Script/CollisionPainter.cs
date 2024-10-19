using UnityEngine;

namespace Es.InkPainter.Sample
{
	[RequireComponent(typeof(Collider), typeof(MeshRenderer))]
	public class CollisionPainter : MonoBehaviour
	{
		[SerializeField]
		private Brush brush = null;

		[SerializeField]
		private int wait = 3;

		private int waitCount;

		public void Awake()
		{
			GetComponent<MeshRenderer>().material.color = brush.Color;
		}

		public void FixedUpdate()
		{
			++waitCount;
		}

		public void OnCollisionEnter(Collision collision)
		{
		  if (waitCount < wait)
		    return;
		  waitCount = 0;
		  Debug.Log("Start Collision:");
		  Debug.Log(name);
		  Debug.Log(collision.gameObject.name);
		  foreach (var p in collision.contacts)
		  {
		    var canvas = p.otherCollider.GetComponent<InkCanvas>();
		    if (canvas != null)
		      canvas.Paint(brush, p.point);
		  }
		}

		// private void OnTriggerEnter(Collider other)
		// {
		// 	if (waitCount < wait)
		// 		return;
		// 	waitCount = 0;
		// 	var collisionPoint = other.ClosestPoint(transform.position);
		// 	var canvas = other.transform.GetComponent<InkCanvas>();
		// 	if (canvas != null)
		// 		canvas.Paint(brush, collisionPoint);

		// }

	}
}