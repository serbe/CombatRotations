using Newtonsoft.Json;
using ReBot.API;

namespace ReBot.Priest
{
	[Rotation ("Serb Priest Discipline SC", "ReBot", WoWClass.Priest, Specialization.PriestDiscipline, 40, 25)]

	public class SerbPriestDisciplineSc : SerbPriest
	{
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("Auto target")]
		public bool UseAutoTarget = true;
		[JsonProperty ("Fight in instance")]
		public bool FightInInstance = true;
		[JsonProperty ("Heal %/100")]
		public double HealPr = 0.8;
		[JsonProperty ("Heal tank %/100")]
		public double TankPr = 0.9;

		public SerbPriestDisciplineSc ()
		{
			GroupBuffs = new [] {
				"Power Word: Fortitude"
			};
			PullSpells = new [] {
				"Smite",
			};
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_intellect_flask
			//	actions.precombat+=/food,type=salty_squid_roll
			//	actions.precombat+=/power_word_fortitude,if=!aura.stamina.up
			if (PowerWordFortitude ())
				return true;
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_intellect
			//	actions.precombat+=/smite

			if (Me.FallingTime > 2) {
				if (Levitate ())
					return true;
			}

			return false;
		}

		public override void Combat ()
		{
			if (Gcd && HasGlobalCooldown ())
				return;

			if (!InRaid && !InBg && !InArena && ((FightInInstance && InInstance) || !InInstance) && (Target == null || !Target.IsEnemy) && UseAutoTarget)
				AutoTarget ();

			if (InArena) {
				if (SetShieldAll ())
					return;
			}

			PowerWordShield (Me);

			if (GroupMembers.Count > 0) {
				if (Target == null)
					SetTarget ();
			
				if (Tank != null && Tank.HealthFraction <= 0.3) {
					if (FlashHeal (Tank))
						return;
				}

				if (HealTarget != null) {
					if (Tank != null && Tank != HealTarget && Tank.HealthFraction < TankPr && Tank.HealthFraction < HealTarget.HealthFraction) {
						if (Healing (Tank))
							return;
					}

					if (HealTarget.HealthFraction < HealPr) {
						if (Healing (HealTarget))
							return;
					}
				}
			}

			if (Me.HealthFraction < 0.5) {
				if (Healing (Me))
					return;
			}

			if (Target == null) {
				Me.SetTarget (Me);
			}

			if (Target != null) {
				if (Target.IsFriendly && Target.HealthFraction < HealPr) {
					if (Healing (Target))
						return;
				}

				if (Target.IsEnemy) {
					if (Damage (Target))
						return;
				}

				if (Me.HealthFraction < 0.3)
					FlashHeal (Me);
			}
		}

		public bool Damage (UnitObject u)
		{
			//	actions=potion,name=draenic_intellect,if=buff.bloodlust.react|target.time_to_die<=40
			if (IsBoss (u) && (Me.HasAura ("Bloodlust") || TimeToDie (u) <= 40)) {
				if (DraenicIntellect ())
					return true;
			}
			//	actions+=/mindbender,if=talent.mindbender.enabled
			if (HasSpell ("Mindbender")) {
				if (Mindbender (u))
					return true;
			}
			//	actions+=/shadowfiend,if=!talent.mindbender.enabled
			if (!HasSpell ("Mindbender")) {
				if (Shadowfiend (u))
					return true;
			}
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/power_infusion,if=talent.power_infusion.enabled
			if (HasSpell ("Power Infusion"))
				PowerInfusion ();
			//	actions+=/shadow_word_pain,if=!ticking
			if (!u.HasAura ("Shadow Word: Pain", true)) {
				if (ShadowWordPain (u))
					return true;
			}
			//	actions+=/penance
			if (Penance (u))
				return true;
			//	actions+=/power_word_solace,if=talent.power_word_solace.enabled
			if (HasSpell ("Power Word: Solace")) {
				if (PowerWordSolace (u))
					return true;
			}
			//	actions+=/holy_fire,if=!talent.power_word_solace.enabled
			if (!HasSpell ("Power Word: Solace")) {
				if (HolyFire (u))
					return true;
			}
			//	actions+=/smite,if=glyph.smite.enabled&(dot.power_word_solace.remains+dot.holy_fire.remains)>cast_time
			if (HasGlyph (55692) && (u.AuraTimeRemaining ("Power Word: Solace") + u.AuraTimeRemaining ("Holy Fire")) > 1.5) {
				if (Smite (u))
					return true;
			}
			//	actions+=/shadow_word_pain,if=remains<(duration*0.3)
			if (u.AuraTimeRemaining ("Shadow Word: Pain", true) < 5.4) {
				if (ShadowWordPain ())
					return true;
			}
			//	actions+=/smite
			if (Smite (u))
				return true;
			//	actions+=/shadow_word_pain
			if (ShadowWordPain (u))
				return true;

			return false;
		}

		public bool Healing (UnitObject u)
		{
			if (Me.HasAura ("Evangelism"))
				Archangel ();
			//	actions=mana_potion,if=mana.pct<=75
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/power_infusion,if=talent.power_infusion.enabled
			PowerInfusion ();
			//	actions+=/power_word_solace,if=talent.power_word_solace.enabled
			if (PowerWordSolace (u))
				return true;
			//	actions+=/mindbender,if=talent.mindbender.enabled&mana.pct<80
			if (HasSpell ("Mindbender") && Me.ManaFraction < 0.8) {
				if (Mindbender (u))
					return true;
			}
			//	actions+=/shadowfiend,if=!talent.mindbender.enabled
			if (!HasSpell ("Mindbender")) {
				if (Shadowfiend (u))
					return true;
			}
			//	actions+=/power_word_shield
			if (PowerWordShield (u))
				return true;
			//	actions+=/penance_heal,if=buff.borrowed_time.up
			if (Me.HasAura ("Borrowed Time")) {
				if (Penance (u))
					return true;
			}
			//	actions+=/penance_heal
			if (Penance (u))
				return true;
			//	actions+=/flash_heal,if=buff.surge_of_light.react
			if (Me.HasAura ("Surge of Light")) {
				if (FlashHeal (u))
					return true;
			}
			//	actions+=/prayer_of_mending
			if (PrayerofMending (u))
				return true;
			//	actions+=/clarity_of_will
			if (ClarityofWill (u))
				return true;
			//	actions+=/heal,if=buff.power_infusion.up|mana.pct>20
			if (Me.HasAura ("Power Infusion") || Me.ManaFraction > 0.2) {
				if (Heal (u))
					return true;
			}
			//	actions+=/heal
			if (Heal (u))
				return true;
			if (FlashHeal (u))
				return true;

			return false;
		}
	}
}

//		public override void Combat()
//		{
//			if (!Target.IsEnemy || (Me.InCombat && Me.IsTargetingMeOrPets == true) || (CombatRole.Equals(CombatRole.DPS) && Me.Target == Me) || Me.Target.DisplayId == 49312)
//			{
//				Healer();
//			}
//			else
//			{
//				DPS();
//			}
//			//Dummy zapping
//			if (Target.DisplayId == 28048 || Target.DisplayId == 27510)
//			{
//				DPS();
//			}
//		}
//
//		void Healer()
//		{
//			// setting group
//			List<PlayerObject> members = Group.GetGroupMemberObjects();
//			int membercount = members.Count + 1;
//
//			if (membercount > 0)
//			{
//				// Finding Tank
//				List<PlayerObject> Tanks = members.FindAll(x => x.IsTank && x.IsInCombatRange && !x.IsDead);
//				PlayerObject Tank1 = Tanks.FirstOrDefault();
//
//				// Group Healing
//				if (Halo_talent == true && Use_Halo == true)
//				{
//					Halo();
//				}
//				else if (Cascade_talent == true)
//				{
//					Cascade();
//				}
//
//				List<PlayerObject> GrpHeal2 = members.FindAll(x => x.HealthFraction <= 0.7 && x.IsInCombatRange && !x.IsDead && x.Distance < 40);
//				PlayerObject healtest = GrpHeal2.FirstOrDefault(x => !x.IsDead && x.IsInCombatRange);
//
//				if (GrpHeal2.Count >= 2)
//				{
//					if (Tank1 != null)
//					{
//						if (Tank1.HealthFraction > healtest.HealthFraction)
//						{
//							Cast("Prayer of Healing", GrpHeal2.First());
//							DebugWrite("Casting Payer of Healing");
//							return;
//						}
//						else
//						{
//							return;
//						}
//					}
//				}
//				Holy_Nova();
//
//				if (Dispel_Group == true)
//				{
//					List<PlayerObject> GrpCleanse = members.FindAll(m => m.IsInCombatRange && m.Auras.Any(a => a.IsDebuff && "Magic,Disease".Contains(a.DebuffType)));
//					DebugWrite("" + GrpCleanse);
//					if (GrpCleanse.Count > 3)
//					{
//						CastOnTerrain("Mass Dispel", GrpCleanse.First().Position);
//						DebugWrite("" + GrpCleanse);
//						return;
//					}
//					foreach (var emd in Group.GetGroupMemberObjects().Where(x => x.IsInCombatRange && x.Auras.Any(a => a.IsDebuff && "Magic,Disease".Contains(a.DebuffType))))
//					{
//						if (Cast("Purify", emd)) return;
//					}
//				}
//
//
//				// Shield Tank
//				if (Tank1 != null)
//				{
//					if (Cast("Power Word: Shield", () => !Tank1.HasAura("Weakened Soul") && !Tank1.HasAura("Power Word: Shield") && Tank1.IsInCombatRange && !Tank1.IsDead, Tank1))
//					{
//						DebugWrite("Shielding " + Tank1);
//					}
//					if (Cast("Prayer of Mending", () => !Tank1.HasAura("Prayer of Mending") && Tank1.IsInCombatRange && !Tank1.IsDead, Tank1))
//					{
//						DebugWrite("POM on " + Tank1);
//					}
//					if (Clarity_of_Will == true)
//					{
//						ClarityOfWill();
//					}
//					if (Divine_Star == true)
//					{
//						DivineStar();
//					}
//
//
//				}
//
//				// Tank 1
//				if (Tank1 != null)
//				{
//					if (Tank1.IsInCombatRange)
//					{
//						if (Tank1.HealthFraction <= 0.3)
//						{
//							Cast("Flash Heal", () => Tank1.IsInCombatRange && !Tank1.IsDead, Tank1);
//							DebugWrite("Casting Flash Heal on " + Tank1);
//						}
//						else if (Tank1.HealthFraction <= 0.7 && !Me.HasAura("Saving Grace") && Saving_Grace == true && Tank1.IsInCombatRange)
//						{
//							Cast("Saving Grace",Tank1);
//						}
//						else if (Tank1.HealthFraction <= 0.7 && SpellCooldown("Pain Suppression") < 0)
//						{
//							Cast("Pain Suppression", () => Tank1.IsInCombatRange && !Tank1.IsDead && SpellCooldown("Pain Suppression") < 0, Tank1);
//							DebugWrite("Casting Pain Suppression on " + Tank1);
//						}
//						else if (Tank1.HealthFraction <= 0.8)
//						{
//							Cast("Heal", () => Tank1.IsInCombatRange && !Tank1.IsDead, Tank1);
//							DebugWrite("Casting Heal on " + Tank1);
//
//						}
//						else if (Tank1.HealthFraction <= 0.9 && SpellCooldown("Penance") < 0)
//						{
//							Cast("Penance", () => Tank1.IsInCombatRange && !Tank1.IsDead, Tank1);
//							DebugWrite("Casting Penance on " + Tank1);
//						}
//
//					}
//				}
//
//				// Tank 2
//				if (Tanks.Count > 1)
//				{
//					PlayerObject Tank2 = Tanks.Last();
//
//					if (Tank2 != null && Tank2.IsInCombatRange)
//					{
//
//
//						if (Cast("Power Word: Shield", () => !Tank2.HasAura("Weakened Soul") && !Tank2.HasAura("Power Word: Shield") && Tank2.IsInCombatRange && !Tank2.IsDead, Tank2))
//						{
//							DebugWrite("Shielding " + Tank2);
//						}
//						if (Cast("Prayer of Mending", () => !Tank2.HasAura("Prayer of Mending") && Tank2.IsInCombatRange && !Tank2.IsDead, Tank2))
//						{
//							DebugWrite("POM on " + Tank2);
//						}
//						if (Tank2.HealthFraction <= 0.3)
//						{
//							Cast("Flash Heal", () => Tank2.IsInCombatRange && !Tank2.IsDead, Tank2);
//							DebugWrite("Casting Flash Heal on " + Tank2);
//						}
//						else if (Tank2.HealthFraction <= 0.5 && !Me.HasAura("Saving Grace") && Saving_Grace == true && Tank2.IsInCombatRange)
//						{
//							Cast("Saving Grace", Tank2);
//						}
//						else if (Tank2.HealthFraction <= 0.7 && SpellCooldown("Pain Suppression") < 0)
//						{
//							Cast("Pain Suppression", () => Tank2.IsInCombatRange && !Tank2.IsDead, Tank2);
//							DebugWrite("Casting Pain Supression on " + Tank2);
//
//						}
//						else if (Tank2.HealthFraction <= 0.8)
//						{
//							Cast("Heal", () => Tank2.IsInCombatRange && !Tank2.IsDead, Tank2);
//							DebugWrite("Casting Heal on " + Tank2);
//
//						}
//						else if (Tank2.HealthFraction <= 0.9 && SpellCooldown("Penance") < 0)
//						{
//							Cast("Penance", () => Tank2.IsInCombatRange && !Tank2.IsDead, Tank2);
//							DebugWrite("Casting Penance on " + Tank2);
//
//						}
//					}
//					else
//					{
//						return;
//					}
//					//Second part of group healing, single target healing
//
//				}
//
//				foreach (var player in Group.GetGroupMemberObjects().Where(x => x.HealthFraction < 0.9 && x.IsInCombatRange))
//				{
//					Cast("Power Word: Shield", player, () => !player.HasAura("Weakened Soul") && !player.HasAura("Power Word: Shield") && player.IsInCombatRange && !player.IsDead && SpellCooldown("Power Word: Shield") < 0);
//					Cast("Saving Grace", player, ()=> player.HealthFraction <= 0.5 && !Me.HasAura("Saving Grace") && Saving_Grace == true && player.IsInCombatRangeAndLoS);
//					Cast("Flash Heal", player, () => !player.IsTank && !player.IsDead && player.HealthFraction < 0.5 && !player.IsDead && !Me.IsCasting);
//					Cast("Penance", player, () => !player.IsTank && !player.IsDead && player.HealthFraction < 0.75 && !player.IsDead && !Me.IsCasting && SpellCooldown("Penance") < 0);
//					Cast("Heal", player, () => !player.IsTank && !player.IsDead && player.HealthFraction < 0.8 && player.HealthFraction > 0.4 && !player.IsDead && !Me.IsCasting);
//				}
//				//Healing me
//				//------------Desperate prayer
//				if (Desperate_Prayer == true)
//				{
//					DesperatePrayer();
//				}
//				CastSelf("Power Word: Shield", () => !Me.HasAura("Weakened Soul") && !Me.HasAura("Power Word: Shield") && Me.HealthFraction < 0.99);
//				CastSelf("Heal", () => Me.HealthFraction <= 0.9);
//				CastSelf("Flash Heal", () => Me.HealthFraction <= 0.4);
//				//------------Desperate Prayer end
//				//Regen mana if low
//				//------------Mindbender
//				if (Mindbender == true)
//				{
//					MindBender();
//				}
//				//------------Mindbender end
//				if (Cast("Shadowfiend", () => Me.ManaFraction < 0.5 && Mindbender == false))
//				{
//					DebugWrite("Low Mana Releasing Shadowfiend");
//					return;
//				}
//
//
//
//			}
//		}
//
//		void DPS()
//		{
//			//Global cooldown check
//			if (HasGlobalCooldown())
//				return;
//			List<PlayerObject> members = Group.GetGroupMemberObjects();
//			List<PlayerObject> GrpHeal1 = members.FindAll(x => x.HealthFraction <= 0.85 && x.IsInCombatRangeAndLoS && !x.IsDead);
//
//			List<PlayerObject> Tanks = members.FindAll(x => x.IsTank && x.IsInCombatRange && !x.IsDead);
//			PlayerObject Tank1 = Tanks.FirstOrDefault();
//			if (Tank1 != null)
//			{
//				Cast("Power Word: Shield", () => !Tank1.HasAura("Weakened Soul") && !Tank1.HasAura("Power Word: Shield") && Tank1.IsInCombatRange && !Tank1.IsDead, Tank1);
//			}
//
//			if (!Me.Target.IsEnemy || Me.HealthFraction < 0.5 || GrpHeal1.Count >= 2 || Tank1Healing() == true)
//			{
//				Healer();
//			}
//			else if (Me.Target.IsEnemy && Me.HealthFraction > 0.5 && GrpHeal1.Count < 2 && Me.IsChanneling == false && Tank1Healing() == false)
//			{
//				if (HasGlobalCooldown())
//					return;
//				//------------Power Word: Solace
//				PowerWordSolace();
//				//------------Power Word: Solace end
//				//finding adds for Dots
//				List<UnitObject> mobs = Adds.FindAll(x => x.DistanceSquared < SpellMaxRangeSq("Shadow Word: Pain") && x.IsEnemy);
//				if (mobs.Count > 0)
//				{
//
//					//Finds adds for SWpain dot
//					foreach (var SWpain in mobs.Where(x => !x.HasAura("Shadow Word: Pain") && x.IsEnemy))
//					{
//						//Dots mobs in range
//						UnitObject SWP = SWpain;
//						CastPreventDouble("Shadow Word: Pain", () => !SWP.IsDead && SWP.IsEnemy, SWP);
//
//						return;
//					}
//				}
//
//				//mana regen
//				//-----------------Mindbender
//				if (Mindbender == true)
//				{
//					MindBender();
//				}
//				//-----------------Mindbender end
//				if (Cast("Shadowfiend", () => Me.ManaFraction < 0.5)) return;
//				//-------------Void Tendrils
//				if (Void_Tendrils == true)
//				{
//					VoidTendrils();
//				}
//				//-------------Void Tendrils end
//
//				// attack rotation
//				if (Cast("Penance", () => Target.IsEnemy)) return;
//
//
//
//				Cast("Holy Fire", () => !Target.HasAura("Holy Fire") && SpellCooldown("Holy Fire") < 0 && Target.IsEnemy);
//				Cast("Shadow Word: Pain", () => !Target.HasAura("Shadow Word: Pain") && Target.IsEnemy);
//				List<UnitObject> MS = Adds.FindAll(x => x.DistanceSquaredTo(Target) < 10 * 10 && x.IsEnemy && x.HasAura("Shadow Word: Pain"));
//				if (MS.Count >= 2 || Target.DisplayId == 28048)
//				{
//					Mind_Sear();
//				}
//
//				if (Cast("Smite", () => Me.ChannelingSpellID != 48045 && !Me.IsCasting && Me.Target.IsEnemy)) return;
//
//			}
//		}
//
//		//--------------Talent Start --------------
//		void DesperatePrayer() // Rotated in
//		{
//			if (CastPreventDouble("Desperate Prayer", () => Me.HealthFraction < 0.5 && !Me.IsCasting)) return;
//		}
//		void AngelicFeather() //Rotated in
//		{
//			if (CastOnTerrain("Angelic Feather", Me.PositionPredicted, () => Me.MovementSpeed > 0 && !HasAura("Angelic Feather"))) return;
//		}
//		void MindBender() //Rotated in
//		{
//			if (Cast("Mindbender", () => Me.ManaFraction < 0.5 && Me.Target.IsEnemy, Adds.FirstOrDefault()))
//			{
//				DebugWrite("Low Mana Casting Mindbender");
//			}
//		}
//		void PowerWordSolace() //Rotated in
//		{
//			List<UnitObject> mobs = Adds.FindAll(x => x.DistanceSquared < SpellMaxRangeSq("Power Word: Solace") && x.IsEnemy);
//			if (mobs.Count > 0)
//			{
//				//Solace CD Check
//				if (SpellCooldown("Power Word: Solace") < 0)
//				{
//
//					//Cast solace on add
//					var Solmob = mobs.FirstOrDefault();
//					CastPreventDouble("Power Word: Solace", () => !Solmob.IsDead && Solmob.IsInCombatRangeAndLoS && Me.Target.IsEnemy, Solmob);
//					DebugWrite("Casting Solace on " + Solmob + SpellCooldown("Power Word: Solace"));
//					return;
//				}
//			}
//
//			if (Cast("Power Word: Solace")) return;
//		}
//		void VoidTendrils() //Rotated in
//		{
//			List<UnitObject> mobs = Adds.FindAll(x => x.DistanceSquaredTo(Target) < 10 * 10);
//			if (CastPreventDouble("Void Tendrils", () => mobs.Count > 2, Adds.FirstOrDefault())) return;
//		}
//		void PsychicScream()
//		{
//			List<UnitObject> mobs = Adds.FindAll(x => x.DistanceSquaredTo(Me) < 8 * 8);
//			if (CastPreventDouble("Psychic Scream", () => mobs.Count > 2)) return;
//		}
//		void DominateMind()
//		{
//			//No idea how to get this working at the min :D
//		}
//		void SpiritShell()
//		{
//			if (CastPreventDouble("Spirit Shell", () => !Target.IsEnemy && Target.HealthFraction < 0.9 && Target.IsInCombatRangeAndLoS && !Me.HasAura("Spirit Shell"))) return;
//		}
//		void Cascade()//Rotated in
//		{
//			List<PlayerObject> members = Group.GetGroupMemberObjects();
//			if (members.Count > 0)
//			{
//				List<PlayerObject> GrpHeal1 = members.FindAll(x => x.HealthFraction <= 0.85 && x.IsInCombatRangeAndLoS && !x.IsDead);
//				PlayerObject healtarget = GrpHeal1.FirstOrDefault();
//				if (GrpHeal1.Count > 2)
//				{
//					if (CastPreventDouble("Cascade", () => Target.HealthFraction < 0.9 && SpellCooldown("Cascade") < 0, healtarget)) return;
//				}
//			}
//		}
//
//
//		void DivineStar()//Rotated in
//		{
//			List<PlayerObject> members = Group.GetGroupMemberObjects();
//			if (members.Count > 0)
//			{
//				// Finding Tank
//				List<PlayerObject> Tanks = members.FindAll(x => x.IsTank && x.IsInCombatRangeAndLoS && !x.IsDead);
//				PlayerObject Tank = Tanks.FirstOrDefault();
//				if (Tank != null)
//				{
//					Cast("Divine Start", () => !Tank.IsDead && Tank.HealthFraction < 0.8, Tank);
//					return;
//				}
//			}
//		}
//		void Halo()// Rotated in
//		{
//			List<PlayerObject> members = Group.GetGroupMemberObjects();
//			if (members.Count > 0)
//			{
//				List<PlayerObject> GrpHeal1 = members.FindAll(x => x.HealthFraction <= 0.85 && x.IsInCombatRangeAndLoS && !x.IsDead && x.DistanceSquaredTo(Me) < SpellMaxRangeSq("Halo"));
//				if (GrpHeal1.Count > 2)
//				{
//					if (CastPreventDouble("Halo", () => Target.HealthFraction < 0.9 && SpellCooldown("Halo") < 0)) return;
//				}
//			}
//		}
//		void ClarityOfWill()
//		{
//			List<PlayerObject> members = Group.GetGroupMemberObjects();
//			int membercount = members.Count + 1;
//
//			if (membercount > 0)
//			{
//
//				// Finding Tank
//				List<PlayerObject> Tanks = members.FindAll(x => x.IsTank && x.IsInCombatRange && !x.IsDead);
//				PlayerObject Tank1 = Tanks.FirstOrDefault();
//				if (Cast("Clarity of Will", ()=> Tank1.HealthMissing > 40000 && Me.Mana > 5000 && !Tank1.HasAura("Clarity of Will"), Tank1)) return;
//			}
//		}
//
//		//------------Talents End-----------
//
//		//------------Holy Nova
//		void Holy_Nova()
//		{
//			List<PlayerObject> members = Group.GetGroupMemberObjects();
//			if (members.Count > 0)
//			{
//				List<PlayerObject> GrpHeal1 = members.FindAll(x => x.HealthFraction <= 0.85 && x.IsInCombatRangeAndLoS && !x.IsDead && x.DistanceSquaredTo(Me) < SpellMaxRangeSq("Holy Nova"));
//				if (GrpHeal1.Count > 2)
//				{
//					if (CastPreventDouble("Holy Nova", () => Target.HealthFraction < 0.9 && SpellCooldown("Holy Nova") < 0)) return;
//				}
//			}
//		}
//
//		//------------Holy Nova end
//		void Mind_Sear()
//		{
//			Cast("Mind Sear", () => !Me.IsCasting && Me.Target.HasAura("Shadow Word: Pain"));
//		}
//
//	}
//}
//
