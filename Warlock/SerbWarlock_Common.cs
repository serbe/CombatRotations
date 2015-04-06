using System;
using ReBot.API;

namespace ReBot
{
	public abstract class SerbWarlock : CombatRotation
	{
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int TTD = 10;
		//		[JsonProperty ("Run to enemy")]
		//		public bool Run;
		[JsonProperty ("Use multitarget")]
		public bool Multitarget = true;
		[JsonProperty ("AOE")]
		public bool AOE = true;
		[JsonProperty ("Use GCD")]
		public bool GCD = true;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public DateTime StartBattle;
		public DateTime StartSleepTime;
		public bool InCombat;
		public UnitObject CycleTarget;
		public Int32 OraliusWhisperingCrystalID = 118922;
		public Int32 CrystalOfInsanityID = 86569;

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

		public bool InBG {
			get {
				return API.MapInfo.Type == MapType.PvP;
			}
		}

		public double Health {
			get {
				return Me.HealthFraction;
			}
		}

		public double TargetHealth {
			get {
				return Target.HealthFraction;
			}
		}

		public bool IsBoss (UnitObject o)
		{
			return(o.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || o.Level >= Me.Level + BossLevelIncrease;
		}

		public bool IsPlayer {
			get { 
				return Target.IsPlayer;
			}
		}

		public bool IsElite { 
			get {
				return Target.IsElite ();
			}
		}

		public bool IsInEnrage (UnitObject o)
		{
			if (o.HasAura ("Enrage") || o.HasAura ("Berserker Rage") || o.HasAura ("Demonic Enrage") || o.HasAura ("Aspect of Thekal") || o.HasAura ("Charge Rage") || o.HasAura ("Electric Spur") || o.HasAura ("Cornered and Enraged!") || o.HasAura ("Draconic Rage") || o.HasAura ("Brood Rage") || o.HasAura ("Determination") || o.HasAura ("Charged Fists") || o.HasAura ("Beatdown") || o.HasAura ("Consuming Bite") || o.HasAura ("Delirious") || o.HasAura ("Angry") || o.HasAura ("Blood Rage") || o.HasAura ("Berserking Howl") || o.HasAura ("Bloody Rage") || o.HasAura ("Brewrific") || o.HasAura ("Desperate Rage") || o.HasAura ("Blood Crazed") || o.HasAura ("Combat Momentum") || o.HasAura ("Dire Rage") || o.HasAura ("Dominate Slave") || o.HasAura ("Blackrock Rabies") || o.HasAura ("Burning Rage") || o.HasAura ("Bloodletting Howl"))
				return true;
			else
				return false;
		}

		public bool IsNotForDamage (UnitObject o)
		{
			if (o.HasAura ("Fear") || o.HasAura ("Polymorph") || o.HasAura ("Gouge") || o.HasAura ("Paralysis") || o.HasAura ("Blind") || o.HasAura ("Hex"))
				return true;
			else
				return false;
		}

		public int Mana { 
			get { 
				return Me.GetPower (WoWPowerType.Mana);
			}
		}

		public int Fury { 
			get { 
				return Me.GetPower (WoWPowerType.WarlockDemonicFury);
			}
		}

		public int Embers {
			get {
				return Me.GetPower (WoWPowerType.WarlockDestructionBurningEmbers);
			}
		}

		//		public double EnergyRegen {
		//			get {
		//				string activeRegen = API.ExecuteLua<string> ("inactiveRegen, activeRegen = GetPowerRegen(); return activeRegen");
		//				return Convert.ToDouble (activeRegen);
		//			}
		//		}

		// public bool TargettingMe { get { return Target.Target ==(UnitObject)Me; } }
		//		public double EnergyTimeToMax {
		//			get {
		//				return EnergyMax - Energy / EnergyRegen;
		//			}
		//		}


		public double Time {
			get {
				TimeSpan CombatTime = DateTime.Now.Subtract (StartBattle);
				return CombatTime.TotalSeconds;
			}
		}

		public double SleepTime {
			get {
				TimeSpan CurrentSleepTime = DateTime.Now.Subtract (StartSleepTime);
				return CurrentSleepTime.TotalSeconds;
			}
		}

//		public double TimeToRegen (double e)
//		{ 
//			if (e > Energy)
//				return (e - Energy) / EnergyRegen;
//			else
//				return 0;
//		}

		public double TimeToStartBattle {
			get {
				return API.ExecuteLua<double> ("return GetBattlefieldInstanceRunTime()") / 1000;
			}
		}

		public int EnemyInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead) {
					x++;
				}
			}
			return x;
		}

		public bool IncapacitatedInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && IsNotForDamage (mob)) {
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

//		public bool HasEnergy (double i)
//		{
//			if (IsCatForm () && Me.HasAura ("Berserk"))
//				i = Math.Floor (i / 2);
//			if (CatForm () && Me.HasAura ("Clearcasting"))
//				i = 0;
//			return Energy >= i;
//		}

//		public bool HasEnergyB (double i)
//		{
//			if (IsCatForm () && Me.HasAura ("Berserk"))
//				i = Math.Floor (i / 2);
//			return Energy >= i;
//		}

//		public bool IsCatForm ()
//		{
//			return (HasAura ("Cat Form") || HasAura ("Claws of Shirvallah"));
//		}

//		public virtual bool CatForm ()
//		{
//			return CastSelf ("Cat Form", () => !Me.HasAura ("Claws of Shirvallah") && !Me.HasAura ("Cat Form"));
//		}

		public double TimeToDie (UnitObject o)
		{
			return o.Health / TTD;
		}

		public SerbWarlock ()
		{
		}

		public virtual bool DarkIntent() {
			return CastSelf("Dark Intent", () => Usable("Dark Intent") && !Me.HasAura("Dark Intent") && !Me.HasAura("Mind Quickening") && !Me.HasAura("Swiftblade's Cunning") && !Me.HasAura("Windflurry") && !Me.HasAura("Arcane Brilliance"));
		}

		public virtual bool SummonPet() {
//			if (Cast ("Felguard", () => !HasSpell ("Demonic Servitude") && (!HasSpell ("Grimoire of Supremacy") && (!HasSpell ("Grimoire of Service") || !Me.HasAura ("Grimoire of Service")))))
//				return true;
//			if (Cast ("Wrathguard", () => !HasSpell ("Demonic Servitude") && (HasSpell ("Grimoire of Supremacy") && (!HasSpell ("Grimoire of Service") || !Me.HasAura ("Grimoire of Service")))))
//				return true;
			return false;
		}

		public virtual bool BloodFury ()
		{
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange(10) > 2));
		}

		public virtual bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange(10) > 2));
		}

		public virtual bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange(10) > 2));
		}

		public virtual bool MannorothsFury ()
		{
			return CastSelf ("Mannoroth's Fury", () => Usable ("Mannoroth's Fury") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange(10) > 2));
		}

		public virtual bool DarkSoul() {
			if (CastSelf("Dark Soul: Misery", () => Usable("Dark Soul: Misery") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange(10) > 2)))
				return true; 
			if (CastSelf("Dark Soul: Instability", () => Usable("Dark Soul: Instability") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange(10) > 2)))
				return true; 
			if (CastSelf("Dark Soul: Knowledge", () => Usable("Dark Soul: Knowledge") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange(10) > 2)))
				return true; 
			return false;
		}

		public virtual bool ImpSwarm() {
			return Cast ("Imp Swarm", () => Usable ("Imp Swarm") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange(10) > 2));
		}

		public virtual bool Healthstone ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (5512) && API.ItemCooldown (5512) == 0)
				return API.UseItem (5512);
			return false;
		}

		public virtual bool CrystalOfInsanity ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (!InArena && API.HasItem (CrystalOfInsanityID) && !Me.HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityID) == 0)
				return API.UseItem (CrystalOfInsanityID);
			return false;
		}

		public virtual bool OraliusWhisperingCrystal ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (OraliusWhisperingCrystalID) && !Me.HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalID) == 0)
				return API.UseItem (OraliusWhisperingCrystalID);
			return false;
		}
	}
}

