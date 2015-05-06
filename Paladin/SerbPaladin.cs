using ReBot.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Geometry;
using System.Linq;

namespace ReBot
{
	public abstract class SerbPaladin : CombatRotation
	{
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;

		// Consts && Vars

		public bool InCombat;
		public DateTime StartBattle;
		public UnitObject CycleTarget;
		public UnitObject LastJudgmentTarget;
		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public float OraliusWhisperingCrystalId = 118922;
		public float CrystalOfInsanityId = 86569;

		// Get

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

		public double TimeToHpg {
			get {
				if (HasGlobalCooldown ())
					return 1.5;
				return 0.35 + 1.5;
			}
		}

		public List<UnitObject> Enemy {
			get {
				var targets = Adds;
				targets.Add (Target);
				return targets;
			}
		}

		public int HolyPower {
			get {
				return Me.GetPower (WoWPowerType.PaladinHolyPower);
			}
		}

		public double Cooldown (string s)
		{
			return SpellCooldown (s) > 0 ? SpellCooldown (s) : 0;
		}

		public int ActiveEnemies (int r)
		{
			int x = 0;
			foreach (UnitObject u in API.CollectUnits(r)) {
				if ((u.IsEnemy || Me.Target == u) && !u.IsDead && u.IsAttackable && u.InCombat) {
					x++;
				}
			}
			return x;
		}

		public int ActiveEnemiesWithTarget (int r, UnitObject t = null)
		{
			t = t ?? Target;
			int x = 0;
			foreach (UnitObject u in API.CollectUnits(45)) {
				if (Vector3.Distance (t.Position, u.Position) <= r && (u.IsEnemy || Me.Target == u) && !u.IsDead && u.IsAttackable) {
					x++;
				}
			}
			return x;
		}

		public double Time {
			get {
				TimeSpan combatTime = DateTime.Now.Subtract (StartBattle);
				return combatTime.TotalSeconds;
			}
		}

		public double Health (UnitObject u = null)
		{
			u = u ?? Target;
			return u.HealthFraction;
		}

		// Check

		public bool CS (string s)
		{
			if (CastSelf (s))
				return true;
			API.Print ("False CastSelf " + s);
			return false;
		}

		public bool C (string s, UnitObject u = null)
		{
			u = u ?? Target;
			if (Cast (s, u))
				return true;
			API.Print ("False Cast " + s + " with " + u.CombatRange + " range");
			return false;
		}

		public bool IsBoss (UnitObject u = null)
		{
			u = u ?? Target;
			return(u.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || u.Level >= Me.Level + BossLevelIncrease;
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

		public bool Usable (string s)
		{
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public double DamageTaken (float t)
		{
			var damage = API.ExecuteLua<double> ("local ResolveName = GetSpellInfo(158300);local n,_,_,_,_,dur,expi,_,_,_,id,_,_,_,val1,val2,val3 = UnitAura(\"player\", ResolveName, nil, \"HELPFUL\");return val2");
			if (Time < 10) {
				if (Time < t / 1000)
					return damage;
				return damage / Time * (t / 1000);
			}
				
			return damage / 10 * (t / 1000);
		}

		public bool Range (int r, UnitObject u = null, int l = 0)
		{
			u = u ?? Target;
			if (l != 0)
				return u.IsInLoS && u.CombatRange <= r && u.CombatRange >= l;
			return u.IsInLoS && u.CombatRange <= r;
		}

		public bool Danger (UnitObject u = null, int r = 0, int e = 2)
		{
			u = u ?? Target;
			if (r != 0)
				return Range (r, u) && (IsElite (u) || IsPlayer (u) || ActiveEnemies (10) > e);
			return u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || ActiveEnemies (10) > e);
		}

		// Combo

		public bool Interrupt ()
		{
			if (InArena || InBg) {
				if (Usable ("Rebuke")) {
					CycleTarget = API.Players.Where (x => x.IsPlayer && x.IsEnemy && x.IsHealer && Range (5, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null && Rebuke (CycleTarget))
						return true; 
					CycleTarget = API.Players.Where (x => x.IsPlayer && x.IsEnemy && Range (5, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null && Rebuke (CycleTarget))
						return true; 
				}
				if (Cooldown ("Fist of Justice") == 0) {
					CycleTarget = API.Players.Where (x => x.IsPlayer && x.IsEnemy && x.IsHealer && Range (20, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null && FistofJustice (CycleTarget))
						return true;
					CycleTarget = API.Players.Where (x => x.IsPlayer && x.IsEnemy && Range (20, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null && FistofJustice (CycleTarget))
						return true;
				}
			} else {
				CycleTarget = Enemy.Where (x => Range (5, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null && Rebuke (CycleTarget))
					return true; 
				if (Cooldown ("Fist of Justice") == 0) {
					CycleTarget = Enemy.Where (x => !IsBoss (x) && Range (20, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null && FistofJustice (CycleTarget))
						return true;
				}
			}

			return false;
		}


		// Spell

		public bool SpeedofLight ()
		{
			return Usable ("Speed of Light") && CS ("Speed of Light");
		}

		public bool BloodFury ()
		{
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && Danger ());
		}

		public bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Danger ());
		}

		public bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Danger ());
		}

		public bool HolyAvenger ()
		{
			return Usable ("Holy Avenger") && Danger () && CS ("Holy Avenger");
		}

		public bool Seraphim (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Seraphim") && u.IsInCombatRangeAndLoS && CS ("Seraphim");
		}

		public bool DivineProtection ()
		{
			return Usable ("Divine Protection") && CS ("Divine Protection");
		}

		public bool GuardianofAncientKings (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Guardian of Ancient Kings") && Range (30, u) && C ("Guardian of Ancient Kings", u);
		}

		public bool ArdentDefender ()
		{
			return Usable ("Ardent Defender") && CS ("Ardent Defender");
		}

		public bool FlashofLight (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Flash of Light") && Range (40, u) && C ("Flash of Light", u);
		}

		public bool EternalFlame (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Eternal Flame") && HolyPower >= 1 && Range (40, u) && C ("Eternal Flame", u);
		}

		public bool ShieldoftheRighteous (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shield of the Righteous") && HolyPower >= 3 && Range (5, u) && C ("Shield of the Righteous", u);
		}

		public bool SealofInsight ()
		{
			return Usable ("Seal of Insight") && CS ("Seal of Insight");
		}

		public bool Consecration ()
		{
			return Usable ("Consecration") && CS ("Consecration");
		}

		public bool SealofRighteousness ()
		{
			return Usable ("Seal of Righteousness") && CS ("Seal of Righteousness");
		}

		public bool AvengersShield (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Avenger's Shield") && Range (30, u) && C ("Avenger's Shield", u);
		}

		public bool SacredShield (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Sacred Shield") && Range (40, u) && C ("Sacred Shield", u);
		}

		public bool HolyPrism (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Holy Prism") && Range (40, u) && C ("Holy Prism", u);
		}

		public bool ExecutionSentence (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Execution Sentence") && Range (40, u) && C ("Execution Sentence", u);
		}

		public bool HammeroftheRighteous (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Hammer of the Righteous") && Range (5, u) && C ("Hammer of the Righteous", u);
		}

		public bool CrusaderStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Crusader Strike") && Range (5, u) && C ("Crusader Strike", u);
		}

		public bool LightsHammer (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Light's Hammer") && Range (30, u) && C ("Light's Hammer", u);
		}

		public bool HammerofWrath (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Hammer of Wrath") && Range (30, u) && C ("Hammer of Wrath", u);
		}

		public bool Judgment (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Judgment") && Range (30, u) && C ("Judgment", u);
		}

		public bool HolyWrath ()
		{
			return Usable ("Holy Wrath") && CS ("Holy Wrath");
		}

		public bool Cleanse (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Cleanse") && Range (40, u) && C ("Cleanse", u);
		}

		public bool RighteousFury ()
		{
			return Usable ("Righteous Fury") && CS ("Righteous Fury");
		}

		public bool Rebuke (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Rebuke") && Range (5, u) && C ("Rebuke", u);
		}

		public bool FistofJustice (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Fist of Justice") && Range (20, u) && C ("Fist of Justice", u);
		}
	}
}

