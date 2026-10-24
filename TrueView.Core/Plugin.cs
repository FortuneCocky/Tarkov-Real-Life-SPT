using BepInEx;
using TrueView.Configuration;
using TrueView.Patches;
using TrueView.Systems;
using TrueView.Utilities;
using UnityEngine;

namespace TrueView.Core;

[BepInPlugin("com.trueview", "TrueView", "2.1.1")]
public class Plugin : BaseUnityPlugin
{
	public const string GUID = "com.trueview";

	public const string Name = "TrueView";

	public const string Version = "2.1.1";

	private void Awake()
	{
		Log.Init(base.Logger);
		HandlingConfig.Init(base.Config);
		PatchRegistry.EnableAll();
		Log.Info("TrueView 2.0.0 loaded");
	}

	private void OnGUI()
	{
		if (HandlingConfig.DeadzoneDebugOverlay?.Value != true || !HandlingState.Enabled ||
			!HandlingState.HasWeapon || !ProceduralHipSystem.HasPose)
			return;

		float fieldOfView = Camera.main != null ? Camera.main.fieldOfView : 60f;
		float focalLength = Screen.height / (2f * Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad));
		float zone = HandlingState.IsAiming
			? HandlingConfig.DeadzoneAds.Value
			: HandlingConfig.HipCameraFollowZone.Value;
		zone = ProceduralHipSystem.EffectiveZoneDegrees(zone);
		float radius = Mathf.Tan(zone * Mathf.Deg2Rad) * focalLength;
		Vector2 center = new(Screen.width * 0.5f, Screen.height * 0.5f);
		Color oldColor = GUI.color;
		Matrix4x4 oldMatrix = GUI.matrix;

		for (int i = 0; i < 72; i++)
		{
			float a = i * Mathf.PI * 2f / 72f;
			float b = (i + 1) * Mathf.PI * 2f / 72f;
			Vector2 directionA = new(Mathf.Cos(a), Mathf.Sin(a));
			Vector2 directionB = new(Mathf.Cos(b), Mathf.Sin(b));
			DrawLine(center + directionA * radius, center + directionB * radius, Color.white, 2f);
		}

		if (ProceduralHipSystem.HasDebugScreenPoint && !ProceduralHipSystem.IsZoneTransitionActive)
		{
			Vector2 dot = ProceduralHipSystem.DebugScreenPoint;
			GUI.color = Color.cyan;
			GUI.DrawTexture(new Rect(dot.x - 4f, dot.y - 4f, 8f, 8f), Texture2D.whiteTexture);
		}
		GUI.matrix = oldMatrix;
		GUI.color = oldColor;
	}

	private static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
	{
		Vector2 delta = end - start;
		float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
		Matrix4x4 matrix = GUI.matrix;
		Color oldColor = GUI.color;
		GUI.color = color;
		GUIUtility.RotateAroundPivot(angle, start);
		GUI.DrawTexture(new Rect(start.x, start.y - width * 0.5f, delta.magnitude, width), Texture2D.whiteTexture);
		GUI.matrix = matrix;
		GUI.color = oldColor;
	}
}
