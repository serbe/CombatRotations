using System.Collections.Generic;
using ReBot.API;

namespace ReBot
{
	public abstract class SerbHashes : CombatRotation
	{
		public HashSet<string> PgUnits = new HashSet<string> {
			"Oto the Protector",
			"Sooli the Survivalist",
			"Kavan the Arcanist",
			"Ki the Assassin"
		};

		public HashSet<string> BurstAura = new HashSet<string> {
			"Adrenaline Rush",
			"Vendetta",
			"Breath of Sindragosa",
			"Celestial Alignment",
			"Berserk",
			"Incarnation: Chosen of Elune",
			"Incarnation: King of the Jungle",
			"Incarnation: Son of Ursoc",
			"Bestial Wrath",
			"Frenzy",
			"Rapid Fire",
			"Serenity",
			"Avenging Wrath",
			"Hand of Protection",
			"Pillar of Frost",
			"Icy Veins",
			"Ascendance",
			"Elemental Mastery",
			"Dark Soul: Misery",
			"Dark Soul: Knowledge",
			"Dark Soul: Instability",
			"Recklessness",
			"Bloodbath"
		};

		public HashSet<string> DefAura = new HashSet<string> {
			"Evasion",
			"Deterrence",
			"Die by the Sword",
			"Ice Block",
			"Evanesce",
			"Life Cocoon",
			"Divine Shield",
			"Touch of Karma"
		};

		public HashSet<string> LittleDefAura = new HashSet<string> {
			"Icebound Fortitude",
			"Bone Shield",
			"Survival Instincts",
			"Savage Defense",
			"Barkskin",
			"Ironbark",
			"Incarnation: Tree of Life",
			"Guard",
			"Hand of Purity",
			"Shield of the Righteous",
			"Ardent Defender",
			"Pain Suppression",
			"Power Word: Barrier",
			"Combat Readiness",
			"Shamanistic Rage",
			"Astral Shift",
			"Unending Resolve",
			"Dark Bargain",
			"Bladestorm",
			"Shield Block",
			"Shield Barrier"
		};

		public HashSet<string> ImmuneDefAura = new HashSet<string> {
			"Bladestorm"
		};


		public HashSet<string> RangeDefAura = new HashSet<string> {
			"Zen Meditation"
		};

		public HashSet<string> MagicDefAura = new HashSet<string> {
			"Diffuse Magic",
			"Devotion Aura",
			"Anti-Magic Shell",
			"Divine Protection"
		};

		public HashSet<string> AoeSkill = new HashSet<string> {
			"Blizzard"
		};

		public HashSet<string> BLSkill = new HashSet<string> {
			"Heroism",
			"Bloodlust",
			"Time Warp"
		};

		public HashSet<string> EnrageSkill = new HashSet<string> {
			"Adrenaline",
			"Angry",
			"Aspect of Thekal",
			"Beatdown",
			"Berserker Rage",
			"Berserking Howl",
			"Blackrock Rabies",
			"Blood Crazed",
			"Blood Rage",
			"Blood Scent",
			"Bloodletting Howl",
			"Bloody Rage",
			"Brewrific",
			"Brood Rage",
			"Burning Rage",
			"Charge Rage",
			"Charged Fists",
			"Combat Momentum",
			"Consuming Bite",
			"Cornered and Enraged!",
			"Delirious",
			"Demonic Enrage",
			"Desperate Rage",
			"Determination",
			"Dire Rage",
			"Dominate Slave",
			"Draconic Rage",
			"Electric Spur",
			"Enrage",
			"Enrage - Battle Pet",
			"Enrage Grublings",
			"Enrage Hatchling",
			"Enraged",
			"Enraged Assault",
			"Enraged Howl",
			"Enraged Mother",
			"Feed Pet",
			"Fel Ragamania",
			"Fellust",
			"Feral Instincts",
			"Ferocious Yell",
			"Ferocity",
			"Fervor",
			"Fixate",
			"Fixated",
			"Flip Out",
			"Flurry",
			"Flurry of Blows",
			"Frenzied Assault",
			"Frenzied Dive",
			"Frenzied for Blood",
			"Frenzied Leap",
			"Frenzy",
			"Frothing Rage",
			"Furious Anger",
			"Furious Roar",
			"Fury",
			"Getting Angry",
			"Hard Worker",
			"Held to Task",
			"Howl of Rage",
			"Hozen Rage",
			"Hunger For Blood",
			"Incite Frenzy",
			"Incite Rage",
			"Indomitable",
			"Iron Battle-Rage",
			"Kafa Rush",
			"Killing Rage",
			"Last of the Herd",
			"Laughing Skull",
			"Might of the Warsong",
			"Mighty!",
			"Mixture of Harnessed Rage",
			"Molten Fury",
			"Nimble Hands",
			"Overtime",
			"Primal Rage",
			"Protective Frenzy",
			"Protective Fury",
			"Protective Rage",
			"Rage",
			"Rage of Kros",
			"Rage of Ragnaros",
			"Rage of the Zandalari",
			"Rampage",
			"Reckless Zeal",
			"Red Frenzy",
			"Sadistic Frenzy",
			"Salivate",
			"Sara's Anger",
			"Savage Howl",
			"Seethe",
			"Sha Inspiration",
			"Shadowflame Axes",
			"Slaver's Rage",
			"Squealing Terror",
			"Stinger Rage",
			"Storm's Fury",
			"Swamp Rodent Surprise",
			"Titanic Strength",
			"Tormented Roar",
			"Tortured Enrage",
			"Treant's Rage",
			"Unflinching Resolve",
			"Unholy Frenzy",
			"Unholy Rage",
			"Unleashed Anger",
			"Unstable Strength",
			"Unstoppable Enrage",
			"Venomous Rage",
			"Victory Rush",
			"Warrior's Will",
			"Whip Rage",
			"Wild Beatdown",
			"Will of the Empress",
			"Wounded Pride",
			"Wrath of Ogudei"
		};

		public HashSet<string> SheepAura = new HashSet<string> {
			"Fear",
			"Polymorph",
			"Gouge",
			"Paralysis",
			"Blind",
			"Hex"
		};

		public HashSet<string> HealSpell = new HashSet<string> {
			"Blazing Light",
			"Cauterize Wound",
			"Circle of Healing",
			"Dark Mending",
			"Eternal Flame",
			"Flash Heal",
			"Flash of Light",
			"Great Heal",
			"Greater Heal",
			"Heal",
			"Heal Other",
			"Healing Wave of Antu'sul",
			"Holy Light",
			"Holy Nova",
			"Janet's Heal",
			"Lesser Heal",
			"Light of Dawn",
			"Major Heal",
			"Medi-Beam",
			"Prayer of Healing",
			"Tender Touch",
			"Unholy Darkness",
			"Chain Heal",
			"Fungal Regrowth",
			"Heal Me!",
			"Healing Touch",
			"Healing Wave",
			"Lesser Healing Wave",
			"Lunar Blessing",
			"Mend",
			"Nourish",
			"Regrowth",
			"Runic Mending",
			"Uplift",
			"Blood Drain",
			"Tranquility"
		};
	}
}

