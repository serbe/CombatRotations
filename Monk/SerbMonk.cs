using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot
{
	public abstract class SerbMonk : CombatRotation
	{
		// Vars Consts

		[JsonProperty ("Maximum Energy")] 
		public int MaxEnergy = 100;
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;
		[JsonProperty ("Time run to use Tiger Lust")]
		public double TTL = 1;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public Int32 OraliusWhisperingCrystalID = 118922;
		public Int32 CrystalOfInsanityID = 86569;
		public DateTime StartBattle;
		public DateTime StartRun;
		public bool InRun;
		public bool InCombat;
		public UnitObject Unit;
		public PlayerObject Player;
		Random Rnd = new Random ();

		// Get

		public List<PlayerObject> Members {
			get {
				return Group.GetGroupMemberObjects ();
			}
		}

		public PlayerObject[] GrWMe {
			get {
				return Group.GetGroupMemberObjects ().Concat (new[] { Me }).ToArray ();
			}
		}

		public PlayerObject LowHp {
			get {
				return GrWMe.OrderBy (p => p.HealthFraction).DefaultIfEmpty (null).FirstOrDefault ((p => p.Health > 0));
			}
		}

		public double EnergyRegen {
			get {
				string activeRegen = API.ExecuteLua<string> ("inactiveRegen, activeRegen = GetPowerRegen(); return activeRegen");
				return Convert.ToDouble (activeRegen);
			}
		}

		public double TimeToMaxEnergy {
			get {
				return (MaxEnergy - Energy) / EnergyRegen;
			}
		}

		public double Health (UnitObject u = null)
		{
			u = u ?? Target;
			return u.HealthFraction;
		}

		public double Mana (UnitObject u = null)
		{
			u = u ?? Target;
			return u.ManaFraction;
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

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) > 0 ? SpellCooldown (s) : 0;
		}

		public int Energy { 
			get {
				return Me.GetPower (WoWPowerType.Energy);
			}
		}

		public double Time {
			get {
				TimeSpan CombatTime = DateTime.Now.Subtract (StartBattle);
				return CombatTime.TotalSeconds;
			}
		}

		public double TimeRun {
			get {
				TimeSpan RunTime = DateTime.Now.Subtract (StartRun);
				return RunTime.TotalSeconds;
			}
		}

		public double TimeToDie (UnitObject u = null)
		{
			u = u ?? Target;
			return u.Health / Ttd;
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

		public int ElusiveBrewStacks {
			get {
				foreach (var a in Me.Auras) {
					if (a.SpellId == 128939)
						return a.StackCount;
				}
				return  0;
			}
		}

		public List<UnitObject> Enemy {
			get {
				var targets = Adds;
				targets.Add (Target);
				return targets;
			}
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

		public PlayerObject Healer {
			get {
				return Group.GetGroupMemberObjects ().Where (p => !p.IsDead && p.IsHealer).DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		public PlayerObject Tank {
			get {
				return Group.GetGroupMemberObjects ().Where (p => !p.IsDead && p.IsTank).DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		public UnitObject ZenSphereTarget {
			get {
				if (Me.Focus != null) {
					if (Me.Focus.IsPlayer && !Me.Focus.IsDead && Range (40, Me.Focus) && !Me.Focus.HasAura ("Zen Sphere", true))
						return Me.Focus;
					return null;
				}
				if (Tank != null) {
					if (!Tank.IsDead && Range (40, Tank) && !Tank.HasAura ("Zen Sphere", true))
						return Tank;
					return null;
				}
				return null;
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

		public bool COTPD (string s, UnitObject u = null, int p = 800)
		{
			u = u ?? Target;
			if (CastOnTerrainPreventDouble (s, u.Position, null, p))
				return true;
			API.Print ("False CastOnTerrain " + s + " with " + u.CombatRange + " range");
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

		// Combo

		public bool MassDispel ()
		{
			if (Members.Count > 0) {
				foreach (PlayerObject p in Members) {
					if (p.Auras.Any (x => x.IsDebuff && "Magic,Poison,Disease".Contains (x.DebuffType)) && Detox (p))
						return true;
				}
			}
			if (Me.Auras.Any (x => x.IsDebuff && "Magic,Poison,Disease".Contains (x.DebuffType)) && Detox (Me))
				return true;
			return false;
		}

		public bool MassResurect ()
		{
			if (CurrentBotName == "Combat" && Members.Count > 0) {
				Player = Members.FirstOrDefault (x => x.IsDead);
				if (Player != null && Resuscitate (Player))
					return true;
			}
			return false;
		}

		public bool Buff (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Legacy of the Emperor")) {
				if (Range (40, u) && u.AuraTimeRemaining ("Legacy of the Emperor") < 300 && !u.HasAura ("Blessing of Kings") && !u.HasAura ("Mark of the Wild") && !u.HasAura ("Legacy of the White Tiger") && C ("Legacy of the Emperor", u))
					return true;
			}
			if (Usable ("Legacy of the White Tiger")) {
				if (Range (40, u) && u.AuraTimeRemaining ("Legacy of the White Tiger") < 300 && u.AuraTimeRemaining ("Blessing of Kings") < 300 && C ("Legacy of the White Tiger", u))
					return true;
			}	
			return false;
		}

		public bool Freedom ()
		{
			if (!Me.CanParticipateInCombat) {
				if (TigersLust ())
					return true;
				if (NimbleBrew ())
					return true;
				if (WilloftheForsaken ())
					return true;
				if (EveryManforHimself ())
					return true;
			}
			return false;
		}

		public bool Interrupt ()
		{
			if (Usable ("Leg Sweep") || Usable ("Spear Hand Strike") || Usable ("Ring of Peace")) {
				Unit = Enemy.Where (u => u.IsCastingAndInterruptible () && !IsBoss (u) && Range (5, u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && (SpearHandStrike (Unit) || LegSweep (Unit) || RingofPeace (Unit)))
					return true;
			}
			return false;
		}

		public bool AggroDizzyingHaze ()
		{
			if (Usable ("Dizzying Haze") && Healer != null) {
				Unit = Enemy.Where (u => u.InCombat && u.Target == Healer && Range (40, u, 8)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && DizzyingHaze (Unit))
					return true;
			}
			return false;
		}

		public bool HealStatue ()
		{
			if (!GrWMe.Any (p => p.InCombat))
				return false;
		
			const int StatueEntryID = 60849;
		
			var statue = API.Units.FirstOrDefault (u => u.EntryID == StatueEntryID && u.CreatedByMe);
			if (statue == null || statue.Distance > 35) {
				foreach (var u in GrWMe.Where(p => p.IsTank || p == Me)) {
					if (u != null && u.Distance < 20) {
						var pos = u.Position;
						for (int i = 0; i < 8; i++) {
							var StatuePos = pos;
							StatuePos.X += (float)Rnd.NextDouble () * 10 - 5;
							StatuePos.Y += (float)Rnd.NextDouble () * 10 - 5;
		
							if (SummonJadeSerpentStatue (StatuePos))
								return true;
						}
					}
				}
			}
			return false;
		}

		// Spell

		public bool SummonJadeSerpentStatue (Vector3 p)
		{
			if (Usable ("Crackling Jade Lightning") && Vector3.Distance (Me.Position, p) <= 40) {
				if (CastOnTerrainPreventDouble ("Crackling Jade Lightning", p, null, 2000))
					return true;
				API.Print ("False CastOnTerrain Crackling Jade Lightning with " + Vector3.Distance (Me.Position, p) + " range");
			}
			return false;
		}

		public bool CracklingJadeLightning (UnitObject u = null)
		{
			u = u ?? Me;
			return Usable ("Crackling Jade Lightning") && Range (40, u) && C ("Crackling Jade Lightning", u);
		}

		public bool ManaTea ()
		{
			return Usable ("Mana Tea") && AuraStackCount ("Mana Tea") > 0 && CS ("Mana Tea");
		}

		public bool RisingSunKick (UnitObject u = null)
		{
			u = u ?? Me;
			return Usable ("Rising Sun Kick") && Chi >= 2 && Range (5, u) && C ("Rising Sun Kick", u);
		}

		public bool RingofPeace (UnitObject u = null)
		{
			u = u ?? Me;
			return Usable ("Ring of Peace") && Range (40, u) && C ("Ring of Peace", u);
		}

		public bool Resuscitate (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Resuscitate") && Range (40, u) && C ("Resuscitate", u);
		}

		public bool DizzyingHaze (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Dizzying Haze") && Range (40, u) && COTPD ("Dizzying Haze", u, 2000);
		}

		public bool ChiBrew ()
		{
			return Usable ("Chi Brew") && ChiMax - Chi >= 2 && CS ("Chi Brew");
		}

		public bool BloodFury ()
		{
			return Usable ("Blood Fury") && Danger () && CS ("BloodFury");
		}

		public bool Berserking ()
		{
			return Usable ("Berserking") && Danger () && CS ("Berserking");
		}

		public bool ArcaneTorrent ()
		{
			return Usable ("Arcane Torrent") && Danger () && CS ("Arcane Torrent");
		}

		public bool NimbleBrew ()
		{
			return Usable ("Nimble Brew") && CS ("Nimble Brew");
		}

		public bool WilloftheForsaken ()
		{
			return Usable ("Will of the Forsaken") && CS ("Will of the Forsaken");
		}

		public bool EveryManforHimself ()
		{
			return Usable ("Every Man for Himself") && CS ("Every Man for Himself");
		}

		public bool OxStance ()
		{
			return Usable ("Stance of the Sturdy Ox") && !IsInShapeshiftForm ("Stance of the Sturdy Ox") && CS ("Stance of the Sturdy Ox");
		}

		public bool DampenHarm ()
		{
			return Usable ("Dampen Harm") && !Me.HasAura ("Dampen Harm") && CS ("Dampen Harm");
		}

		public bool FortifyingBrew ()
		{
			return Usable ("Fortifying Brew") && !Me.HasAura ("Fortifying Brew") && CS ("Fortifying Brew");
		}

		public bool ElusiveBrew ()
		{
			return Usable ("Elusive Brew") && CS ("Elusive Brew");
		}

		public bool InvokeXuentheWhiteTiger (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Invoke Xuen, the White Tiger") && Range (40, u) && DangerBoss () && C ("Invoke Xuen, the White Tiger", u);
		}

		public bool Serenity ()
		{
			return Usable ("Serenity") && CS ("Serenity");
		}

		public bool TouchofDeath (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Touch of Death") && (Chi >= 3 || HasGlyph (123391)) && ((IsBoss (u) && Health (u) < 0.1) || u.Health < Me.MaxHealth) && Range (5, u) && Me.HasAura ("Death Note") && C ("Touch of Death", u);
		}

		public bool PurifyingBrew ()
		{
			return Usable ("Purifying Brew") && (Chi >= 1 || Me.HasAura ("Purifier")) && CS ("Purifying Brew");
		}

		public bool BlackoutKick (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Blackout Kick") && (Chi >= 2 || Me.HasAura ("Combo Breaker: Blackout Kick")) && Range (5, u) && C ("Blackout Kick", u);
		}

		public bool ChiExplosion (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Chi Explosion") && (Chi >= 1 || Me.HasAura ("Combo Breaker: Chi Explosion")) && Range (40, u) && C ("Chi Explosion", u);
		}

		public bool Guard ()
		{
			return Usable ("Guard") && Chi >= 2 && SpellCharges ("Guard") > 0 && CS ("Guard");
		}

		public bool KegSmash (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Keg Smash") && Energy >= 40 && (Range (5, u) || (HasGlyph (159495) && Range (10, u))) && C ("Keg Smash", u);
		}

		public bool ChiBurst (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Chi Burst") && Range (40, u) && !Me.IsMoving && C ("Chi Burst", u);
		}

		public bool ChiWave (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Chi Wave") && Range (40, u) && C ("Chi Wave", u);
		}

		public bool SpearHandStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Spear Hand Strike") && Range (5, u) && C ("Spear Hand Strike", u);
		}

		public bool LegSweep (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Leg Sweep") && Range (5, u) && CS ("Leg Sweep");
		}

		public bool ZenSphere (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Zen Sphere") && Range (40, u) && u.IsFriendly && C ("Zen Sphere", u);
		}

		public bool ExpelHarm ()
		{
			return Usable ("Expel Harm") && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && (Energy >= 40 || (HasGlyph (159487) && Health (Me) < 0.35 && Energy >= 35))) || Me.HasAura ("Stance of the Wise Serpent")) && CS ("Expel Harm");
		}

		public bool Jab (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Jab") && Range (5, u) && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && (Energy >= 40 || (Me.HasAura ("Heightened Senses") && Energy >= 10))) || Me.HasAura ("Stance of the Wise Serpent")) && C ("Jab", u);
		}

		public bool TigerPalm (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Tiger Palm") && Range (5, u) && (Chi >= 1 || Me.HasAura ("Combo Breaker: Tiger Palm") || Me.Specialization == Specialization.MonkBrewmaster) && C ("Tiger Palm", u);
		}

		public bool RushingJadeWind ()
		{
			return Usable ("Rushing Jade Wind") && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && Energy >= 40) || Me.HasAura ("Stance of the Wise Serpent")) && CS ("Rushing Jade Wind");
		}

		public bool SurgingMist (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Surging Mist") && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && Energy >= 30 && Range (40, u)) || (Me.HasAura ("Stance of the Wise Serpent") && (Range (40, u) || HasGlyph (120483)))) && C ("Surging Mist", u);
		}

		public bool Detox (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Detox") && Range (40, u) && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && Energy >= 40) || Me.HasAura ("Stance of the Wise Serpent")) && C ("Detox", u);
		}

		public bool TigersLust ()
		{
			return Usable ("Tiger's Lust") && CS ("Tiger's Lust");
		}

		public bool SpinningCraneKick ()
		{
			return Usable ("Spinning Crane Kick") && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && Energy >= 40) || Me.HasAura ("Stance of the Wise Serpent")) && CS ("Spinning Crane Kick");
		}

		// Items

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
	}
}