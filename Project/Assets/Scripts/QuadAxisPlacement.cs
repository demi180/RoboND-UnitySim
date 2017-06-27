using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuadAxisPlacement : MonoBehaviour
{
	public QuadController quad;
	public FollowCamera cam;

	public Image[] arrows;
	public RectTransform[] arrowParents;
	public Vector2 arrowSize = new Vector2 ( 200, 150 );
	public Vector2 circleSize = new Vector2 ( 100, 100 );

	void Start ()
	{
		quad = QuadController.ActiveController;
		cam = FollowCamera.ActiveCamera;
	}

	void LateUpdate ()
	{
		Vector3 up = quad.Up * 0.5f;
		Vector3 fwd = -cam.transform.forward;
		arrowParents [ 0 ].position = quad.Position + up;// + quad.XAxis * 2;
		arrowParents [ 1 ].position = quad.Position + up;// + quad.YAxis * 2;
		arrowParents [ 2 ].position = quad.Position + up;// * 1.5f;
		arrowParents [ 0 ].rotation = Quaternion.LookRotation ( quad.XAxis, fwd );
//		arrowParents [ 0 ].rotation = quad.Rotation;
		arrowParents [ 1 ].rotation = Quaternion.LookRotation ( quad.YAxis, fwd );
		arrowParents [ 2 ].rotation = Quaternion.LookRotation ( quad.Up, fwd );



/*		// position the arrows
//		Vector3 up = quad.Up * 0.5f;
		arrows [ 0 ].rectTransform.position = quad.Position + up + quad.XAxis * 2;// + quad.Forward * 2;
		arrows [ 1 ].rectTransform.position = quad.Position + up + quad.YAxis * 2;// - quad.Right * 2;
		arrows [ 2 ].rectTransform.position = quad.Position + up * 1.5f;// + quad.Up * 2;
		arrows [ 0 ].rectTransform.rotation = Quaternion.LookRotation ( -quad.Up, quad.YAxis );
		arrows [ 1 ].rectTransform.rotation = Quaternion.LookRotation ( -quad.Up, -quad.XAxis );
		arrows [ 2 ].rectTransform.rotation = Quaternion.LookRotation ( quad.forward.forward, -quad.right.forward );

		// size the arrows
		float sizeMult = Mathf.InverseLerp ( 2, 20, cam.followDistance );
		arrows [ 0 ].rectTransform.sizeDelta = arrows [ 1 ].rectTransform.sizeDelta = arrows [ 2 ].rectTransform.sizeDelta = arrowSize;// * sizeMult;
		arrows [ 3 ].rectTransform.sizeDelta = arrows [ 4 ].rectTransform.sizeDelta = arrows [ 5 ].rectTransform.sizeDelta = circleSize;// * sizeMult;*/

		// make sure they're enabled correctly
		arrows [ 0 ].enabled = !quad.ConstrainForceX;
		arrows [ 1 ].enabled = !quad.ConstrainForceY;
		arrows [ 2 ].enabled = !quad.ConstrainForceZ;
		arrows [ 3 ].enabled = !quad.ConstrainTorqueX;
		arrows [ 4 ].enabled = !quad.ConstrainTorqueY;
		arrows [ 5 ].enabled = !quad.ConstrainTorqueZ;
	}
}