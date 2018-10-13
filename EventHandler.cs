using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public enum npcAction {TALK, WALK, ANIM, TURN, ENCOUNTER, SAVE, TITLE, PLAYSFX };

public class EventHandler : MonoBehaviour {

	private const float textSpd = 25;

	[System.Serializable]
	public class Event {
		[System.Serializable]
		public class Talk {
			public GameObject animMe;
			public int duck;
			public string name;
			public string[] dialouge;
			public Sprite[] image;
            public float pitch;
		}

		[System.Serializable]	
		public class Walk {
			public GameObject walker;
			public Vector2 spot;
			public float speed;
			public Sprite[] anim;
			public float fps;
		}

		[System.Serializable]	
		public class Anim {
			public GameObject animated;
			public Sprite[] anim;
			public float fps;
			public int frameAmt;
		}
		[System.Serializable]	
		public class Turn {
			public GameObject turner;
			public Sprite theTurn;
			public bool flip;
		}
		[System.Serializable]	
		public class Encounter {
			public string enemySet;
			public Sprite background;
            public AudioClip battleMusic, encMusic;
		}

		public npcAction act;
		public Talk talk;
		public Walk walk;
		public Anim anim;
		public Turn turn;
		public Encounter enc;
        public AudioClip playSFX;

		public npcAction getAct() { return this.act; }
		public Talk getTalk()	  { return this.talk; }
		public Walk getWalk()	  { return this.walk; }
		public Anim getAnim()	  { return this.anim; }
		public Turn getTurn()	  { return this.turn; }
		public Encounter getEnc() { return this.enc; }
	}

	public string nextScene; // Scene to load once event is done
	public bool newPos;
	public float xPos, yPos;
	public Event[] actionList; // Array of actions to show
	private int curAction; // Hold index of current action
	public GameObject blackScreen; // Blackscreen for transitioning
	private bool fadedIn; // Sees if the scene has been faded in yet

	// Variables for dialouge
	public GameObject textbox;
	public GameObject[] duckSprite;
	public TextMeshProUGUI speech;

    public AudioClip encMusic;
    public AudioClip[] talking;
    public GameObject mainSFX;

	private Dialouge curDia;
	private Dialouge getDia(Event.Talk curTalk){
		if (curDia == null)
			curDia = new Dialouge(curTalk.dialouge, curTalk.image, curTalk.pitch);
		return curDia;
	}
	private void delDia(){
		this.curDia = null;
		return;
	}

	private void displayTalking(bool disp){
		textbox.GetComponent<SpriteRenderer>().enabled = disp;
		for(int i = 0; i < duckSprite.Length; i++)
			duckSprite[i].GetComponent<SpriteRenderer>().enabled = disp;
		speech.enabled = disp;
	}

	// Use this for initialization
	void Start () {
		curAction = 0;
		showDisp();
		// Make screen black at first
		blackScreen.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 1);
	}
	
	// Update is called once per frame
	void Update () {
		if (!fadedIn) {
			// Fade to screen
			float theAlpha = blackScreen.GetComponent<SpriteRenderer>().color.a - Time.deltaTime;
			if (theAlpha < 0) {
				theAlpha = 0;
			}

			speech.color = new Color(1 - theAlpha, 1 - theAlpha, 1 - theAlpha);
			blackScreen.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, theAlpha);
			if (theAlpha == 0) {
				fadedIn = true;
			}
		}

		// Preform an action until it's done, then move onto the next
		if (curAction < actionList.Length) {
			switch (actionList[curAction].getAct()) {
				case npcAction.TALK:
					Event.Talk curTalk = actionList[curAction].getTalk();
					Dialouge dia = getDia(curTalk);

					if (dia.isDone()) {
						dia.resetDia();
						delDia();
						curAction++;
						showDisp();
					}
					else {
						if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
							if (dia.getReadingLine(dia.getLine(), textSpd) == dia.getLine()) {
								dia.nextLine();
								if (!dia.isDone()) {
									duckSprite[curTalk.duck].GetComponent<SpriteRenderer>().sprite = dia.getSprite();
									if(curTalk.animMe != null)
										curTalk.animMe.GetComponent<SpriteRenderer>().sprite = dia.getSprite();
								}
							}
							else {
								speech.text = curTalk.name + ": " + dia.getLine();
								dia.endLineRead();
							}
						}
						else {
							speech.text = curTalk.name + ": " + dia.getReadingLine(dia.getLine(), textSpd);
                            // Play talking sound effect
                            if(dia.IsNewChar()) {
                                mainSFX.GetComponent<AudioSource>().clip = talking[(int)Random.Range(0, talking.Length - 1)];
                                mainSFX.GetComponent<AudioSource>().pitch = dia.getPitch();
                                mainSFX.GetComponent<AudioSource>().Play();
                            }
                        }
					}
					break;



				case npcAction.WALK:
					Event.Walk curWalk = actionList[curAction].getWalk();
					if (moveToTarget(curWalk.walker, curWalk.spot, curWalk.speed)) {
						curAction++;
						showDisp();
					}
					else if (curWalk.walker.GetComponent<SpriteRenderer>() != null){
						UpdateSprite(curWalk.walker, curWalk.anim, curWalk.fps);
					}
					break;



				case npcAction.ANIM:
					Event.Anim curAnim = actionList[curAction].getAnim();
					if (WaitForXFrames(curAnim.frameAmt)) {
						curAction++;
						showDisp();
					}
					else {
						UpdateSprite(curAnim.animated, curAnim.anim, curAnim.fps);
					}
					break;



				case npcAction.TURN:
					Event.Turn curTurn = actionList[curAction].getTurn();
					curTurn.turner.GetComponent<SpriteRenderer>().sprite = curTurn.theTurn;
					if (curTurn.flip)
						curTurn.turner.transform.localScale = new Vector2(-curTurn.turner.transform.localScale.x, curTurn.turner.transform.localScale.y);
					curAction++;
					showDisp();
					break;



				case npcAction.ENCOUNTER:
					Event.Encounter curEnc = actionList[curAction].getEnc();

                    // Play encounter music
                    if(blackScreen.GetComponent<SpriteRenderer>().color.a == 0) {
                        mainSFX.GetComponent<AudioSource>().clip = encMusic;
                        mainSFX.GetComponent<AudioSource>().Play();
                    }

                    // Fade to black
                    float theAlpha = blackScreen.GetComponent<SpriteRenderer>().color.a + Time.deltaTime;
					if (theAlpha > 1) {
						theAlpha = 1;
					}
					// Fade text
					speech.color = new Color(1 - theAlpha, 1 - theAlpha, 1 - theAlpha);

					blackScreen.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, theAlpha);
					if (theAlpha == 1) {
						if (newPos)
							TitleManager.curFile.setLocation(xPos, yPos);
						TitleManager.curFile.setScene(nextScene);
						string foeSetDataPath = curEnc.enemySet;
						TextAsset set = Resources.Load<TextAsset>(@foeSetDataPath);
						GameProgress.jankFile setTxt = new GameProgress.jankFile(set);
						TitleManager.curFile.setEnemySet(readFoes(setTxt));
						TitleManager.curFile.setBackground(curEnc.background);
                        TitleManager.curFile.setSound(curEnc.battleMusic);
                        SceneManager.LoadScene("BattleScene", LoadSceneMode.Single);
					}
					break;



				case npcAction.SAVE:
					saveFile();
					curAction++;
					break;



				case npcAction.TITLE:
					if (newPos)
						TitleManager.curFile.setLocation(xPos, yPos);
					TitleManager.curFile.setScene(nextScene);
					saveFile();
					SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);
					curAction++;
					break;



                case npcAction.PLAYSFX:
                    mainSFX.GetComponent<AudioSource>().clip = actionList[curAction].playSFX;
                    mainSFX.GetComponent<AudioSource>().Play();
                    curAction++;
                    break;

                default:

					break;
			}

		}
		// Load the next scene
		else {
			// Fade to black
			float theAlpha = blackScreen.GetComponent<SpriteRenderer>().color.a + Time.deltaTime;
			if (theAlpha > 1) {
				theAlpha = 1;
			}
			// Fade text
			speech.color = new Color(1 - theAlpha, 1 - theAlpha, 1 - theAlpha);

			blackScreen.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, theAlpha);
			if (theAlpha == 1) {
				DungeonHandler.curState = gameState.OVERWORLD;
				DungeonHandler.preState = gameState.DIALOUGE;
				SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
			}
		}




	}

	// Dicide if the text display should be shown
	void showDisp(){
		if (curAction < actionList.Length) {
			displayTalking(actionList[curAction].act == npcAction.TALK || (actionList[curAction].act == npcAction.TURN && speech.enabled));
			if (actionList[curAction].act == npcAction.TALK) {
				int ducky = actionList[curAction].getTalk().duck;
				duckSprite[ducky].GetComponent<SpriteRenderer>().sprite = actionList[curAction].getTalk().image[0];
				if(actionList[curAction].getTalk().animMe != null)
					actionList[curAction].getTalk().animMe.GetComponent<SpriteRenderer>().sprite = actionList[curAction].getTalk().image[0];
			}
		}
	}

	// Changes the sprite of a gameObject based on given sprite animation and fps
	void UpdateSprite(GameObject dude, Sprite[] animation, float fps) {
		int index = (int)(Time.timeSinceLevelLoad * fps);
		index %= animation.Length;
		dude.GetComponent<SpriteRenderer>().sprite = animation[index];
		return;
	}

	// Moves a GameObject to a given location
	bool moveToTarget(GameObject entity, Vector2 target, float speed){
		entity.transform.localPosition = Vector2.MoveTowards(entity.transform.localPosition, target, speed * Time.deltaTime);
		if (entity.transform.localPosition.x == target.x && entity.transform.localPosition.y == target.y) {
			return true;
		}
		return false;
	}

	// A small function used to delay actions for a certain amount of time
	private int initialFrame;
	private bool WaitForXFrames(int x){
		if (initialFrame == 0) {
			initialFrame = Time.frameCount;
		}

		if (Time.frameCount - this.initialFrame >= x) {
			this.initialFrame = 0;
			return true;
		}
		return false;
	}
	private void resetWait() { this.initialFrame = 0; return; }

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

				List<Foe> spawnSet = new List<Foe>();

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
				}
				else if (split[split.Length - 1].CompareTo("Spawn") == 0) {
					skill = skillType.SPAWN;
					basePower = 0;
					effectChance = 0;
					isMagical = true;
					int spawnAmt, curSpawn;

					// Attack element
					s = input.ReadLine();
					split = s.Split(' ');
					atkEle = decideEle(split[split.Length - 1]);

					// mp cost
					s = input.ReadLine();
					split = s.Split(' ');
					int.TryParse(split[split.Length - 1], out mpCost);

					// range
					range = 1;
					// positions
					start = -1;
					end = -1;

					// Get number of dudes in spawn set
					s = input.ReadLine();
					split = s.Split(' ');
					int.TryParse(split[split.Length - 1], out spawnAmt);
					// Add each monster in the spawn set
					for (int k = 0; k < spawnAmt; k++) {
						s = input.ReadLine();
						int.TryParse(s, out curSpawn);
						spawnSet.Add(community[curSpawn].copy());
					}
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
				}

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

                // Get SFX path 
                input.ReadLine();
                skillSFX = input.ReadLine();

                Skill move = new Skill(skill, atkName, des, atkEle, mpCost, range, start, end, anim, skillSFX);
				if (skill == skillType.OFFENSIVE)
					move.setAttack(basePower, isMagical, effectChance);
				else if (skill == skillType.SPAWN)
					move.setSpawn(spawnSet);
				else if (skill == skillType.SUPPORT)		
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

	private void saveFile(){
		// Save file 1
		if (TitleManager.curSave == 1) {
			DataManager.file1 = TitleManager.curFile;
			DataManager.Save(1);
		}
		// Save file 2
		else if (TitleManager.curSave == 2) {
			DataManager.file2 = TitleManager.curFile;
			DataManager.Save(2);
		}
		// Save file 3
		else {
			DataManager.file3 = TitleManager.curFile;
			DataManager.Save(3);
		}
	}
}
