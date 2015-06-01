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
				if (Unit != null) {
					if (WindShear (Unit))
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
			Player = Group.GetGroupMemberObjects ().Where (p => !p.IsDead && Range (40, p) && p.Auras.Any (x => x.IsDebuff && "Curse".Contains (x.DebuffType))).DefaultIfEmpty (null).FirstOrDefault ();
			if (Player != null && CleanseSpirit (Unit))
				return true;

			return false;
		}

		// Spell

		public bool LightningBolt (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Lightning Bolt") && (Range (30, u) || (Me.HasAura ("Elemental Reach") && Range (40, u)))) {
				if (Cast ("Lightning Bolt", u))
					return true;
				API.Print ("Lightning Bolt");
			}
			return false;
		}

		//		Primal Strike

		public bool HealingSurge (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Healing Surge") && Range (40, u)) {
				if (Cast ("Healing Surge", u))
					return true;
				API.Print ("Healing Surge");
			}
			return false;
		}

		public bool LightningShield ()
		{
			if (Usable ("Lightning Shield") && !Me.HasAura ("Lightning Shield")) {
				if (CastSelf ("Lightning Shield"))
					return true;
				API.Print ("Lightning Shield");
			}
			return false;
		}

		public bool FlameShock (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Flame Shock") && (Range (25, u) || (Me.HasAura ("Elemental Reach") && Range (40, u)))) {
				if (Cast ("Flame Shock", u))
					return true;
				API.Print ("Flame Shock");

			}
			return false;
		}

		public bool GhostWolf ()
		{
			if (Usable ("Ghost Wolf") && !Me.HasAura ("Ghost Wolf")) {
				if (CastSelf ("Ghost Wolf"))
					return true;
				API.Print ("Ghost Wolf");
			}
			return false;
		}

		public bool SearingTotem ()
		{
			if (Usable ("Searing Totem") && Range (25)) {
				if (CastSelf ("Searing Totem"))
					return true;
				API.Print ("Searing Totem");
			}
			return false;
		}

		public bool WindShear (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Wind Shear") && Range (25, u)) {
				if (Cast ("Wind Shear", u))
					return true;
				API.Print ("Wind Shear");
			}
			return false;
		}

		public bool CleanseSpirit (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Cleanse Spirit") && Range (40, u)) {
				if (Cast ("Cleanse Spirit", u))
					return true;
				API.Print ("Cleanse Spirit");
			}
			return false;
		}

		public bool FrostShock (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Frost Shock") && (Range (25, u) || (Me.HasAura ("Elemental Reach") && Range (40, u)))) {
				if (Cast ("Frost Shock", u))
					return true;
				API.Print ("Frost Shock");
			}
			return false;
		}

		public bool ChainLightning (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Chain Lightning") && (Range (30, u) || (Me.HasAura ("Elemental Reach") && Range (40, u)))) {
				if (Cast ("Chain Lightning", u))
					return true;
				API.Print ("Chain Lightning");
			}
			return false;
		}

		public bool HealingStreamTotem ()
		{
			if (Usable ("Healing Stream Totem") && !Me.IsMoving) {
				if (CastSelf ("Healing Stream Totem"))
					return true;
				API.Print ("Healing Stream Totem");
			}
			return false;
		}

		public bool HealingRain (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Healing Rain") && Range (40, u)) {
				if (CastOnTerrain ("Healing Rain", u.Position))
					return true;
				API.Print ("Healing Rain");
			}
			return false;
		}

		public bool FireElementalTotem (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Fire Elemental Totem") && DangerBoss (u, 30)) {
				if (CastSelf ("Fire Elemental Totem"))
					return true;
				API.Print ("Fire Elemental Totem");
			}
			return false;
		}

		public bool Bloodlust (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Bloodlust") && DangerBoss (u, 0, 15)) {
				if (CastSelf ("Bloodlust"))
					return true;
				API.Print ("Bloodlust");
			}
			return false;
		}

		public bool ElementalMastery ()
		{
			if (Usable ("Elemental Mastery") && Danger ()) {
				if (CastSelf ("Elemental Mastery"))
					return true;				
				API.Print ("Elemental Mastery");
			}
			return false;
		}

		public bool FeralSpirit (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Feral Spirit") && Danger (u, 30)) {
				if (Cast ("Feral Spirit", u))
					return true;			
				API.Print ("Feral Spirit");
			}
			return false;
		}

		public bool AncestralSwiftness ()
		{
			if (Usable ("Ancestral Swiftness") && Danger ()) {
				if (CastSelf ("Ancestral Swiftness"))
					return true;			
				API.Print ("Ancestral Swiftness");
			}
			return false;
		}

		public bool Ascendance ()
		{
			if (Usable ("Ascendance") && Danger ()) {
				if (CastSelf ("Ascendance"))
					return true;
				API.Print ("Ascendance");
			}
			return false;
		}

		public bool StormElementalTotem (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Storm Elemental Totem") && DangerBoss (u, 30)) {
				if (CastSelf ("Storm Elemental Totem"))
					return true;
				API.Print ("Storm Elemental Totem");
			}
			return false;
		}

		public bool UnleashElements (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Unleash Elements") && Range (40, u)) {
				if (Cast ("Unleash Elements", u))
					return true;
				API.Print ("Unleash Elements");
			}
			return false;
		}

		public bool ElementalBlast (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Elemental Blast") && Range (40, u) && (!Me.IsMoving || Me.HasAura ("Ancestral Swiftness"))) {
				if (Cast ("Elemental Blast", u))
					return true;
				API.Print ("Elemental Blast");
			}
			return false;
		}

		public bool Stormstrike (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Stormstrike") && Range (5, u)) {
				if (Cast ("Stormstrike", u))
					return true;
				API.Print ("Stormstrike");
			}
			return false;
		}

		public bool LavaLash (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Lava Lash") && SpellCharges ("Lava Lash") >= 1 && Range (5, u)) {
				if (Cast ("Lava Lash", u))
					return true;
				API.Print ("Lava Lash");
			}
			return false;
		}

		public bool EarthShock (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Earth Shock") && (Range (25, u) || (Me.HasAura ("Elemental Reach") && Range (40, u)))) {
				if (Cast ("Earth Shock", u))
					return true;
				API.Print ("Earch Shock");
			}
			return false;
		}

		public bool FireNova (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Fire Nova") && u.IsInLoS) {
				if (Cast ("Fire Nova", u))
					return true;
				API.Print ("Fire Nova");
			}
			return false;
		}

		public bool UnleashFlame (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Unleash Flame") && Range (40, u)) {
				if (Cast ("Unleash Flame", u))
					return true;
				API.Print ("Unleash Flame");
			}
			return false;
		}

		public bool LiquidMagma ()
		{
			if (Usable ("Liquid Magma") && !Me.HasAura ("Liquid Magma")) {
				if (CastSelf ("Liquid Magma"))
					return true;
				API.Print ("Liquid Magma");
			}
			return false;
		}

		public bool SpiritwalkersGrace ()
		{
			if (Usable ("Spiritwalker's Grace")) {
				if (CastSelf ("Spiritwalker's Grace"))
					return true;
				API.Print ("Spiritwalker's Grace");
			}
			return false;
		}

		public bool LavaBurst (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Lava Burst") && (Range (30, u) || (Me.HasAura ("Elemental Reach") && Range (40, u)))) {
				if (Cast ("Lava Burst", u))
					return true;
				API.Print ("Lava Burst");
			}
			return false;
		}

		public bool Earthquake (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Earthquake") && Range (35, u)) {
				if (CastOnTerrain ("Earthquake", u.Position))
					return true;
				API.Print ("Earthquake");
			}
			return false;
		}

		public bool LavaBeam (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Lava Beam") && Range (40, u)) {
				if (Cast ("Lava Beam", u))
					return true;
				API.Print ("Lava Beam");
			}
			return false;
		}

		public bool Thunderstorm ()
		{
			if (Usable ("Thunderstorm")) {
				if (CastSelf ("Thunderstorm"))
					return true;
				API.Print ("Thunderstorm");
			}
			return false;
		}

		public bool GiftoftheNaaru (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Gift of the Naaru") && Range (40, u)) {
				if (Cast ("Gift of the Naaru", u))
					return true;
				API.Print ("Gift of the Naaru");
			}
			return false;
		}

		public bool AncestralGuidance ()
		{
			if (Usable ("Ancestral Guidance")) {
				if (CastSelf ("Ancestral Guidance"))
					return true;
				API.Print ("Ancestral Guidance");
			}
			return false;
				
		}

		public bool AstralShift ()
		{
			if (Usable ("Astral Shift")) {
				if (CastSelf ("Astral Shift"))
					return true;
				API.Print ("Astral Shift");
			}
			return false;
		}

		public bool ShamanisticRage ()
		{
			if (Usable ("Shamanistic Rage")) {
				if (CastSelf ("Shamanistic Rage"))
					return true;
				API.Print ("Shamanistic Rage");
			}
			return false;
		}

	}
}

