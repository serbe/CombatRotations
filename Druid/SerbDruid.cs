using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot
{
	public abstract class SerbDruid : CombatRotation
	{
		[JsonProperty ("Maximum Energy")] 
		public int EnergyMax = 100;
		[JsonProperty ("Healing %")]
		public int HealingPercent = 80;
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;
		[JsonProperty ("Run to enemy")]
		public bool Run;
		[JsonProperty ("Use multitarget")]
		public bool Multitarget = true;
		[JsonProperty ("AOE")]
		public bool Aoe = true;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public DateTime StartBattle;
		public DateTime StartSleepTime;
		public bool InCombat;
		public UnitObject CycleTarget;
		public Int32 OraliusWhisperingCrystalId = 118922;
		public Int32 CrystalOfInsanityId = 86569;

		// Check

		public bool C (string s, UnitObject u = null)
		{
			u = u ?? Target;
			if (Cast (s, u))
				return true;
			API.Print ("False Cast " + s + " with " + u.CombatRange + " range, and " + Energy + " energy");
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
			API.Print ("False CastOnTerrain " + s + " with " + u.CombatRange + " range, and " + Energy + " energy");
			return false;
		}

		public bool COTPD (string s, UnitObject u = null)
		{
			u = u ?? Target;
			if (CastOnTerrainPreventDouble (s, u.Position))
				return true;
			API.Print ("False CastOnTerrain " + s + " with " + u.CombatRange + " range, and " + Energy + " energy");
			return false;
		}

		public bool IsSolo {
			get {
				return Group.GetNumGroupMembers () != 1;
			}
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

		public bool Usable (string s)
		{ 
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public bool HasEnergy (double i)
		{
			if (InCatForm && Me.HasAura ("Berserk"))
				i = Math.Floor (i / 2);
			if (CatForm () && Me.HasAura ("Clearcasting"))
				i = 0;
			return Energy >= i;
		}

		public bool HasEnergyB (double i)
		{
			if (InCatForm && Me.HasAura ("Berserk"))
				i = Math.Floor (i / 2);
			return Energy >= i;
		}

		public bool InCatForm {
			get {
				return IsInShapeshiftForm ("Cat Form");
			}
		}

		public bool InBearForm {
			get {
				return IsInShapeshiftForm ("Bear Form");
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
				return Range (r, u) && (IsElite (u) || IsPlayer (u) || ActiveEnemies (10) > e || Health (Me) < 0.3);
			return u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || ActiveEnemies (10) > e);
		}

		public bool DangerBoss (UnitObject u = null, int r = 0, int e = 6)
		{
			u = u ?? Target;
			if (r != 0)
				return Range (r, u) && (IsBoss (u) || IsPlayer (u) || ActiveEnemies (10) > e || Health (Me) < 0.3);
			return u.IsInCombatRangeAndLoS && (IsBoss (u) || IsPlayer (u) || ActiveEnemies (10) > e);
		}

		public bool NoInvisible (UnitObject u = null)
		{
			u = u ?? Target;
			if (u.IsPlayer && (u.Class == WoWClass.Rogue || u.Class == WoWClass.Priest || u.Class == WoWClass.Mage) && !u.HasAura ("Faerie Swarm", true)) {
				if (FaerieSwarm (u))
					return true;
			}
			if (u.IsPlayer && (u.Class == WoWClass.Rogue || u.Class == WoWClass.Priest || u.Class == WoWClass.Mage) && !u.HasAura ("Faerie Fire", true)) {
				if (FaerieFire (u))
					return true;
			}
			return false;
		}

		// Get

		public double CastTime (Int32 i)
		{
			return API.ExecuteLua<double> ("local _, _, _, castTime, _, _ = GetSpellInfo(" + i + "); return castTime;");
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
			u = u ?? Target;
			return u.HealthFraction;
		}

		public string Direction {
			get {
				return API.ExecuteLua<string> ("return GetEclipseDirection()");
			}
		}

		public int Eclipse {
			get {
				return API.ExecuteLua<int> ("return UnitPower('player', SPELL_POWER_ECLIPSE)");
			}
		}

		public int Energy {
			get {
				return Me.GetPower (WoWPowerType.Energy);
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

		public double EnergyRegen {
			get {
				string activeRegen = API.ExecuteLua<string> ("inactiveRegen, activeRegen = GetPowerRegen(); return activeRegen");
				return Convert.ToDouble (activeRegen);
			}
		}

		public double EnergyTimeToMax {
			get {
				return EnergyMax - Energy / EnergyRegen;
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

		public double TimeToRegen (double e)
		{ 
			if (e > Energy)
				return (e - Energy) / EnergyRegen;
			return 0;
		}

		public double TimeToStartBattle {
			get {
				return API.ExecuteLua<double> ("return GetBattlefieldInstanceRunTime()") / 1000;
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

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public double CooldownById (Int32 i)
		{ 
			return SpellCooldown (i) < 0 ? 0 : SpellCooldown (i);
		}

		public double TimeToDie (UnitObject u = null)
		{
			u = u ?? Target;
			return u.Health / Ttd;
		}

		public double EclipseChange {
			get {
				double timeToChange = 20;
				if (Direction == "sun") {
					if (Eclipse > 0 && Eclipse <= 100)
						timeToChange = 10 + (100 - Eclipse) / 10;
					if (Eclipse > -100 && Eclipse < 0)
						timeToChange = (0 - Eclipse) / 10;
				} 
				if (Direction == "moon") {
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

		// Combo

		public bool Interrupt ()
		{
			if (Usable ("Mighty Bash")) {
				if (ActiveEnemies (6) > 1 && Multitarget) {
					CycleTarget = Enemy.Where (x => Range (5, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null && MightyBash (CycleTarget))
						return true;
				} else {
					if (Target.IsCastingAndInterruptible () && Range (5) && Target.RemainingCastTime > 0) {
						if (MightyBash ())
							return true;
					}
				}
			}
			if (Usable ("Solar Beam")) {
				if (ActiveEnemies (40) > 1 && Multitarget) {
					CycleTarget = Enemy.Where (x => Range (40, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null && SolarBeam (CycleTarget))
						return true;
				} else {
					if (Target.IsCastingAndInterruptible () && Range (40) && Target.RemainingCastTime > 0) {
						if (SolarBeam ())
							return true;
					}
				}
			}
			if (Usable ("Skull Bash")) {
				if (ActiveEnemies (13) > 1 && Multitarget) {
					CycleTarget = Enemy.Where (x => Range (13, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null && SkullBash (CycleTarget))
						return true;
				} else {
					if (Target.IsCastingAndInterruptible () && Range (13) && Target.RemainingCastTime > 0) {
						if (SkullBash ())
							return true;
					}
				}
			}
			if (Usable ("Wild Charge")) {
				if (ActiveEnemies (25) > 1 && Multitarget) {
					CycleTarget = Enemy.Where (x => Range (25, x, 8) && x.IsCasting && !IsBoss (x) && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null && WildCharge (CycleTarget))
						return true;
				} else {
					if (Range (25, Target, 8) && Target.IsCasting && !IsBoss (Target) && Target.RemainingCastTime > 0) {
						if (WildCharge ())
							return true;
					}
				}
			}
			if (Usable ("Maim") && ComboPoints >= 3) {
				if (ActiveEnemies (6) > 1 && Multitarget) {
					CycleTarget = Enemy.Where (x => Range (5, x) && x.IsCasting && !IsBoss (x) && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Maim (CycleTarget))
							return true;
					} 
				} else {
					if (Range (5) && Target.IsCasting && !IsBoss (Target) && Target.RemainingCastTime > 0) {
						if (Maim ())
							return true;
					}
				}
			}
			return false;
		}

		public bool RunToTarget (UnitObject u = null)
		{
			u = u ?? Target;
			if (InCatForm && !Me.HasAura ("Prowl") && u.CombatRange >= 20 && u.IsFleeing) {
				if (Dash ())
					return true;
			}
			// // if (CastSelfPreventDouble("Stealth", () => !Me.InCombat && !HasAura("Stealth"))) return;
			// if (Cast("Shadowstep", () => !HasAura("Sprint") && HasSpell("Shadowstep"))) return;
			// // if (CastSelf("Sprint", () => !HasAura("Sprint") && !HasAura("Burst of Speed"))) return;
			// // if (CastSelf("Burst of Speed", () => !HasAura("Sprint") && !HasAura("Burst of Speed") && HasSpell("Burst of Speed") && Energy > 20)) return;
			// if (Cast(RangedAtk, () => Energy >= 40 && !HasAura("Stealth") && Target.IsInLoS)) return;
			return false;
		}

		// public virtual bool UnEnrage() {
		// 	var targets = Adds;
		// 	targets.Add(Target);

		// 	if (HasSpell("Shiv") && Cooldown("Shiv") == 0 && HasCost(20)) {
		// 		if (ActiveEnemies(6) > 1 && Multitarget) {
		// 			CycleTarget = Enemy.Where(x => x.IsInCombatRangeAndLoS && IsInEnrage(x) && !IsBoss(x)).DefaultIfEmpty(null).FirstOrDefault();
		// 			if (Cast("Shiv", CycleTarget, () => CycleTarget != null)) return true;
		// 		} else
		// 			if (Cast("Shiv", () => !IsBoss(Target) && IsInEnrage(Target) && !IsBoss(Target))) return true;
		// 	}
		// 	return false;
		// }

		public bool HealPartyMember ()
		{
			if (InArena && InInstance) {
				CycleTarget = Group.GetGroupMemberObjects ().Where (x => !x.IsDead && Range (40, x) && Health (x) <= HealingPercent / 100 && !x.HasAura ("Rejuvenation", true) && !x.HasAura ("Cenarion Ward", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null && Rejuvenation (CycleTarget))
					return true;
				if (Me.HasAura ("Predatory Swiftness")) {
					CycleTarget = Group.GetGroupMemberObjects ().Where (x => !x.IsDead && Range (40, x) && Health (x) <= HealingPercent / 100 && Health (x) < Health (Me) && !x.HasAura ("Cenarion Ward", true)).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null && HealingTouch (CycleTarget))
						return true;
				}				
			}

			return false;
		}

		public bool Heal ()
		{
			if (Health (Me) < 0.4) {
				if (EternalWilloftheMartyr ())
					return true;
			}
			if (Health (Me) < 0.45) {
				if (Healthstone ())
					return true;
			}
			if (Health (Me) < 0.5) {
				if (SurvivalInstincts ())
					return true;	
			}
			if (Health (Me) < 0.6) {
				if (Barkskin ())
					return true;
			}
			if (Me.HasAura ("Predatory Swiftness") && Health (Me) < HealingPercent / 100 && !Me.HasAura ("Cenarion Ward", true)) {
				if (HealingTouch (Me))
					return true;
			}
			if (Health (Me) <= HealingPercent / 100) {
				if (CenarionWard (Me))
					return true;
			}
			if (Health (Me) <= HealingPercent / 100 && !Me.HasAura ("Rejuvenation", true) && !Me.HasAura ("Cenarion Ward", true)) {
				if (Rejuvenation (Me))
					return true;
			}

			return false;
		}

		// Spells

		public bool CatForm ()
		{
			return !InCatForm && CS ("Cat Form");
		}

		public bool BearForm ()
		{
			return !InBearForm && CS ("Bear Form");
		}

		public bool MightyBash (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mighty Bash") && Range (5, u) && C ("Mighty Bash", u);
		}

		public bool SolarBeam (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Solar Beam") && Range (40, u) && C ("Solar Beam", u);
		}

		public bool MarkoftheWild (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mark of the Wild") && u.AuraTimeRemaining ("Mark of the Wild") < 300 && u.AuraTimeRemaining ("Blessing of Kings") < 300 && Range (40, u) && C ("Mark of the Wild", u);
		}

		public bool Rejuvenation (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Rejuvenation") && !u.HasAura ("Rejuvenation", true) && Range (40, u) && C ("Rejuvenation", u);
		}

		public bool HealingTouch (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Healing Touch") && Range (40, u) && C ("Healing Touch", u);
		}

		public bool RemoveCorruption (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Remove Corruption") && Range (40, u) && C ("Remove Corruption", u);
		}

		public bool FerociousBite (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Ferocious Bite") && HasEnergy (25) && ComboPoints > 0 && Range (5, u) && C ("Ferocious Bite", u);
		}

		public bool CenarionWard (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Cenarion Ward") && !u.HasAura ("Cenarion Ward") && Range (40, u) && C ("Cenarion Ward", u);
		}

		public bool Barkskin ()
		{
			return Usable ("Barkskin") && Health (Me) < 0.9 && CS ("Barkskin");
		}

		public bool BristlingFur ()
		{
			return Usable ("Bristling Fur") && CS ("Bristling Fur");
		}

		public bool MoonkinForm ()
		{
			return Usable ("Moonkin Form") && !Me.HasAura ("Moonkin Form") && CS ("Moonkin Form");
		}

		public bool FaerieSwarm (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Faerie Swarm") && Range (35, u) && C ("Faerie Swarm", u);
		}

		public bool FaerieFire (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Faerie Fire") && Range (35, u) && C ("Faerie Fire", u);
		}

		public bool SkullBash (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Skull Bash") && Range (13, u) && C ("Skull Bash", u);
		}

		public bool WildCharge (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Wild Charge") && Range (25, u, 5) && C ("Wild Charge", u);
		}

		public bool Starfall (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Starfall") && Range (40) && C ("Starfall", u);
		}

		public bool BloodFury ()
		{
			return Usable ("Blood Fury") && Danger () && CS ("Blood Fury");
		}

		public bool Berserking ()
		{
			return Usable ("Berserking") && Danger () && CS ("Berserking");
		}

		public bool ArcaneTorrent ()
		{
			return Usable ("Arcane Torrent") && Danger () && CS ("Arcane Torrent");
		}

		public bool ForceofNature (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Force of Nature") && Range (40) && C ("Force of Nature", u);
		}

		public bool Starsurge (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Starsurge") && Range (40, u) && (!Me.IsMoving || Me.HasAura ("Empowered Moonkin") || HasSpell ("Enhanced Starsurge")) && C ("Starsurge", u);
		}

		public bool IncarnationChosenofElune ()
		{
			return Usable ("Incarnation: Chosen of Elune") && Danger () && CS ("Incarnation: Chosen of Elune");
		}

		public bool Sunfire (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Sunfire") && Eclipse > 0 && Range (40, u) && C ("Sunfire", u);
		}

		public bool StellarFlare (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Stellar Flare") && Range (40, u) && (!Me.IsMoving || Me.HasAura ("Empowered Moonkin")) && C ("Stellar Flare", u);
		}

		public bool Moonfire (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Moonfire") && Range (40, u) && C ("Moonfire", u);
		}

		public bool Wrath (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Wrath") && Range (40, u) && (!Me.IsMoving || Me.HasAura ("Empowered Moonkin")) && C ("Wrath", u);
		}

		public bool Starfire (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Starfire") && Range (40, u) && (!Me.IsMoving || Me.HasAura ("Elune's Wrath") || Me.HasAura ("Empowered Moonkin")) && C ("Starfire", u);
		}

		public bool Maim (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Maim") && HasEnergy (35) && ComboPoints > 0 && Range (5, u) && C ("Maim", u);
		}

		public bool Berserk ()
		{
			return Usable ("Berserk") && Danger () && CS ("Berserk");
		}

		public bool TigersFury (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Tiger's Fury") && Range (10, u) && CS ("Tiger's Fury");
		}

		public bool IncarnationKingoftheJungle ()
		{
			return Usable ("Incarnation: King of the Jungle") && Danger () && CS ("Incarnation: King of the Jungle");
		}

		public bool Shadowmeld ()
		{
			return Usable ("Shadowmeld") && !Me.IsMoving && CS ("Shadowmeld");
		}

		public bool CelestialAlignment ()
		{
			return Usable ("Celestial Alignment") && Danger () && CS ("Celestial Alignment");
		}

		public bool Rake (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Rake") && HasEnergy (35) && u.IsInCombatRangeAndLoS && C ("Rake", u);
		}

		public bool SavageRoar ()
		{
			return Usable ("Savage Roar") && HasEnergyB (25) && Range (20) && ComboPoints > 0 && CS ("Savage Roar");
		}

		public bool Rip (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Rip") && HasEnergy (30) && ComboPoints > 0 && u.IsInCombatRangeAndLoS && C ("Rip", u);
		}

		public bool SavageDefense (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Savage Defense") && Rage > 60 && Range (20, u) && CS ("Savage Defense");
		}

		public bool Freedom ()
		{
			return WilloftheForsaken () || EveryManforHimself ();
		}

		public bool WilloftheForsaken ()
		{
			return Usable ("Will of the Forsaken") && CS ("Will of the Forsaken");
		}

		public bool EveryManforHimself ()
		{
			return Usable ("Every Man for Himself") && CS ("Every Man for Himself");
		}

		public bool Swipe (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Swipe") && HasEnergy (45) && C ("Swipe", u);
		}

		public bool Shred (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shred") && HasEnergy (40) && Range (5, u) && C ("Shred", u);
		}

		public bool Thrash (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Thrash") && ((InCatForm && HasEnergy (50))) && Range (10, u) && C ("Thrash", u);
		}

		public bool SurvivalInstincts ()
		{
			return Usable ("Survival Instincts") && CS ("Survival Instincts");
		}

		public bool Dash ()
		{
			return Usable ("Dash") && CS ("Dash");
		}


		public bool Maul (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Maul") && (Rage >= 20 || (Rage >= 10 && Me.HasAura ("Tooth and Claw"))) && Range (5, u) && C ("Maul", u);
		}

		public bool Pulverize (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Pulverize") && Range (5, u) && C ("Pulverize", u);
		}

		public bool Lacerate (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Lacerate") && Range (5, u) && C ("Lacerate", u);
		}

		public bool Mangle (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mangle") && Range (5, u) && C ("Mangle", u);
		}

		public bool FrenziedRegeneration ()
		{
			return Usable ("Frenzied Regeneration") && CS ("Frenzied Regeneration");
		}

		public bool Renewal ()
		{
			return Usable ("Renewal") && CS ("Renewal");
		}

		public bool HeartoftheWild ()
		{
			return Usable ("Heart of the Wild") && DangerBoss () && CS ("Heart of the Wild");
		}

		public bool IncarnationSonofUrsoc ()
		{
			return Usable ("Incarnation: Son of Ursoc") && DangerBoss () && CS ("Incarnation: Son of Ursoc");
		}

		public bool NaturesVigil ()
		{
			return Usable ("Nature's Vigil") && CS ("Nature's Vigil");
		}

		// Items

		public bool Healthstone ()
		{
			return API.HasItem (5512) && API.ItemCooldown (5512) == 0 && API.UseItem (5512);
		}

		public bool CrystalOfInsanity ()
		{
			return !InArena && API.HasItem (CrystalOfInsanityId) && !HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityId) == 0 && API.UseItem (CrystalOfInsanityId);
		}

		public bool OraliusWhisperingCrystal ()
		{
			return API.HasItem (OraliusWhisperingCrystalId) && !HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalId) == 0 && API.UseItem (OraliusWhisperingCrystalId);
		}

		public bool EternalWilloftheMartyr ()
		{
			return API.HasItem (122668) && API.ItemCooldown (122668) == 0 && API.UseItem (122668);
		}
	
	}
}
