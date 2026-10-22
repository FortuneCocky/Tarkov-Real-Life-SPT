using BepInEx.Configuration;

namespace IRLWeaponHandling.Configuration;

internal sealed class ConfigurationManagerAttributes
{
	public int? Order { get; set; }
	public bool? IsAdvanced { get; set; }
}

internal static class HandlingConfig
{
	private static ConfigDescription Toggle(string description) => new(description, null, new ConfigurationManagerAttributes { Order = 1000 });
	private const string GeneralSection = "01 - General";

	private const string AimingSection = "05 - Aiming Speed & Weight";

	private const string SwaySection = "06 - Weapon Sway & Inertia";

	private const string DeadzoneSection = "04 - ADS Weapon Handling";

	private const string ShotSection = "07 - Recoil & Shot Response";

	private const string CameraSection = "08 - Camera Sway";

	private const string LeanSection = "13 - Turn Lean";

	private const string MomentumSection = "12 - Body Momentum";

	private const string FluidSection = "16 - Fluid Movement";

	public static ConfigEntry<bool> Enabled { get; private set; }

	public static ConfigEntry<bool> VerboseLogging { get; private set; }

	public static ConfigEntry<bool> SprintCancelAds { get; private set; }

	public static ConfigEntry<float> FreeLookReturnSpeed { get; private set; }

	public static ConfigEntry<bool> AimingEnabled { get; private set; }

	public static ConfigEntry<float> LightWeightKg { get; private set; }

	public static ConfigEntry<float> HeavyWeightKg { get; private set; }

	public static ConfigEntry<float> AdsTimeLight { get; private set; }

	public static ConfigEntry<float> AdsTimeHeavy { get; private set; }

	public static ConfigEntry<float> ErgonomicsInfluence { get; private set; }

	public static ConfigEntry<float> StaminaAdsInfluence { get; private set; }

	public static ConfigEntry<bool> SwayEnabled { get; private set; }

	public static ConfigEntry<float> SwayMultiplier { get; private set; }

	public static ConfigEntry<float> HipSwayMultiplier { get; private set; }

	public static ConfigEntry<float> AimSwayMultiplier { get; private set; }

	public static ConfigEntry<float> MotionInertiaMultiplier { get; private set; }

	public static ConfigEntry<float> StaminaSwayInfluence { get; private set; }

	public static ConfigEntry<float> TremorMultiplier { get; private set; }

	public static ConfigEntry<bool> DeadzoneEnabled { get; private set; }

	public static ConfigEntry<float> DeadzoneAds { get; private set; }

	public static ConfigEntry<float> AdsFollowTightness { get; private set; }

	public static ConfigEntry<bool> HipDeadzoneEnabled { get; private set; }

	public static ConfigEntry<float> HipCameraFollowZone { get; private set; }

	public static ConfigEntry<bool> DeadzoneDebugOverlay { get; private set; }

	public static ConfigEntry<float> HipDeadzoneInfluence { get; private set; }
	public static ConfigEntry<float> HipDeadzoneSmoothing { get; private set; }

	public static ConfigEntry<float> HipNaturalMotion { get; private set; }

	public static ConfigEntry<float> HipOrganicDrift { get; private set; }

	public static ConfigEntry<float> HipMouseWhip { get; private set; }

	public static ConfigEntry<float> HipDirectionalTilt { get; private set; }

	// Procedural arm/wrist motion layers for hip fire
	public static ConfigEntry<float> HipStrafeRoll { get; private set; }
	public static ConfigEntry<float> HipAccelSway { get; private set; }
	public static ConfigEntry<float> HipWristRoll { get; private set; }
	public static ConfigEntry<float> HipBodyTurnLead { get; private set; }

	public static ConfigEntry<bool> ShotMisalignmentEnabled { get; private set; }

	public static ConfigEntry<float> ShotMisalignmentStrength { get; private set; }

	public static ConfigEntry<float> ShotMisalignmentMax { get; private set; }

	public static ConfigEntry<float> ShotBreathPenalty { get; private set; }

	public static ConfigEntry<float> StaminaShotInfluence { get; private set; }

	public static ConfigEntry<bool> CameraLagEnabled { get; private set; }

	public static ConfigEntry<float> CameraSwayFollow { get; private set; }

	public static ConfigEntry<float> CameraSwayFollowSpeed { get; private set; }

	public static ConfigEntry<float> CameraSettleSmoothing { get; private set; }

	public static ConfigEntry<bool> LeanEnabled { get; private set; }

	public static ConfigEntry<float> LeanCameraAngle { get; private set; }

	public static ConfigEntry<float> LeanWeaponAngle { get; private set; }

	public static ConfigEntry<float> LeanMultiplier { get; private set; }

	// Procedural Q/E lean: extra roll from the vanilla lean input
	public static ConfigEntry<bool> ProceduralLeanEnabled { get; private set; }
	public static ConfigEntry<float> ProceduralLeanRoll { get; private set; }
	public static ConfigEntry<float> ProceduralLeanWeaponRoll { get; private set; }
	public static ConfigEntry<float> ProceduralLeanSmooth { get; private set; }

	public static ConfigEntry<bool> GaitEnabled { get; private set; }

	public static ConfigEntry<float> GaitIntensity { get; private set; }

	public static ConfigEntry<float> GaitTransitionSpeed { get; private set; }

	public static ConfigEntry<bool> InjuryHandlingEnabled { get; private set; }

	public static ConfigEntry<float> InjuryHandlingStrength { get; private set; }

	public static ConfigEntry<bool> InjuryPainkillerMasking { get; private set; }

	public static ConfigEntry<bool> MomentumEnabled { get; private set; }

	public static ConfigEntry<float> MomentumAmount { get; private set; }

	public static ConfigEntry<float> MomentumWeight { get; private set; }

	public static ConfigEntry<float> MomentumTurn { get; private set; }

	public static ConfigEntry<float> MomentumStopDelay { get; private set; }

	public static ConfigEntry<float> MomentumWalkForwardLean { get; private set; }

	public static ConfigEntry<float> MomentumWalkSideLean { get; private set; }

	// Fluid Movement — landing dip, crouch dip
	public static ConfigEntry<bool> HeadAttachEnabled { get; private set; }
	public static ConfigEntry<float> HeadAttachPosition { get; private set; }
	public static ConfigEntry<float> HeadAttachRotation { get; private set; }
	public static ConfigEntry<float> HeadEyeStabilization { get; private set; }
	public static ConfigEntry<float> HeadDirectionLag { get; private set; }
	public static ConfigEntry<float> HeadWeaponCoupling { get; private set; }
	public static ConfigEntry<float> HeadShotShake { get; private set; }
	public static ConfigEntry<float> HeadShotShakeRecovery { get; private set; }
	public static ConfigEntry<bool> FluidMovementEnabled { get; private set; }
	public static ConfigEntry<float> FluidAimReduction { get; private set; }
	public static ConfigEntry<float> FluidLandingDip { get; private set; }
	public static ConfigEntry<float> FluidLandingRecovery { get; private set; }
	public static ConfigEntry<float> FluidCrouchDip { get; private set; }
	public static ConfigEntry<float> FluidCrouchSpeed { get; private set; }

	public static ConfigEntry<bool> TurnInPlaceEnabled { get; private set; }
	public static ConfigEntry<float> TurnInPlaceThreshold { get; private set; }
	public static ConfigEntry<float> TurnInPlaceIntensity { get; private set; }

	public static ConfigEntry<bool> HitReactionEnabled { get; private set; }
	public static ConfigEntry<float> HitReactionStrength { get; private set; }
	public static ConfigEntry<bool> MuzzleJoltEnabled { get; private set; }
	public static ConfigEntry<float> MuzzleJoltAmount { get; private set; }
	public static ConfigEntry<bool> DynamicFovEnabled { get; private set; }
	public static ConfigEntry<float> DynamicFovAds { get; private set; }

	public static ConfigEntry<bool> LazyPhysicsEnabled { get; private set; }
	public static ConfigEntry<float> LazySpringiness { get; private set; }
	public static ConfigEntry<float> LazyTension { get; private set; }

	public static ConfigEntry<bool> AdrenalineEnabled { get; private set; }
	public static ConfigEntry<float> AdrenalineRadius { get; private set; }
	public static ConfigEntry<float> AdrenalineTremor { get; private set; }
	public static ConfigEntry<float> AdrenalineFovPinch { get; private set; }
	public static ConfigEntry<float> AdrenalineHeartbeat { get; private set; }
	public static ConfigEntry<float> AdrenalineDecay { get; private set; }

	public static void Init(ConfigFile config)
	{
		Enabled = config.Bind("01 - General", "Enable mod", true, Toggle("Master switch. When off, all handling systems leave the vanilla values untouched."));
		VerboseLogging = config.Bind("01 - General", "Verbose logging", false, "Log recalculated weapon handling values to the BepInEx console.");
		SprintCancelAds = config.Bind("01 - General", "Cancel sprint to ADS on right click", true, "Pressing right mouse while sprinting cancels sprint and immediately aims down sights.");
		FreeLookReturnSpeed = config.Bind("01 - General", "Free look return speed", 12f, new ConfigDescription("How fast the camera smoothly returns to forward view after exiting free look. Higher = snappier, lower = smoother. 0 = instant snap.", new AcceptableValueRange<float>(0f, 30f)));
		HeadAttachEnabled = config.Bind("02 - Head Camera", "Enable head camera", true, Toggle("Attaches the camera to the character's head so the view follows body motion (steps, crouching, leaning, landing)."));
		HeadAttachPosition = config.Bind("02 - Head Camera", "Position follow", 0.08f, new ConfigDescription("How much the camera follows the head bone's position. 0 = off, 0.05 = natural, 0.5 = exaggerated.", new AcceptableValueRange<float>(0f, 0.5f)));
		HeadAttachRotation = config.Bind("02 - Head Camera", "Rotation follow", 0.05183098f, new ConfigDescription("How much the camera tilts with the head bone's pitch and roll. Yaw is never affected so aiming stays accurate. 0 = off, 0.05 = natural, 0.5 = exaggerated.", new AcceptableValueRange<float>(0f, 0.5f)));
		HeadEyeStabilization = config.Bind("02 - Head Camera", "Eye stabilization", 0.7f, new ConfigDescription("How strongly the eyes smooth rapid head-bone movement while preserving slower body motion. 0 = raw head motion, 1 = maximum stabilization.", new AcceptableValueRange<float>(0f, 1f)));
		HeadDirectionLag = config.Bind("02 - Head Camera", "Direction change lag", 0.55f, new ConfigDescription("Physical head lag and follow-through during rapid turns and direction changes. 0 = off, 1 = strong.", new AcceptableValueRange<float>(0f, 1f)));
		HeadWeaponCoupling = config.Bind("02 - Head Camera", "Weapon to head coupling", 1f, new ConfigDescription("How much shouldered weapon movement transfers into the head camera. Stronger in ADS. 0 = independent, 1 = strongly coupled.", new AcceptableValueRange<float>(0f, 1f)));
		HeadShotShake = config.Bind("02 - Head Camera", "Shot shake amount", 1.3f, new ConfigDescription("Smooth head-mounted camera kick from firing. 0 = off, 1 = natural, 2 = strong.", new AcceptableValueRange<float>(0f, 2f)));
		HeadShotShakeRecovery = config.Bind("02 - Head Camera", "Shot shake recovery", 10f, new ConfigDescription("How quickly firing shake settles. Lower = heavier and floatier, higher = tighter.", new AcceptableValueRange<float>(2f, 20f)));
		HipDeadzoneEnabled = config.Bind("03 - Hip Weapon Handling", "Enable hip deadzone", true, Toggle("Bodycam-style weapon handling. The camera follows player input while the weighted weapon trails behind it."));
		HipCameraFollowZone = config.Bind("03 - Hip Weapon Handling", "Weapon lag limit (deg)", 12f, new ConfigDescription("Maximum angle the weapon can trail behind the camera during hip fire.", new AcceptableValueRange<float>(1f, 30f)));
		DeadzoneDebugOverlay = config.Bind("03 - Hip Weapon Handling", "Show deadzone debug overlay", false, "Draw the maximum weapon-lag boundary and EFT's real adjusted firing-direction dot.");
		HipDeadzoneInfluence = config.Bind("03 - Hip Weapon Handling", "Deadzone influence", 0.4507042f, new ConfigDescription("How much player movement/turning adds procedural drift to the weapon. 0 = none, 1 = maximum sway.", new AcceptableValueRange<float>(0f, 1f)));
		HipDeadzoneSmoothing = config.Bind("03 - Hip Weapon Handling", "Weapon follow speed", 0.4f, new ConfigDescription("How quickly the weighted weapon catches up with the player-driven camera. 0 = heavy/slow, 1 = responsive/fast.", new AcceptableValueRange<float>(0f, 1f)));
		HipNaturalMotion = config.Bind("03 - Hip Weapon Handling", "Natural motion", 1.1f, new ConfigDescription("Overall strength of human-like weapon motion at hip. Adds organic drift, mouse whip and directional tilt for an immersive feel. 0 = off, 1 = natural, higher = more pronounced.", new AcceptableValueRange<float>(0f, 3f)));
		HipOrganicDrift = config.Bind("03 - Hip Weapon Handling", "Organic drift", 0.4f, new ConfigDescription("Perlin-noise micro-drift simulating hands holding a weapon. 0 = robotic/still, 1 = subtle human tremor, 2 = heavy drift.", new AcceptableValueRange<float>(0f, 2f)));
		HipMouseWhip = config.Bind("03 - Hip Weapon Handling", "Mouse whip", 1f, new ConfigDescription("How much the weapon whips and lags behind fast mouse movements, then springs back. 0 = rigid, 1 = natural, 2 = heavy lag.", new AcceptableValueRange<float>(0f, 3f)));
		HipDirectionalTilt = config.Bind("03 - Hip Weapon Handling", "Directional tilt", 0.9f, new ConfigDescription("Weapon roll/tilt in the direction of mouse motion, like wrists flexing. 0 = none, 1 = subtle, 2 = pronounced.", new AcceptableValueRange<float>(0f, 3f)));
		HipStrafeRoll = config.Bind("03 - Hip Weapon Handling", "Strafe wrist roll", 1f, new ConfigDescription("Weapon rolls (wrist rotation) when strafing left/right, as if the arms swing with body momentum. 0 = off, 1 = natural, 2 = pronounced.", new AcceptableValueRange<float>(0f, 3f)));
		HipAccelSway = config.Bind("03 - Hip Weapon Handling", "Acceleration sway", 1.6f, new ConfigDescription("Weapon pitches forward/back when starting or stopping movement, simulating arm inertia. 0 = off, 1 = natural, 2 = heavy.", new AcceptableValueRange<float>(0f, 3f)));
		HipWristRoll = config.Bind("03 - Hip Weapon Handling", "Idle wrist roll", 0.7f, new ConfigDescription("Subtle random wrist roll while holding the weapon at hip, simulating micro-adjustments of the grip. 0 = off, 1 = noticeable.", new AcceptableValueRange<float>(0f, 2f)));
		HipBodyTurnLead = config.Bind("03 - Hip Weapon Handling", "Body turn lead", 1.760563f, new ConfigDescription("Wrist-turn lead: when you move the mouse, the muzzle swings ahead of the turn direction and the gun banks like your wrist is twisting, then springs back to neutral. 0 = off, 1 = natural, 3 = dramatic.", new AcceptableValueRange<float>(0f, 3f)));
		DeadzoneEnabled = config.Bind("04 - ADS Weapon Handling", "Enable ADS deadzone", true, Toggle("Bodycam-style ADS weapon handling. The camera stays player-driven while the weapon trails toward the selected sight."));
		DeadzoneAds = config.Bind("04 - ADS Weapon Handling", "ADS lag limit (deg)", 12f, new ConfigDescription("Maximum weapon-to-camera separation while aiming down sights.", new AcceptableValueRange<float>(0f, 12f)));
		AdsFollowTightness = config.Bind("04 - ADS Weapon Handling", "ADS follow tightness", 0.08685446f, new ConfigDescription("How tightly the aimed weapon follows the camera. 0 = heavy delayed catch-up, 1 = complete immediate follow. Weapon weight, ergonomics, movement and fatigue are calculated automatically.", new AcceptableValueRange<float>(0f, 1f)));
		AimingEnabled = config.Bind("05 - Aiming Speed & Weight", "Enable aiming changes", true, Toggle("Scale ADS speed by weapon weight, ergonomics and hands stamina."));
		LightWeightKg = config.Bind("05 - Aiming Speed & Weight", "Light weapon weight (kg)", 2f, new ConfigDescription("Weight treated as a light weapon.", new AcceptableValueRange<float>(0.5f, 8f)));
		HeavyWeightKg = config.Bind("05 - Aiming Speed & Weight", "Heavy weapon weight (kg)", 10f, new ConfigDescription("Weight treated as a heavy weapon.", new AcceptableValueRange<float>(3f, 20f)));
		AdsTimeLight = config.Bind("05 - Aiming Speed & Weight", "ADS time, light weapon (s)", 1f, new ConfigDescription("Time to fully aim a light, high ergonomics weapon.", new AcceptableValueRange<float>(0.05f, 2f)));
		AdsTimeHeavy = config.Bind("05 - Aiming Speed & Weight", "ADS time, heavy weapon (s)", 1f, new ConfigDescription("Time to fully aim a heavy, high ergonomics weapon.", new AcceptableValueRange<float>(0.1f, 3f)));
		ErgonomicsInfluence = config.Bind("05 - Aiming Speed & Weight", "Ergonomics influence", 1f, new ConfigDescription("How much low ergonomics adds to the ADS time. 0 disables the ergonomics term.", new AcceptableValueRange<float>(0f, 2f)));
		StaminaAdsInfluence = config.Bind("05 - Aiming Speed & Weight", "Hands stamina influence", 1f, new ConfigDescription("How much drained hands stamina slows aiming down.", new AcceptableValueRange<float>(0f, 1f)));
		SwayEnabled = config.Bind("06 - Weapon Sway & Inertia", "Enable sway changes", true, Toggle("Scale sway, weapon inertia and tremor by weight, ergonomics and stamina."));
		SwayMultiplier = config.Bind("06 - Weapon Sway & Inertia", "Sway multiplier", 0.2f, new ConfigDescription("Scales the ergonomic weight that drives weapon sway and recoil behaviour. 0 = none, 2 = double.", new AcceptableValueRange<float>(0f, 2f)));
		AimSwayMultiplier = config.Bind("06 - Weapon Sway & Inertia", "Aim sway multiplier", 0.15f, new ConfigDescription("Breath/sway strength while aiming. 0 = none, 2 = double.", new AcceptableValueRange<float>(0f, 2f)));
		MotionInertiaMultiplier = config.Bind("06 - Weapon Sway & Inertia", "Weapon inertia multiplier", 2.6f, new ConfigDescription("How much the weapon lags behind player movement and mouse motion.", new AcceptableValueRange<float>(0.1f, 4f)));
		StaminaSwayInfluence = config.Bind("06 - Weapon Sway & Inertia", "Stamina influence", 1f, new ConfigDescription("How much drained stamina and oxygen add to sway.", new AcceptableValueRange<float>(0f, 2f)));
		TremorMultiplier = config.Bind("06 - Weapon Sway & Inertia", "Tremor multiplier", 0.1f, new ConfigDescription("Multiplier on hand shake / tremor intensity.", new AcceptableValueRange<float>(0.1f, 4f)));
		ShotMisalignmentEnabled = config.Bind("07 - Recoil & Shot Response", "Enable shot misalignment", true, Toggle("Every shot nudges the weapon out of alignment, scaled by weight and ergonomics."));
		HipSwayMultiplier = config.Bind("07 - Recoil & Shot Response", "Recoil intensity", 0.3286385f, new ConfigDescription("How chaotic recoil is while hip firing. 0 = vanilla-calm, 1 = very chaotic.", new AcceptableValueRange<float>(0f, 1f)));
		ShotMisalignmentStrength = config.Bind("07 - Recoil & Shot Response", "Strength", 1.7f, new ConfigDescription("Impulse applied to the hands rotation spring per shot.", new AcceptableValueRange<float>(0f, 4f)));
		ShotMisalignmentMax = config.Bind("07 - Recoil & Shot Response", "Maximum per shot (deg)", 1.5f, new ConfigDescription("Clamp on a single shot's misalignment impulse.", new AcceptableValueRange<float>(0.1f, 10f)));
		ShotBreathPenalty = config.Bind("07 - Recoil & Shot Response", "Accuracy loss per shot", 0.95f, new ConfigDescription("Multiplier on the vanilla breath amplitude gain applied per shot.", new AcceptableValueRange<float>(0f, 3f)));
		StaminaShotInfluence = config.Bind("07 - Recoil & Shot Response", "Stamina recoil influence", 3f, new ConfigDescription("How much drained hands stamina increases shot misalignment and recoil shake.", new AcceptableValueRange<float>(0f, 3f)));
		CameraLagEnabled = config.Bind("08 - Camera Sway", "Enable camera lag", true, Toggle("The head/camera trails the weapon's sway instead of sitting rigidly on it."));
		CameraSwayFollow = config.Bind("08 - Camera Sway", "Sway follow amount", 0.4037559f, new ConfigDescription("Fraction of the weapon sway the camera drifts after.", new AcceptableValueRange<float>(0f, 1f)));
		CameraSwayFollowSpeed = config.Bind("08 - Camera Sway", "Sway follow speed", 11.8f, new ConfigDescription("How fast the camera catches up with the weapon sway. Lower lags further behind.", new AcceptableValueRange<float>(0.5f, 20f)));
		CameraSettleSmoothing = config.Bind("08 - Camera Sway", "Hip camera smoothing", 1f, new ConfigDescription("Multiplier on the vanilla non-aiming camera settle speed. Below 1 is softer.", new AcceptableValueRange<float>(0.2f, 2f)));
		GaitEnabled = config.Bind("09 - Locomotion Gaits", "Enable gait system", true, Toggle("Adds smooth posture-specific movement rhythms and handling changes for walking, jogging, sprinting, crouching and prone movement."));
		GaitIntensity = config.Bind("09 - Locomotion Gaits", "Gait intensity", 1f, new ConfigDescription("Overall strength of gait-driven camera and weapon movement. 0 = off, 1 = natural, 2 = pronounced.", new AcceptableValueRange<float>(0f, 2f)));
		GaitTransitionSpeed = config.Bind("09 - Locomotion Gaits", "Gait transition speed", 6f, new ConfigDescription("How quickly the body blends between movement gaits. Lower = heavier, higher = more responsive.", new AcceptableValueRange<float>(1f, 12f)));
		InjuryHandlingEnabled = config.Bind("11 - Injury-Driven Handling", "Enable injury handling", true, Toggle("Makes arm, leg, torso and head damage physically affect weapon control, movement and camera stability."));
		InjuryHandlingStrength = config.Bind("11 - Injury-Driven Handling", "Injury handling strength", 1f, new ConfigDescription("Overall strength of physical injury effects. 0 = off, 1 = natural, 2 = severe.", new AcceptableValueRange<float>(0f, 2f)));
		InjuryPainkillerMasking = config.Bind("11 - Injury-Driven Handling", "Painkillers mask handling", true, Toggle("Painkillers partially reduce perceived handling penalties without restoring damaged body-part health."));
		MomentumEnabled = config.Bind("12 - Body Momentum", "Enable player momentum", true, Toggle("Adds weight and inertia-driven camera/weapon sway for more realistic movement."));
		MomentumAmount = config.Bind("12 - Body Momentum", "Momentum amount", 1f, new ConfigDescription("Overall strength of the momentum effect. 0 is off, 1 is natural.", new AcceptableValueRange<float>(0f, 1f)));
		MomentumWeight = config.Bind("12 - Body Momentum", "Inventory weight influence", 1f, new ConfigDescription("How much inventory weight and inertia scale the effect. 0 is off, 1 is full.", new AcceptableValueRange<float>(0f, 1f)));
		MomentumTurn = config.Bind("12 - Body Momentum", "Turn momentum", 0.5492957f, new ConfigDescription("Extra roll from turning left/right, scaled by weight. 0 is off, 1 is natural.", new AcceptableValueRange<float>(0f, 1f)));
		MomentumStopDelay = config.Bind("12 - Body Momentum", "Stop delay", 2f, new ConfigDescription("How long the camera/weapon momentum lingers after the player stops moving. 0 is instant, 1 is heavy.", new AcceptableValueRange<float>(0f, 2f)));
		MomentumWalkForwardLean = config.Bind("12 - Body Momentum", "Walk forward/back lean", 2f, new ConfigDescription("Camera pitch lean from walking forward and backward. Positive leans the camera with the movement.", new AcceptableValueRange<float>(-2f, 2f)));
		MomentumWalkSideLean = config.Bind("12 - Body Momentum", "Walk side lean", 0.2f, new ConfigDescription("Camera roll lean from strafing left and right. Positive leans the camera into the strafe.", new AcceptableValueRange<float>(-2f, 2f)));
		LeanEnabled = config.Bind("13 - Turn Lean", "Enable turn lean", true, Toggle("Turning with the mouse leans the body into the turn, so the camera and weapon bank instead of rotating perfectly level."));
		LeanCameraAngle = config.Bind("13 - Turn Lean", "Camera lean (deg)", 2.5f, new ConfigDescription("Maximum camera roll at the reference turn rate. Negative flips the direction of the lean.", new AcceptableValueRange<float>(-10f, 10f)));
		LeanWeaponAngle = config.Bind("13 - Turn Lean", "Weapon lean (deg)", 0f, new ConfigDescription("Maximum extra weapon roll at the reference turn rate. Negative flips the direction of the lean.", new AcceptableValueRange<float>(-15f, 15f)));
		LeanMultiplier = config.Bind("13 - Turn Lean", "Lean amount", 1.5f, new ConfigDescription("0 = no lean, 1 = natural, up to 3 for drastic lean.", new AcceptableValueRange<float>(0f, 3f)));
		// Procedural Q/E lean: adds extra camera and weapon roll when leaning with Q/E,
		// producing a more pronounced body-lean feel on top of the vanilla tilt.
		ProceduralLeanEnabled = config.Bind("14 - Q/E Lean", "Enable procedural Q/E lean", false, Toggle("EXPERIMENTAL: When leaning with Q/E, adds extra camera and weapon roll for a more pronounced body-lean feel. May interact unpredictably with other camera or stance systems."));
		ProceduralLeanRoll = config.Bind("14 - Q/E Lean", "Camera roll (deg)", 8.3f, new ConfigDescription("Additional camera roll from Q/E lean on top of the vanilla tilt.", new AcceptableValueRange<float>(0f, 15f)));
		ProceduralLeanWeaponRoll = config.Bind("14 - Q/E Lean", "Weapon roll (deg)", 13.79343f, new ConfigDescription("Additional weapon roll from Q/E lean. The weapon banks further than the camera for a natural peeking feel.", new AcceptableValueRange<float>(0f, 20f)));
		ProceduralLeanSmooth = config.Bind("14 - Q/E Lean", "Smooth speed", 5.9f, new ConfigDescription("How quickly the procedural lean transitions. Lower = slower/more deliberate, higher = snappier.", new AcceptableValueRange<float>(1f, 20f)));
		FluidMovementEnabled = config.Bind("16 - Fluid Movement", "Enable fluid movement", true, Toggle("Master toggle for landing dip and crouch dip. When off, vanilla camera behavior is untouched."));
		FluidAimReduction = config.Bind("16 - Fluid Movement", "Aim reduction", 0.4037559f, new ConfigDescription("How much camera effects are reduced while aiming down sights. 0 = no reduction (full effects while aiming), 1 = fully suppressed while aiming.", new AcceptableValueRange<float>(0f, 1f)));
		FluidLandingDip = config.Bind("16 - Fluid Movement", "Landing dip", 1.5f, new ConfigDescription("Camera dip intensity when landing from a fall. 0 = no dip, 1 = full dip proportional to fall duration.", new AcceptableValueRange<float>(0f, 2f)));
		FluidLandingRecovery = config.Bind("16 - Fluid Movement", "Landing recovery speed", 2.7f, new ConfigDescription("How quickly the landing dip springs back to neutral. Higher = faster recovery.", new AcceptableValueRange<float>(1f, 10f)));
		FluidCrouchDip = config.Bind("16 - Fluid Movement", "Crouch dip", 1f, new ConfigDescription("Camera lower amount when crouching. 0 = no dip, 0.3 = subtle dip when entering crouch.", new AcceptableValueRange<float>(0f, 1f)));
		FluidCrouchSpeed = config.Bind("16 - Fluid Movement", "Crouch dip speed", 1.4f, new ConfigDescription("How quickly the crouch dip transitions. Higher = snappier crouch dip.", new AcceptableValueRange<float>(1f, 10f)));
		TurnInPlaceEnabled = config.Bind("17 - Turn-in-Place Footwork", "Enable turn-in-place footwork", true, Toggle("While standing still, turns are absorbed by the waist first. Once the torso twist builds far enough, the feet reposition with a physical step that moves the camera and weapon."));
		TurnInPlaceThreshold = config.Bind("17 - Turn-in-Place Footwork", "Foot reposition angle (deg)", 45f, new ConfigDescription("How far the torso can twist before the feet step to catch up. Lower = steps happen sooner, higher = more free waist turning.", new AcceptableValueRange<float>(20f, 90f)));
		TurnInPlaceIntensity = config.Bind("17 - Turn-in-Place Footwork", "Footwork intensity", 1.5f, new ConfigDescription("Overall strength of twist and step effects on the camera and weapon. 0 = off, 1 = natural, 2 = pronounced.", new AcceptableValueRange<float>(0f, 2f)));
		HitReactionEnabled = config.Bind("19 - Combat Feedback", "Enable hit reaction", true, Toggle("Getting shot physically jolts the camera and weapon away from the struck body part. Requires EFTEffectManager."));
		HitReactionStrength = config.Bind("19 - Combat Feedback", "Hit reaction strength", 0.8f, new ConfigDescription("Magnitude of the hit jolt. 0.5 = subtle, 1 = natural, 3 = violent.", new AcceptableValueRange<float>(0f, 3f), new ConfigurationManagerAttributes { IsAdvanced = true }));
		MuzzleJoltEnabled = config.Bind("19 - Combat Feedback", "Enable muzzle jolt", true, Toggle("Firing shoves the head camera straight back a few millimeters instead of only rotating it."));
		MuzzleJoltAmount = config.Bind("19 - Combat Feedback", "Muzzle jolt amount", 1f, new ConfigDescription("Strength of the translational camera kick on firing. 0.5 = subtle, 1 = natural, 3 = heavy.", new AcceptableValueRange<float>(0f, 3f), new ConfigurationManagerAttributes { IsAdvanced = true }));
		DynamicFovEnabled = config.Bind("19 - Combat Feedback", "Enable dynamic FOV", true, Toggle("Field of view narrows slightly while aiming down sights, plus a tunnel-vision pinch under adrenaline."));
		DynamicFovAds = config.Bind("19 - Combat Feedback", "ADS FOV change (%)", -3f, new ConfigDescription("Percent FOV change while aiming. Negative = narrower (focus).", new AcceptableValueRange<float>(-15f, 15f), new ConfigurationManagerAttributes { IsAdvanced = true }));
		LazyPhysicsEnabled = config.Bind("20 - Lazy Weapon Physics", "Enable lazy weapon physics", true, Toggle("Replaces weapon camera-follow with a physical spring: the gun lags, overshoots and settles like a held object, and bounces on each footstep. Toggle off to A/B against the classic smoothed tracking."));
		LazySpringiness = config.Bind("20 - Lazy Weapon Physics", "Springiness", 0.6f, new ConfigDescription("How under-damped the weapon spring is. 0 = critically damped (no overshoot), 1 = natural wobble, 2 = rubber band.", new AcceptableValueRange<float>(0f, 2f), new ConfigurationManagerAttributes { IsAdvanced = true }));
		LazyTension = config.Bind("20 - Lazy Weapon Physics", "Muscle tension", 1.7f, new ConfigDescription("Stiffness multiplier on the weapon spring — how hard the arms pull the gun toward the camera. Lower = lazier, higher = tighter.", new AcceptableValueRange<float>(0.25f, 3f), new ConfigurationManagerAttributes { IsAdvanced = true }));

		AdrenalineEnabled = config.Bind("21 - Adrenaline", "Enable adrenaline response", true, Toggle("Nearby gunfire, your own shots, and hits taken raise an adrenaline level: hands tremor slightly, FOV pinches into tunnel vision, and a low heartbeat thumps faster and louder as the level climbs."));
		AdrenalineRadius = config.Bind("21 - Adrenaline", "Trigger radius (m)", 40f, new ConfigDescription("Enemy gunfire within this distance feeds adrenaline; hits taken always count. Your own shots only sustain adrenaline once a fight is already active.", new AcceptableValueRange<float>(5f, 150f)));
		AdrenalineTremor = config.Bind("21 - Adrenaline", "Tremor amount", 0.8f, new ConfigDescription("Extra hand-shake added on top of sway while adrenaline is up. 0 = none, 1 = noticeable wobble.", new AcceptableValueRange<float>(0f, 3f)));
		AdrenalineFovPinch = config.Bind("21 - Adrenaline", "FOV pinch (%)", 4f, new ConfigDescription("Tunnel-vision FOV reduction at full adrenaline. 0 = none, 4 = subtle focus, 8 = strong tunnel.", new AcceptableValueRange<float>(0f, 12f)));
		AdrenalineHeartbeat = config.Bind("21 - Adrenaline", "Heartbeat volume", 0.5f, new ConfigDescription("Volume of the synthesized heartbeat (rate and loudness scale with adrenaline). 0 = off.", new AcceptableValueRange<float>(0f, 1f)));
		AdrenalineDecay = config.Bind("21 - Adrenaline", "Decay time (s)", 8f, new ConfigDescription("Seconds for adrenaline to fully drain after the last trigger.", new AcceptableValueRange<float>(2f, 30f)));

	}
}
