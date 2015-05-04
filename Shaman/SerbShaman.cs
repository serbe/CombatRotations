using ReBot.API;
using Newtonsoft.Json;
using System.Linq;
using System;

namespace ReBot
{
	public abstract class SerbShaman : CombatRotation
	{
		// Vars && Consts

		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public UnitObject CycleTarget;
		public DateTime StartBattle;
		public DateTime StartSleepTime;
		public bool InCombat;
		public Int32 OraliusWhisperingCrystalId = 118922;
		public Int32 CrystalOfInsanityId = 86569;


		// Get

		public double Health (UnitObject u = null)
		{
			u = u ?? Target;
			return u.HealthFraction;
		}

		public bool IsBoss (UnitObject u = null)
		{
			u = u ?? Target;
			return(u.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || u.Level >= Me.Level + BossLevelIncrease;
		}

		public int ActiveEnemies (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && mob.IsAttackable && mob.InCombat) {
					x++;
				}
			}
			return x;
		}

		public double Frac (string s)
		{
			string scurrentCharges = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" + s + "\"); return currentCharges";
			string smaxCharges = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" + s + "\"); return maxCharges";
			string scooldownStart = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" + s + "\"); return cooldownStart";
			string scooldownDuration = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" + s + "\"); return cooldownDuration";

			double currentCharges = API.ExecuteLua<double> (scurrentCharges);
			double maxCharges = API.ExecuteLua<double> (smaxCharges);
			double cooldownStart = API.ExecuteLua<double> (scooldownStart);
			double cooldownDuration = API.ExecuteLua<double> (scooldownDuration);

			double f = currentCharges;

			if (f != maxCharges) {
				double currentTime = API.ExecuteLua<double> ("return GetTime()");
				f = f + ((currentTime - cooldownStart) / cooldownDuration);
			}

			return f;
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public double TimeToDie (UnitObject u = null)
		{
			u = u ?? Target;
			return u.Health / Ttd;
		}

		public double CastTime (Int32 i)
		{
			return API.ExecuteLua<double> ("local _, _, _, castTime, _, _ = GetSpellInfo(" + i + "); return castTime;");
		}

		public double Time {
			get {
				TimeSpan combatTime = DateTime.Now.Subtract (StartBattle);
				return combatTime.TotalSeconds;
			}
		}

		public double TotemRemainTime (string s)
		{
			if (s == "Searing Totem") {
				if (HasActiveSearingTotem)
					return TotemTime (0);
			}
			if (s == "Fire Searing Totem") {
				if (HasActiveFireElementalTotem)
					return TotemTime (0);
			}

			return 0;
		}

		public double TotemTime (int s)
		{
			var StartTime = API.ExecuteLua<double> ("local haveTotem, totemName, startTime, duration = GetTotemInfo(" + s + "); return startTime;");
			var Duration = API.ExecuteLua<double> ("local haveTotem, totemName, startTime, duration = GetTotemInfo(" + s + "); return duration;");
			var CurrentTime = API.ExecuteLua<double> ("return GetTime();");
			return StartTime + Duration - CurrentTime;
		}

		public int MaxLightningShieldCharges {
			get {
				int c = 15;
				if (HasSpell ("Improved Lightning Shield"))
					c = c + 5;
				return c;
			}
		}

		// Check

		public bool InRaid {
			get {
				return API.MapInfo.Type == MapType.Raid;
			}
		}

		public bool InInstance {
			get {
				return API.MapInfo.Type == MapType.Instance;
			}
		}

		public bool InArena {
			get {
				return API.MapInfo.Type == MapType.Arena;
			}
		}

		public bool InBg {
			get {
				return API.MapInfo.Type == MapType.PvP;
			}
		}

		public bool Range (int r, UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsInLoS && u.CombatRange <= r;
		}

		public bool Danger (UnitObject u = null, int r = 0, int e = 2)
		{
			u = u ?? Target;
			if (r != 0)
				return Range (r, u) && (IsElite (u) || IsPlayer (u) || ActiveEnemies (10) > e);
			return u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || ActiveEnemies (10) > e);
		}

		public bool IsPlayer (UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsPlayer;
		}

		public bool IsElite (UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsElite ();
		}

		public bool IsInEnrage (UnitObject o)
		{
			if (o.HasAura ("Enrage") || o.HasAura ("Berserker Rage") || o.HasAura ("Demonic Enrage") || o.HasAura ("Aspect of Thekal") || o.HasAura ("Charge Rage") || o.HasAura ("Electric Spur") || o.HasAura ("Cornered and Enraged!") || o.HasAura ("Draconic Rage") || o.HasAura ("Brood Rage") || o.HasAura ("Determination") || o.HasAura ("Charged Fists") || o.HasAura ("Beatdown") || o.HasAura ("Consuming Bite") || o.HasAura ("Delirious") || o.HasAura ("Angry") || o.HasAura ("Blood Rage") || o.HasAura ("Berserking Howl") || o.HasAura ("Bloody Rage") || o.HasAura ("Brewrific") || o.HasAura ("Desperate Rage") || o.HasAura ("Blood Crazed") || o.HasAura ("Combat Momentum") || o.HasAura ("Dire Rage") || o.HasAura ("Dominate Slave") || o.HasAura ("Blackrock Rabies") || o.HasAura ("Burning Rage") || o.HasAura ("Bloodletting Howl"))
				return true;
			return false;
		}

		public bool IsNotForDamage (UnitObject o)
		{
			if (o.HasAura ("Fear") || o.HasAura ("Polymorph") || o.HasAura ("Gouge") || o.HasAura ("Paralysis") || o.HasAura ("Blind") || o.HasAura ("Hex"))
				return true;
			return false;
		}

		public bool IncapacitatedInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && mob.IsAttackable && IsNotForDamage (mob)) {
					x++;
				}
			}
			return x > 0;
		}

		public bool Usable (string s)
		{ 
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public bool HasActiveFireElementalTotem {
			get {
				foreach (UnitObject u in API.CollectUnits(40)) {
					if (u.CreatedByMe && u.EntryID == 15439)
						return true;
				}
				return false;
			}
		}

		public bool HasActiveSearingTotem {
			get {
				foreach (UnitObject u in API.CollectUnits(40)) {
					if (u.CreatedByMe && u.EntryID == 2523)
						return true;
				}
				return false;
			}
		}

		// Combo

		public bool Interrupt ()
		{
			if (Usable ("Wind Shear")) {
				var targets = Adds;
				targets.Add (Target);

				CycleTarget = targets.Where (t => t.IsCastingAndInterruptible () && t.CastingTime > 0 && Range (25, t)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (WindShear (CycleTarget))
						return true;
				}
			}
			return false;
		}

		public bool CleanCurse ()
		{
			if (Me.Auras.Any (x => x.IsDebuff && "Curse".Contains (x.DebuffType))) {
				if (CleanseSpirit (Me))
					return true;
			}
			var players = Group.GetGroupMemberObjects ();
			CycleTarget = players.Where (p => !p.IsDead && p.IsInLoS && Range (40, p) && !p.HasAura ("Zen Sphere", true)).DefaultIfEmpty (null).FirstOrDefault ();
			if (CycleTarget != null) {
				if (CleanseSpirit (CycleTarget))
					return true;
			}

			return false;
		}

		// Spell

		public bool LightningShield ()
		{
			return CastSelf ("Lightning Shield", () => Usable ("Lightning Shield") && !Me.HasAura ("Lightning Shield"));
		}

		public bool Bloodlust (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Bloodlust", () => Usable ("Bloodlust") && (IsBoss (u) || IsPlayer (u)) && u.IsInCombatRangeAndLoS);
		}

		public bool BloodFury ()
		{
			return CastSelf ("BloodFury", () => Usable ("Blood Fury") && Danger ());
		}

		public bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Danger ());
		}

		public bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Danger ());
		}

		public bool ElementalMastery ()
		{
			return CastSelf ("Elemental Mastery", () => Usable ("Elemental Mastery") && Danger ());
		}

		public bool FeralSpirit (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Feral Spirit", () => Usable ("Feral Spirit") && Danger (u, 30), u);
		}

		public bool AncestralSwiftness ()
		{
			return CastSelf ("Ancestral Swiftness", () => Usable ("Ancestral Swiftness") && Danger ());
		}

		public bool Ascendance ()
		{
			return CastSelf ("Ascendance", () => Usable ("Ascendance") && Danger ());
		}

		public bool StormElementalTotem (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Storm Elemental Totem", () => Usable ("Storm Elemental Totem") && Danger (u, 30, 5));
		}

		public bool FireElementalTotem (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Fire Elemental Totem", () => Usable ("Fire Elemental Totem") && Danger (u, 30, 5));
		}

		public bool SearingTotem ()
		{
			return CastSelf ("Searing Totem", () => Usable ("Searing Totem") && Range (25));
		}

		public bool UnleashElements (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && Range (40, u), u);
		}

		public bool ElementalBlast (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Elemental Blast", () => Usable ("Elemental Blast") && Range (40, u) && (!Me.IsMoving || Me.HasAura ("Ancestral Swiftness")), u);
		}

		public bool LightningBolt (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Lightning Bolt", () => Usable ("Lightning Bolt") && Range (30, u) && (!Me.IsMoving || Me.HasAura ("Ancestral Swiftness")), u);
		}

		public bool Stormstrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Stormstrike", () => Usable ("Stormstrike") && Range (5, u), u);
		}

		public bool LavaLash (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Lava Lash", () => Usable ("Lava Lash") && Range (5, u), u);
		}

		public bool FlameShock (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Flame Shock", () => Usable ("Flame Shock") && Range (25, u), u);
		}

		public bool FrostShock (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Frost Shock", () => Usable ("Frost Shock") && Range (25, u), u);
		}

		public bool EarthShock (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Earth Shock", () => Usable ("Earth Shock") && Range (25, u), u);
		}

		public bool FireNova (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Fire Nova", () => Usable ("Fire Nova") && u.IsInLoS, u);
		}

		public bool WindShear (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Wind Shear", () => Usable ("Wind Shear") && Range (25, u), u);
		}

		public bool UnleashFlame (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Unleash Flame", () => Usable ("Unleash Flame") && Me.HasAura ("Unleash Flame") && Range (40, u), u);
		}

		public bool LiquidMagma ()
		{
			return CastSelf ("Liquid Magma", () => Usable ("Liquid Magma") && !Me.HasAura ("Liquid Magma"));
		}

		public bool SpiritwalkersGrace ()
		{
			return CastSelf ("Spiritwalker's Grace", () => Usable ("Spiritwalker's Grace"));
		}

		public bool LavaBurst (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Lava Burst", () => Usable ("Lava Burst") && Range (30, u), u);
		}

		public bool Earthquake (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Earthquake", u.Position, () => Usable ("Earthquake") && Range (35, u));
		}

		public bool LavaBeam (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Lava Beam", () => Usable ("Lava Beam") && Range (40, u), u);
		}

		public bool Thunderstorm ()
		{
			return CastSelf ("Thunderstorm", () => Usable ("Thunderstorm"));
		}

		public bool ChainLightning (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Chain Lightning", () => Usable ("Chain Lightning") && Range (30, u), u);
		}

		public bool HealingSurge (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Healing Surge", () => Usable ("Healing Surge") && Range (40, u), u);
		}

		public bool CleanseSpirit (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Cleanse Spirit", () => Usable ("Cleanse Spirit") && Range (40, u), u);
		}

		public bool GhostWolf ()
		{
			return CastSelf ("Ghost Wolf", () => Usable ("Ghost Wolf") && !Me.HasAura ("Ghost Wolf"));
		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && Range(40, u), u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && Range(40, u), u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && Range(40, u), u);
		//		}


		// Items

		public bool Healthstone ()
		{
			if (API.HasItem (5512) && API.ItemCooldown (5512) == 0)
				return API.UseItem (5512);
			return false;
		}

		public bool CrystalOfInsanity ()
		{
			if (!InArena && API.HasItem (CrystalOfInsanityId) && !HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityId) == 0)
				return API.UseItem (CrystalOfInsanityId);
			return false;
		}

		public bool OraliusWhisperingCrystal ()
		{
			if (API.HasItem (OraliusWhisperingCrystalId) && !HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalId) == 0)
				return API.UseItem (OraliusWhisperingCrystalId);
			return false;
		}
	}
}

