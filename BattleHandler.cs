using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public enum options { EMPTY, ATTACK, DEFEND, SKILL, SWITCH, ITEMS, FLEE, STRUGGLE };

public class BattleHandler : MonoBehaviour {

	// Cameras
	public Camera battle;
	public GameObject blackScreen;
	public GameObject background;

	// Death Messages
	[System.Serializable]
	public class GameOverText {
		public character duck;
		public string[] dialouge;
		public Sprite[] image;
        public float pitch;
	}
	public GameOverText[] lastWords;
	private Dialouge curSpeech;
	private Ally lastTarget;
	public GameObject duckSprite;

	// Constants
	private const int MENU_OPTS = 6;
	private const int MAX_DUCKS = 5;
	private readonly float[] boostMul = { 0.1f, 0.25f, 0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 1.75f, 2.0f};
	private const float MAX_TIME = 3f;
	private const float WALKSPD = 3f;

	private enum battleStates {LOAD, TALK, SELECTION, PLAYERMSG, BATTLE, COMBAT, WIN, LOSE};
	private battleStates curState = battleStates.SELECTION;

	// Position for each combatant
	private readonly Vector2[] playersPos = new [] {
		new Vector2(-2.883f, 0.433f), 
		new Vector2(-3.25f, 0.729f), 
		new Vector2(-3.25f, 0.138f), 
		new Vector2(-3.635f, 0.926f), 
		new Vector2(-3.635f, -0.077f)
	};
	private readonly Vector2[] mobsPos = new [] {
		new Vector2(2.84f, 0.433f), 
		new Vector2(3.23f, 0.729f), 
		new Vector2(3.23f, 0.138f), 
		new Vector2(3.575f, 0.926f), 
		new Vector2(3.575f, -0.077f)
	};

	// Variables for event messages and such
	private int turnAmt;
	private BattleEvent.BEvent[] batEvents;
	private int atLastTurn;

	// Stuff for talking
	private Dialouge curDia;
	private Dialouge getDia(BattleEvent.BEvent.Talk curTalk){
		if (curDia == null)
			curDia = new Dialouge(curTalk.dialouge, curTalk.image, curTalk.pitch);
		return curDia;
	}

	// Main objects used to fight
	private List<Ally> party; // Holds the current party 
	public static List<Foe> enemies = new List<Foe>(); // Holds the current set of foes for the battle
	public GameObject[] allies = new GameObject[MAX_DUCKS];
	public GameObject[] foes = new GameObject[MAX_DUCKS];
	public GameObject[] statusHolder = new GameObject[MAX_DUCKS * 2]; // Used to show status animation over a character
	public TextMeshProUGUI timer;
	private float timeLeft;
	private List<GameObject> projectiles;

	// Handles selection of moves
	private int curOpt = 0, maxOpt = 6;
	private enum menus {MAIN, TARGET, SKILLS, ITEMS}; 
	private menus curMenu = menus.MAIN;
	public TextMeshProUGUI[] mainOptions = new TextMeshProUGUI[MENU_OPTS];
	public TextMeshProUGUI[] subOptions = new TextMeshProUGUI[MENU_OPTS];
	public TextMeshProUGUI description;
	public TextMeshProUGUI eventText;
	private string[] menuDes = { 
		"Perform a weak physical hit that will damage one foe.",
		"Brace yourself for one turn to take less damage.",
		"Use one of your special attacks to perform in battle.",
		"Swap your position with an ally.",
		"Use one of your battle items that's in your inventory.",
		"Make an attempt to flee from battle."
	};
	public TextMeshProUGUI[] skillInfo;

	// Shows what character is being selected
	private int curChar = 0; 

	// Used to keep track of what moves were selected
	private readonly int[] trueTarget = { 3, 1, 0, 2, 4 }; // Convert to ordered number
	private readonly int[] layerTarget = { 2, 1, 3, 0, 4 }; // Convert to layer numbers
	private options[] actions = new options[MAX_DUCKS];
	private int curTarget = MAX_DUCKS / 2;
	private bool targetIsFoe;
	private int[] target = new int[MAX_DUCKS]; // Keeps track of the target for actions
	private int[] subSelect = new int[MAX_DUCKS]; // Used for skills and items
	private bool[] preventCombat = new bool[MAX_DUCKS]; // Used to see if current player can go into combat phase
	private Item[] itemSel = new Item[MAX_DUCKS];
	private int enemyTarget; // Keeps track of current target for enemy
	private Skill curESkill;
	private int[] combatantOrder = new int[MAX_DUCKS * 2];
	private int curTurn = 0; // Used to keep track of the battle phase
	private bool fled = false; // True when user actually flees
	private int expPool = 0; // Holds total exp gained as enemies are killed
	private List<Item> dropPool;
	private int curExpGain;

	// Slots
	public GameObject activeBars, nonactiveBars, barDown;
	private Sprite[] textFrames = new Sprite[2];
	private Sprite[] slotFrames = new Sprite[2];
	public GameObject textBox;
	public Sprite[] textboxDisp;
	public GameObject[] slots = new GameObject[MAX_DUCKS];
	public TextMeshProUGUI[] names = new TextMeshProUGUI[MAX_DUCKS];
	public GameObject[] namePos = new GameObject[MAX_DUCKS];
	public GameObject[] portrait = new GameObject[MAX_DUCKS];
	public GameObject[] symbol = new GameObject[MAX_DUCKS];
	public TextMeshProUGUI[] health = new TextMeshProUGUI[MAX_DUCKS];
	public TextMeshProUGUI[] mana = new TextMeshProUGUI[MAX_DUCKS];
	public GameObject[] hp = new GameObject[MAX_DUCKS];
	public GameObject[] mp = new GameObject[MAX_DUCKS];	
	public GameObject[] meter = new GameObject[MAX_DUCKS];
	public GameObject[] exp = new GameObject[MAX_DUCKS];
	// 2D array of stat displays
	[System.Serializable]
	public class statSet{
		public GameObject[] stats;
	}
	public statSet[] statSets = new statSet[MAX_DUCKS];

	// Sprites for statuses
	private SpriteAnimation buff;
	private SpriteAnimation debuff;
	private SpriteAnimation heal;
	private SpriteAnimation cure;
	private SpriteAnimation regen;
	private SpriteAnimation bleed;

	// Sprites for status display
	[System.Serializable]
	public class statDisp{
		public Sprite[] stats;
	}
	public statDisp[] statDisplays = new statDisp[MAX_DUCKS];
	public Sprite[] statusEffects = new Sprite[8];

    // Sound Players
    public GameObject mainSFX;
    public GameObject mainBGM;
    public AudioClip[] statusSFX;
    public AudioClip itemUse;
    public AudioClip select, cancel;
    public AudioClip[] fighterSFX;
    public AudioClip[] quack;
    private float pitch;
    public AudioClip win, lose;
    public AudioClip levelGain, levelUp;

    void Start () {
		// Read in tutorial text if it hasn't been done yet
		batEvents = TitleManager.curFile.getBattleEvent();

		// Hide game over duck
		duckSprite.GetComponent<SpriteRenderer>().enabled = false;

		// Update background
		background.GetComponent<SpriteRenderer>().sprite = TitleManager.curFile.getBackground();

		// Make sure variables are initialized correctly
		enemies = TitleManager.curFile.getEnemySet();
		dropPool = new List<Item>();
		curState = battleStates.LOAD;
		curOpt = 0;
		curMenu = menus.MAIN;

		// Make status animations
		Sprite[] battleSprites = Resources.LoadAll<Sprite>(@"Sprites/Battle assets");
		textFrames[0] = getSprite(battleSprites, "battleMenu");
		textFrames[1] = getSprite(battleSprites, "battleMenu"); // change me later into speech menu
		slotFrames[0] = getSprite(battleSprites, "AllySlot");
		slotFrames[1] = getSprite(battleSprites, "EnemySlot");
		string[] statusNames = { "buff", "debuff", "heal", "cure", "regen", "bleed" };
		Sprite[][] statusAnims = new Sprite[statusNames.Length][];
		for(int i = 0; i < statusNames.Length; i++)
			statusAnims[i] = new Sprite[5];
		for (int i = 1; i <= 5; i++) {
			for (int j = 0; j < statusNames.Length; j++) {
				string temp = statusNames[j];
				temp += i.ToString();
				statusAnims[j][i - 1] = getSprite(battleSprites, temp);
			}
		}
		buff = new SpriteAnimation(statusAnims[0], new int[0], 20, true);
		debuff = new SpriteAnimation(statusAnims[1], new int[0], 20, true);
		heal = new SpriteAnimation(statusAnims[2], new int[0], 20, true);
		cure = new SpriteAnimation(statusAnims[3], new int[0], 20, true);
		regen = new SpriteAnimation(statusAnims[4], new int[0], 20, true);
		bleed = new SpriteAnimation(statusAnims[5], new int[0], 20, true);

		// Hide all of the text that would appear in battle
		textDisplay(mainOptions, false);
		textDisplay(subOptions, false);
		description.enabled = false;
		eventText.enabled = false;
		timer.enabled = false;
		textDisplay(names, false);
		textDisplay(health, false);
		textDisplay(mana, false);
		textDisplay(skillInfo, false);

		// Hide combatants
		for (int i = 0; i < allies.Length; i++) {
			allies[i].GetComponent<SpriteRenderer>().enabled = false;
			foes[i].GetComponent<SpriteRenderer>().enabled = false;
		}
	}
	
	void Update () {
		TitleManager.curFile.updateTime();

        switch (curState) {
			case battleStates.LOAD:

                // Load battle music
                mainBGM.GetComponent<AudioSource>().clip = TitleManager.curFile.getSound();
                mainBGM.GetComponent<AudioSource>().Play();

                // Make sure variables are set correctly
                curOpt = 0;
				maxOpt = MENU_OPTS;
				curMenu = menus.MAIN;
				curTarget = 2;

				// Hide status animation
				for (int i = 0; i < statusHolder.Length; i++) {
					statusHolder[i].GetComponent<SpriteRenderer>().enabled = false;
				}
				// Show and update main menu text
				textDisplay(mainOptions, true);
				updateSelection(mainOptions, curOpt);
				description.enabled = true;
				description.text = "Perform a weak physical hit that will damage one foe.";

				// Update the player slots with correct data
				party = TitleManager.curFile.getParty();
				updateSlots(party);

				// Reset stat boosts
				for (int i = 0; i < party.Count; i++) {
					party[i].resetBoosts();
					party[i].resetEX();
				}

				// Hide enemies that don't exist
				for (int i = 0; i < enemies.Count; i++) {
					if (enemies[i].getCurHP() <= 0) {
						foes[i].GetComponent<SpriteRenderer>().enabled = false;
					}
					else {
						foes[i].GetComponent<SpriteRenderer>().enabled = true;
						foes[i].GetComponent<SpriteRenderer>().sprite = enemies[i].playIdle();
					}
				}

				// Reset order of combatants?

				if (batEvents != null) {
					textDisplay(mainOptions, false);
					description.enabled = false;
					eventText.enabled = true;
					duckSprite.GetComponent<SpriteRenderer>().enabled = true;
					duckSprite.GetComponent<SpriteRenderer>().sprite = batEvents[atLastTurn].talk.image[0];
					curState = battleStates.TALK;
					textBox.GetComponent<SpriteRenderer>().sprite = textboxDisp[1];
				}
				else {
					curState = battleStates.SELECTION;
				}
				break;



			case battleStates.TALK:
				
				// Set idle animations for everyone
				for (int i = 0; i < MAX_DUCKS; i++) {
					if (party[i].getName().CompareTo("") != 0) {
						if (party[i].getCurHP() > 0)
							allies[i].GetComponent<SpriteRenderer>().sprite = party[i].playIdle();
						else
							allies[i].GetComponent<SpriteRenderer>().sprite = party[i].playKO();
					}
					if (enemies[i].getCurHP() > 0) {
						foes[i].GetComponent<SpriteRenderer>().sprite = enemies[i].playIdle();
					}
				}

				if (batEvents == null) {
					eventText.enabled = false;
					duckSprite.GetComponent<SpriteRenderer>().enabled = false;
					curState = battleStates.SELECTION;
				}
				else if(atLastTurn >= batEvents.Length || batEvents[atLastTurn].act != battleActions.TALK 
					|| batEvents[atLastTurn].onTurn != turnAmt) {
					eventText.enabled = false;
					duckSprite.GetComponent<SpriteRenderer>().enabled = false;
					curState = battleStates.SELECTION;
				}
				else {
					bool updateSprite = false;
					if (this.curDia == null) {
						updateSprite = true;
					}
					BattleEvent.BEvent.Talk curTalk = batEvents[atLastTurn].talk;
					Dialouge dia = getDia(curTalk);

					if (updateSprite) {
						duckSprite.GetComponent<SpriteRenderer>().enabled = true;
						duckSprite.GetComponent<SpriteRenderer>().sprite = dia.getSprite();
					}

					if (dia.isDone()) {
						dia.resetDia();
						this.curDia = null;
						atLastTurn += 1;
					}
					else {
						if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
							if (dia.getReadingLine(dia.getLine(), 25) == dia.getLine()) {
								dia.nextLine();
								if (!dia.isDone()) {
									duckSprite.GetComponent<SpriteRenderer>().sprite = dia.getSprite();
								}
							}
							else {
								eventText.text = curTalk.name + ": " + dia.getLine();
								dia.endLineRead();
							}
						}
						else {
							eventText.text = curTalk.name + ": " + dia.getReadingLine(dia.getLine(), 25);

                            // Play select cancel effect
                            if(dia.IsNewChar()) {
                                mainSFX.GetComponent<AudioSource>().clip = quack[(int)Random.Range(0,quack.Length - 1)];
                                mainSFX.GetComponent<AudioSource>().pitch = dia.getPitch();
                                mainSFX.GetComponent<AudioSource>().Play();
                            }
                        }
					}
				}
				break;


			case battleStates.SELECTION:
				
				// Check if the character has made a premove
				bool premove = false;
				if (batEvents != null) {
					for (int i = atLastTurn; i < batEvents.Length; i++) {
						if (batEvents[i].onTurn != turnAmt) {
							break;
						}
						if (batEvents[i].act == battleActions.PREACT) {
							if (batEvents[i].preact.charName == party[curChar].getName()) {
								if (Input.GetKey(DataManager.savedOptions.controls[(int)key.B2])) {
									curChar -= 1;
									if (curChar < 0)
										curChar = 0;
								}
								else {
									if (batEvents[i].preact.act == options.ATTACK) {
										actions[curChar] = options.SKILL;
										subSelect[curChar] = 0;
									}
									else {
										actions[curChar] = batEvents[i].preact.act;
										subSelect[curChar] = batEvents[i].preact.subTarget;
									}
									target[curChar] = batEvents[i].preact.target;
									itemSel[curChar] = (Item)TitleManager.curFile.getItemList()[batEvents[i].preact.itemIndex];
									preventCombat[curChar] = !batEvents[i].preact.doCombat;

									premove = true;
									curChar += 1;
								}
								break;
							}
						}
					}
				}

				// Show/hide status menu
				if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B3]) && curMenu != menus.TARGET && !premove) {
					if (textBox.GetComponent<SpriteRenderer>().enabled == true) {
						// Show only the status slots at the bottom of the screen
						textBox.GetComponent<SpriteRenderer>().enabled = false;
						for (int i = 0; i < slots.Length; i++) {
							slots[i].transform.localPosition = new Vector2(slots[i].transform.localPosition.x, -1.79f);
							names[i].GetComponent<Transform>().position = Camera.main.WorldToScreenPoint(namePos[i].transform.position);

							health[i].GetComponent<RectTransform>().position = new Vector2(health[i].GetComponent<RectTransform>().position.x, Camera.main.WorldToScreenPoint(barDown.transform.position).y);
							mana[i].GetComponent<RectTransform>().position = new Vector2(mana[i].GetComponent<RectTransform>().position.x, Camera.main.WorldToScreenPoint(barDown.transform.position).y);
						}

						// Hide all of the text that would appear in battle
						textDisplay(mainOptions, false);
						textDisplay(subOptions, false);
						textDisplay(skillInfo, false);
						description.enabled = false;
                    }
					else {
						// Move all of the slots back to how they were
						textBox.GetComponent<SpriteRenderer>().enabled = true;
						updateSlots(TitleManager.curFile.getParty());

						// Show the text that would appear in battle based on the current menu
						description.enabled = true;
						if (curMenu == menus.SKILLS || curMenu == menus.ITEMS) {
							textDisplay(subOptions, true);
							textDisplay(skillInfo, true);
						}
						else {
							textDisplay(mainOptions, true);
						}
                    }

                    // Play sound effect
                    mainSFX.GetComponent<AudioSource>().clip = select;
                    mainSFX.GetComponent<AudioSource>().Play();

                }

				// Set idle animations for everyone
				for (int i = 0; i < MAX_DUCKS; i++) {
					if (party[i].getName().CompareTo("") != 0) {
						if (party[i].getCurHP() <= 0)
							allies[i].GetComponent<SpriteRenderer>().sprite = party[i].playKO();
						else if (i == curChar && !premove)
							allies[i].GetComponent<SpriteRenderer>().sprite = party[i].playThink();
						else if (party[i].getCurHP() > 0)
							allies[i].GetComponent<SpriteRenderer>().sprite = party[i].playIdle();
					}
					if (enemies[i].getCurHP() > 0) {
						foes[i].GetComponent<SpriteRenderer>().sprite = enemies[i].playIdle();
					}
				}
					
				// Skip character if they're KO'd or nonexistent
				if (party[curChar].getCurHP() <= 0 || party[curChar].getName().CompareTo("") == 0) {
					actions[curChar] = options.EMPTY;
					curChar += 1;

					// Hide all of the text that would appear in battle
					textDisplay(mainOptions, false);
					description.enabled = false;
				}
				else {
					// Otherwise wait for player input for current character
					if (textBox.GetComponent<SpriteRenderer>().enabled == true && !premove) {
						textBox.GetComponent<SpriteRenderer>().sprite = textboxDisp[0];

						// Show all of the text that would appear in battle
						if (!subOptions[0].enabled && textBox.GetComponent<SpriteRenderer>().enabled) {
							textDisplay(mainOptions, true);
							description.enabled = true;
						}

						// Move the current character's slot up
						setSlotActive(curChar);
						// Allow user to move through the various options and make selections
						handelOpts(party);
					}
				}

				// End selection phase once all moves have been selected
				if (curChar >= MAX_DUCKS) {
					curChar = 0;
					curTurn = 0;
					timeLeft = MAX_TIME;
					setSlotActive(-1);
					// Hide all of the text that would appear during selection
					textDisplay(mainOptions, false);
					textDisplay(subOptions, false);
					textDisplay(skillInfo, false);
					description.enabled = false;
					// Arrange the combatants based on speed and priority
					setTurnOrder(party, enemies);
					curState = battleStates.BATTLE;
					textBox.GetComponent<SpriteRenderer>().sprite = textboxDisp[1];
				}
				break;



			case battleStates.PLAYERMSG:
				// Speak last words
				if (curSpeech.isDone()) {
					textDisplay(mainOptions, true);
					description.enabled = true;
					eventText.enabled = false;
					eventText.text = "";
					duckSprite.GetComponent<SpriteRenderer>().enabled = false;
					curState = battleStates.SELECTION;
                    mainSFX.GetComponent<AudioSource>().pitch = 1;
                }
				else {
					if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
						if (curSpeech.getReadingLine(curSpeech.getLine(), 25) == curSpeech.getLine()) {
							curSpeech.nextLine();
							if (!curSpeech.isDone())
								duckSprite.GetComponent<SpriteRenderer>().sprite = curSpeech.getSprite();
						}
						else {
							eventText.text = curSpeech.getName() + ": " + curSpeech.getLine();
							curSpeech.endLineRead();
						}
					}
					else {
						eventText.text = curSpeech.getName() + ": " + curSpeech.getReadingLine(curSpeech.getLine(), 25);
                        // Play talking sound effect
                        if(curSpeech.IsNewChar()) {
                            mainSFX.GetComponent<AudioSource>().clip = quack[(int)Random.Range(0,quack.Length - 1)];
                            mainSFX.GetComponent<AudioSource>().pitch = curSpeech.getPitch();
                            mainSFX.GetComponent<AudioSource>().Play();
                        }
                    }
				}
				break;



			case battleStates.BATTLE:

				// Place everyone into idle animation at first
				for (int i = 0; i < MAX_DUCKS; i++) {
					if (party[i].getName().CompareTo("") != 0) {
						if (party[i].getCurHP() > 0) {
							allies[i].GetComponent<SpriteRenderer>().sprite = party[i].playIdle();
						}
						else
							allies[i].GetComponent<SpriteRenderer>().sprite = party[i].playKO();
					}
					if (enemies[i].getCurHP() > 0) {
						foes[i].GetComponent<SpriteRenderer>().sprite = enemies[i].playIdle();
					}
				}

				if (curTurn <= 0)
					eventText.enabled = true; // Show event text

				if (curTurn < MAX_DUCKS * 20) {
					int curDude = combatantOrder[curTurn / (MAX_DUCKS * 2)];
					// Player action
					if (curDude < MAX_DUCKS) {
						if (party[curDude].getCurHP() <= 0) {
							actions[curDude] = options.EMPTY;
						}

						switch (actions[curDude]) {
							case options.SWITCH:
								// Check if the target is still alive
								if (party[target[curDude]].getCurHP() > 0) {
									if ((curTurn % (MAX_DUCKS * 2)) < 1) {
										eventText.text = party[curDude].getName() + " switches spots with " + party[target[curDude]].getName() + "."; 
										if (allies[curDude].transform.localPosition.x >= allies[target[curDude]].transform.localPosition.x) {
											allies[curDude].transform.localScale = new Vector2(-1, 1);
											allies[target[curDude]].transform.localScale = new Vector2(1, 1);
										}
										else {
											allies[curDude].transform.localScale = new Vector2(1, 1);
											allies[target[curDude]].transform.localScale = new Vector2(-1, 1);
										}
										curTurn += 1;
									}
									else {
										Vector2 target1 = playersPos[target[curDude]];
										Vector2 target2 = playersPos[curDude];
										Vector2 midpoint = target1 + ((target2 - target1) / 2);
										// Walk to respective positions
										bool targetReached = moveToTarget(allies[curDude], target1, WALKSPD / 2);
										bool targetReached2 = moveToTarget(allies[target[curDude]], target2, WALKSPD / 2);
										// Play walk animations
										allies[curDude].GetComponent<SpriteRenderer>().sprite = party[curDude].getFighter().playWalk(allies[curDude].transform.localScale.x == 1);
										allies[target[curDude]].GetComponent<SpriteRenderer>().sprite = party[target[curDude]].getFighter().playWalk(allies[target[curDude]].transform.localScale.x == 1);
										// Swap the players' render order at the midpoint
										if (Vector2.Distance(allies[curDude].transform.localPosition, midpoint) < 0.05f) {
											allies[curDude].GetComponent<SpriteRenderer>().sortingOrder = layerTarget[target[curDude]] * 2;
											allies[target[curDude]].GetComponent<SpriteRenderer>().sortingOrder = layerTarget[curDude] * 2;
										}
										if (targetReached && targetReached2) {
											// Return the objects back to how they were but have everything swapped so things don't get confusing
											allies[curDude].transform.localPosition = target2;
											allies[target[curDude]].transform.localPosition = target1;
											// Swap the render order again
											allies[curDude].GetComponent<SpriteRenderer>().sortingOrder = layerTarget[curDude] * 2;
											allies[target[curDude]].GetComponent<SpriteRenderer>().sortingOrder = layerTarget[target[curDude]] * 2;
											// Face forward again
											allies[curDude].transform.localScale = new Vector2(1, 1);
											allies[target[curDude]].transform.localScale = new Vector2(1, 1);
											// Swap data
											Ally temp = party[curDude];
											party[curDude] = party[target[curDude]];
											party[target[curDude]] = temp;
											updateSlots(party);
											// Swap current options
											options tempOpt = actions[curDude];
											actions[curDude] = actions[target[curDude]];
											actions[target[curDude]] = tempOpt;
											// Swap current sub selections
											int tempSub = subSelect[curDude];
											subSelect[curDude] = subSelect[target[curDude]];
											subSelect[target[curDude]] = tempSub;
											// Find turn order of target
											for (int i = 0; i < combatantOrder.Length; i++) {
												if (combatantOrder[i] == target[curDude]) {
													// Swap turn order
													int tempOrder = combatantOrder[curTurn / (MAX_DUCKS * 2)];
													combatantOrder[curTurn / (MAX_DUCKS * 2)] = combatantOrder[i];
													combatantOrder[i] = tempOrder;
													break;
												}
											}
											// Swap current targets
											int tempTar = target[curDude];
											target[curDude] = target[tempTar];
											target[tempTar] = tempTar;
											endTurn();
										}
									}
								}
								else {
									endTurn();
								}
								break;

							case options.SKILL:
								Skill curSkill = party[curDude].getSkill(subSelect[curDude]);
								eventText.text = party[curDude].getName() + " uses " + curSkill.getName() + ".";
								if (curSkill.getType() == skillType.OFFENSIVE) {
									// Make sure target is still alive, change target otherwise
									if ((curTurn % (MAX_DUCKS * 2)) < 1) {
										if (party[curDude].getCurMP() >= curSkill.getMpCost()) {
											while (enemies[target[curDude]].getCurHP() <= 0) {
												target[curDude] += 1;
												target[curDude] %= MAX_DUCKS;
											}
											curTurn += 1;
										}
										else {
											eventText.text = party[curDude].getName() + " doesn't have enough MP to use " + curSkill.getName() + "...";
											if (WaitForXFrames(15)) {
												endTurn();	
											}
										}
									}
									// Walk to target
									else if ((curTurn % (MAX_DUCKS * 2)) < 2) {
										allies[curDude].GetComponent<SpriteRenderer>().sprite = party[curDude].getFighter().playWalk(true);
										Vector2 targetPos = new Vector2(mobsPos[target[curDude]].x - curSkill.getStartDist(), mobsPos[target[curDude]].y);
										bool targetReached = moveToTarget(allies[curDude], targetPos, WALKSPD);
										// Change layer order of player once they reach the midpoint of the screen
										if (allies[curDude].transform.localPosition.x > (targetPos.x + playersPos[0].x) / 2) {
											allies[curDude].GetComponent<SpriteRenderer>().sortingOrder = (layerTarget[target[curDude]] * 2) + 1;
										}
										// Go to skill display
										if (targetReached) {
											allies[curDude].GetComponent<SpriteRenderer>().sprite = curSkill.playSkill();
											curTurn += 1;
                                        }
									}
									// Play skill
									else if ((curTurn % (MAX_DUCKS * 2)) < 3) {
										if (!curSkill.skillIsDone()) {
											allies[curDude].GetComponent<SpriteRenderer>().sprite = curSkill.playSkill();
										}
										else {
                                            // Play skill sound effect
                                            mainSFX.GetComponent<AudioSource>().clip = curSkill.getSFX();
                                            mainSFX.GetComponent<AudioSource>().Play();

                                            // Apply mp cost
                                            party[curDude].adjustMP(-1 * curSkill.getMpCost());
											updateSlots(party);
	
											// Moveplayer to where they should end up after the move
											allies[curDude].transform.localPosition = new Vector2(mobsPos[target[curDude]].x - curSkill.getEndDist(), mobsPos[target[curDude]].y);

											// Show hurt enemy
											int theTarget = layerTarget[target[curDude]];
											for (int i = 0; i < curSkill.getRange(); i++) {
												int finalT = 0;
												if (i % 2 == 1 && theTarget + (i / 2) + 1 < MAX_DUCKS) {
													finalT = trueTarget[theTarget + (i / 2) + 1];
												}
												else if (i % 2 == 0 && theTarget - (i / 2) >= 0) {
													finalT = trueTarget[theTarget - (i / 2)];
												}

												if(enemies[finalT].getHP() > 0)
													foes[finalT].GetComponent<SpriteRenderer>().sprite = enemies[finalT].playHurt();
											}

											curTurn += 3;
										}
									}
									// Determine what to do from support data
									else if ((curTurn % (MAX_DUCKS * 2)) < 5){
										//checkpoint
										SpriteAnimation statusEffect = heal;
										if (curSkill.getSupport() == battleType.STATUS)
											statusEffect = cure;
										else if (curSkill.getSupport() == battleType.BOOST) {
											statusEffect = buff;
											if (curSkill.getScalar() < 0)
												statusEffect = debuff;
										}

										if ((curTurn % (MAX_DUCKS * 2)) < 4) {
											for (int i = 0; i < curSkill.getRange(); i++) {
												int translation = i;
												if (i % 2 == 1) {
													translation = target[curDude] + (i % 2) + 1;
												}
												else {
													translation = target[curDude] - (i / 2);
												}

												if (translation >= 0 && translation < MAX_DUCKS) {
													statusHolder[translation].transform.localPosition = mobsPos[translation];
													statusHolder[translation].GetComponent<SpriteRenderer>().enabled = true;
													statusHolder[translation].GetComponent<SpriteRenderer>().enabled = statusEffect.playAnim();
													statusHolder[translation].GetComponent<SpriteRenderer>().sortingOrder = layerTarget[translation] * 2 + 1;                                                    

                                                    if (curSkill.getSupport() == battleType.HEAL) {
														int amt = (int)((float)enemies[translation].getHP() * (float)curSkill.getScalar() / 100);
														enemies[translation].adjustHP(amt);
														createDamage(amt, Color.green, mobsPos[translation]);
                                                        mainSFX.GetComponent<AudioSource>().clip = statusSFX[0];
                                                    }
													else if (curSkill.getSupport() == battleType.CURE) {
														int amt = (int)((float)enemies[translation].getMP() * (float)curSkill.getScalar() / 100);
														enemies[translation].adjustMP(amt);
														createDamage(amt, Color.blue, mobsPos[translation]);
                                                        mainSFX.GetComponent<AudioSource>().clip = statusSFX[1];
                                                    }
													else if (curSkill.getSupport() == battleType.BOOST) {
														enemies[translation].setBoost(curSkill.getStatBoost(), curSkill.getScalar());
                                                        if(curSkill.getScalar() >= 0)
                                                            mainSFX.GetComponent<AudioSource>().clip = statusSFX[0];
                                                        else
                                                            mainSFX.GetComponent<AudioSource>().clip = statusSFX[0];
                                                    }
													else if (curSkill.getSupport() == battleType.STATUS) {
														if (curSkill.getScalar() >= 0) {
															enemies[translation].setStatus(curSkill.getStatus(), false);
														}
														else {
															party[translation].setStatus(curSkill.getStatus(), true);
															statusHolder[translation].transform.localPosition = playersPos[layerTarget[target[curDude] + (i % 2) + 1]];
														}
                                                        mainSFX.GetComponent<AudioSource>().clip = statusSFX[4];
                                                    }

                                                    // Play skill sound effect
                                                    mainSFX.GetComponent<AudioSource>().Play();
                                                }
											}
											curTurn += 1;
										}
										// Play animations and stop after 2 seconds
										else {
                                            if(!WaitForXFrames(30)) {
                                                for(int i = 0; i < curSkill.getRange(); i++) {
                                                    int translation = i;
                                                    if(i % 2 == 1) { translation = target[curDude] + (i % 2) + 1; } else { translation = target[curDude] - (i / 2); }

                                                    if(translation >= 0 && translation < MAX_DUCKS) {
                                                        statusHolder[translation].GetComponent<SpriteRenderer>().sprite = statusEffect.playAnim();
                                                    }
                                                }
                                            } else {
                                                for(int i = 0; i < curSkill.getRange(); i++) {
                                                    int translation = i;
                                                    if(i % 2 == 1) { translation = target[curDude] + (i % 2) + 1; } else { translation = target[curDude] - (i / 2); }

                                                    if(translation >= 0 && translation < MAX_DUCKS) {
                                                        statusHolder[translation].GetComponent<SpriteRenderer>().enabled = false;
                                                    }
                                                }
                                                curTurn += 2;

                                                timer.enabled = true;
                                                timeLeft = MAX_TIME;
                                                party[curDude].getFighter().clearBuffer();
                                                projectiles = new List<GameObject>();
                                                curState = battleStates.COMBAT;
                                                initialFrame = Time.frameCount;
                                            }
										}
									}
									// Calculate damage to opponent and go to combat if they're alive
									else if ((curTurn % (MAX_DUCKS * 2)) < 6) {
										//checkpoint
										float atk, def, atkMul, defMul;
										// Show hurt enemy
										int theTarget = layerTarget[target[curDude]];
										for (int i = 0; i < curSkill.getRange(); i++) {
											int finalT = 0;
											if (i % 2 == 1 && theTarget + (i / 2) + 1 < MAX_DUCKS) {
												finalT = trueTarget[theTarget + (i / 2) + 1];
											}
											else if (i % 2 == 0 && theTarget - (i / 2) >= 0) {
												finalT = trueTarget[theTarget - (i / 2)];
											}

											if (enemies[finalT].getHP() > 0) {
												foes[finalT].GetComponent<SpriteRenderer>().sprite = enemies[finalT].playHurt();

												if (curSkill.getMagical()) {
													atk = party[curDude].getMATK(); 
													def = enemies[finalT].getMDEF();  
													atkMul = boostMul[party[curDude].getMatkBoost() + 4];
													defMul = boostMul[enemies[finalT].getMdefBoost() + 4];
												}
												else {
													atk = party[curDude].getATK(); 
													def = enemies[finalT].getDEF();
													atkMul = boostMul[party[curDude].getAtkBoost() + 4];
													defMul = boostMul[enemies[finalT].getDefBoost() + 4];
												}

												int damage = curSkill.calcDamage(false, curDude, finalT, atk, def, atkMul, defMul);
												createDamage(damage, Color.red, mobsPos[finalT]);
												enemies[finalT].adjustHP(-damage);
											}
										}

										enemies[target[curDude]].setHitstun(15);
                                        curTurn += 1;
                                        if (enemies[target[curDude]].getCurHP() > 0 && !preventCombat[curDude]) {
                                            if(curSkill.getChance() > Random.Range(1,101))
                                                curTurn -= 3;
                                            else {
                                                timer.enabled = true;
                                                timeLeft = MAX_TIME;
                                                party[curDude].getFighter().clearBuffer();
                                                party[curDude].getFighter().clearInput();
                                                party[curDude].getFighter().resetState();
                                                projectiles = new List<GameObject>();
                                                curState = battleStates.COMBAT;
                                                initialFrame = Time.frameCount;
                                            }
										}
									}
									// Death animation
									else if ((curTurn % (MAX_DUCKS * 2)) < 7) {
										if (enemies[target[curDude]].getCurHP() <= 0) {
											if (fadeToNothing(foes[target[curDude]])) {
												foes[target[curDude]].GetComponent<SpriteRenderer>().enabled = false;
												foes[target[curDude]].GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
												allies[curDude].transform.localScale = new Vector2(-1, 1);
												// Add exp to pool
												expPool += enemies[target[curDude]].getEXP();
												// Add enemy drops to pool
												Item[] theDrops = enemies[target[curDude]].getDrops();
												for (int i = 0; i < theDrops.Length; i++) {
													if (theDrops[i] != null) {
														bool isInPool = false;
														// Make sure there are no duplicate items, if so then stack them
														for (int j = 0; j < dropPool.Count; j++) {
															if (dropPool[j].equals(theDrops[i])) {
																isInPool = true;
																dropPool[j].addItem(theDrops[i].getAmt());
																break;
															}
														}
														if (!isInPool)
															dropPool.Add(theDrops[i].copy());
													}
												}
                                                enemies[target[curDude]].killEnemy();

                                                curTurn += 1;
											}
										}
										else {
											if (WaitForXFrames(15)) {
												allies[curDude].transform.localScale = new Vector2(-1, 1);
												curTurn += 1;
											}
											else if(preventCombat[curDude]) {
												foes[target[curDude]].GetComponent<SpriteRenderer>().sprite = enemies[target[curDude]].playHurt();
											}
										}
									}
									// Walk back
									else if ((curTurn % (MAX_DUCKS * 2)) < 8) {
										// Move dudes back to their original spots
										bool targetReached = moveToTarget(allies[curDude], playersPos[curDude], WALKSPD);
										bool targetReached2 = moveToTarget(foes[target[curDude]], mobsPos[target[curDude]], WALKSPD);
										// Return layer order of player once they reach the midpoint of the screen
										if (allies[curDude].transform.localPosition.x < (playersPos[curDude].x + mobsPos[curDude].x) / 2) {
											allies[curDude].GetComponent<SpriteRenderer>().sortingOrder = layerTarget[curDude] * 2;
										}
										// Flip player forward if they have reached their origin
										if (targetReached) {
											allies[curDude].transform.localScale = new Vector2(1, 1);
										}
										else {
											allies[curDude].GetComponent<SpriteRenderer>().sprite = party[curDude].getFighter().playWalk(false);
										}
										// Flip enemy forward if they have reached their origin
										if (targetReached2) {
											foes[target[curDude]].transform.localScale = new Vector2(1, 1);
										}
										else {
											if (enemies[target[curDude]].getCurHP() > 0)
												foes[target[curDude]].GetComponent<SpriteRenderer>().sprite = enemies[target[curDude]].playWalk();
										}
										// End turn if both destinations have been reached
										if (targetReached && targetReached2)
											endTurn();
									}
								}
								else {
									// Play the skill animation
									if ((curTurn % (MAX_DUCKS * 2)) < 1) {
										allies[curDude].GetComponent<SpriteRenderer>().sprite = curSkill.playSkill();
										curTurn += 1;
									}
									if ((curTurn % (MAX_DUCKS * 2)) < 2) {
										if (!curSkill.skillIsDone()) {
											allies[curDude].GetComponent<SpriteRenderer>().sprite = curSkill.playSkill();
										}
										else {
											party[curDude].adjustMP(-1 * curSkill.getMpCost());
											updateSlots(party);
                                            // Play skill sound effect
                                            mainSFX.GetComponent<AudioSource>().clip = curSkill.getSFX();
                                            mainSFX.GetComponent<AudioSource>().Play();
                                            curTurn += 1;
										}
									}
									// Determine what to do from support data
									else {
										switch (curSkill.getSupport()) {
											case battleType.HEAL:
												if (performSupport(curSkill.getSupport(), curSkill.getStatus(),
                                                        curSkill.getStatBoost(), curSkill.getRange(), curSkill.getScalar(), curDude, heal))
													endTurn();
												break;

											case battleType.CURE:
												if (performSupport(curSkill.getSupport(), curSkill.getStatus(), 
                                                        curSkill.getStatBoost(), curSkill.getRange(), curSkill.getScalar(), curDude, heal))
													endTurn();
												break;

											case battleType.BOOST:
												SpriteAnimation anim = buff;
												if (curSkill.getScalar() < 0)
													anim = debuff;
												if (performSupport(curSkill.getSupport(), curSkill.getStatus(), 
                                                        curSkill.getStatBoost(), curSkill.getRange(), curSkill.getScalar(), curDude, anim))
													endTurn();
												;
												break;

											case battleType.STATUS:
												if (performSupport(curSkill.getSupport(), curSkill.getStatus(), 
                                                        curSkill.getStatBoost(), curSkill.getRange(), curSkill.getScalar(), curDude, cure))
													endTurn();
												break;

											default:
												endTurn();
												break;
										}
									}
								}
								break;

							case options.ITEMS:
								// temp play basic attack anim and use item once anim is done
								Item curItem = TitleManager.curFile.getInventoryOfType(itemType.BATTLE)[subSelect[curDude]];

								// Check if selected item is still available
								if (!TitleManager.curFile.isInInventory(itemSel[curDude])) {
									eventText.text = "You no longer have " + itemSel[curDude].getName() + "s in your inventory...";
									if (WaitForXFrames(25)) {
										endTurn();
									}
								}
								else {
									// Start item use animation
									if ((curTurn % (MAX_DUCKS * 2)) < 1) {
										allies[curDude].GetComponent<SpriteRenderer>().sprite = party[curDude].getSkill(0).playSkill();
										curTurn += 1;
									}
									// Play the character's basic attack
									if ((curTurn % (MAX_DUCKS * 2)) < 2) {
										eventText.text = party[curDude].getName() + " uses " + curItem.getName() + ".";
										if (!party[curDude].getSkill(0).skillIsDone()) {
											allies[curDude].GetComponent<SpriteRenderer>().sprite = party[curDude].getSkill(0).playSkill();
										}
										else {
                                            // Play skill sound effect
                                            mainSFX.GetComponent<AudioSource>().clip = itemUse;
                                            mainSFX.GetComponent<AudioSource>().Play();

                                            curTurn += 1;
										}
									}
									// Determine what to do from item data
									else {
										switch (curItem.getBattleType()) {
											case battleType.HEAL:
												if (performSupport(curItem.getBattleType(), curItem.getStatus(), curItem.getStatBoost(),
													     curItem.getRange(), curItem.getScalar(), curDude, heal)) {
													TitleManager.curFile.removeItem(curItem, 1);
													endTurn();
												}
												break;

											case battleType.CURE:
												if (performSupport(curItem.getBattleType(), curItem.getStatus(), curItem.getStatBoost(),
													     curItem.getRange(), curItem.getScalar(), curDude, heal)) {
													TitleManager.curFile.removeItem(curItem, 1);
													endTurn();
												}
												break;

											case battleType.BOOST:
												SpriteAnimation anim = buff;
												if (curItem.getScalar() < 0)
													anim = debuff;
												if (performSupport(curItem.getBattleType(), curItem.getStatus(), curItem.getStatBoost(),
													     curItem.getRange(), curItem.getScalar(), curDude, anim)) {
													TitleManager.curFile.removeItem(curItem, 1);
													endTurn();
												}
												break;

											case battleType.STATUS:
												if (performSupport(curItem.getBattleType(), curItem.getStatus(), curItem.getStatBoost(),
													     curItem.getRange(), curItem.getScalar(), curDude, cure)) {
													TitleManager.curFile.removeItem(curItem, 1);
													endTurn();
												}
												break;

											default:
												endTurn();
												break;
										}
									}
								}
								break;

							case options.FLEE:
								if ((curTurn % (MAX_DUCKS * 2)) < 1) {
									eventText.text = party[curDude].getName() + " makes an attempt to flee...";
									if (WaitForXFrames(15)) {
										int fastestEnemy = 0;
										for (int i = 0; i < enemies.Count; i++) {
											float curSPD = enemies[fastestEnemy].getSPD() * boostMul[enemies[fastestEnemy].getSpdBoost()];
											float newSPD = enemies[i].getSPD() * boostMul[enemies[i].getSpdBoost()];
											if (curSPD < newSPD) {
												fastestEnemy = i;
											}
										}
										float enemySpd = enemies[fastestEnemy].getSPD() * boostMul[enemies[fastestEnemy].getSpdBoost()];
										float playerSpd = party[curDude].getSPD() * boostMul[party[curDude].getSpdBoost()];
										float speedRatio = (float)playerSpd / (float)enemySpd;
										if (speedRatio > Random.Range(0.5f, 1.5f)) {
											fled = true;
										}

										if (fled)
											eventText.text = "Got away!";
										else
											eventText.text = "The attempt was not successful...";
										curTurn += 1;
									}
								}
								else {
									if (fled) {
										// Fade to black
										float curAlpha = blackScreen.GetComponent<SpriteRenderer>().color.a + Time.deltaTime;
										if (curAlpha > 1) {
											curAlpha = 1;
										}
										// Fade text
										for (int i = 0; i < names.Length; i++) {
											names[i].color = new Color(1 - curAlpha, 1 - curAlpha, 1 - curAlpha);
										}
										eventText.color = new Color(1 - curAlpha, 1 - curAlpha, 1 - curAlpha);

										blackScreen.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, curAlpha);
										if (curAlpha == 1) {
											for (int i = 0; i < party.Count; i++) {
												if (party[i].getName() != "") {
													party[i].resetBoosts();
													party[i].resetStatus();
													party[i].resetHP();
													party[i].resetMP();
													party[i].resetEX();
												}
											}
											DungeonHandler.curState = gameState.OVERWORLD;
											DungeonHandler.preState = gameState.BATTLE;
											SceneManager.LoadScene(TitleManager.curFile.getScene(), LoadSceneMode.Single);
										}
									}
									else if (WaitForXFrames(15)) {
										endTurn();
									}
								}
								break;

							case options.STRUGGLE:
								eventText.text = party[curDude].getName() + " struggles...";
								allies[curDude].GetComponent<SpriteRenderer>().sprite = party[curDude].playHurt();
								if (WaitForXFrames(30)) {
									endTurn();
								}
								break;

							default:
								endTurn();
								break;
						}
					}
					// Enemy action
					else if (enemies[curDude % MAX_DUCKS].getCurHP() > 0) {
						int badDude = curDude % MAX_DUCKS;

						// Pick a target from the enemy
						if ((curTurn % (MAX_DUCKS * 2)) < 1) {
							curESkill = enemies[badDude].getSkill();
							if (curESkill.getType() == skillType.SUPPORT && curESkill.isSelfTarget()) {
								enemyTarget = badDude;
								curTurn += 1;
							}
							else if (curESkill.getType() == skillType.SPAWN) {
								enemyTarget = -1;
								for (int i = 0; i < MAX_DUCKS; i++) {
									if (enemies[i].getCurHP() <= 0) {
										enemyTarget = i;
										break;
									}
								}
								if (enemyTarget >= 0) {
									enemies[enemyTarget] = curESkill.spawnDude();
									foes[enemyTarget].transform.localPosition = new Vector2(5, mobsPos[enemyTarget].y);
									foes[enemyTarget].GetComponent<SpriteRenderer>().enabled = true;
                                    // Play skill sound effect
                                    mainSFX.GetComponent<AudioSource>().clip = curESkill.getSFX();
                                    mainSFX.GetComponent<AudioSource>().Play();
                                    curTurn += 7;
								}
								else {
									curTurn += 8;
								}
							} 
							else {
								int[] hpList = new int[MAX_DUCKS];
								for (int i = 0; i < MAX_DUCKS; i++) {
									if (party[i].getName().CompareTo("") == 0)
										hpList[i] = 0;
									else
										hpList[i] = party[i].getCurHP();

									if (curESkill.getType() == skillType.SUPPORT && !(curESkill.getSupport() == battleType.STATUS && curESkill.getScalar() > 0)) {
										hpList[i] = (int)((float)enemies[i].getCurHP()/enemies[i].getHP() * 100);
									}
								}
								int skillIndex = enemies[badDude].getSkillList().IndexOf(curESkill);
								enemyTarget = enemies[badDude].getTarget(skillIndex, hpList);
								lastTarget = party[enemyTarget];

								// Flip enemy if it target's a fellow enemy and target is behind enemy
								if(curESkill.getType() == skillType.SUPPORT && !(curESkill.getSupport() == battleType.STATUS && curESkill.getScalar() > 0)) {
									if(badDude < enemyTarget)
										foes[badDude].transform.localScale = new Vector2(-1, 1);
								}
								
								curTurn += 1;
							}
							eventText.text = enemies[badDude].getName() + " uses " + curESkill.getName() + ".";
						}
						// Walk to target if it's not targeting itself
						else if ((curTurn % (MAX_DUCKS * 2)) < 2) {
							if (badDude == enemyTarget && curESkill.getType() == skillType.SUPPORT && !(curESkill.getSupport() == battleType.STATUS && curESkill.getScalar() > 0)) {
								foes[badDude].GetComponent<SpriteRenderer>().sprite = curESkill.playSkill();
								curTurn += 1;
							}
							else {
								foes[badDude].GetComponent<SpriteRenderer>().sprite = enemies[badDude].playWalk();
								Vector2 targetPos = new Vector2(curESkill.getStartDist() + playersPos[enemyTarget].x, playersPos[enemyTarget].y);
								if (curESkill.getType() == skillType.SUPPORT && !(curESkill.getSupport() == battleType.STATUS && curESkill.getScalar() > 0)) {
									targetPos = new Vector2(curESkill.getStartDist() + mobsPos[enemyTarget].x, mobsPos[enemyTarget].y);
								}
								bool targetReached = moveToTarget(foes[badDude], targetPos, WALKSPD);
								// Change layer order of player once they reach the midpoint of the screen
								if (foes[badDude].transform.localPosition.x < (mobsPos[badDude].x + targetPos.x) / 2) {
									foes[badDude].GetComponent<SpriteRenderer>().sortingOrder = (layerTarget[enemyTarget] * 2) + 1;
								}
								// Go to skill display
								if (targetReached) {
									foes[badDude].GetComponent<SpriteRenderer>().sprite = curESkill.playSkill();
									foes[badDude].transform.localScale = new Vector2(1, 1);
									curTurn += 1;
								}
							}
						}
						// Play skill
						else if ((curTurn % (MAX_DUCKS * 2)) < 3) {
							if (!curESkill.skillIsDone()) {
								foes[badDude].GetComponent<SpriteRenderer>().sprite = curESkill.playSkill();
							}
							else {
                                // Play skill sound effect
                                mainSFX.GetComponent<AudioSource>().clip = curESkill.getSFX();
                                mainSFX.GetComponent<AudioSource>().Play();

                                // Move foe to where they should end up after the move if it's not self target
                                if (!(curESkill.getType() == skillType.SUPPORT && !(curESkill.getSupport() == battleType.STATUS && curESkill.getScalar() > 0))) {
									foes[badDude].transform.localPosition = new Vector2(curESkill.getEndDist() + playersPos[enemyTarget].x, playersPos[enemyTarget].y);
								}
								if (badDude != enemyTarget && curESkill.getType() == skillType.SUPPORT && !(curESkill.getSupport() == battleType.STATUS && curESkill.getScalar() > 0)) {
									foes[badDude].transform.localPosition = new Vector2(curESkill.getEndDist() + mobsPos[enemyTarget].x, mobsPos[enemyTarget].y);
								}
								curTurn += 1;
							}
						}
						// Calculate damage to ally
						else if ((curTurn % (MAX_DUCKS * 2)) < 5) {
							if (curESkill.getType() == skillType.OFFENSIVE) {
								float atk, def, atkMul, defMul;

								// Show hurt enemy
								int theTarget = layerTarget[enemyTarget];
								for (int i = 0; i < curESkill.getRange(); i++) {
									int finalT = 0;
									if (i % 2 == 1 && theTarget + (i / 2) + 1 < MAX_DUCKS) {
										finalT = trueTarget[theTarget + (i / 2) + 1];
									}
									else if (i % 2 == 0 && theTarget - (i / 2) >= 0) {
										finalT = trueTarget[theTarget - (i / 2)];
									}

									if (party[finalT].getCurHP() > 0 && party[finalT].getName() != "") {
										if (curESkill.getMagical()) {
											atk = enemies[badDude].getMATK();
											def = party[finalT].getMDEF();
											atkMul = boostMul[enemies[badDude].getMatkBoost() + 4];
											defMul = boostMul[party[finalT].getMdefBoost() + 4];
										}
										else {
											atk = enemies[badDude].getATK();
											def = party[finalT].getDEF();
											atkMul = boostMul[enemies[badDude].getAtkBoost() + 4];
											defMul = boostMul[party[finalT].getDefBoost() + 4];
										}

										int damage = curESkill.calcDamage(actions[enemyTarget] == options.DEFEND,
											            badDude, enemyTarget, atk, def, atkMul, defMul);
										createDamage(damage, Color.red, playersPos[finalT]);
										party[finalT].adjustHP(-damage);
										// Show hurt ally
										allies[finalT].GetComponent<SpriteRenderer>().sprite = party[finalT].playHurt();
									}
								}
									
								updateSlots(party);
								curTurn += 2;
							}
							else if (curESkill.getSupport() != battleType.NULL) {
								SpriteAnimation statusEffect = heal;
								if (curESkill.getSupport() == battleType.STATUS)
									statusEffect = cure;
								else if (curESkill.getSupport() == battleType.BOOST) {
									statusEffect = buff;
									if (curESkill.getScalar() < 0)
										statusEffect = debuff;
								}

								if ((curTurn % (MAX_DUCKS * 2)) < 4) {
									for (int i = 0; i < curESkill.getRange(); i++) {
										int translation = i;
										if (i % 2 == 1) {
											translation = enemyTarget + (i % 2) + 1;
										}
										else {
											translation = enemyTarget - (i / 2);
										}

										if (translation >= 0 && translation < MAX_DUCKS) {
											statusHolder[translation].transform.localPosition = mobsPos[translation];
											statusHolder[translation].GetComponent<SpriteRenderer>().enabled = true;
											statusHolder[translation].GetComponent<SpriteRenderer>().enabled = statusEffect.playAnim();
											statusHolder[translation].GetComponent<SpriteRenderer>().sortingOrder = layerTarget[translation] * 2 + 1;

                                            if (curESkill.getSupport() == battleType.HEAL) {
												int amt = (int)((float)enemies[translation].getHP() * (float)curESkill.getScalar() / 100);
												enemies[translation].adjustHP(amt);
												createDamage(amt, Color.green, mobsPos[translation]);
                                                mainSFX.GetComponent<AudioSource>().clip = statusSFX[0];
                                            }
											else if (curESkill.getSupport() == battleType.CURE) {
												int amt = (int)((float)enemies[translation].getMP() * (float)curESkill.getScalar() / 100);
												enemies[translation].adjustMP(amt);
												createDamage(amt, Color.blue, mobsPos[translation]);
                                                mainSFX.GetComponent<AudioSource>().clip = statusSFX[1];
                                            }
											else if (curESkill.getSupport() == battleType.BOOST) {
												enemies[translation].setBoost(curESkill.getStatBoost(), curESkill.getScalar());
                                                if(curESkill.getScalar() >= 0)
                                                    mainSFX.GetComponent<AudioSource>().clip = statusSFX[2];
                                                else
                                                    mainSFX.GetComponent<AudioSource>().clip = statusSFX[3];
                                            }
											else if (curESkill.getSupport() == battleType.STATUS) {
												if (curESkill.getScalar() >= 0) {
													enemies[translation].setStatus(curESkill.getStatus(), false);
												}
												else {
													party[translation].setStatus(curESkill.getStatus(), true);
													statusHolder[translation].transform.localPosition = playersPos[layerTarget[enemyTarget + (i % 2) + 1]];
												}
                                                mainSFX.GetComponent<AudioSource>().clip = statusSFX[4];
                                            }

                                            // Play skill sound effect
                                            mainSFX.GetComponent<AudioSource>().Play();
                                        }
									}
									curTurn += 1;
								}
								// Play animations and stop after 2 seconds
								else {
									if (!WaitForXFrames(30)) {
										for (int i = 0; i < curESkill.getRange(); i++) {
											int translation = i;
											if (i % 2 == 1) 	{ translation = enemyTarget + (i % 2) + 1; }
											else 				{ translation = enemyTarget - (i / 2); }

											if (translation >= 0 && translation < MAX_DUCKS) {
												statusHolder[translation].GetComponent<SpriteRenderer>().sprite = statusEffect.playAnim();
											}
										}
									}
									else {
										for (int i = 0; i < curESkill.getRange(); i++) {
											int translation = i;
											if (i % 2 == 1) 	{ translation = enemyTarget + (i % 2) + 1; }
											else 				{ translation = enemyTarget - (i / 2); }

											if (translation >= 0 && translation < MAX_DUCKS) {
												statusHolder[translation].GetComponent<SpriteRenderer>().enabled = false;
											}
										}
										curTurn += 1;
									}
								}
							}
							else {
								curTurn += 2;
							}
						}
						// Death animation
						else if ((curTurn % (MAX_DUCKS * 2)) < 6) {
							bool waitOver = WaitForXFrames(30);
							for (int i = 0; i < curESkill.getRange() && curESkill.getType() == skillType.OFFENSIVE; i++) {
								int translation = i;
								if (i % 2 == 1) 	{ translation = enemyTarget + (i % 2) + 1; }
								else 				{ translation = enemyTarget - (i / 2); }

								if (translation >= 0 && translation < MAX_DUCKS) {
									if (party[translation].getName() != "" && !party[translation].isKO()) {
										allies[translation].GetComponent<SpriteRenderer>().sprite = party[translation].playHurt();
									}
									if (waitOver && party[translation].getCurHP() <= 0 && party[translation].getName() != "") {
										allies[translation].GetComponent<SpriteRenderer>().sprite = party[translation].playKO();
										party[translation].resetBoosts();
										party[translation].resetStatus();
										party[translation].setKO(true);
										updateSlots(party);
									}
								}
							}

							if(waitOver) {
								curTurn += 1;
							}

							if (badDude <= enemyTarget && curESkill.getType() == skillType.SUPPORT && !(curESkill.getSupport() == battleType.STATUS && curESkill.getScalar() > 0)) {
								foes[badDude].transform.localScale = new Vector2(1, 1);
							}
							else {
								foes[badDude].transform.localScale = new Vector2(-1, 1);
							}
						}
						// Walk back
						else if ((curTurn % (MAX_DUCKS * 2)) < 7) {
							if (badDude == enemyTarget && curESkill.getType() == skillType.SUPPORT && !(curESkill.getSupport() == battleType.STATUS && curESkill.getScalar() > 0)) {
								endTurn();
							}
							else {
								foes[badDude].GetComponent<SpriteRenderer>().sprite = enemies[badDude].playWalk();
								Vector2 targetPos = new Vector2(mobsPos[badDude].x, mobsPos[badDude].y);
								bool targetReached = moveToTarget(foes[badDude], targetPos, WALKSPD);
								// Change layer order of player once they reach the midpoint of the screen
								if (foes[badDude].transform.localPosition.x > (mobsPos[badDude].x + playersPos[badDude].x) / 4) {
									foes[badDude].GetComponent<SpriteRenderer>().sortingOrder = (layerTarget[badDude] * 2);
								}
								else if (party[enemyTarget].getCurHP() > 0 && curESkill.getType() == skillType.OFFENSIVE) {
									// Show hurt allies
									int theTarget = layerTarget[enemyTarget];
									for (int i = 0; i < curESkill.getRange(); i++) {
										int finalT = 0;
										if (i % 2 == 1 && theTarget + (i / 2) + 1 < MAX_DUCKS) {
											finalT = trueTarget[theTarget + (i / 2) + 1];
										}
										else if (i % 2 == 0 && theTarget - (i / 2) >= 0) {
											finalT = trueTarget[theTarget - (i / 2)];
										}

										if (party[finalT].getCurHP() > 0 && party[finalT].getName() != "") {
											allies[finalT].GetComponent<SpriteRenderer>().sprite = party[finalT].playHurt();
										}
									}
								}
								// End turn
								if (targetReached) {
									foes[badDude].transform.localScale = new Vector2(1, 1);
									endTurn();
								}
							}
						}
						// Summon spawn monster
						else if ((curTurn % (MAX_DUCKS * 2)) < 8) {
							foes[badDude].GetComponent<SpriteRenderer>().sprite = curESkill.playSkill();
							foes[enemyTarget].GetComponent<SpriteRenderer>().sprite = enemies[enemyTarget].playWalk();

							Vector2 targetPos = mobsPos[enemyTarget];
							bool targetReached = moveToTarget(foes[enemyTarget], targetPos, WALKSPD);
							// End turn
							if (targetReached) {
								endTurn();
							}
						}
						else {
							eventText.text = enemies[badDude].getName() + " uses " + curESkill.getName() + "...";
							if (WaitForXFrames(30))
								endTurn();
						}
					}
					else {
						endTurn();
					}
				}

				// Reset everything and go back to talk
				else {
					curTurn = 0;
					turnAmt += 1;
				
					// Update to most recent turn
					for (int i = atLastTurn; batEvents != null && i < batEvents.Length; i++) {
						if (batEvents[i].onTurn < turnAmt) {
							atLastTurn += 1;
						}
						else
							break;
					}
					curState = battleStates.TALK;
					textBox.GetComponent<SpriteRenderer>().sprite = textboxDisp[1];
				}

				// Check if the battle is over after each turn
				if (curTurn % 10 == 0) {
					bool playersLeft = false, enemiesLeft = false;
					for (int i = 0; i < MAX_DUCKS; i++) {
						if (party[i].getCurHP() > 0 && party[i].getName() != "") {
							playersLeft = true;
						}
						if (enemies[i].getCurHP() > 0) {
							enemiesLeft = true;
						}
					}
					if (!playersLeft) {
						curState = battleStates.LOSE;
                        mainBGM.GetComponent<AudioSource>().clip = lose;
                        mainBGM.GetComponent<AudioSource>().loop = false;
                        mainBGM.GetComponent<AudioSource>().Play();

                        // Pick which duck's last words should be said
                        GameOverText theLastWords = lastWords[0];
						List<GameOverText> duckWords = new List<GameOverText>();
						for (int i = 0; i < lastWords.Length; i++) {
							if (lastWords[i].duck == lastTarget.getCharacter()) {
								duckWords.Add(lastWords[i]);
							}
						}
						if (duckWords.Count > 0) {
							theLastWords = duckWords[Random.Range(0, duckWords.Count)];
						}
						curSpeech = new Dialouge(theLastWords.dialouge, theLastWords.image, theLastWords.pitch);
						duckSprite.GetComponent<SpriteRenderer>().enabled = true;
						duckSprite.GetComponent<SpriteRenderer>().sprite = curSpeech.getSprite();
                        mainSFX.GetComponent<AudioSource>().pitch = curSpeech.getPitch();
                        curTurn = 0;
					}
					else if (!enemiesLeft) {
						curState = battleStates.WIN;
                        mainBGM.GetComponent<AudioSource>().clip = win;
                        mainBGM.GetComponent<AudioSource>().Play();
                        curTurn = 0;
					}
				}
				break;


			case battleStates.COMBAT:
				int dude = combatantOrder[curTurn / (MAX_DUCKS * 2)];
				// Place everyone into idle animation at first
				for (int i = 0; i < MAX_DUCKS; i++) {
					if (party[i].getName().CompareTo("") != 0 && i != dude) {
						if (party[i].getCurHP() > 0)
							allies[i].GetComponent<SpriteRenderer>().sprite = party[i].playIdle();
						else
							allies[i].GetComponent<SpriteRenderer>().sprite = party[i].playKO();
					}
					if (enemies[i].getCurHP() > 0 && i != target[dude]) {
						foes[i].GetComponent<SpriteRenderer>().sprite = enemies[i].playIdle();
					}
				}
						
				print("Dude: " + dude);
				Fighter curFighter = party[dude].getFighter();
				if (timeLeft > 0) {
					// Confuse animation for enemy
					if (enemies[target[dude]].inHitstun()) {
						foes[target[dude]].GetComponent<SpriteRenderer>().sprite = enemies[target[dude]].playHurt();
					}
					else {
						foes[target[dude]].GetComponent<SpriteRenderer>().sprite = enemies[target[dude]].playConfused();
					}

					// Check if there's preset controls for combat
					bool presetControls = false;
					if (batEvents != null) {
						for (int i = atLastTurn; i < batEvents.Length; i++) {
							if (batEvents[i].onTurn != turnAmt) {
								break;
							}
							if (batEvents[i].act == battleActions.COMBACT) {
								atLastTurn = i;
								presetControls = true;
							}
						}
					}

					// Update input for fighter
					if (presetControls) {
						BattleEvent.BEvent.CombatAct combact = batEvents[atLastTurn].combact;
						int curframe = Time.frameCount - initialFrame;

						int inputIndex = combact.inputs.Length;
						for (int i = 0; i < combact.inputs.Length; i++) {
							if (combact.inputs[i].onFrame == curframe) {
								inputIndex = i;
							}
							else if (combact.inputs[i].onFrame > curframe) {
								break;
							}
						}

						if (inputIndex < combact.inputs.Length) {
							for (int i = 0; i < DataManager.savedOptions.controls.Length; i++) {
								if (combact.inputs[inputIndex].onPress[i])
									curFighter.buttonDown((key)i);
								else if (combact.inputs[inputIndex].pressing[i])
									curFighter.buttonHold((key)i);
								else if (combact.inputs[inputIndex].released[i])
									curFighter.buttonUp((key)i);
								else
									curFighter.changeButton((key)i, false);
							}
						}
					}
					else {
						for (int i = 0; i < DataManager.savedOptions.controls.Length; i++) {
							if (Input.GetKeyDown(DataManager.savedOptions.controls[i]))
								curFighter.buttonDown((key)i);
							else if (Input.GetKey(DataManager.savedOptions.controls[i]))
								curFighter.buttonHold((key)i);
							else if (Input.GetKeyUp(DataManager.savedOptions.controls[i]))
								curFighter.buttonUp((key)i);
							else
								curFighter.changeButton((key)i, false);
						}
					}

					// Control fighter
					curFighter.controlFighter(allies[dude], mobsPos[target[dude]].y, -2.211002f, 
						mobsPos[mobsPos.Length - 1].x, foes[target[dude]].transform.localPosition.x, mainSFX, fighterSFX);
					
					// Flip enemy based on position of player
					if (allies[dude].transform.localPosition.x > foes[target[dude]].transform.localPosition.x) {
						foes[target[dude]].transform.localScale = new Vector2(-1, 1);	
					}
					else {
						foes[target[dude]].transform.localScale = new Vector2(1, 1);
					}

                    // Check if the target has been hit by fighter
                    if(curFighter.checkHit(allies[dude],foes[target[dude]],enemies[target[dude]].getHurtBoxes())) {
                        // Calculate and apply damage
                        float atk = 0, def = 0, atkMul = 0, defMul = 0;
                        if(party[dude].getATK() >= party[dude].getMATK()) {
                            atk = party[dude].getATK();
                            def = enemies[target[dude]].getDEF();
                            atkMul = boostMul[party[dude].getAtkBoost() + 4];
                            defMul = boostMul[enemies[target[dude]].getDefBoost() + 4];
                        } 
                        else {
                            atk = party[dude].getMATK();
                            def = enemies[target[dude]].getMDEF();
                            atkMul = boostMul[party[dude].getMatkBoost() + 4];
                            defMul = boostMul[enemies[target[dude]].getMdefBoost() + 4];
                        }
                        int damage = (int)(curFighter.getBasePow() * (atk / def) * atkMul / defMul);
						createDamage(damage, Color.red, foes[target[dude]].transform.position);
						enemies[target[dude]].adjustHP(-damage);
						// Hurt animation and set hitstun
						foes[target[dude]].GetComponent<SpriteRenderer>().sprite = enemies[target[dude]].playHurt();
						enemies[target[dude]].setHitstun(curFighter.getHitStun());
						enemies[target[dude]].setXVel(curFighter.getXPow());
						if (foes[target[dude]].transform.position.y > mobsPos[target[dude]].y) {
							enemies[target[dude]].setYVel(curFighter.getYPow());
						}
						// Slide player back
						curFighter.slideDudeBack();
					}
							
					// Check if current skill by fighter creates a projectile
					GameObject projPrefab = curFighter.getProjectile();
					if (projPrefab != null) {
						GameObject curProj = Instantiate(projPrefab);
						curProj.transform.localPosition = allies[dude].transform.localPosition;
						curProj.transform.Translate(allies[dude].transform.localScale.x * projPrefab.GetComponent<ProjObj>().xShift,
							projPrefab.GetComponent<ProjObj>().yShift, 0);
						curProj.transform.localScale = allies[dude].transform.localScale;
						curProj.GetComponent<SpriteRenderer>().sortingOrder = allies[dude].GetComponent<SpriteRenderer>().sortingOrder;
						projectiles.Add(curProj);
					}

					// Check if the target has been hit by a projectile
					for (int i = 0; i < projectiles.Count; i++) {
						GameObject theProj = projectiles[i];
						if (theProj.GetComponent<ProjObj>().checkHit(foes[target[dude]], enemies[target[dude]].getHurtBoxes())) {
                            // Calculate and apply damage
                            float atk = 0, def = 0, atkMul = 0, defMul = 0;
                            if(party[dude].getATK() >= party[dude].getMATK()) {
                                atk = party[dude].getATK();
                                def = enemies[target[dude]].getDEF();
                                atkMul = boostMul[party[dude].getAtkBoost() + 4];
                                defMul = boostMul[enemies[target[dude]].getDefBoost() + 4];
                            } else {
                                atk = party[dude].getMATK();
                                def = enemies[target[dude]].getMDEF();
                                atkMul = boostMul[party[dude].getMatkBoost() + 4];
                                defMul = boostMul[enemies[target[dude]].getMdefBoost() + 4];
                            }
                            int damage = (int)(curFighter.getBasePow() * (atk / def) * atkMul / defMul);
                            createDamage(damage, Color.red, foes[target[dude]].transform.position);
							enemies[target[dude]].adjustHP(-damage);
							// Hurt animation and set hitstun
							foes[target[dude]].GetComponent<SpriteRenderer>().sprite = enemies[target[dude]].playHurt();
							enemies[target[dude]].setHitstun(theProj.GetComponent<ProjObj>().hitstun);
							enemies[target[dude]].setXVel(theProj.GetComponent<ProjObj>().xPow);
							if (foes[target[dude]].transform.position.y > mobsPos[target[dude]].y) {
								enemies[target[dude]].setYVel(theProj.GetComponent<ProjObj>().yPow);
							}

							// Delete projectile
							projectiles.Remove(theProj);
							Destroy(theProj);
						}
						// Destroy object if it is out of the borders
						else if (theProj.transform.position.y < 0.1f || theProj.transform.position.x < -2.211002f
						          || theProj.transform.position.x > mobsPos[mobsPos.Length - 1].x) {
							projectiles.Remove(theProj);
							Destroy(theProj);
						}
					}

					// Prevent foe from leaving screen
					if (foes[target[dude]].transform.position.x < -2.211002f) {
						foes[target[dude]].transform.position = new Vector2(-2.211002f, foes[target[dude]].transform.position.y);
					}
					else if (foes[target[dude]].transform.position.x > mobsPos[mobsPos.Length - 1].x) {
						foes[target[dude]].transform.position = new Vector2(mobsPos[mobsPos.Length - 1].x, foes[target[dude]].transform.position.y);
					}

					// Apply gravity to enemy and update timer display
					enemies[target[dude]].applyGravity(foes[target[dude]], mobsPos[target[dude]].y);
					timeLeft -= Time.deltaTime;
					if (timeLeft < 0)
						timeLeft = 0;
					timer.text = timeLeft.ToString("F2");
				}
				else {
					enemies[target[dude]].applyGravity(foes[target[dude]], mobsPos[target[dude]].y);
					curFighter.applyGravity(allies[dude]);
					if (allies[dude].transform.localPosition.y <= mobsPos[target[dude]].y) {
						allies[dude].transform.localPosition = new Vector2(allies[dude].transform.localPosition.x, mobsPos[target[dude]].y);
					}
					if (allies[dude].transform.localPosition.y <= mobsPos[target[dude]].y && foes[target[dude]].transform.localPosition.y <= mobsPos[target[dude]].y) {
						timeLeft = MAX_TIME;
						timer.enabled = false;
						// Flip if away from spot
						if (allies[dude].transform.localPosition.x > playersPos[dude].x) {
							allies[dude].transform.localScale = new Vector2(-1, 1);
						}
						if (foes[target[dude]].transform.localPosition.x < mobsPos[target[dude]].x) {
							foes[target[dude]].transform.localScale = new Vector2(-1, 1);
						}
						// Clear buffer and input
						curFighter.clearBuffer();
						curFighter.clearInput();
						curFighter.resetState();
						// Destroy all projectiles
						for (int i = 0; i < projectiles.Count; i++) {
							GameObject theProj = projectiles[i];
							projectiles.Remove(theProj);
							Destroy(theProj);
						}

						initialFrame = 0;
						curState = battleStates.BATTLE;
					}
				}
				break;



			case battleStates.WIN:
				
				// Play idle animation for living ducks
				for (int i = 0; i < MAX_DUCKS; i++) {
					if (party[i].getName() != "" && party[i].getCurHP() > 0) {
						allies[i].GetComponent<SpriteRenderer>().sprite = party[i].playIdle();
					}
				}

				// Make each duck that's still alive gain exp
				if ((curTurn % (MAX_DUCKS * 2)) < 1) {
					if (curChar >= party.Count)
						curTurn += 7;
					else if (party[curChar].getName() != "" && party[curChar].getCurHP() > 0) {
						eventText.text = party[curChar].getName() + " gains " + expPool.ToString() + " exp.";
						curExpGain = expPool;
						curTurn += 1;
					}
					else {
						curChar += 1;
					}
				}
				else if ((curTurn % (MAX_DUCKS * 2)) < 2) {
                    float expRatio = (float)(party[curChar].getCurExp() + curExpGain) / (float)party[curChar].getMaxExp();

                    if(exp[curChar].transform.localScale.x >= 1 || exp[curChar].transform.localScale.x >= expRatio) {
                        if(curExpGain >= 0) 
                            curExpGain = party[curChar].adjustExp(curExpGain);
                        if(mainSFX.GetComponent<AudioSource>().clip == levelGain)
                            mainSFX.GetComponent<AudioSource>().clip = null;

                        if(curExpGain < 0 && Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
                            curChar += 1;
                            curTurn -= 1;

                            mainSFX.GetComponent<AudioSource>().clip = select;
                            mainSFX.GetComponent<AudioSource>().Play();
                        } 
                        else if(curExpGain >= 0) {
                            int newLevel = party[curChar].getLevel() + 1;
                            eventText.text = party[curChar].getName() + " grows up to level " + newLevel.ToString() + "!";
                            curTurn += 1;

                            mainSFX.GetComponent<AudioSource>().clip = levelUp;
                            mainSFX.GetComponent<AudioSource>().Play();
                        }
                        updateSlots(party);
                    } 
                    else {
                        if(mainSFX.GetComponent<AudioSource>().clip != levelGain) {
                            mainSFX.GetComponent<AudioSource>().clip = levelGain;
                            mainSFX.GetComponent<AudioSource>().Play();
                        }

                        float gain = exp[curChar].transform.localScale.x + Time.deltaTime / 2;
                        if(gain > expRatio)
                            gain = expRatio;
                        if(gain > 1)
                            gain = 1;
                        exp[curChar].transform.localScale = new Vector2(gain,exp[curChar].transform.localScale.y);
                    }
				}
				else if ((curTurn % (MAX_DUCKS * 2)) < 3) {
					if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
						eventText.text = party[curChar].getName() + " gains ";
						string[] statString = { " HP, ", " MP, ", " ATK, ", " DEF, ", " MATK, ", " MDEF, ", " SPD." };
						int[] gains = new int[7];
						for (int i = 0; i < gains.Length; i++) {
							gains[i] = Random.Range(2, 6);
							eventText.text += gains[i].ToString() + statString[i];
						}
						party[curChar].levelUp(gains);
						party[curChar].adjustHP(gains[0]);
						party[curChar].adjustMP(gains[1]);

						party[curChar].getFighter().updateLevel(party[curChar].getLevel());
						updateSlots(party);

						// Check if a new skill has been learned
						if ((party[curChar].getLevel() % 10 == 0 && party[curChar].getSkill((party[curChar].getLevel() / 10) + 2) != null)
                                || (party[curChar].getLevel() - 5) % 10 == 0 && party[curChar].getFighter().getLearnedSkill() != null)
							curTurn += 1;
						else 
							curTurn -= 1;

                        mainSFX.GetComponent<AudioSource>().clip = select;
                        mainSFX.GetComponent<AudioSource>().Play();
                    }
				}
				// Display newly learned skill
				else if ((curTurn % (MAX_DUCKS * 2)) < 4) {
					if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
						if (party[curChar].getLevel() % 10 == 0) {
							eventText.text = party[curChar].getName() + " learned " + party[curChar].getSkill((party[curChar].getLevel() / 10) + 2).getName() + "!";
							curTurn -= 2;
						}
						else {
							eventText.text = party[curChar].getName() + " learned a new command skill!";
							curTurn += 1;
                            projectiles = new List<GameObject>();
                        }

                        mainSFX.GetComponent<AudioSource>().clip = select;
                        mainSFX.GetComponent<AudioSource>().Play();
                    }
				}
				// Display input for new skill
				else if ((curTurn % (MAX_DUCKS * 2)) < 5) {
					if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
						eventText.text = "Try it out by inputting: ";
						KeyCode[] theInput = party[curChar].getFighter().getLearnedSkill().getInput();
						for (int i = 0; i < theInput.Length; i++) {
							if (theInput[i] == DataManager.savedOptions.controls[(int)key.LEFT])
								eventText.text += "BACK";
							else if (theInput[i] == DataManager.savedOptions.controls[(int)key.RIGHT])
								eventText.text += "FOWARD";
							else {
								for (int j = 0; j < DataManager.savedOptions.controls.Length; j++) {
									if (theInput[i] == DataManager.savedOptions.controls[j])
										eventText.text += (key)j;
								}
							}

							if (i != theInput.Length - 1)
								eventText.text += " ";
						}
						curTurn += 1;

                        mainSFX.GetComponent<AudioSource>().clip = select;
                        mainSFX.GetComponent<AudioSource>().Play();
                    }
				}
				// Wait for user to input new skill 
				else if ((curTurn % (MAX_DUCKS * 2)) < 6) {
					Fighter theFighter = party[curChar].getFighter();
					if (!theFighter.getLearnedSkill().equals(theFighter.getCurSkill())) {
						for (int i = 0; i < DataManager.savedOptions.controls.Length; i++) {
							if (Input.GetKeyDown(DataManager.savedOptions.controls[i]))
								theFighter.buttonDown((key)i);
							else if (Input.GetKey(DataManager.savedOptions.controls[i]))
								theFighter.buttonHold((key)i);
							else if (Input.GetKeyUp(DataManager.savedOptions.controls[i]))
								theFighter.buttonUp((key)i);
							else
								theFighter.changeButton((key)i, false);
						}

						// Control fighter
						theFighter.controlFighter(allies[curChar], mobsPos[curChar].y, playersPos[playersPos.Length - 1].x, 
							mobsPos[mobsPos.Length - 1].x, mobsPos[mobsPos.Length - 1].x, mainSFX, fighterSFX);
					}
					else {
						eventText.text = "Good. You got it!";
						curTurn += 1;
					}
				}
				// Reset to neutral position
				else if ((curTurn % (MAX_DUCKS * 2)) < 7) {
					if (party[curChar].getFighter().getCurSkill() == null) {
						if (allies[curChar].transform.localPosition.y > mobsPos[curChar].y)
							party[curChar].getFighter().applyGravity(allies[curChar]);
						else {
							// Move dudes back to their original spots
							bool targetReached = moveToTarget(allies[curChar], playersPos[curChar], WALKSPD);
							// Flip player forward if they have reached their origin
							if (targetReached) {
								allies[curChar].transform.localScale = new Vector2(1, 1);
							}
							else {
								allies[curChar].GetComponent<SpriteRenderer>().sprite = party[curChar].getFighter().playWalk(false);
							}
							// End turn if both destinations have been reached
							if (targetReached) {
								// Clear buffer and input
								party[curChar].getFighter().clearBuffer();
								party[curChar].getFighter().clearInput();
								party[curChar].getFighter().resetState();
								// Destroy all projectiles
								for (int i = 0; i < projectiles.Count; i++) {
									GameObject theProj = projectiles[i];
									projectiles.Remove(theProj);
									Destroy(theProj);
								}

								curTurn -= 5;
							}
						}

						if (allies[curChar].transform.localPosition.y <= mobsPos[curChar].y) {
							allies[curChar].transform.localPosition = new Vector2(allies[curChar].transform.localPosition.x, mobsPos[curChar].y);
							// Flip if away from spot
							if (allies[curChar].transform.localPosition.x > playersPos[curChar].x) {
								allies[curChar].transform.localScale = new Vector2(-1, 1);
							}
						}
					}
					// Play out move
					else {
						for (int i = 0; i < DataManager.savedOptions.controls.Length; i++) {
							party[curChar].getFighter().changeButton((key)i, false);
						}
						// Control fighter
						party[curChar].getFighter().controlFighter(allies[curChar], mobsPos[curChar].y, playersPos[playersPos.Length - 1].x, 
							mobsPos[mobsPos.Length - 1].x, mobsPos[mobsPos.Length - 1].x, mainSFX, fighterSFX);

						// Check if current skill by fighter creates a projectile
						GameObject projPrefab = party[curChar].getFighter().getProjectile();
						if (projPrefab != null) {
							GameObject curProj = Instantiate(projPrefab);
							curProj.transform.localPosition = allies[curChar].transform.localPosition;
							curProj.transform.Translate(allies[curChar].transform.localScale.x * projPrefab.GetComponent<ProjObj>().xShift,
								projPrefab.GetComponent<ProjObj>().yShift, 0);
							curProj.transform.localScale = allies[curChar].transform.localScale;
							curProj.GetComponent<SpriteRenderer>().sortingOrder = allies[curChar].GetComponent<SpriteRenderer>().sortingOrder;
							projectiles.Add(curProj);
						}

						// Check if the projectile is out of bounds
						for (int i = 0; i < projectiles.Count; i++) {
							GameObject theProj = projectiles[i];		
							// Destroy object if it is out of the borders
							if (theProj.transform.position.y < 0.1f || theProj.transform.position.x < playersPos[playersPos.Length - 1].x
								|| theProj.transform.position.x > mobsPos[mobsPos.Length - 1].x) {
								projectiles.Remove(theProj);
								Destroy(theProj);
							}
						}
					}
				}

				// Get items from the drops
				else if ((curTurn % (MAX_DUCKS * 2)) < 8) {
					if (dropPool.Count <= 0) {
						curTurn += 2;
					}
					else {
						if (dropPool[0].getName() == "Coin") {
							eventText.text = "Obtained $" + dropPool[0].getAmt().ToString() + "!";
							TitleManager.curFile.adjustBalance(dropPool[0].getAmt());
						}
						else {
							eventText.text = "Obatined " + dropPool[0].getName() + " x" + dropPool[0].getAmt().ToString() + ".";
							TitleManager.curFile.addToInventory(dropPool[0].copy());
						}
						curTurn += 1;
					}
				}
				// Delete drop
				else if ((curTurn % (MAX_DUCKS * 2)) < 9) {
					if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
						dropPool.Remove(dropPool[0]);
						curTurn -= 1;

                        mainSFX.GetComponent<AudioSource>().clip = select;
                        mainSFX.GetComponent<AudioSource>().Play();
                    }
				}
				else {
					// Fade to black
					float theAlpha = blackScreen.GetComponent<SpriteRenderer>().color.a + Time.deltaTime;
					if (theAlpha > 1) {
						theAlpha = 1;
					}
					// Fade text
					for (int i = 0; i < names.Length; i++) {
						names[i].color = new Color(1 - theAlpha, 1 - theAlpha, 1 - theAlpha);
					}
					eventText.color = new Color(1 - theAlpha, 1 - theAlpha, 1 - theAlpha);

					blackScreen.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, theAlpha);
					if (theAlpha == 1) {
						for (int i = 0; i < party.Count; i++) {
							if (party[i].getName() != "") {
								party[i].resetBoosts();
								party[i].resetStatus();
								party[i].resetHP();
								party[i].resetMP();
								party[i].resetEX();
							}
						}
						DungeonHandler.curState = gameState.OVERWORLD;
						DungeonHandler.preState = gameState.BATTLE;
						SceneManager.LoadScene(TitleManager.curFile.getScene(), LoadSceneMode.Single);
					}
				}

				break;



			case battleStates.LOSE:
				// Play idle animation for enemies
				for (int i = 0; i < MAX_DUCKS; i++) {
					if (enemies[i].getCurHP() > 0) {
						foes[i].GetComponent<SpriteRenderer>().sprite = enemies[i].playIdle();
					}
				}

				// Speak last words
				if (curSpeech.isDone()) {
					// Fade to black
					float curAlpha = blackScreen.GetComponent<SpriteRenderer>().color.a + Time.deltaTime;
					if (curAlpha > 1) {
						curAlpha = 1;
					}
					// Fade text
					for (int i = 0; i < names.Length; i++) {
						names[i].color = new Color(1 - curAlpha, 1 - curAlpha, 1 - curAlpha);
					}
					eventText.color = new Color(1 - curAlpha, 1 - curAlpha, 1 - curAlpha);

					blackScreen.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, curAlpha);
					if (curAlpha == 1) {
						// Go to title screen
						SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);
					}
				}
				else {
					if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {

						if (curSpeech.getReadingLine(curSpeech.getLine(), 25) == curSpeech.getLine()) {
							curSpeech.nextLine();
							if (!curSpeech.isDone())
								duckSprite.GetComponent<SpriteRenderer>().sprite = curSpeech.getSprite();
						}
						else {
							eventText.text = lastTarget.getName() + ": " + curSpeech.getLine();
							curSpeech.endLineRead();
						}
					}
					else {
						eventText.text = lastTarget.getName() + ": " + curSpeech.getReadingLine(curSpeech.getLine(), 25);

                        // Play talking sound effect
                        if(curSpeech.IsNewChar()) {
                            mainSFX.GetComponent<AudioSource>().clip = quack[(int)Random.Range(0, quack.Length - 1)];
                            mainSFX.GetComponent<AudioSource>().pitch = curSpeech.getPitch();
                            mainSFX.GetComponent<AudioSource>().Play();
                        }
                    }
				}
				break;



			default:
				break;
		}
	}

	// Creates a text object that will display a number on the screen for a period of time
	public GameObject damagePrefab;
	public Canvas textCanvas;
	void createDamage(int damage, Color damageType, Vector2 recipient){
		GameObject health = Instantiate(damagePrefab, recipient, Quaternion.identity);
		health.transform.SetParent(textCanvas.transform, false);
		health.GetComponent<Transform>().position = Camera.main.WorldToScreenPoint(recipient);

		health.GetComponent<TextMeshProUGUI>().enabled = true;
		health.GetComponent<TextMeshProUGUI>().text = damage.ToString();
		health.GetComponent<TextMeshProUGUI>().color = damageType;
	}

	// Gradually reduces a GameObject's opacity and returns true once it's gone
	bool fadeToNothing(GameObject dying){
		float curAlpha = dying.GetComponent<SpriteRenderer>().color.a - Time.deltaTime;
		if (curAlpha < 0) {
			curAlpha = 0;
		}

		dying.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, curAlpha);
		if (curAlpha == 0) {
			return true;
		}
		return false;
	}

	// Moves a GameObject to a given location
	bool moveToTarget(GameObject entity, Vector2 target, float speed){
		entity.transform.localPosition = Vector2.MoveTowards(entity.transform.localPosition, target, speed * Time.deltaTime);
		if (entity.transform.localPosition.x == target.x && entity.transform.localPosition.y == target.y) {
			return true;
		}
		return false;
	}

	// Changes the turn tracker to the next turn
	private void endTurn(){
		int rem = curTurn % 10;
		curTurn += (10 - rem);
		eventText.text = "";
	}

	// Update the data and icons of each slot
	private void updateSlots(List<Ally> party){
		for (int i = 0; i < MAX_DUCKS; i++) {
			updateSlotSingle(party, i);
		}
	}
	private void updateSlotSingle(List<Ally> party, int duck){
		if (party[duck].getName().CompareTo("") != 0 && duck < party.Count) {
			// Show everything
			slots[duck].GetComponent<SpriteRenderer>().enabled = true;
			names[duck].enabled = true;
			portrait[duck].GetComponent<SpriteRenderer>().enabled = true;
			symbol[duck].GetComponent<SpriteRenderer>().enabled = true;
			health[duck].enabled = true;
			mana[duck].enabled = true;
			allies[duck].GetComponent<SpriteRenderer>().enabled = true;

			// Update name and sprite of ducky
			names[duck].text = party[duck].getName();
			if (party[duck].getCurHP() > 0)
				allies[duck].GetComponent<SpriteRenderer>().sprite = party[duck].playIdle();
			else if(!party[duck].isHurt(allies[duck].GetComponent<SpriteRenderer>().sprite)){
				allies[duck].GetComponent<SpriteRenderer>().sprite = party[duck].playKO();
			}
			
			// Update icon
			portrait[duck].GetComponent<SpriteRenderer>().sprite = party[duck].getFace();
			symbol[duck].GetComponent<SpriteRenderer>().sprite = party[duck].getWing();

			// Update hp and mp text display
			health[duck].text = party[duck].getCurHP().ToString() + "/" + party[duck].getHP().ToString();
			mana[duck].text = party[duck].getCurMP().ToString() + "/" + party[duck].getMP().ToString();

			// Update the meters by their ratios
			float hpRatio = (float)party[duck].getCurHP() / (float)party[duck].getHP();
			float mpRatio = (float)party[duck].getCurMP() / (float)party[duck].getMP();
			float exRatio = (float)party[duck].getCurEX() / 100;
			float expRatio = (float)party[duck].getCurExp() / (float)party[duck].getMaxExp();
			hp[duck].transform.localScale = new Vector2(hpRatio, hp[duck].transform.localScale.y);
			mp[duck].transform.localScale = new Vector2(mpRatio, mp[duck].transform.localScale.y);
			meter[duck].transform.localScale = new Vector2(exRatio, meter[duck].transform.localScale.y);
			exp[duck].transform.localScale = new Vector2(expRatio, exp[duck].transform.localScale.y);

			// Update status display
			int statusAmt = 0;
			bool[] isStatus = party[duck].getStatus();
			for (int i = 0; i < isStatus.Length; i++) {
				if (isStatus[i]) {
					statSets[duck].stats[statusAmt].GetComponent<SpriteRenderer>().enabled = true;
					statSets[duck].stats[statusAmt].GetComponent<SpriteRenderer>().sprite = statusEffects[i];
					statusAmt += 1;
					if (i == 0)
						break;
				}
			}
			int[] theBoosts = party[duck].getBoosts();
			for (int i = 0; i < theBoosts.Length; i++) {
				if (theBoosts[i] < 0) {
					statSets[duck].stats[statusAmt].GetComponent<SpriteRenderer>().enabled = true;
					statSets[duck].stats[statusAmt].GetComponent<SpriteRenderer>().sprite = statDisplays[i].stats[theBoosts[i] + 4];
					statusAmt += 1;
				}
				else if (theBoosts[i] > 0) {
					statSets[duck].stats[statusAmt].GetComponent<SpriteRenderer>().enabled = true;
					statSets[duck].stats[statusAmt].GetComponent<SpriteRenderer>().sprite = statDisplays[i].stats[theBoosts[i] + 3];
					statusAmt += 1;
				}
			}

			for(int i = statusAmt; i < statSets[duck].stats.Length; i++){
				statSets[duck].stats[i].GetComponent<SpriteRenderer>().enabled = false;
			}
		}
		else {
			// Hide everything
			slots[duck].GetComponent<SpriteRenderer>().enabled = false;
			names[duck].enabled = false;
			portrait[duck].GetComponent<SpriteRenderer>().enabled = false;
			symbol[duck].GetComponent<SpriteRenderer>().enabled = false;
			health[duck].enabled = false;
			mana[duck].enabled = false;
			hp[duck].transform.localScale = new Vector2(0, hp[duck].transform.localScale.y);
			mp[duck].transform.localScale = new Vector2(0, mp[duck].transform.localScale.y);
			meter[duck].transform.localScale = new Vector2(0, meter[duck].transform.localScale.y);
			exp[duck].transform.localScale = new Vector2(0, exp[duck].transform.localScale.y);
			allies[duck].GetComponent<SpriteRenderer>().enabled = false;
			for(int i = 0; i < statSets[duck].stats.Length; i++){
				statSets[duck].stats[i].GetComponent<SpriteRenderer>().enabled = false;
			}
		}
	}

	// Shifts the active slot up on the screen while everything is shifted down
	private void setSlotActive(int activeChar){
		for (int i = 0; i < slots.Length; i++) {
			if (i == activeChar) {
				slots[i].transform.localPosition = new Vector2(slots[i].transform.localPosition.x, -1.141f);
				names[i].GetComponent<Transform>().position = Camera.main.WorldToScreenPoint(namePos[i].transform.position);

				health[i].GetComponent<Transform>().position = new Vector2(health[i].GetComponent<Transform>().position.x, 
																	Camera.main.WorldToScreenPoint(activeBars.transform.position).y);
				mana[i].GetComponent<Transform>().position = new Vector2(mana[i].GetComponent<Transform>().position.x, 
																	Camera.main.WorldToScreenPoint(activeBars.transform.position).y);
			}
			else {
				slots[i].transform.localPosition = new Vector2(slots[i].transform.localPosition.x, -1.301f);
				names[i].GetComponent<Transform>().position = Camera.main.WorldToScreenPoint(namePos[i].transform.position);

				health[i].GetComponent<Transform>().position = new Vector2(health[i].GetComponent<Transform>().position.x, 
																	Camera.main.WorldToScreenPoint(nonactiveBars.transform.position).y);
				mana[i].GetComponent<Transform>().position = new Vector2(mana[i].GetComponent<Transform>().position.x, 
																	Camera.main.WorldToScreenPoint(nonactiveBars.transform.position).y);
			}
		}
	}

	// Change the visibility of a text array
	private void textDisplay(TextMeshProUGUI[] t, bool display){
		for (int i = 0; i < t.Length; i++) {
			t[i].enabled = display;
		}
	}

	// Change the color of an array of text to show a selection
	private void updateSelection(TextMeshProUGUI[] t, int sel){
		for (int i = 0; i < t.Length; i++) {
			if (i == sel)		t[i].color = Color.black;
			else 				t[i].color = Color.white;
		}
	}

	// Change color of gameObject based off a selected target
	private void targetSel(GameObject[] g, int sel){
		for (int i = 0; i < g.Length; i++) {
			if (i == sel)		g[i].GetComponent<SpriteRenderer>().color = new Color(1, 0.7f, 0.7f);
			else 				g[i].GetComponent<SpriteRenderer>().color = Color.white;
		}
	}

	// Return a desired sprite in an array of sprites
	private Sprite getSprite(Sprite[] sheet, string name){
		for (int i = 0; i < sheet.Length; i++) {
			if (sheet[i].name == name) {
				return sheet[i];
			}
		}
		return null;
	}

	// Controls the option variables as user selections their desired options
	private void handelOpts(List<Ally> party){
		bool updateText = true;
		// For directional input, change the current option or target based on the current menu
		if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.UP])) {
			if (curMenu == menus.TARGET) {
				if (targetIsFoe) {
					do {
						curTarget -= 1;
						curTarget = (curTarget + MAX_DUCKS) % MAX_DUCKS;
					} while(enemies[trueTarget[curTarget]].getCurHP() <= 0);
				}
				else {
					do {
						curTarget -= 1;
						curTarget = (curTarget + MAX_DUCKS) % MAX_DUCKS;
					} while(party[trueTarget[curTarget]].getName().CompareTo("") == 0 ||
					        (actions[curChar] == options.SWITCH && trueTarget[curTarget] == curChar));
					//while(party[trueTarget[curTarget]].getName().CompareTo("") == 0 || party[trueTarget[curTarget]].getCurHP() <= 0
					//	|| (actions[curChar] == options.SWITCH && trueTarget[curTarget] == curChar));
				}
			}
			else {
				curOpt -= 1;
				curOpt = (curOpt + maxOpt) % maxOpt;
			}

            // Play sound effect
            mainSFX.GetComponent<AudioSource>().clip = select;
            mainSFX.GetComponent<AudioSource>().Play();
        }
		else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.LEFT])) {
			if (curMenu != menus.TARGET) {
				if (curMenu == menus.MAIN)
					curOpt -= 2;
				else
					curOpt -= 3;
				curOpt = (curOpt + maxOpt) % maxOpt;
			}

            // Play sound effect
            mainSFX.GetComponent<AudioSource>().clip = select;
            mainSFX.GetComponent<AudioSource>().Play();
        }
		else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.RIGHT])) {
			if (curMenu != menus.TARGET) {
				if (curMenu == menus.MAIN)
					curOpt += 2;
				else
					curOpt += 3;
				curOpt = curOpt % maxOpt;
			}

            // Play sound effect
            mainSFX.GetComponent<AudioSource>().clip = select;
            mainSFX.GetComponent<AudioSource>().Play();
        }
		else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.DOWN])) {
			if (curMenu == menus.TARGET) {
				if (targetIsFoe) {
					do {
						curTarget += 1;
						curTarget = (curTarget + MAX_DUCKS) % MAX_DUCKS;
					} while(enemies[trueTarget[curTarget]].getCurHP() <= 0);
				}
				else {
					do {
						curTarget += 1;
						curTarget = (curTarget + MAX_DUCKS) % MAX_DUCKS;
					} while(party[trueTarget[curTarget]].getName().CompareTo("") == 0 || 
						(actions[curChar] == options.SWITCH && trueTarget[curTarget] == curChar));
				}
			}
			else {
				curOpt += 1;
				curOpt = curOpt % maxOpt;
			}

            // Play sound effect
            mainSFX.GetComponent<AudioSource>().clip = select;
            mainSFX.GetComponent<AudioSource>().Play();
        }

		// Act based on the confirmed option and the current menu
		else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
			if (curMenu == menus.MAIN) {
				// Check to see if an action cannot be used
				if (batEvents != null && atLastTurn < batEvents.Length && batEvents[atLastTurn].act == battleActions.NOACT) {
					for (int i = 0; i < batEvents[atLastTurn].noact.cantAct.Length; i++) {
						preventCombat[curChar] = !batEvents[atLastTurn].noact.doCombat;
						if (batEvents[atLastTurn].noact.cantAct[i] == (options)(curOpt + 1)) {
							textDisplay(mainOptions, false);
							description.enabled = false;
							eventText.enabled = true;
							duckSprite.GetComponent<SpriteRenderer>().enabled = true;

							curSpeech = new Dialouge(batEvents[atLastTurn].noact.displayMsg[i].dialouge, batEvents[atLastTurn].noact.displayMsg[i].image,
								batEvents[atLastTurn].noact.displayMsg[i].name, batEvents[atLastTurn].noact.displayMsg[i].pitch);
							duckSprite.GetComponent<SpriteRenderer>().sprite = batEvents[atLastTurn].noact.displayMsg[i].image[0];
							eventText.text = "";
							curState = battleStates.PLAYERMSG;
							textBox.GetComponent<SpriteRenderer>().sprite = textboxDisp[1];
							break;
						}
					}
				}

				if (curState == battleStates.SELECTION) {
					switch (curOpt) {
						case 0: // Attack
							// Store attack as current action
							actions[curChar] = options.ATTACK;
							// Ask for target of action
							curMenu = menus.TARGET;
							targetIsFoe = true;
							description.text = "Select a target.";
							break;
						case 1: // Defend
							// Store defend as current action
							actions[curChar] = options.DEFEND; 
							// Move onto next Character
							curChar += 1;
							curOpt = 0;
							break;
						case 2: // Skills
							// Change to skill submenu
							curMenu = menus.SKILLS;
							textDisplay(mainOptions, false);
							textDisplay(subOptions, true);
							textDisplay(skillInfo, true);
							// Select first skill as default
							curOpt = 0;
							maxOpt = party[curChar].getSkillAmt() - 1;
							// Display name of each skill
							updateSkillSet(0, curChar, party);
							break;
						case 3: //Switch
							// Store switch as current action
							actions[curChar] = options.SWITCH;
							// Make sure there are actual targets
							bool moreThanOnePlayer = false;
							for (int i = 0; i < MAX_DUCKS; i++) {
								if (trueTarget[i] != curChar && party[trueTarget[i]].getName().CompareTo("") != 0) {
									if (party[trueTarget[i]].getCurHP() > 0) {
										moreThanOnePlayer = true;
										break;
									}
								}
							}

							if (moreThanOnePlayer) {
								// Ask for target of action
								curMenu = menus.TARGET;
								description.text = "Select a target.";
								targetIsFoe = false;
								// Make sure you can't target yourself or dead peeps
								do {
									curTarget += 1;
									curTarget = (curTarget + MAX_DUCKS) % MAX_DUCKS;
								} while(party[trueTarget[curTarget]].getName().CompareTo("") == 0 || party[trueTarget[curTarget]].getCurHP() <= 0
								       || trueTarget[curTarget] == curChar);
							}
							else {
								updateText = false;
								description.text = "There's no one left to switch with!";
							}
							break;
						case 4: // Items
							if (TitleManager.curFile.getInventoryOfType(itemType.BATTLE).Count > 0) {
								// Change to item submenu
								curMenu = menus.ITEMS;
								textDisplay(mainOptions, false);
								textDisplay(subOptions, true);
								// Select first item as default
								curOpt = 0;
								maxOpt = TitleManager.curFile.getInventoryOfType(itemType.BATTLE).Count;
								// Present first portion of items possible
								updateItemSet(0, TitleManager.curFile.getInventoryOfType(itemType.BATTLE));
							}
							else {
								updateText = false;
								description.text = "Your inventory is empty!";
							}
							break;
						case 5: // Flee
							// Store flee as current action
							actions[curChar] = options.FLEE;
							// Move onto next character
							curChar += 1;
							curOpt = 0;
							break;
						default: 
							break;
					}
				}
			}
			else if (curMenu == menus.SKILLS) {
				// Store skill as current action
				actions[curChar] = options.SKILL;
				// Store which skill was selected
				subSelect[curChar] = curOpt + 1;
				if (party[curChar].getSkill(curOpt + 1).isSelfTarget()) {
					target[curChar] = curChar;
					// Move onto the next character
					curChar += 1;
					curMenu = menus.MAIN;
					curOpt = 0;
					maxOpt = MENU_OPTS;
					// Make sure the main options are shown
					textDisplay(mainOptions, true);
					textDisplay(subOptions, false);
				}
				else {
					if (party[curChar].getSkill(curOpt + 1).getType() == skillType.SUPPORT)
						targetIsFoe = false;
					else {
						targetIsFoe = true;
					}
					// Ask for target of action
					curMenu = menus.TARGET;
					description.text = "Select a target.";
				}
			}
			else if (curMenu == menus.ITEMS) {
				List<Item> curInven = TitleManager.curFile.getInventoryOfType(itemType.BATTLE);
				// Store item as current action
				actions[curChar] = options.ITEMS;
				// Store which item was selected
				subSelect[curChar] = curOpt;
				itemSel[curChar] = curInven[curOpt];
				if (curInven[curOpt].getBattleType() == battleType.STATUS && curInven[curOpt].getScalar() < 0) {
					targetIsFoe = true;
				}
				else {
					targetIsFoe = false;
				}
				// Ask for target of action
				curMenu = menus.TARGET;
				description.text = "Select a target.";
			}
			else if (curMenu == menus.TARGET) {
				// Store the target of the current character's action
				target[curChar] = trueTarget[curTarget];
				// Treat attack as the 0th skill
				if (actions[curChar] == options.ATTACK) {
					actions[curChar] = options.SKILL;
					subSelect[curChar] = 0;
				}
				// Reset target and make sure no one is highlighted
				curTarget = 2;
				targetSel(allies, -1);
				targetSel(foes, -1);
				// Move onto the next character
				curChar += 1;
				curMenu = menus.MAIN;
				curOpt = 0;
				maxOpt = MENU_OPTS;
				// Make sure the main options are shown
				textDisplay(mainOptions, true);
				textDisplay(subOptions, false);
				textDisplay(skillInfo, false);
			}

            // Play sound effect
            mainSFX.GetComponent<AudioSource>().clip = select;
            mainSFX.GetComponent<AudioSource>().Play();
        }
		// Cancel the current option and go back to the previous menu
		else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2])) {
			if (curMenu == menus.SKILLS) {
				// Go back to main menu and have the skill option highlighted
				curOpt = 2;
				maxOpt = MENU_OPTS;
				curMenu = menus.MAIN;
				textDisplay(mainOptions, true);
				textDisplay(subOptions, false);
				textDisplay(skillInfo, false);
			}
			else if (curMenu == menus.ITEMS) {
				// Go back to main menu and have the items option highlighted
				curOpt = 4;
				maxOpt = MENU_OPTS;
				curMenu = menus.MAIN;
				textDisplay(mainOptions, true);
				textDisplay(subOptions, false);
			}
			else if (curMenu == menus.TARGET) {
				// Reset target and make sure no one is highlighted
				curTarget = 2;
				targetSel(allies, -1);
				targetSel(foes, -1);

				// Go back to previous menu based on selected option
				switch (actions[curChar]) {
					case options.ATTACK:
						curMenu = menus.MAIN;
						break;
					case options.SKILL:
						curMenu = menus.SKILLS;
						break;
					case options.SWITCH:
						curMenu = menus.MAIN;
						break;
					case options.ITEMS:
						curMenu = menus.ITEMS;
						break;
					default:
						break;
				}
			}
			else if (curChar > 0) {
				// Go back to the previous character if there was one
				do {
					curChar -= 1;
				} while (curChar > 0 && (party[curChar].getName().CompareTo("") == 0 || party[curChar].getCurHP() <= 0));
				curOpt = 0;
			}

            // Play sound effect
            mainSFX.GetComponent<AudioSource>().clip = cancel;
            mainSFX.GetComponent<AudioSource>().Play();
        }

		// Update the current diplay based on what's being selected
		if (Input.anyKeyDown && updateText) {
			switch (curMenu) {
				case menus.MAIN:
					updateSelection(mainOptions, curOpt);
					description.text = menuDes[curOpt];
					break;
				case menus.SKILLS:
					updateSelection(subOptions, curOpt % subOptions.Length);
					updateSkillSet(curOpt, curChar, party);
					description.text = party[curChar].getSkill(curOpt+1).getDes();
					// Update skill info
					Skill curSkill = party[curChar].getSkill(curOpt+1);
					skillInfo[0].text = "Type: " + curSkill.getType().ToString();
					skillInfo[1].text = "Element: " + curSkill.getElement().ToString();
					skillInfo[2].text = "Base Power: " + curSkill.getPower().ToString();
					skillInfo[3].text = "MP Cost: " + curSkill.getMpCost().ToString();
					skillInfo[4].text = "Target: " + curSkill.getRange().ToString();
					if(curSkill.getEndDist() == -1)
						skillInfo[5].text = "Range: pretty far"; 
					else
						skillInfo[5].text = "Range: " + curSkill.getEndDist().ToString() + " dft";
					break;
				case menus.ITEMS:
					updateSelection(subOptions, curOpt % subOptions.Length);
					updateItemSet(curOpt, TitleManager.curFile.getInventoryOfType(itemType.BATTLE));
					description.text = TitleManager.curFile.getInventoryOfType(itemType.BATTLE)[curOpt].getDes();
					break;
				case menus.TARGET:
					if (!targetIsFoe) {
						targetSel(allies, trueTarget[curTarget]);

						int range = 0;
						if (actions[curChar] == options.SKILL) {
							range = party[curChar].getSkill(subSelect[curChar]).getRange();
						}
						else if (actions[curChar] == options.ITEMS) {
							range = itemSel[curChar].getRange(); 
						}
					
						for (int i = 1; i < range; i++) {
							if (i % 2 == 1 && curTarget + (i / 2) + 1 < MAX_DUCKS) {
								allies[trueTarget[curTarget + (i / 2) + 1]].GetComponent<SpriteRenderer>().color = new Color(1, 0.7f, 0.7f);
							}
							else if (i % 2 == 0 && curTarget - (i / 2) >= 0) {
								allies[trueTarget[curTarget - (i / 2)]].GetComponent<SpriteRenderer>().color = new Color(1, 0.7f, 0.7f);
							}
						}
					}
					else {
						while(enemies[trueTarget[curTarget]].getCurHP() <= 0) {
							curTarget += 1;
							curTarget = (curTarget + MAX_DUCKS) % MAX_DUCKS;
						}
						targetSel(foes, trueTarget[curTarget]);
						int range = 0;
						if (actions[curChar] == options.SKILL) {
							range = party[curChar].getSkill(subSelect[curChar]).getRange();
						}
						else if (actions[curChar] == options.ITEMS) {
							range = itemSel[curChar].getRange(); 
						}
						for (int i = 1; i < range; i++) {
							if (i % 2 == 1 && curTarget + (i / 2) + 1 < MAX_DUCKS) {
								foes[trueTarget[curTarget + (i / 2) + 1]].GetComponent<SpriteRenderer>().color = new Color(1, 0.7f, 0.7f);
							}
							else if (i % 2 == 0 && curTarget - (i / 2) >= 0) {
								foes[trueTarget[curTarget - (i / 2)]].GetComponent<SpriteRenderer>().color = new Color(1, 0.7f, 0.7f);
							}
						}
					}
					break;
				default:
					break;
			}
		}
	}

	// Organize actions based on speed and priority
	private void setTurnOrder(List<Ally> party, List<Foe> enemies){
		// Arrange the index of allies based on speed
		int[] ally = {0, 1, 2, 3, 4}; 
		bool swapped = false;
		do {
			swapped = false;
			for(int i = 1; i <= ally.Length-1; i ++){
				if(party[ally[i-1]].getSPD() < party[ally[i]].getSPD()){
					int temp = ally[i];
					ally[i] = ally[i-1];
					ally[i-1] = temp;
					swapped = true;
				}
			}
		} while(swapped);

		// Prioritize characters who selected switch
		for(int i = 1; i < ally.Length; i++){
			if(actions[ally[i]] == options.SWITCH){
				int temp = ally[i]; 				// Hold the current index
				for (int j = i; j > 0; j--) { 		// Shift the slots before the current index up by one
					ally[j] = ally[j - 1];
				}
				ally[0] = temp;						// Store the current index value at the beginning
			}
		}

		// Arrange the index of foes based on speed
		int[] enemy = {0, 1, 2, 3, 4}; 
		do {
			swapped = false;
			for(int i = 1; i <= enemy.Length-1; i ++){
				if(enemies[enemy[i-1]].getSPD() < enemies[enemy[i]].getSPD()){
					int temp = enemy[i];
					enemy[i] = enemy[i-1];
					enemy[i-1] = temp;
					swapped = true;
				}
			}
		} while(swapped);

		for (int i = 4; i > 0; i--) {
			for (int j = 4; j > 0; j--) {
				if (enemies[enemy[j]].getCurHP() <= 0) {
					int temp = enemy[j];
					enemy[j] = enemy[j-1];
					enemy[j-1] = temp;
				}
			}
		}

		// Combine the two sorted arrays
		int allyAdded = 0, enemyAdded = 0;
		for (int i = 0; i < combatantOrder.Length; i++) {
			// Place dead people to the top of the list
			if (allyAdded < MAX_DUCKS && party[ally[allyAdded]].getCurHP() <= 0) {
				combatantOrder[i] = ally[allyAdded];
				allyAdded += 1;
			}
			else if(enemyAdded < MAX_DUCKS && enemies[enemy[enemyAdded]].getCurHP() <= 0){
				combatantOrder[i] = enemy[enemyAdded] + 5;
				enemyAdded += 1;
			}
			// Priortize switch action
			else if (allyAdded < 5 && actions[ally[allyAdded]] == options.SWITCH) {
				combatantOrder[i] = ally[allyAdded];
				allyAdded += 1;
			}
			else {
				if (allyAdded >= MAX_DUCKS) {
					combatantOrder[i] = enemy[enemyAdded] + 5;
					enemyAdded += 1;
				}
				else if (enemyAdded >= MAX_DUCKS) {
					combatantOrder[i] = ally[allyAdded];
					allyAdded += 1;
				}

				else if (party[ally[allyAdded]].getSPD() >= enemies[enemy[enemyAdded]].getSPD()) {
					combatantOrder[i] = ally[allyAdded];
					allyAdded += 1;
				}
				else if (party[ally[allyAdded]].getSPD() < enemies[enemy[enemyAdded]].getSPD()) {
					combatantOrder[i] = enemy[enemyAdded] + 5;
					enemyAdded += 1;
				}
			}
		}
	}

	private void updateSkillSet(int curOpt, int curChar, List<Ally> party){
		int rem = curOpt % subOptions.Length;
		int start = rem * subOptions.Length;
		for (int i = start; i < subOptions.Length; i++) {
			if ((i+1) < party[curChar].getSkillAmt())		
				subOptions[i].text = party[curChar].getSkill(i+1).getName();
			else 										
				subOptions[i].text = "";
		}
	}

	private void updateItemSet(int curOpt, List<Item> fieldItems){
		int rem = curOpt % subOptions.Length;
		int start = rem * subOptions.Length;
		for (int i = start; i >= 0 && i < subOptions.Length; i++) {
			if (i < fieldItems.Count) {	
				subOptions[i].text = fieldItems[i].getName();
				subOptions[i].text += " x" + fieldItems[i].getAmt().ToString();
			}
			else 									
				subOptions[i].text = "";
		}
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

	// Performs animation and effect of a supporting action(return its completeion)
	private bool hasSetSupport = false;
	private bool performSupport(battleType b, status s, stat boost, int range, int scalar, int curDude, SpriteAnimation statusEffect){
		// For each target of item: heal and display healing animation
		if (!hasSetSupport) {
			for (int i = 0; i < range; i++) {
				int translation = i;
				if (i % 2 == 1) 	{ translation = target[curDude] + (i / 2) + 1; }
				else 				{ translation = target[curDude] - (i / 2); }

				if (translation >= 0 && translation < MAX_DUCKS) {
					if (party[translation].getName() != "") {
						statusHolder[translation].transform.localPosition = playersPos[translation];
						statusHolder[translation].GetComponent<SpriteRenderer>().enabled = true;
						statusHolder[translation].GetComponent<SpriteRenderer>().enabled = statusEffect.playAnim();
						statusHolder[translation].GetComponent<SpriteRenderer>().sortingOrder = layerTarget[translation] * 2 + 1;

						if (b == battleType.HEAL) {
							if (party[translation].getCurHP() > 0) {
								int amt = (int)((float)party[translation].getHP() * (float)scalar / 100);
								party[translation].adjustHP(amt);
								createDamage(amt, Color.green, playersPos[translation]);
							}
							else {
								createDamage(0, Color.green, playersPos[translation]);
							}
                            mainSFX.GetComponent<AudioSource>().clip = statusSFX[0];
                        }
						else if (b == battleType.CURE) {
							int amt = (int)((float)party[translation].getHP() * (float)scalar / 100);
							party[translation].adjustMP(amt);
							createDamage(amt, Color.blue, playersPos[translation]);
                            mainSFX.GetComponent<AudioSource>().clip = statusSFX[1];
                        }
						else if (b == battleType.BOOST) {
							party[translation].setBoost(boost, scalar);
                            if(scalar >= 0)
                                mainSFX.GetComponent<AudioSource>().clip = statusSFX[2];
                            else
                                mainSFX.GetComponent<AudioSource>().clip = statusSFX[3];
                        }
						else if (b == battleType.STATUS) {
							if (s == status.KO && scalar > 0 && party[translation].isKO()) {
								int amt = (int)((float)party[translation].getHP() * (float)scalar / 100);
								party[translation].adjustHP(Mathf.Abs(amt));
								createDamage(Mathf.Abs(amt), Color.green, playersPos[translation]);
							}
							party[translation].setStatus(s, scalar < 0);
                            mainSFX.GetComponent<AudioSource>().clip = statusSFX[4];
                        }
					}
				}
			}
            // Play skill sound effect
            mainSFX.GetComponent<AudioSource>().Play();

            updateSlots(party);
			hasSetSupport = true;
		}
		// Play animations and stop after 2 seconds
		else {
			if (!WaitForXFrames(30)) {
				for (int i = 0; i < range; i++) {
					int translation = i;
					if (i % 2 == 1) 	{ translation = target[curDude] + (i / 2) + 1; }
					else 				{ translation = target[curDude] - (i / 2); }

					if (translation >= 0 && translation < MAX_DUCKS) {
						statusHolder[translation].GetComponent<SpriteRenderer>().sprite = statusEffect.playAnim();
					}
				}
			}
			else {
                for (int i = 0; i < range; i++) {
					int translation = i;
					if (i % 2 == 1) 	{ translation = target[curDude] + (i / 2) + 1; }
					else 				{ translation = target[curDude] - (i / 2); }

					if (translation >= 0 && translation < MAX_DUCKS) {
						statusHolder[translation].GetComponent<SpriteRenderer>().enabled = false;
					}
				}
				hasSetSupport = false;
				return true;
			}
		}
		return false;
	}
}
