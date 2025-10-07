using UnityEngine;

// Holds a baked 360-angle sprite sheet and lookup helpers
public sealed class SpriteSheet360 : ScriptableObject
{
	[Header("Baked Texture and Frames")]
	public Texture2D texture;
	public Sprite[] frames;

	[Header("Layout")]
	public int frameWidth;
	public int frameHeight;
	public int columns;
	public int rows;

	[Header("Angles")] 
	[Tooltip("Total number of frames (angles). Typically 360 for 1 degree per frame.")]
	public int frameCount = 360;
	[Tooltip("Degrees per frame. 1 for 360, 2 for 180, etc.")]
	public int degreesPerFrame = 1;
	[Tooltip("Degrees yaw that corresponds to frame index 0. 0 = world +Z, positive clockwise when looking down Y.")]
	public int zeroDegreesFacing = 0;

	public Sprite GetSpriteForYaw(float yawDegrees)
	{
		if (frames == null || frames.Length == 0) return null;
		int index = GetFrameIndexForYaw(yawDegrees);
		index = Mathf.Clamp(index, 0, frames.Length - 1);
		return frames[index];
	}

	public int GetFrameIndexForYaw(float yawDegrees)
	{
		float normalized = Mathf.Repeat(yawDegrees - zeroDegreesFacing, 360f);
		int index = Mathf.RoundToInt(normalized / Mathf.Max(1, degreesPerFrame));
		if (frameCount > 0) index %= frameCount;
		return index;
	}
}


