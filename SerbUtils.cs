﻿using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Newtonsoft.Json;
using ReBot;
using ReBot.API;

namespace ReBot
{
	public abstract class SerbUtils : SerbHashes
	{
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;

		const int BossHealthPercentage = 500;
		const int BossLevelIncrease = 5;
		public DateTime StartBattle;
		public DateTime StartSleepTime;
		public DateTime StartRun;
		public bool InCombat;
		//		public UnitObject Unit;
		public UnitObject InterruptTarget;
		//		public PlayerObject Player;
		public IEnumerable<UnitObject> MaxCycle;
		public String RangedAttack = "Throw";
		public bool InRun;

		public enum Keys
		{
			Shift,
			Ctrl,
			Alt,
			None
		}

		public enum Facing
		{
			Off,
			CurrentTarget,
			TargetAndSelect
		}

		// Getters

		public double TimeRun {
			get {
				TimeSpan RunTime = DateTime.Now.Subtract (StartRun);
				return RunTime.TotalSeconds;
			}
		}

		public double TimeToDie (UnitObject u = null)
		{
			u = u ?? Target;
			if (InGroup) {
				if (Ttd * PlayerFigthWithTarget (u) == 0)
					return u.Health / Ttd;
				return u.Health / (Ttd * PlayerFigthWithTarget (u));
			}
			return u.Health / Ttd;
		}

		public List<UnitObject> Enemy {
			get {
				var targets = Adds;
				targets.Add (Target);
				return targets;
			}
		}

		public int PlayerFigthWithTarget (UnitObject u)
		{
			int x = 0;
			foreach (PlayerObject p in MyParty) {
				if (!p.IsDead && u.IsPlayer && !IsHealer (p) && p.Target == u) {
					x++;
				}
			}
			return x;
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

		public int ActiveEnemiesPlayer (int range)
		{
			int x = 0;
			foreach (PlayerObject u in API.CollectUnits(range)) {
				if (u.IsPlayer && (u.IsEnemy || Me.Target == u) && !u.IsDead && u.IsAttackable && u.InCombat) {
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
				if ((u.IsEnemy || Me.Target == u) && !u.IsDead && u.IsAttackable && Vector3.Distance (t.Position, u.Position) <= r) {
					x++;
				}
			}
			return x;
		}

		public PlayerObject EnemyPlayerTargetToMe {
			get {
				foreach (PlayerObject u in API.CollectUnits(45)) {
					if (!u.IsDead && u.IsEnemy && u.Target == Me) {
						return u;
					}
				}
				return null;
			}
		}

		public int MaxPower {
			get {
				return API.ExecuteLua <int> ("return UnitPowerMax(\"player\");");
			}
		}

		public double Health (UnitObject u = null)
		{
			u = u ?? Target;
			return u.HealthFraction;
		}

		public int Energy {
			get {
				return Me.GetPower (WoWPowerType.Energy);
			}
		}

		public int RunicPower {
			get { 
				return Me.GetPower (WoWPowerType.RunicPower);
			}
		}

		public int Rage {
			get {
				return Me.GetPower (WoWPowerType.Rage);
			}
		}

		public int ComboPoints {
			get {
				return Me.ComboPoints;
			}
		}

		public int Anticipation {
			get {
				return SpellCharges ("Anticipation");
			}
		}

		public double Mana (UnitObject u = null)
		{
			u = u ?? Me;
			return u.ManaFraction;
		}

		public int HolyPower {
			get {
				return Me.GetPower (WoWPowerType.PaladinHolyPower);
			}
		}

		public int DemonicFury { 
			get { 
				return Me.GetPower (WoWPowerType.WarlockDemonicFury);
			}
		}

		public int BurningEmbers {
			get {
				return Me.GetPower (WoWPowerType.WarlockDestructionBurningEmbers);
			}
		}

		public int Eclipse {
			get {
				return API.ExecuteLua<int> ("return UnitPower('player', SPELL_POWER_ECLIPSE)");
			}
		}

		public string EclipseDirection {
			get {
				return API.ExecuteLua<string> ("return GetEclipseDirection()");
			}
		}

		public int Focus {
			get {
				return Me.GetPower (WoWPowerType.Focus);
			}
		}

		public int FocusDeflict {
			get {
				return MaxPower - Focus;
			}
		}

		public double EnergyTimeToMax {
			get {
				return MaxPower - Energy / RegenPower;
			}
		}

		public double EclipseChange {
			get {
				double timeToChange = 20;
				if (EclipseDirection == "sun") {
					if (Eclipse > 0 && Eclipse <= 100)
						timeToChange = 10 + (100 - Eclipse) / 10;
					if (Eclipse > -100 && Eclipse < 0)
						timeToChange = (0 - Eclipse) / 10;
				} 
				if (EclipseDirection == "moon") {
					if (Eclipse > 0 && Eclipse < 100)
						timeToChange = Eclipse / 10;
					if (Eclipse >= -100 && Eclipse < 0)
						timeToChange = 10 + (100 + Eclipse) / 10;
				}
				if (Eclipse == 0)
					timeToChange = 20;
				return timeToChange;
			}
		}

		public int Orb {
			get {
				return Me.GetPower (WoWPowerType.PriestShadowOrbs);
			}
		}

		public int Chi {
			get { return Me.GetPower (WoWPowerType.MonkLightForceChi); }
		}

		public int ChiMax {
			get {
				int Max = 4;
				if (HasSpell ("Ascension"))
					Max = Max + 1;
				if (HasSpell ("Empowered Chi"))
					Max = Max + 1;
				return Max;
			}
		}

		public double RegenPower {
			get {
				string activeRegen = API.ExecuteLua<string> ("inactiveRegen, activeRegen = GetPowerRegen(); return activeRegen");
				return Convert.ToDouble (activeRegen);
			}
		}

		public double Time {
			get {
				TimeSpan combatTime = DateTime.Now.Subtract (StartBattle);
				return combatTime.TotalSeconds;
			}
		}

		public double SleepTime {
			get {
				TimeSpan currentSleepTime = DateTime.Now.Subtract (StartSleepTime);
				return currentSleepTime.TotalSeconds;
			}
		}

		public double TimeToRegenPower (double e)
		{ 
			return e > Energy ? (e - Energy) / RegenPower : 0;
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public double CooldownById (Int32 i)
		{ 
			return SpellCooldown (i) < 0 ? 0 : SpellCooldown (i);
		}

		public double CastTime (Int32 i)
		{
			return API.ExecuteLua<double> ("local _, _, _, castTime, _, _ = GetSpellInfo(" + i + "); return castTime;");
		}

		public double DamageTaken (float t)
		{
			var damage = API.ExecuteLua<double> ("local ResolveName = GetSpellInfo (158300);local n,_,_,_,_,dur,expi,_,_,_,id,_,_,_,val1,val2,val3 = UnitAura (\"player\", ResolveName, nil, \"HELPFUL\");return val2");
			if (Time < 10) {
				if (Time < t / 1000)
					return damage;
				return damage / Time * (t / 1000);
			}
			return damage / 10 * (t / 1000);
		}

		public UnitObject BestAOETarget (int spellRange, int aoeRange, int minCount)
		{
			var Unit = Enemy.Where (u => Range (spellRange, u)).OrderByDescending (u => Enemy.Count (o => Vector3.Distance (u.Position, o.Position) <= aoeRange)).DefaultIfEmpty (null).FirstOrDefault ();
			if (Unit != null) {
				if (Enemy.Where (u => Vector3.Distance (u.Position, Unit.Position) <= aoeRange).ToList ().Count >= minCount)
					return Unit;
			}
			return null;
		}

		public PlayerObject BestAOEPlayer (int spellRange, int aoeRange, int minCount, double minHealth = 1)
		{
			var Player = MyParty.Where (u => Range (spellRange, u) && Health (u) <= minHealth).OrderByDescending (u => MyParty.Count (o => Vector3.Distance (u.Position, o.Position) <= aoeRange)).DefaultIfEmpty (null).FirstOrDefault ();
			if (Player != null) {
				if (Enemy.Where (u => Vector3.Distance (u.Position, Player.Position) <= aoeRange).ToList ().Count >= minCount)
					return Player;
			}
			return null;
		}

		public int GetAuraStack (string s, UnitObject u = null)
		{
			u = u ?? Target;
			if (!u.HasAura (s, true))
				return 0;
			return u.GetAura (s, true).StackCount;
		}

		// Checkers

		public bool C (string s, UnitObject u = null, bool t = false)
		{
			u = u ?? Target;
			if (Cast (s, u)) {
				if (t)
					API.Print ("--- Cast " + s + " on " + u.Name);
				return true;
			}
			API.Print ("False Cast " + s + " with " + u.CombatRange + " range on " + u.Name);
			return false;
		}

		public bool CPD (string s, UnitObject u = null, int d = 800, bool t = false)
		{
			u = u ?? Target;
			if (CastPreventDouble (s, null, u, d)) {
				if (t)
					API.Print ("--- CastPreventDouble " + s + " on " + u.Name);
				return true;
			}
			API.Print ("False CastPreventDouble " + s + " with " + u.CombatRange + " range " + d + " delay on " + u.Name);
			return false;
		}

		public bool CS (string s, bool t = false)
		{
			if (CastSelf (s)) {
				if (t)
					API.Print ("--- CastSelf " + s + " on self");
				return true;
			}
			API.Print ("False CastSelf " + s);
			return false;
		}

		public bool CSPD (string s, int d = 800, bool t = false)
		{
			if (CastSelfPreventDouble (s, null, d)) {
				if (t)
					API.Print ("--- CastSelfPreventDouble " + s + " on self " + d);
				return true;
			}
			API.Print ("False CastSelfPreventDouble " + s + " delay " + d);
			return false;
		}

		public bool COT (string s, UnitObject u = null, bool t = false)
		{
			u = u ?? Target;
			if (CastOnTerrain (s, u.Position)) {
				if (t)
					API.Print ("--- CastOnTerrain " + s + " on " + u.Name);
				return true;
			}
			API.Print ("False CastOnTerrain " + s + " with " + u.CombatRange + " range on " + u.Name);
			return false;
		}

		public bool COTPD (string s, UnitObject u = null, int d = 800, bool t = false)
		{
			u = u ?? Target;
			if (CastOnTerrainPreventDouble (s, u.Position, null, d)) {
				if (t)
					API.Print ("--- CastOnTerrainPreventDouble " + s + " on " + u.Name);
				return true;
			}
			API.Print ("False CastOnTerrainPreventDouble " + s + " with " + u.CombatRange + " range on " + u.Name);
			return false;
		}

		public bool Range (int r, UnitObject u = null, int l = 0)
		{
			u = u ?? Target;
			if (l != 0)
				return u.IsInLoS && u.CombatRange <= r && u.CombatRange >= l;
			return u.IsInLoS && u.CombatRange <= r;
		}

		public bool Range (UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsInCombatRangeAndLoS;
		}

		public bool Danger (UnitObject u = null, int r = 0, int e = 2)
		{
			u = u ?? Target;
			if (Me.IsHealer)
				return true;
			if (r != 0)
				return Range (r, u) && (IsBoss (u) || IsPlayer (u) || ActiveEnemies (10) > e || Health (Me) < 0.5);
			return u.IsInCombatRangeAndLoS && (IsBoss (u) || IsPlayer (u) || ActiveEnemies (10) > e || Health (Me) < 0.5);
		}

		public bool DangerBoss (UnitObject u = null, int r = 0, int e = 6)
		{
			u = u ?? Target;
			if (Me.IsHealer)
				return true;
			if (r != 0)
				return Range (r, u) && (IsBoss (u) || IsPlayer (u) || ActiveEnemies (10) > e || Health (Me) < 0.2);
			return u.IsInCombatRangeAndLoS && (IsBoss (u) || IsPlayer (u) || ActiveEnemies (10) > e || Health (Me) < 0.2);
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

		public bool InPG {
			get {
				return API.MapInfo.Name.Contains ("Proving Grounds");
			}
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

		public bool IsInEnrage (UnitObject u = null)
		{
			u = u ?? Target;
			return u.Auras.Any (a => EnrageSkill.Contains (a.Name));
		}

		public bool IsNotForDamage (UnitObject o = null)
		{
			o = o ?? Target;
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

		public bool Usable (string s, double d = 0)
		{ 
			if (d == 0)
				return HasSpell (s) && Cooldown (s) <= 0;
			return HasSpell (s) && Cooldown (s) <= d;
		}

		public bool UsableItem (int i)
		{ 
			return API.HasItem (i) && API.ItemCooldown (i) <= 0;
		}

		public bool MeIsBusy {
			get {
				if (HasGlobalCooldown ())
					return true;
				if (Me.IsMounted || Me.IsFlying || Me.IsOnTaxi || Me.IsMoving)
					return true;
				if (Me.HasAura ("Drink") || Me.HasAura ("Food"))
					return true;
				if (Me.IsChanneling)
					return true;
				if (Me.IsCasting)
					return true;
				if (Me.HasAura ("Feign Death"))
					return true; 

				return false;
			}
		}

		public bool InDef (UnitObject u = null)
		{
			u = u ?? Target;
			return u.Auras.Any (a => DefAura.Contains (a.Name));
		}

		// Party

		public PlayerObject Healer {
			get {
				return Healers.DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		public IEnumerable<PlayerObject> Healers {
			get {
				return MyGroup.Where (p => IsHealer (p)).Distinct ();
			}
		}

		public bool IsTank (PlayerObject unit)
		{
			return Tanks ().Contains (unit); 
		}

		public bool IsHealer (PlayerObject u)
		{
			return Healers.Contains (u); 
		}

		public IEnumerable<PlayerObject> Tanks ()
		{
//			if (InPG) {
//				return API.Units.Where (p => p.Name == "Oto the Protector");
//			}
			return Group.GetGroupMemberObjects ().Where (p => p.IsTank).Distinct ();
		}

		public PlayerObject Tank {
			get {
				foreach (PlayerObject u in Tanks()) {
					if (u != null && !u.IsDead && Range (40, u) && u.InCombat)
						return u;
				}
				return null;
			}
		}

		public PlayerObject Tank2 {
			get {
				foreach (PlayerObject u in Tanks()) {
					if (u != null && !u.IsDead && Range (40, u) && u.InCombat)
						return u;
				}
				return null;
			}
		}

		public UnitObject TankTarget {
			get {
				if (Tank != null && Tank.Target != null && !Tank.Target.IsDead && Tank.Target.IsEnemy && Tank.Target.InCombat && Range (40, Tank.Target))
					return Tank.Target;
				return null;
			}
		}

		public bool InGroup {
			get {
				return MyGroup.Count > 0;
			}
		}

		public int GroupMemberCount {
			get {
				return Group.GetNumGroupMembers ();
			}
		}

		public List<PlayerObject> MyGroup {
			get {
				return Group.GetGroupMemberObjects ();
			}
		}

		public IEnumerable<PlayerObject> MyParty {
			get {
				List<PlayerObject> Units;
				Units = MyGroup;
				Units.Add (Me);
				return Units.Distinct ();
			}
		}

		public IEnumerable<UnitObject> PGParty {
			get {
				var pgGroup = new List<UnitObject> ();
				var Units = API.Units.Where (p => p != null && !p.IsDead && p.IsValid).ToList ();
				if (Units.Any ()) {
					foreach (var PGUnit in Units) {
						if (PgUnits.Contains (PGUnit.Name)) {
							pgGroup.Add (PGUnit);
						}
					}
				}
				pgGroup.Add (Me);
				return pgGroup.Distinct ();
			}
		}

		public PlayerObject LowestPlayer {
			get {
				return MyParty.Where (u => !u.IsDead && Range (40, u) && Health (u) < 1).OrderBy (u => Health (u)).DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		public PlayerObject Lowest (double h, int r = 40)
		{
			return MyParty.Where (p => !p.IsDead && Health (p) < h && Range (r, p)).OrderBy (p => Health (p)).DefaultIfEmpty (null).FirstOrDefault ();
		}

		public PlayerObject LowestNoAura (double h, string a, int r = 40)
		{
			return MyParty.Where (p => !p.IsDead && Health (p) < h && Range (r, p) && !p.HasAura (a, true)).OrderBy (p => Health (p)).DefaultIfEmpty (null).FirstOrDefault ();
		}


		public PlayerObject FocusTankorMe (double h, int r = 40)
		{
			if (Me.Focus != null && Me.Focus.IsFriendly) {
				if (Me.Focus.IsPlayer && !Me.Focus.IsDead && Range (r, Me.Focus) && Health (Me.Focus) <= h)
					return (PlayerObject)Me.Focus;
			} else if (Tank != null) {
				if (!Tank.IsDead && Range (r, Tank) && Health (Tank) <= h)
					return Tank;
			} else if (Health (Me) <= h) {
				return Me;
			}
			return null;
		}

		public PlayerObject FocusTankorLowest (double h, int r = 40)
		{
			if (Me.Focus != null && Me.Focus.IsFriendly) {
				if (Me.Focus.IsPlayer && !Me.Focus.IsDead && Range (r, Me.Focus) && Health (Me.Focus) <= h)
					return (PlayerObject)Me.Focus;
			} else if (Tank != null) {
				if (!Tank.IsDead && Range (r, Tank) && Health (Tank) <= h)
					return Tank;
			}
			return Lowest (h);
		}

		public PlayerObject FocusTankorLowestNoAura (double h, string a, int r = 40)
		{
			if (Me.Focus != null && Me.Focus.IsFriendly) {
				if (Me.Focus.IsPlayer && !Me.Focus.IsDead && Range (r, Me.Focus) && Health (Me.Focus) <= h && !Me.Focus.HasAura (a, true))
					return (PlayerObject)Me.Focus;
			} else if (Tank != null) {
				if (!Tank.IsDead && Range (r, Tank) && Health (Tank) <= h && !Tank.HasAura (a, true))
					return Tank;
			}
			return LowestNoAura (h, a);
		}

		public PlayerObject FocusTankorMeNoAura (double h, string a, int r = 40)
		{
			if (Me.Focus != null && Me.Focus.IsFriendly) {
				if (Me.Focus.IsPlayer && !Me.Focus.IsDead && Range (r, Me.Focus) && Health (Me.Focus) <= h && !Me.Focus.HasAura (a, true))
					return (PlayerObject)Me.Focus;
			} else if (Tank != null) {
				if (!Tank.IsDead && Range (r, Tank) && Health (Tank) <= h && !Tank.HasAura (a, true))
					return Tank;
			} else if (Health (Me) <= h && !Me.HasAura (a, true)) {
				return Me;
			}
			return null;
		}

		public int LowestPlayerCount (double h, int r = 40)
		{
			return MyParty.Count (p => Health (p) < h && Range (r, p));
		}

		public int AOECount {
			get {
				return GroupMemberCount > 5 ? 6 : 3;
			}
		}

		public double GetDR (double dungeon, double raid)
		{
			return GroupMemberCount > 5 ? raid : dungeon;
		}

		// Scripts

		public void BeerTimersInit ()
		{
			if (API.ExecuteLua<int> ("return BeerTimerInit;") != 1)
				API.ExecuteLua ("local f = CreateFrame(\"Frame\");" +
				"BeerTimer = 0;" +
				"BeerTimerInit = 1;" +
				"f:RegisterEvent(\"CHAT_MSG_ADDON\");" +
				"f:SetScript(\"OnEvent\", function(self, event, prefix, msg, channel, sender) if prefix == \"D4\" then local dbmPrefix, arg1, arg2, arg3, arg4 = strsplit(\"\t\", msg); if dbmPrefix == \"PT\" then BeerTimer = arg1 end end end);" +
				"f:SetScript(\"OnUpdate\", function(self, e) BeerTimer = BeerTimer - e; if BeerTimer < 0 then BeerTimer = 0 end end);");
		}

		// Combos

		public bool Freedom ()
		{
			return WilloftheForsaken () || EveryManforHimself ();
		}

		public void CheckTargetFacing (Facing autoFacing, int r)
		{
			if (API.CombatSettings.RotationOnly && autoFacing != Facing.Off) {
				if (Me.Target != null)
					API.SetFacing (Me.Target);
				else if (autoFacing == Facing.TargetAndSelect) {
					var Unit = API.CollectUnits (u => u.IsAttackable && Range (r, u) && u.IsTargetingMeOrPets && u.CanParticipateInCombat && u.InCombat).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						Me.SetTarget (Unit);
						API.SetFacing (Unit);
					}
				}
			}
		}

		// Spells

		public bool WilloftheForsaken ()
		{
			return Usable ("Will of the Forsaken") && CS ("Will of the Forsaken");
		}

		public bool EveryManforHimself ()
		{
			return Usable ("Every Man for Himself") && CS ("Every Man for Himself");
		}

		public bool BloodFury (UnitObject u = null, int r = 8)
		{
			u = u ?? Target;
			return Usable ("Blood Fury") && Danger (u, r) && CS ("Blood Fury");
		}

		public bool Berserking (UnitObject u = null, int r = 8)
		{
			u = u ?? Target;
			return Usable ("Berserking") && Danger (u, r) && CS ("Berserking");
		}

		public bool ArcaneTorrent (UnitObject u = null, int r = 8)
		{
			u = u ?? Target;
			return Usable ("Arcane Torrent") && Danger (u, r) && CS ("Arcane Torrent");
		}


		// Items

		public bool Healthstone ()
		{
			return UsableItem (5512) && API.ItemCooldown (5512) == 0 && API.UseItem (5512);
		}

		public bool CrystalOfInsanity ()
		{
			return !InArena && UsableItem (86569) && !HasAura ("Visions of Insanity") && API.UseItem (86569);
		}

		public bool OraliusWhisperingCrystal ()
		{
			return UsableItem (118922) && !HasAura ("Whispers of Insanity") && API.UseItem (118922);
		}

		public bool DraenicArmor ()
		{
			return UsableItem (109220) && !Me.HasAura ("Draenic Armor Potion") && API.UseItem (109220);
		}

		public bool DraenicIntellect ()
		{
			return UsableItem (109218) && !Me.HasAura ("Draenic Intellect Potion") && API.UseItem (109218);
		}

		public bool EternalWilloftheMartyr ()
		{
			return UsableItem (122668) && API.UseItem (122668);
		}

		public bool DraenicStrength ()
		{
			return UsableItem (109219) && !Me.HasAura ("Draenic Strength Potion") && API.UseItem (109219);
		}
	}
}

