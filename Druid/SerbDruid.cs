using System;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot.Druid
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

		public bool IsSolo {
			get {
				return Group.GetNumGroupMembers () != 1;
			}
		}

		// public int EnergyMax {
		// 	get {
		// 		int energy = 100;
		// 		if (HasSpell("Venom Rush")) energy = energy + 15;
		// 		if (HasGlyph(159634)) energy = energy + 20;
		// 		return energy;
		// 	}
		// }

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

		public double Health (UnitObject u = null)
		{
			u = u ?? Me;
			return u.HealthFraction;
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

		// public bool isInterruptable { get { return Target.IsCastingAndInterruptible(); } }
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

		public int EnemyInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && mob.IsAttackable) {
					x++;
				}
			}
			return x;
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

		public bool Usable (string s)
		{ 
			// Analysis disable once CompareOfFloatsByEqualityOperator
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public bool HasEnergy (double i)
		{
			if (IsCatForm () && Me.HasAura ("Berserk"))
				i = Math.Floor (i / 2);
			if (CatForm () && Me.HasAura ("Clearcasting"))
				i = 0;
			return Energy >= i;
		}

		public bool HasEnergyB (double i)
		{
			if (IsCatForm () && Me.HasAura ("Berserk"))
				i = Math.Floor (i / 2);
			return Energy >= i;
		}

		public bool IsCatForm ()
		{
			return (HasAura ("Cat Form") || HasAura ("Claws of Shirvallah"));
		}

		public bool CatForm ()
		{
			return CastSelf ("Cat Form", () => !Me.HasAura ("Claws of Shirvallah") && !Me.HasAura ("Cat Form"));
		}

		public double TimeToDie (UnitObject o)
		{
			if (o != null)
				return o.Health / Ttd;
			return 0;
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


		public bool Interrupt ()
		{
			var targets = Adds;
			targets.Add (Target);

			if (Usable ("Mighty Bash")) {
				if (EnemyInRange (6) > 1 && Multitarget) {
					CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange <= 5 && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (MightyBash (CycleTarget))
							return true;
					}
				} else {
					if (Target.IsCastingAndInterruptible () && Target.IsInLoS && Target.CombatRange <= 5 && Target.RemainingCastTime > 0) {
						if (MightyBash ())
							return true;
					}
				}
			}
			if (Usable ("Solar Beam")) {
				if (EnemyInRange (40) > 1 && Multitarget) {
					CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange <= 40 && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (SolarBeam (CycleTarget))
							return true;
					} 
				} else {
					if (Target.IsCastingAndInterruptible () && Target.IsInLoS && Target.CombatRange <= 40 && Target.RemainingCastTime > 0) {
						if (SolarBeam ())
							return true;
					}
				}
			}
			if (Usable ("Skull Bash")) {
				if (EnemyInRange (13) > 1 && Multitarget) {
					CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange <= 13 && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (SkullBash (CycleTarget))
							return true;
					} 
				} else {
					if (Target.IsCastingAndInterruptible () && Target.IsInLoS && Target.CombatRange <= 13 && Target.RemainingCastTime > 0) {
						if (SkullBash ())
							return true;
					}
				}
			}
			if (Usable ("Wild Charge")) {
				if (EnemyInRange (25) > 1 && Multitarget) {
					CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange >= 8 && x.CombatRange <= 25 && x.IsCasting && !IsBoss (x) && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (WildCharge (CycleTarget))
							return true;
					} 
				} else {
					if (Target.IsInLoS && Target.CombatRange >= 8 && Target.CombatRange <= 25 && Target.IsCasting && !IsBoss (Target) && Target.RemainingCastTime > 0) {
						if (WildCharge ())
							return true;
					}
				}
			}
			if (Usable ("Maim") && ComboPoints >= 3) {
				if (EnemyInRange (6) > 1 && Multitarget) {
					CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange <= 5 && x.IsCasting && !IsBoss (x) && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Maim (CycleTarget))
							return true;
					} 
				} else {
					if (Target.IsInLoS && Target.CombatRange <= 5 && Target.IsCasting && !IsBoss (Target) && Target.RemainingCastTime > 0) {
						if (Maim ())
							return true;
					}
				}
			}
			return false;
		}

		public bool MightyBash (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Mighty Bash", () => Usable ("Mighty Bash") && u.IsInLoS && u.CombatRange <= 5, u);
		}

		public bool SolarBeam (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Solar Beam", () => Usable ("Solar Beam") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool MarkoftheWild (UnitObject u = null)
		{
			u = u ?? Me;
			return Cast ("Mark of the Wild", () => Usable ("Mark of the Wild") && u.AuraTimeRemaining ("Mark of the Wild") < 300 && u.AuraTimeRemaining ("Blessing of Kings") < 300 && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool Rejuvenation (UnitObject u = null)
		{
			u = u ?? Me;
			return Cast ("Rejuvenation", () => Usable ("Rejuvenation") && !u.HasAura ("Rejuvenation", true) && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public virtual bool HealingTouch (UnitObject u = null)
		{
			u = u ?? Me;
			return Cast ("Healing Touch", () => Usable ("Healing Touch") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public virtual bool RemoveCorruption (UnitObject u = null)
		{
			u = u ?? Me;
			return Cast ("Remove Corruption", () => Usable ("Remove Corruption") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		// public virtual bool UnEnrage() {
		// 	var targets = Adds;
		// 	targets.Add(Target);

		// 	if (HasSpell("Shiv") && Cooldown("Shiv") == 0 && HasCost(20)) {
		// 		if (EnemyInRange(6) > 1 && Multitarget) {
		// 			CycleTarget = targets.Where(x => x.IsInCombatRangeAndLoS && IsInEnrage(x) && !IsBoss(x)).DefaultIfEmpty(null).FirstOrDefault();
		// 			if (Cast("Shiv", CycleTarget, () => CycleTarget != null)) return true;
		// 		} else
		// 			if (Cast("Shiv", () => !IsBoss(Target) && IsInEnrage(Target) && !IsBoss(Target))) return true;
		// 	}
		// 	return false;
		// }

		public bool FerociousBite (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Ferocious Bite", () => Usable ("Ferocious Bite") && HasEnergy (25) && ComboPoints > 0 && u.IsInLoS && u.CombatRange <= 5, u);
		}

		public bool CenarionWard (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Cenarion Ward", () => Usable ("Cenarion Ward") && !u.HasAura ("Cenarion Ward") && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool Barkskin ()
		{
			return CastSelf ("Barkskin", () => Usable ("Barkskin"));
		}

		public bool MoonkinForm ()
		{
			return CastSelf ("Moonkin Form", () => Usable ("Moonkin Form") && !Me.HasAura ("Moonkin Form"));
		}

		public bool FaerieSwarm (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Faerie Swarm", () => Usable ("Faerie Swarm") && u.CombatRange < 35 && u.IsInLoS, u);
		}

		public bool FaerieFire (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Faerie Fire", () => Usable ("Faerie Fire") && u.CombatRange < 35 && u.IsInLoS, u);
		}

		public bool SkullBash (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Skull Bash", () => Usable ("Skull Bash") && u.CombatRange < 13 && u.IsInLoS, u);
		}

		public bool WildCharge (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Wild Charge", () => Usable ("Wild Charge") && u.IsInLoS && u.CombatRange >= 5 && u.CombatRange <= 25, u);
		}

		public bool Starfall ()
		{
			return Cast ("Starfall", () => Usable ("Starfall") && Target.IsInLoS && Target.CombatRange <= 40);
		}

		public bool BloodFury ()
		{
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer () || EnemyInRange (10) > 2));
		}

		public bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer () || EnemyInRange (10) > 2));
		}

		public bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer () || EnemyInRange (10) > 2));
		}

		public bool ForceofNature ()
		{
			return Cast ("Force of Nature", () => Usable ("Force of Nature") && Target.IsInLoS && Target.CombatRange <= 40);
		}

		public bool Starsurge (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Starsurge", () => Usable ("Starsurge") && u.IsInLoS && u.CombatRange <= 40 && (!Me.IsMoving || Me.HasAura ("Empowered Moonkin") || HasSpell ("Enhanced Starsurge")), u);
		}

		public bool IncarnationChosenofElune ()
		{
			return CastSelf ("Incarnation: Chosen of Elune", () => Usable ("Incarnation: Chosen of Elune") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer () || EnemyInRange (10) > 2));
		}

		public bool Sunfire (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Sunfire", () => Usable ("Sunfire") && Eclipse > 0 && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool StellarFlare (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Stellar Flare", () => Usable ("Stellar Flare") && u.IsInLoS && u.CombatRange <= 40 && (!Me.IsMoving || Me.HasAura ("Empowered Moonkin")), u);
		}

		public bool Moonfire (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Moonfire", () => Usable ("Moonfire") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool Wrath (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Wrath", () => Usable ("Wrath") && u.IsInLoS && u.CombatRange <= 40 && (!Me.IsMoving || Me.HasAura ("Empowered Moonkin")));
		}

		public bool Starfire (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Starfire", () => Usable ("Starfire") && u.IsInLoS && u.CombatRange <= 40 && (!Me.IsMoving || Me.HasAura ("Elune's Wrath") || Me.HasAura ("Empowered Moonkin")));
		}

		public bool Maim (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Maim", () => Usable ("Maim") && HasEnergy (35) && ComboPoints > 0 && u.IsInLoS && u.CombatRange <= 5, u);
		}

		public bool Berserk ()
		{
			return CastSelf ("Berserk", () => Usable ("Berserk") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer () || EnemyInRange (10) > 2));
		}

		public bool TigersFury (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Tiger's Fury", () => Usable ("Tiger's Fury") && u.IsInLoS && u.CombatRange <= 10);
		}

		public bool IncarnationKingoftheJungle ()
		{
			return CastSelf ("Incarnation: King of the Jungle", () => Usable ("Incarnation: King of the Jungle") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer () || EnemyInRange (10) > 2));
		}

		public bool Shadowmeld ()
		{
			return CastSelf ("Shadowmeld", () => Usable ("Shadowmeld"));
		}

		public bool Healthstone ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (5512) && API.ItemCooldown (5512) == 0)
				return API.UseItem (5512);
			return false;
		}

		public bool CrystalOfInsanity ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (!InArena && API.HasItem (CrystalOfInsanityId) && !Me.HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityId) == 0)
				return API.UseItem (CrystalOfInsanityId);
			return false;
		}

		public bool OraliusWhisperingCrystal ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (OraliusWhisperingCrystalId) && !Me.HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalId) == 0)
				return API.UseItem (OraliusWhisperingCrystalId);
			return false;
		}

		public bool CelestialAlignment ()
		{
			return CastSelf ("Celestial Alignment", () => Usable ("Celestial Alignment") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer () || EnemyInRange (10) > 2));
		}

		public bool Rake (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Rake", () => Usable ("Rake") && HasEnergy (35) && u.IsInCombatRangeAndLoS, u);
		}

		public bool SavageRoar ()
		{
			return CastSelf ("Savage Roar", () => Usable ("Savage Roar") && HasEnergyB (25) && ComboPoints > 0);
		}

		public bool Rip (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Rip", () => Usable ("Rip") && HasEnergy (30) && ComboPoints > 0 && u.IsInCombatRangeAndLoS, u);
		}

		public bool Freedom ()
		{
			return WilloftheForsaken () || EveryManforHimself ();
		}

		public bool WilloftheForsaken ()
		{
			return CastSelf ("Will of the Forsaken", () => Usable ("Will of the Forsaken"));
		}

		public bool EveryManforHimself ()
		{
			return CastSelf ("Every Man for Himself", () => Usable ("Every Man for Himself"));
		}

		public bool Swipe ()
		{
			return Cast ("Swipe", () => Usable ("Swipe") && HasEnergy (45));
		}

		public bool Shred (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shred", () => Usable ("Shred") && HasEnergy (40) && u.IsInLoS && u.CombatRange <= 5, u);
		}

		public bool Thrash (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Thrash", () => Usable ("Thrash") && ((IsCatForm () && HasEnergy (50))) && u.IsInLoS && u.CombatRange <= 10, u);
		}

		public bool SurvivalInstincts ()
		{
			return CastSelf ("Survival Instincts", () => Usable ("Survival Instincts"));
		}

		public bool Dash ()
		{
			return CastSelf ("Dash", () => Usable ("Dash"));
		}

		public bool Heal ()
		{
			if (Health () < 0.45) {
				if (Healthstone ())
					return true;
			}
			if (Health () < 0.5) {
				if (SurvivalInstincts ())
					return true;	
			}
			if (Health () < 0.6) {
				if (Barkskin ())
					return true;
			}
			if (Me.HasAura ("Predatory Swiftness") && Health () < HealingPercent && !Me.HasAura ("Cenarion Ward", true)) {
				if (HealingTouch ())
					return true;
			}
			if (Health () <= HealingPercent) {
				if (CenarionWard ())
					return true;
			}
			if (Health () <= HealingPercent && !Me.HasAura ("Rejuvenation", true) && !Me.HasAura ("Cenarion Ward", true)) {
				if (Rejuvenation ())
					return true;
			}

			return false;
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

		public bool HealPartyMember ()
		{
			if (InArena && InInstance) {
				CycleTarget = Group.GetGroupMemberObjects ().Where (x => !x.IsDead && x.IsInLoS && x.CombatRange < 40 && x.HealthFraction <= HealingPercent && !x.HasAura ("Rejuvenation", true) && !x.HasAura ("Cenarion Ward", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Rejuvenation (CycleTarget))
						return true;
				}
				if (Me.HasAura ("Predatory Swiftness")) {
					CycleTarget = Group.GetGroupMemberObjects ().Where (x => !x.IsDead && x.IsInLoS && x.CombatRange < 40 && x.HealthFraction <= HealingPercent && Health (x) < Health () && !x.HasAura ("Cenarion Ward", true)).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (HealingTouch (CycleTarget))
							return true;
					}
				}				
			}
				
			return false;
		}

		public bool RunToTarget (UnitObject u = null)
		{
			u = u ?? Target;
			if (IsCatForm () && !Me.HasAura ("Prowl") && u.CombatRange >= 20 && u.IsFleeing) {
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
	}
}
