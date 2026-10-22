# TarkovRL — Tarkov Real Life

A BepInEx plugin for **SPT 4.1.x** that makes weapon handling feel real: your gun has weight, it drifts, it lags behind your aim, and your body moves like a person's — not a tripod's.

> Based on [TarkovIRL](https://github.com/crm85/TarkovIRL-public) by **adishee**. The weapon deadzone system and several handling ideas were ported from TarkovIRL — full credit for the original work.

## Features

- **Weapon weight you can feel** — heavy guns aim slower, sway more, and settle harder
- **Aim deadzone** — your gun moves inside a free-aim cone; the camera only follows when the gun reaches the edge
- **Hip-fire deadzone** — natural drift, wrist roll, and muzzle movement when not aiming
- **Sway & inertia** — breathing, walking, fatigue, and weapon momentum all move the gun
- **Camera lag** — your head trails the weapon instead of being glued to it
- **Turn lean & Q/E lean** — your body banks into turns and leans for real
- **Recoil that misaligns** — every shot knocks the gun off target; tired arms make it worse
- **Reload efficiency** — reload speed follows ergonomics and arm health
- **Injuries matter** — wounded arms shake and slow you; crippled legs force a limp or crawl
- **Stamina effects** — exhaustion adds tremor and sway
- **Free-look friendly** — camera work stays smooth with free look enabled
- **Collision-aware ADS** — the gun pulls back near walls instead of clipping
- **Focus zoom on ADS** — subtle tunnel-vision zoom when you settle on target (off while mounted)
- **Everything is configurable** — tune or disable any part via the config file

## Install

1. Download the latest `TarkovRL-x.x.x.zip` from [Releases](https://github.com/FortuneCocky/TarkovRL-SPT/releases)
2. Extract into your SPT install folder (merge the `BepInEx` folder)
3. Launch the game once — settings appear at `BepInEx/config/com.tarkovrl.cfg`

## Compatibility

- SPT 4.1.x — client only, no server mod needed
- Works alongside **TacticalStances** — it layers stances on top of this motion

## Credits

- **adishee** — author of [TarkovIRL](https://forge.sp-tarkov.com/mod/1459/tarkovirl-weapons-handling-mod), the original implementation this mod is derived from
- **SPT team**, **BepInEx**, **HarmonyX**

## License

MIT — see [LICENSE](LICENSE). Ported TarkovIRL code retains attribution to adishee.
