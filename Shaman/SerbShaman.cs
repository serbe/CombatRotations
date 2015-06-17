using System;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot
{
	public abstract class SerbShaman : SerbUtils
	{
		// Vars && Consts

		// Get

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
				Unit = Enemy.Where (t => t.IsCastingAndInterruptible () && t.CastingTime > 0 && Range (25, t)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && WindShear (Unit))
					return true;
			}
			return false;
		}

		public bool CleanCurse ()
		{
			if (Me.Auras.Any (x => x.IsDebuff && "Curse".Contains (x.DebuffType))) {
				if (CleanseSpirit (Me))
					return true;
			}
			Player = MyGroup.Where (p => !p.IsDead && Range (40, p) && p.Auras.Any (x => x.IsDebuff && "Curse".Contains (x.DebuffType))).DefaultIfEmpty (null).FirstOrDefault ();
			if (Player != null && CleanseSpirit (Unit))
				return true;

			return false;
		}

		// Spell

		public bool LightningBolt (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Lightning Bolt") && (Range (30, u) || (Me.HasAura ("Elemental Reach") && Range (40, u))) && C ("Lightning Bolt", u);
		}

		//		Primal Strike

		public bool HealingSurge (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Healing Surge") && Range (40, u) && C ("Healing Surge", u);
		}

		public bool LightningShield ()
		{
			return Usable ("Lightning Shield") && !Me.HasAura ("Lightning Shield") && CS ("Lightning Shield");
		}

		public bool FlameShock (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Flame Shock") && (Range (25, u) || (Me.HasAura ("Elemental Reach") && Range (40, u))) && C ("Flame Shock", u);
		}

		public bool GhostWolf ()
		{
			return Usable ("Ghost Wolf") && !Me.HasAura ("Ghost Wolf") && CS ("Ghost Wolf");
		}

		public bool SearingTotem ()
		{
			return Usable ("Searing Totem") && Range (25) && CS ("Searing Totem");
		}

		public bool WindShear (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Wind Shear") && Range (25, u) && C ("Wind Shear", u);
		}

		public bool CleanseSpirit (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Cleanse Spirit") && Range (40, u) && C ("Cleanse Spirit", u);
		}

		public bool FrostShock (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Frost Shock") && (Range (25, u) || (Me.HasAura ("Elemental Reach") && Range (40, u))) && C ("Frost Shock", u);
		}

		public bool ChainLightning (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Chain Lightning") && (Range (30, u) || (Me.HasAura ("Elemental Reach") && Range (40, u))) && C ("Chain Lightning", u);
		}

		public bool HealingStreamTotem ()
		{
			return Usable ("Healing Stream Totem") && !Me.IsMoving && CS ("Healing Stream Totem");
		}

		public bool HealingRain (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Healing Rain") && Range (40, u) && COT ("Healing Rain", u);
		}

		public bool FireElementalTotem (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Fire Elemental Totem") && DangerBoss (u, 30) && CS ("Fire Elemental Totem");
		}

		public bool Bloodlust (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Bloodlust") && DangerBoss (u, 0, 15) && CS ("Bloodlust");
		}

		public bool ElementalMastery ()
		{
			return Usable ("Elemental Mastery") && Danger () && CS ("Elemental Mastery");
		}

		public bool FeralSpirit (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Feral Spirit") && Danger (u, 30) && C ("Feral Spirit", u);
		}

		public bool AncestralSwiftness ()
		{
			return Usable ("Ancestral Swiftness") && Danger () && CS ("Ancestral Swiftness");
		}

		public bool Ascendance ()
		{
			return Usable ("Ascendance") && Danger () && CS ("Ascendance");
		}

		public bool StormElementalTotem (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Storm Elemental Totem") && DangerBoss (u, 30) && CS ("Storm Elemental Totem");
		}

		public bool UnleashElements (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Unleash Elements") && Range (40, u) && C ("Unleash Elements", u);
		}

		public bool ElementalBlast (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Elemental Blast") && Range (40, u) && (!Me.IsMoving || Me.HasAura ("Ancestral Swiftness")) && C ("Elemental Blast", u);
		}

		public bool Stormstrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Stormstrike") && Range (5, u) && C ("Stormstrike", u);
		}

		public bool LavaLash (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Lava Lash") && SpellCharges ("Lava Lash") >= 1 && Range (5, u) && C ("Lava Lash", u);
		}

		public bool EarthShock (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Earth Shock") && (Range (25, u) || (Me.HasAura ("Elemental Reach") && Range (40, u))) && C ("Earth Shock", u);
		}

		public bool FireNova (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Fire Nova") && u.IsInLoS && C ("Fire Nova", u);
		}

		public bool UnleashFlame (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Unleash Flame") && Range (40, u) && C ("Unleash Flame", u);
		}

		public bool LiquidMagma ()
		{
			return Usable ("Liquid Magma") && !Me.HasAura ("Liquid Magma") && CS ("Liquid Magma");
		}

		public bool SpiritwalkersGrace ()
		{
			return Usable ("Spiritwalker's Grace") && CS ("Spiritwalker's Grace");
		}

		public bool LavaBurst (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Lava Burst") && (Range (30, u) || (Me.HasAura ("Elemental Reach") && Range (40, u))) && C ("Lava Burst", u);
		}

		public bool Earthquake (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Earthquake") && Range (35, u) && COT ("Earthquake", u);
		}

		public bool LavaBeam (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Lava Beam") && Range (40, u) && C ("Lava Beam", u);
		}

		public bool Thunderstorm ()
		{
			return Usable ("Thunderstorm") && CS ("Thunderstorm");
		}

		public bool GiftoftheNaaru (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Gift of the Naaru") && Range (40, u) && C ("Gift of the Naaru", u);
		}

		public bool AncestralGuidance ()
		{
			return Usable ("Ancestral Guidance") && CS ("Ancestral Guidance");
		}

		public bool AstralShift ()
		{
			return Usable ("Astral Shift") && CS ("Astral Shift");
		}

		public bool ShamanisticRage ()
		{
			return Usable ("Shamanistic Rage") && CS ("Shamanistic Rage");
		}

	}
}

