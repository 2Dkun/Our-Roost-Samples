using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum battleActions { TALK, PREACT, NOACT, COMBACT };

public class BattleEvent : MonoBehaviour {

	[System.Serializable]
	public class BEvent{
		[System.Serializable]
		public class Talk {
			public string name;
			public string[] dialouge;
			public Sprite[] image;
            public float pitch;
		}

		[System.Serializable]
		public class PreAct {
			// hold name and action of the named character
			// if name isn't in here, allow the player to make an action
			public string charName;
			public options act;
			public int target, subTarget;
			public int itemIndex;
			public bool doCombat;
		}
			
		[System.Serializable]
		public class NoAct {
			// hold list of actions that the player cannot select
			// for each action mentioned, have a talk class OR a string name and an array of sprites and strings for talking
			public options[] cantAct;
			public Talk[] displayMsg;
			public bool doCombat;
		}

		[System.Serializable]
		public class CombatAct {
			[System.Serializable]
			public class InputChange {
				public int onFrame;
				public bool[] onPress = new bool[7];
				public bool[] pressing = new bool[7];
				public bool[] released = new bool[7];
			}

			public bool isCombat;
			public InputChange[] inputs;
		}


		public battleActions act;
		public int onTurn; // Holds the turn in which the action will occur
		public Talk talk;
		public PreAct preact;
		public NoAct noact;
		public CombatAct combact;
	}

	public string postEvent;
	public BEvent[] battleActions;
	public string enemySet;
	public Sprite background;
    public AudioClip battleMusic;

	void Start(){
		TitleManager.curFile.setScene(postEvent);
		string foeSetDataPath = enemySet;
		TextAsset set = Resources.Load<TextAsset>(@foeSetDataPath);
		GameProgress.jankFile setTxt = new GameProgress.jankFile(set);
		TitleManager.curFile.setEnemySet(readFoes(setTxt));
		TitleManager.curFile.setBattleEvent(battleActions);
		TitleManager.curFile.setBackground(background);
        TitleManager.curFile.setSound(battleMusic);
		SceneManager.LoadScene("BattleScene", LoadSceneMode.Single);
	}

	public List<Foe> readFoes(GameProgress.jankFile input){
		// Determine number of unique foes
		string s = input.ReadLine();
		string[] split = s.Split(' ');
		int foeAmt = 0;
		int.TryParse(split[split.Length - 1], out foeAmt);

		// Read in each foe in the ecosystem
		Foe[] community = new Foe[foeAmt];
		for (int i = 0; i < foeAmt; i++) {
			input.ReadLine();
			input.ReadLine();
			string foeName = "";
			string path = "";
			int[] stats = new int[7];
			List<Skill> skillList = new List<Skill>();

			// Read in name
			s = input.ReadLine();
			split = s.Split(' ');
			for(int j = 1; j < split.Length; j++){
				foeName += split[j];
				if(j != split.Length -  1)
					foeName += " ";
			}
			// Read in path 
			path = input.ReadLine();
			// Read in stats
			s = input.ReadLine();
			split = s.Split(' ');
			for(int j = 1; j < split.Length; j++){
				int.TryParse(split[j], out stats[j - 1]);
			}
			// Open up the sprites in the path file
			Sprite[] source = Resources.LoadAll<Sprite>(@path);

			// Read in items
			int itemAmt;
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out itemAmt);
			Item[] drops = new Item[itemAmt];
			int[] dropRate = new int[itemAmt];

			for(int j = 0; j < itemAmt; j++){
				int itemID, itemCount, itemOdds;
				s = input.ReadLine();
				split = s.Split(' ');
				int.TryParse(split[0], out itemID);
				int.TryParse(split[1], out itemCount);
				int.TryParse(split[2], out itemOdds);

				Item theDrop = (Item) TitleManager.curFile.getItemList()[itemID];
				drops[j] = theDrop.copy(itemCount);
				dropRate[j] = itemOdds;
			}

			// Read in skills
			int skillAmt;
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out skillAmt);

			int[] skillOdds = new int[skillAmt];
			AItype[] ai = new AItype[skillAmt];
			for(int j = 0; j < skillAmt; j++){
				skillType skill;
				string atkName, des, skillSFX;
				element atkEle;
				int basePower, mpCost, range, effectChance;
				bool isMagical, animLoop;
				float start, end, fps;
				SpriteAnimation anim;

				battleType support = battleType.NULL;
				status statusMod = status.NULL;
				int scalar = 0;
				stat statBoost = stat.NULL;
				bool selfTarget = false;
				bool targetAlly = false;

				s = input.ReadLine();
				split = s.Split('-');
				atkName = split[1];
				des = input.ReadLine();

				// Read in odds then ai type
				s = input.ReadLine();
				split = s.Split(' ');
				int.TryParse(split[split.Length - 1], out skillOdds[j]);
				s = input.ReadLine();
				split = s.Split(' ');
				ai[j] = decideAI(split[split.Length - 1]);

				s = input.ReadLine();
				split = s.Split(' ');
				if (split[split.Length - 1].CompareTo("Offensive") == 0) {
					skill = skillType.OFFENSIVE;

					// Attack element
					s = input.ReadLine();
					split = s.Split(' ');
					atkEle = decideEle(split[split.Length - 1]);

					// base power
					s = input.ReadLine();
					split = s.Split(' ');
					int.TryParse(split[split.Length - 1], out basePower);

					// mp cost
					s = input.ReadLine();
					split = s.Split(' ');
					int.TryParse(split[split.Length - 1], out mpCost);

					// is magical
					s = input.ReadLine();
					split = s.Split(' ');
					bool.TryParse(split[split.Length - 1], out isMagical);

					// effect chance
					s = input.ReadLine();
					split = s.Split(' ');
					int.TryParse(split[split.Length - 1], out effectChance);
					if (effectChance > 0) {
						/* 
					 * implement me eventually
					 */
					}

					// range
					s = input.ReadLine();
					split = s.Split(' ');
					int.TryParse(split[split.Length - 1], out range);

					// positions
					s = input.ReadLine();
					split = s.Split(' ');
					float.TryParse(split[split.Length - 1], out start);
					s = input.ReadLine();
					split = s.Split(' ');
					float.TryParse(split[split.Length - 1], out end);

					// FPS
					s = input.ReadLine();
					split = s.Split(' ');
					float.TryParse(split[split.Length - 1], out fps);

					// Sprite animation
					int spriteAmt;
					s = input.ReadLine();
					split = s.Split(' ');
					if (split.Length > 2) {
						int.TryParse(split[split.Length - 2], out spriteAmt);
						animLoop = true;
					}
					else {
						int.TryParse(split[split.Length - 1], out spriteAmt);
						animLoop = false;
					}
					Sprite[] sprites = new Sprite[spriteAmt];
					for (int k = 0; k < spriteAmt; k++) {
						s = input.ReadLine();
						sprites[k] = getSprite(source, s);
					}
					anim = new SpriteAnimation(sprites, new int[0], fps, animLoop);
				}
				else {
					skill = skillType.SUPPORT;
					basePower = 0;
					effectChance = 0;
					isMagical = true;

					// Attack element
					s = input.ReadLine();
					split = s.Split(' ');
					atkEle = decideEle(split[split.Length - 1]);

					// mp cost
					s = input.ReadLine();
					split = s.Split(' ');
					int.TryParse(split[split.Length - 1], out mpCost);

					// range
					s = input.ReadLine();
					split = s.Split(' ');
					int.TryParse(split[split.Length - 1], out range);

					// positions
					s = input.ReadLine();
					split = s.Split(' ');
					float.TryParse(split[split.Length - 1], out start);
					s = input.ReadLine();
					split = s.Split(' ');
					float.TryParse(split[split.Length - 1], out end);

					// status variables
					s = input.ReadLine();
					split = s.Split(' ');
					support = decideSupport(split[split.Length - 1]);
					s = input.ReadLine();
					split = s.Split(' ');
					statusMod = decideStatus(split[split.Length - 1]);
					s = input.ReadLine();
					split = s.Split(' ');
					statBoost = decideStat(split[split.Length - 1]);
					s = input.ReadLine();
					split = s.Split(' ');
					int.TryParse(split[split.Length - 1], out scalar);
					s = input.ReadLine();
					split = s.Split(' ');
					bool.TryParse(split[split.Length - 1], out selfTarget);
					s = input.ReadLine();
					split = s.Split(' ');
					bool.TryParse(split[split.Length - 1], out targetAlly);

					// FPS
					s = input.ReadLine();
					split = s.Split(' ');
					float.TryParse(split[split.Length - 1], out fps);

					// Sprite animation
					int spriteAmt;
					s = input.ReadLine();
					split = s.Split(' ');
					if (split.Length > 2) {
						int.TryParse(split[split.Length - 2], out spriteAmt);
						animLoop = true;
					}
					else {
						int.TryParse(split[split.Length - 1], out spriteAmt);
						animLoop = false;
					}
					Sprite[] sprites = new Sprite[spriteAmt];
					for (int k = 0; k < spriteAmt; k++) {
						s = input.ReadLine();
						sprites[k] = getSprite(source, s);
					}
					anim = new SpriteAnimation(sprites, new int[0], fps, animLoop);
				}
                // Get SFX path 
                input.ReadLine();
                skillSFX = input.ReadLine();

                Skill move = new Skill(skill, atkName, des, atkEle, mpCost, range, start, end, anim, skillSFX);
				move.setAttack(basePower, isMagical, effectChance);
				if (skill == skillType.SUPPORT)		
					move.setSupport(support, statusMod, statBoost, scalar, selfTarget, targetAlly);
				skillList.Add(move);
			}
			// Hitboxes
			int hitboxAmt;
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out hitboxAmt);
			HitBox[] boxes = new HitBox[hitboxAmt];
			for (int j = 0; j < hitboxAmt; j++) {
				s = input.ReadLine();
				split = s.Split(' ');
				float x, y, xs, ys;
				float.TryParse(split[0], out x);
				float.TryParse(split[1], out y);
				float.TryParse(split[2], out xs);
				float.TryParse(split[3], out ys);

				boxes[j] = new HitBox((x / 2) + xs, (y / 2) + ys, (-x / 2) + xs, (-y / 2) + ys);
			}
			// Gravity
			float gravity;
			s = input.ReadLine();
			split = s.Split(' ');
			float.TryParse(split[split.Length - 1], out gravity);

			// Read in standard animations
			input.ReadLine();
			input.ReadLine();
			string facePath = input.ReadLine();
			Sprite face = getSprite(Resources.LoadAll<Sprite>(@facePath), input.ReadLine());
			List<SpriteAnimation> anims = new List<SpriteAnimation>();
			for (int j = 0; j < 5; j++) {
				input.ReadLine();
				// FPS
				float fps;
				s = input.ReadLine();
				split = s.Split(' ');
				float.TryParse(split[split.Length - 1], out fps);

				// Sprite animation
				int spriteAmt;
				s = input.ReadLine();
				split = s.Split(' ');
				int.TryParse(split[split.Length - 1], out spriteAmt);

				Sprite[] sprites = new Sprite[spriteAmt];
				for (int k = 0; k < spriteAmt; k++) {
					s = input.ReadLine();
					sprites[k] = getSprite(source, s);
				}
				SpriteAnimation anim = new SpriteAnimation(sprites, new int[0], fps, true);
				anims.Add(anim);
			}

			Foe inhabitant = new Foe(foeName, stats, skillList, skillOdds, ai, drops, dropRate, boxes, gravity);
			inhabitant.setAnimations(face, anims[0], anims[1], anims[2], anims[3], anims[4]);
			community[i] = inhabitant;
		}

		// Create an empty foe set so you don't have to use NULL
		int[] nothing = { 0, 0, 0, 0, 0, 0, 0 };
		Foe empty = new Foe("", nothing, new List<Skill>(), new int[0], new AItype[0], new Item[0], new int[0], new HitBox[0], 0);

		// Read in sets of enemies
		input.ReadLine();
		int setAmt;
		s = input.ReadLine();
		split = s.Split(' ');
		int.TryParse(split[split.Length - 1], out setAmt);
		List<Foe> foeList = new List<Foe>();
		for (int j = 0; j < 1; j++) {
			input.ReadLine();
			// Read in rate
			s = input.ReadLine();
			// Read in foes
			int foeCount;
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out foeCount);
			for (int k = 0; k < 5; k++) {
				if (k < foeCount) {
					int foeNum;
					s = input.ReadLine();
					int.TryParse(s, out foeNum);
					foeList.Add(community[foeNum]);
				}
				else {
					foeList.Add(empty);
				}
			}
		}
		return foeList;
	}

	private Sprite getSprite(Sprite[] sheet, string name){
		for (int i = 0; i < sheet.Length; i++) {
			if (sheet[i].name == name) {
				return sheet[i];
			}
		}
		return null;
	}

	private element decideEle(string ele){
		switch (ele) {
			case "Fire":
				return element.FIRE;
			case "Water":
				return element.WATER;
			case "Earth":
				return element.EARTH;
			case "Light":
				return element.LIGHT;
			case "Dark":
				return element.DARK;
			default:
				return element.NEUTRAL;
		}
	}

	private battleType decideSupport(string support){
		switch (support) {
			case "Heal":
				return battleType.HEAL;
			case "Cure":
				return battleType.CURE;
			case "Status":
				return battleType.STATUS;
			case "Boost":
				return battleType.BOOST;
			default:
				return battleType.NULL;
		}
	}

	private status decideStatus(string s){
		switch(s){
			case "KO":
				return status.KO;
			case "Bleed":
				return status.BLEED;
			case "Burn":
				return status.BURN;
			case "Freeze":
				return status.FREEZE;
			case "Paralyze":
				return status.PARALYZE;
			case "Poison":
				return status.POISON;
			case "Sleep":
				return status.SLEEP;
			default:
				return status.NULL;
		}
	}

	private stat decideStat(string s){
		switch (s) {
			case "Atk":
				return stat.ATK;
			case "Def":
				return stat.DEF;
			case "MAtk":
				return stat.MATK;
			case "MDef":
				return stat.MDEF;
			case "Spd":
				return stat.SPD;
			default:
				return stat.NULL;
		}
	}

	private AItype decideAI(string s){
		switch (s) {
			case "Lowest":
				return AItype.LOWEST;
			case "Highest":
				return AItype.HIGHEST;
			case "Random":
				return AItype.RANDOM;
			default:
				return AItype.CUSTOM;
		}
	}
}
