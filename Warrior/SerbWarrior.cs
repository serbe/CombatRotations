using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ReBot.API;
using System.Linq;

namespace ReBot
{
	public abstract class SerbWarrior : CombatRotation
	{
		public enum WarCry
		{
			CommandingShout = 0,
			BattleShout = 1,
		}

		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("Auto change stance")]
		public bool UseStance = true;

		public bool InCombat;
		public bool WaitBloodthirst;
		public DateTime StartBattle;
		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public UnitObject CycleTarget;
		public Int32 OraliusWhisperingCrystalID = 118922;
		public Int32 CrystalOfInsanityID = 86569;
		public DateTime PrevBloodthirst;

		// Get

		public bool PrevGcdBloodthirst {
			get {
				TimeSpan CombatTime = DateTime.Now.Subtract (PrevBloodthirst);
				return CombatTime.TotalSeconds < 3;
			}
		}

		public bool AttackPowerBuff {
			get {
				return Me.HasAura ("Battle Shout") || Me.HasAura ("Horn of Winter");
			}
		}


		public List<UnitObject> Enemy {
			get {
				var targets = Adds;
				targets.Add (Target);
				return targets;
			}
		}

		public double TimeToDie (UnitObject u = null)
		{
			u = u ?? Target;
			return u.Health / Ttd;
		}

		public int Rage {
			get {
				return Me.GetPower (WoWPowerType.Rage);
			}
		}

		public int ActiveEnemies (int range)
		{
			int x = 0;
			foreach (UnitObject u in API.CollectUnits(range)) {
				if ((u.IsEnemy || Me.Target == u) && !u.IsDead && u.IsAttackable && u.InCombat) {
					x++;
				}
			}
			return x;
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
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

		public double Health (UnitObject u = null)
		{
			u = u ?? Me;
			return u.HealthFraction;
		}

		public double Time {
			get {
				TimeSpan combatTime = DateTime.Now.Subtract (StartBattle);
				return combatTime.TotalSeconds;
			}
		}

		// Check

		public bool C (string s, UnitObject u = null)
		{
			u = u ?? Target;
			if (Cast (s, u))
				return true;
			API.Print ("False Cast " + s + " with " + u.CombatRange + " range");
			return false;
		}

		public bool CPD (string s, UnitObject u = null, int d = 800)
		{
			u = u ?? Target;
			if (CastPreventDouble (s, null, u, d))
				return true;
			API.Print ("False CastPreventDouble " + s + " with " + u.CombatRange + " range " + d + " delay");
			return false;
		}

		public bool CS (string s)
		{
			if (CastSelf (s))
				return true;
			API.Print ("False CastSelf " + s);
			return false;
		}

		public bool COT (string s, UnitObject u = null)
		{
			u = u ?? Target;
			if (CastOnTerrain (s, u.Position))
				return true;
			API.Print ("False CastOnTerrain " + s + " with " + u.CombatRange + " range");
			return false;
		}

		public bool COTPD (string s, UnitObject u = null)
		{
			u = u ?? Target;
			if (CastOnTerrainPreventDouble (s, u.Position))
				return true;
			API.Print ("False CastOnTerrain " + s + " with " + u.CombatRange + " range");
			return false;
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

		public bool DangerBoss (UnitObject u = null, int r = 0, int e = 6)
		{
			u = u ?? Target;
			if (r != 0)
				return Range (r, u) && (IsBoss (u) || IsPlayer (u) || ActiveEnemies (10) > e);
			return u.IsInCombatRangeAndLoS && (IsBoss (u) || IsPlayer (u) || ActiveEnemies (10) > e);
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

		public bool HasRage (int r)
		{
			if (Me.HasAura ("Spirits of the Lost"))
				r = r - 5;
			return r <= Rage;
		}

		public bool Usable (string s)
		{ 
			return HasSpell (s) && Cooldown (s) == 0;
		}

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

		// Combo


		public bool Buff (WarCry shout)
		{
			if (shout == WarCry.CommandingShout && (AttackPowerBuff || !Me.HasAura ("Commanding Shout")) && CS ("Commanding Shout"))
				return true;
			if (shout == WarCry.BattleShout && !AttackPowerBuff && CS ("Battle Shout"))
				return true;

			return false;
		}

		public bool Interrupt ()
		{
			if (Usable ("Pummel")) {
				CycleTarget = Enemy.Where (u => u.IsCastingAndInterruptible () && Range (6, u) && u.RemainingCastTime > 0 && (u.Target == Me && !Me.HasAura ("Spell Reflect")) && !Me.HasAura ("Mass Spell Reflection")).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null && Pummel (CycleTarget))
					return true;
			}
			if (Usable ("Storm Bolt")) {
				CycleTarget = Enemy.Where (u => u.IsCasting && !IsBoss (u) && Range (30, u) && u.RemainingCastTime > 0 && (u.Target == (UnitObject)Me && !Me.HasAura ("Spell Reflect")) && !Me.HasAura ("Mass Spell Reflection")).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null && StormBolt (CycleTarget))
					return true;
			}

			return false;
		}

		public bool Reflect ()
		{
			if (Usable ("Spell Reflection") && !HasGlobalCooldown ()) {
				CycleTarget = Enemy.Where (u => u.IsCasting && u.RemainingCastTime > 0 && u.Target == Me && !Me.HasAura ("Mass Spell Reflection")).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null && SpellReflection ())
					return true;
			}
			if (Usable ("Mass Spell Reflection") && !HasGlobalCooldown ()) {
				CycleTarget = Enemy.Where (u => u.IsCasting && u.RemainingCastTime > 0 && u.Target == Me && !Me.HasAura ("Spell Reflection")).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null && MassSpellReflection ())
					return true;
			}

			return false;
		}

		// Items

		public bool DraenicArmor ()
		{
			if (API.HasItem (109220) && API.ItemCooldown (109220) == 0 && !Me.HasAura ("Draenic Armor Potion"))
				return API.UseItem (109220);
			return false;
		}

		public bool Healthstone ()
		{
			if (API.HasItem (5512) && API.ItemCooldown (5512) == 0)
				return API.UseItem (5512);
			return false;
		}

		public bool CrystalOfInsanity ()
		{
			if (!InArena && API.HasItem (CrystalOfInsanityID) && !HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityID) == 0)
				return API.UseItem (CrystalOfInsanityID);
			return false;
		}

		public bool OraliusWhisperingCrystal ()
		{
			if (API.HasItem (OraliusWhisperingCrystalID) && !HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalID) == 0)
				return API.UseItem (OraliusWhisperingCrystalID);
			return false;
		}

		// ------- Spells

		public bool Charge (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Charge") && (Range (25, u, 8) || (HasGlyph (58097) && Range (30, u, 8))) && C ("Charge", u);
		}

		public bool BloodFury ()
		{
			return Usable ("Blood Fury") && Danger () && CS ("Blood Fury");
		}

		public bool MassSpellReflection ()
		{
			return Usable ("Mass Spell Reflection") && CS ("Mass Spell Reflection");
		}

		public bool SpellReflection ()
		{
			return Usable ("Spell Reflection") && CS ("Spell Reflection");
		}

		public bool DiebytheSword ()
		{
			return Usable ("Die by the Sword") && CS ("Die by the Sword");
		}

		public bool Berserking ()
		{
			return Usable ("Berserking") && Danger () && CS ("Berserking");
		}

		public bool ArcaneTorrent ()
		{
			return Usable ("Arcane Torrent") && Danger () && CS ("Arcane Torrent");
		}

		public bool BerserkerRage ()
		{
			return Usable ("Berserker Rage") && Target.IsInLoS && CS ("Berserker Rage");
		}

		public bool ShieldBlock ()
		{
			return Usable ("Shield Block") && HasRage (60) && CS ("Shield Block");
		}

		public bool ShieldBarrier ()
		{
			return Usable ("Shield Barrier") && HasRage (20) && CS ("Shield Barrier");
		}

		public bool DemoralizingShout ()
		{
			return Usable ("Demoralizing Shout") && CS ("Demoralizing Shout");
		}

		public bool EnragedRegeneration ()
		{
			return Usable ("Enraged Regeneration") && CS ("Enraged Regeneration");
		}

		public bool ShieldWall ()
		{
			return Usable ("Shield Wall") && CS ("Shield Wall");
		}

		public bool LastStand ()
		{
			return Usable ("Last Stand") && CS ("Last Stand");
		}

		public bool Stoneform ()
		{
			return Usable ("Stoneform") && CS ("Stoneform");
		}

		public bool HeroicStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Heroic Strike") && HasRage (30) && u.IsInCombatRangeAndLoS && C ("Heroic Strike", u);
		}

		public bool Bloodbath ()
		{
			return Usable ("Bloodbath") && CS ("Bloodbath");
		}

		public bool Avatar ()
		{
			return Usable ("Avatar") && Danger () && CS ("Avatar");
		}

		public bool ShieldSlam (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shield Slam") && u.IsInCombatRangeAndLoS && C ("Shield Slam", u);
		}

		public bool Revenge (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Revenge") && u.IsInCombatRangeAndLoS && C ("Revenge", u);
		}

		public bool Ravager (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Ravager") && u.IsInCombatRangeAndLoS && C ("Ravager", u);
		}

		public bool StormBolt (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Storm Bolt") && Range (30, u) && C ("Storm Bolt", u);
		}

		public bool DragonRoar ()
		{
			return Usable ("Dragon Roar") && Target.IsInCombatRangeAndLoS && CS ("Dragon Roar");
		}

		public bool ImpendingVictory (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Impending Victory") && HasRage (10) && u.IsInCombatRangeAndLoS && C ("Impending Victory", u);
		}

		public bool VictoryRush (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Victory Rush") && u.IsInCombatRangeAndLoS && Me.HasAura ("Victorious") && C ("Victory Rush", u);
		}

		public bool Execute (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Execute") && u.IsInCombatRangeAndLoS && ((HasRage (30) && Health (u) <= 0.2) || Me.HasAura ("Sudden Death")) && C ("Execute", u);
		}

		public bool Devastate (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Devastate") && u.IsInCombatRangeAndLoS && C ("Devastate", u);
		}

		public bool ThunderClap (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Thunder Clap") && (Range (8, u) || (HasGlyph (63324) && Range (12, u))) && CS ("Thunder Clap");
		}

		public bool Bladestorm ()
		{
			return Usable ("Bladestorm") && Danger () && CS ("Bladestorm");
		}

		public bool Shockwave (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shockwave") && Range (10, u) && C ("Shockwave", u);
		}

		public bool ShieldCharge (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shield Charge") && HasRage (20) && Range (10, u) && C ("Shield Charge", u);
		}

		public bool HeroicLeap (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Heroic Leap") && Range (40, u, 8) && ((HasGlyph (63325) && Range (25, u)) || Range (40, u)) && COT ("Heroic Leap", u);
		}

		public bool HeroicThrow (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Heroic Throw") && Range (30, u, 8) && CPD ("Heroic Throw", u, 2000);
		}

		public bool DefensiveStance ()
		{
			return Usable ("Defensive Stance") && !(Me.HasAura ("Defensive Stance") || Me.HasAura ("Improved Defensive Stance")) && CS ("Defensive Stance");
		}

		public bool BattleStance ()
		{
			return Usable ("Battle Stance") && !IsInShapeshiftForm ("Battle Stance") && CS ("Battle Stance");
		}

		public bool RallyingCry ()
		{
			return Usable ("Rallying Cry") && CS ("Rallying Cry");
		}

		public bool Recklessness ()
		{
			return Usable ("Recklessness") && DangerBoss () && CS ("Recklessness");
		}

		public bool WildStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Wild Strike") && (Rage >= 45 || (Me.HasAura ("Furious Strikes") && Rage >= 20)) && Range (5, u) && C ("Wild Strike", u);
		}

		public bool Bloodthirst (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Bloodthirst") && Range (5, u) && C ("Bloodthirst", u);
		}

		public bool Siegebreaker (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Siegebreaker") && Range (5, u) && C ("Siegebreaker", u);
		}

		public bool RagingBlow (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Raging Blow") && Rage >= 10 && Range (5, u) && C ("Raging Blow", u);
		}

		public bool Pummel (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Pummel") && Range (5, u) && C ("Pummel", u);
		}

		public bool Whirlwind (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Whirlwind") && HasRage (20) && (Range (8, u) || (HasGlyph (63324) && Range (12, u))) && C ("Whirlwind", u);
		}
	}
}
