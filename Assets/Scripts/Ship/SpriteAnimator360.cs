using UnityEngine;

// Selects the appropriate sprite from a 360-sheet based on current yaw (around Y axis)
[RequireComponent(typeof(SpriteRenderer))]
public sealed class SpriteAnimator360 : MonoBehaviour
{
	public SpriteSheet360 sheet;
	[Tooltip("Optional override. If null, this transform is used.")]
	public Transform directionSource;
	[Tooltip("When enabled, this object will billboard to the main camera around Y only.")]
	public bool billboardToCamera = true;

	private SpriteRenderer _renderer;

	private void Awake()
	{
		_renderer = GetComponent<SpriteRenderer>();
	}

	private void LateUpdate()
	{
		if (sheet == null || sheet.frames == null || sheet.frames.Length == 0) return;

		Transform src = directionSource != null ? directionSource : transform;
		Vector3 forward = src.forward;
		forward.y = 0f;
		if (forward.sqrMagnitude < 0.0001f)
		{
			// fallback: keep current sprite
			return;
		}
		forward.Normalize();

		// Yaw: 0 degrees = +Z, right-handed
		float yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
		var sprite = sheet.GetSpriteForYaw(yaw);
		if (sprite != null) _renderer.sprite = sprite;

		if (billboardToCamera && Camera.main != null)
		{
			var camForward = Camera.main.transform.forward;
			camForward.y = 0f;
			if (camForward.sqrMagnitude > 0.0001f)
			{
				camForward.Normalize();
				transform.rotation = Quaternion.LookRotation(camForward, Vector3.up);
			}
		}
	}
}


