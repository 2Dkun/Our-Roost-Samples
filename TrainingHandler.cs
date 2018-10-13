using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TrainingHandler : MonoBehaviour {

	// Menu variables
	enum menu { MAIN, COMMAND, HELP };
	private menu curMenu;
	private int curOpt;
	public TextMeshProUGUI[] mainOpts;
	public GameObject menuScreen, blackScreen;
	public Sprite[] screensForMenu;
	private bool toTitle, reset;
	// Command List variables
	public TextMeshProUGUI[] skillInfo;
	public TextMeshProUGUI[] skills;
	private List<FighterSkill> skillList;
	// Help variables
	public TextMeshProUGUI[] tips;
	public TextMeshProUGUI answer, description;
	private string[] theAnswers = { 
		"In the previous menu, there was an option to change your character selection. Instead " +
		"of pressing B1 while this option is selected, you can switch between characters using " +
		"the LEFT and RIGHT buttons. If you forget what they are, hold escape to reset controls. " +
		"Default input for LEFT is \"A\" and RIGHT is \"D\".",

		"Normal attacks are considered to be attacks that are performed on the ground without requiring" +
		" complex input. These are generally done by inputting B1, B2, or B3 with or without holding DOWN.",

		"Aerial attacks are considered to be attacks that are performed in the air without requiring" +
		" complex input. These are generally done by inputting B1, B2, or B3 while being in the air after" +
		" a jump, which can be done by pressing UP while on the ground.",

		"Specials are considered to be moves that are performed on the ground and require some form of" +
		" complex input. These are generally done by inputting B1, B2, or B3 after certain directional input." +
		" The input for these moves can be seen in the command list.",

		"You can get more special moves by leveling up your characters in story mode. As of right now, there are" +
		" only two specials for each character that can be learned. To get additional specials, you could donate to" +
		" the game on Kickstarter. I need an artist who can actually make attack animations.",

		"A dash is a form of movement on the ground that allows your character to move forward faster than their " +
		"normal walk speed. Rather than pressing FORWARD once to walk, you can quickly press it twice to perform " +
		"a dash. To stop dashing, just release the FORWARD button.",

		"A backhop is an option that you could do on the ground to become temporarily immune to all attacks. In this " +
		"state, your character will move slightly backwards and cannot act until the backhop is over. To perform this " +
		"option, quickly press BACK twice while on the ground.",

		"A glide is a form of movement in the air that allows your character to quickly move forward for a moment in time" +
		". Your character cannot act until the glide is over. To perform a glide, quickly press FORWARD or BACK twice while" +
		" in the air. You can also glide right after jumping by quickly pressing the same horizontal direction as you pressed " +
		"with your jump.",

		"A normal cancel is when you immediately stop your attack with another attack. " +
		"This can only be done if your first attack hits an opponent. Generally, you cannot " +
		"cancel a stronger normal into a weaker normal.",

		"A special cancel is when you immediately stop your attack with a special move. " +
		"This can only be done if your first attack hits an opponent. You can see what moves " +
		"can be cancelled into special moves in the command list. Jump cancels are the same, but " +
		"instead of stopping your attack with a special, you jump instead.",

		"I'm not a good artist, so it was difficult to make the animations for these current attacks. Most of them use the " +
		"same sprites, but recolored and slightly adjusted. To start making this a better fighting game, I'd like to hire an artist to" +
		" better animate any potential attack/option. This would allow me to make better moves, such as attacks with more range," +
		" sensible endlag, etc."

	};

	// Fighter variables
	public GameObject helper, fighter, baggie;
	public Sprite[] anniesHelper;
	public float minX, maxX, minY;
	[System.Serializable]
	public class EnemyBag {
		public Sprite ok, hurt;
		public HitBox[] hurtbox;
		public int def, mdef;
	}
	public EnemyBag theBag;
    public GameObject mySound;
    public AudioClip[] basicSFX;
    public AudioClip select, cancel;

    private List<Ally> party;
	private Ally curDude, annie;
	private Foe mrsBag;
	private List<GameObject> projectiles;
	private float b3time;	

	// Use this for initialization
	void Start () {
		projectiles = new List<GameObject>();
		party = TitleManager.curFile.getParty();
		curDude = party[0];
		for (int i = 0; i < party.Count; i++) {
			if (party[i].getCharacter() == character.ANNIE) {
				annie = party[i];
				break;
			}
		}

		// Make the bag enemy
		int[] theStats = { 0, 0, 0, theBag.def, 0, theBag.mdef, 0 };
		Sprite[] idleBag = new Sprite[1];
		Sprite[] hurtBag = new Sprite[1];
		idleBag[0] = theBag.ok;
		hurtBag[0] = theBag.hurt;
		SpriteAnimation idle = new SpriteAnimation(idleBag, new int[0], 0, true);
		SpriteAnimation hurt = new SpriteAnimation(hurtBag, new int[0], 0, true);

		mrsBag = new Foe("Mrs. Bag", theStats, new List<Skill>(), new int[0], 
			new AItype[0], new Item[0], new int[0], theBag.hurtbox, 3f);
		mrsBag.setAnimations(null, idle, idle, idle, idle, hurt);

		ResetFighters();
		b3time = 0;

		// Hide menu
		textDisplay(mainOpts, false);
		textDisplay(skills, false);
		textDisplay(skillInfo, false);
		textDisplay(tips, false);
		description.enabled = false;
		answer.enabled = false;
		menuScreen.GetComponent<SpriteRenderer>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		// Go to title screen
		if(toTitle){
			// Fade to black
			float theAlpha = blackScreen.GetComponent<SpriteRenderer>().color.a + Time.deltaTime;
			if (theAlpha > 1) {
				theAlpha = 1;
			}
            // Fade text
            for(int i = 0; i < mainOpts.Length; i++) {
                if(i != curOpt)
                    mainOpts[i].color = new Color(1 - theAlpha,1 - theAlpha,1 - theAlpha);
            }

			blackScreen.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, theAlpha);
			if (theAlpha == 1) {
				SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);
			}
		}

		// Determine hold of b3
		if (Input.GetKey(DataManager.savedOptions.controls[(int)key.B3])) {
			b3time += Time.deltaTime;
			if (b3time >= 1 && curMenu == menu.MAIN) {
				textDisplay(mainOpts, true);
				menuScreen.GetComponent<SpriteRenderer>().enabled = true;
				menuScreen.GetComponent<SpriteRenderer>().sprite = screensForMenu[0];
				updateSelection(mainOpts, curOpt);
			}
		}
		else if(Input.GetKeyUp(DataManager.savedOptions.controls[(int)key.B3]) && b3time < 1) {
			b3time = 0;
		}

		// Check if user is trying to open menu
		if (b3time >= 1) {
			HandleMenu();
		}
		else {
			AnimHelper();
			HandleFighters();
		}
	}

	// Animate the helper in the background
	void AnimHelper(){
		if (curDude.getCharacter() == character.ANNIE) {
			helper.GetComponent<SpriteRenderer>().sprite = anniesHelper[(Time.frameCount/60)%anniesHelper.Length];
		}
		else
			helper.GetComponent<SpriteRenderer>().sprite = annie.playIdle();
	}

	// Reset positions of fighters
	void ResetFighters(){
		Fighter curFighter = curDude.getFighter();
		// Clear buffer and input
		curFighter.clearBuffer();
		curFighter.clearInput();
		curFighter.resetState();

        if(projectiles == null) {
            projectiles = new List<GameObject>();
        }

		// Destroy all projectiles
		for (int i = 0; i < projectiles.Count; i++) {
			GameObject theProj = projectiles[i];
			projectiles.Remove(theProj);
			Destroy(theProj);
		}

		// Return fighter to original position
		fighter.transform.localPosition = new Vector2(-1.817f, -0.64f);
		baggie.transform.localPosition = new Vector2(1.911407f, -0.64f);
		fighter.transform.localScale = new Vector2(1, 1);
		baggie.transform.localScale = new Vector2(1, 1);
	}

	// Move current fighter and baggie
	void HandleFighters(){
		Fighter curFighter = curDude.getFighter();

		// Confuse animation for enemy
		if (mrsBag.inHitstun()) 
			baggie.GetComponent<SpriteRenderer>().sprite = mrsBag.playHurt();
		else 
			baggie.GetComponent<SpriteRenderer>().sprite = mrsBag.playConfused();

		// Take in player control
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

		// Control fighter
		curFighter.controlFighter(fighter, minY, minX, maxX, baggie.transform.localPosition.x, mySound, basicSFX);

		// Flip enemy based on position of player
		if (fighter.transform.localPosition.x > baggie.transform.localPosition.x) 
			baggie.transform.localScale = new Vector2(-1, 1);	
		else 
			baggie.transform.localScale = new Vector2(1, 1);

		// Check if the target has been hit by fighter
		if (curFighter.checkHit(fighter, baggie, theBag.hurtbox)) {
			// Calculate and apply damage
            float atk = 0, def = 0;
            if(curDude.getATK() >= curDude.getMATK()) {
                atk = curDude.getATK();
                def = mrsBag.getDEF();
            } 
            else {
                atk = curDude.getMATK();
                def = mrsBag.getMDEF();
            }
            int damage = (int)(curFighter.getBasePow() * (atk / def));
			createDamage(damage, Color.red, baggie.transform.position);
			// Hurt animation and set hitstun
			baggie.GetComponent<SpriteRenderer>().sprite = theBag.hurt;
			mrsBag.setHitstun(curFighter.getHitStun());
			mrsBag.setXVel(curFighter.getXPow());
			if (baggie.transform.position.y > minY) {
				mrsBag.setYVel(curFighter.getYPow());
			}
			// Slide player back
			curFighter.slideDudeBack();
		}

		// Check if current skill by fighter creates a projectile
		GameObject projPrefab = curFighter.getProjectile();
		if (projPrefab != null) {
			GameObject curProj = Instantiate(projPrefab);
			curProj.transform.localPosition = fighter.transform.localPosition;
			curProj.transform.Translate(fighter.transform.localScale.x * projPrefab.GetComponent<ProjObj>().xShift,
				projPrefab.GetComponent<ProjObj>().yShift, 0);
			curProj.transform.localScale = fighter.transform.localScale;
			curProj.GetComponent<SpriteRenderer>().sortingOrder = fighter.GetComponent<SpriteRenderer>().sortingOrder;
			projectiles.Add(curProj);
		}

		// Check if the target has been hit by a projectile
		for (int i = 0; i < projectiles.Count; i++) {
			GameObject theProj = projectiles[i];
			if (theProj.GetComponent<ProjObj>().checkHit(baggie, mrsBag.getHurtBoxes())) {
                // Calculate and apply damage
                float atk = 0, def = 0;
                if(curDude.getATK() >= curDude.getMATK()) {
                    atk = curDude.getATK();
                    def = mrsBag.getDEF();
                } 
                else {
                    atk = curDude.getMATK();
                    def = mrsBag.getMDEF();
                }
                int damage = (int)(theProj.GetComponent<ProjObj>().basePow * (atk / def));
				createDamage(damage, Color.red, baggie.transform.position);
				// Hurt animation and set hitstun
				baggie.GetComponent<SpriteRenderer>().sprite = theBag.hurt;
				mrsBag.setHitstun(theProj.GetComponent<ProjObj>().hitstun);
				mrsBag.setXVel(theProj.GetComponent<ProjObj>().xPow);
				if (baggie.transform.position.y > minY) 
					mrsBag.setYVel(theProj.GetComponent<ProjObj>().yPow);

				// Delete projectile
				projectiles.Remove(theProj);
				Destroy(theProj);
			}
			// Destroy object if it is out of the borders
			else if (theProj.transform.position.y < minY - 0.4f || theProj.transform.position.x < minX
				|| theProj.transform.position.x > maxX) {
				projectiles.Remove(theProj);
				Destroy(theProj);
			}
		}

		// Prevent foe from leaving screen
		if (baggie.transform.position.x < minX) 
			baggie.transform.position = new Vector2(minX, baggie.transform.position.y);
		else if (baggie.transform.position.x > maxX) 
			baggie.transform.position = new Vector2(maxX, baggie.transform.position.y);

		// Apply gravity to enemy and update timer display
		mrsBag.applyGravity(baggie, minY);

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

	// Deals with input during the menu
	void HandleMenu(){
		
		switch (curMenu) {
			
			case menu.MAIN:
				if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
					if (curOpt == 1) {
						curMenu = menu.COMMAND;
						curOpt = 0;
						skillList = curDude.getFighter().getAllSkills();

						// Display command menu
						menuScreen.GetComponent<SpriteRenderer>().sprite = screensForMenu[1];
						textDisplay(mainOpts, false);
						description.enabled = true;
						textDisplay(skills, true);
						textDisplay(skillInfo, true);
						updateSelection(skills, curOpt);
						updateSkillInfo(curOpt);
					}
					else if (curOpt == 2) {
						curMenu = menu.HELP;
						curOpt = 0;

						// Display help menu
						menuScreen.GetComponent<SpriteRenderer>().sprite = screensForMenu[1];
						textDisplay(mainOpts, false);
						textDisplay(tips, true);
						description.enabled = true;
						answer.enabled = true;
						updateSelection(tips, curOpt);
						answer.text = theAnswers[curOpt];
					}
					else if (curOpt == 3) {
						toTitle = true;
					}

                    // Play select sound effect
                    mySound.GetComponent<AudioSource>().clip = select;
                    mySound.GetComponent<AudioSource>().Play();
                }
				else if (curOpt == 0 && Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.LEFT])) {
					int dude = party.IndexOf(curDude);
					do {
						dude += party.Count - 1;
						dude %= party.Count;
					} while (party[dude].getCharacter() == character.NULL);

					curDude = party[dude];
					reset = true;
					mainOpts[0].text = "Character Select:\n<" + curDude.getName() + ">";

                    // Play select sound effect
                    mySound.GetComponent<AudioSource>().clip = select;
                    mySound.GetComponent<AudioSource>().Play();
                }
				else if (curOpt == 0 && Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.RIGHT])) {
					int dude = party.IndexOf(curDude);
					do {
						dude += 1;
						dude %= party.Count;
					} while (party[dude].getCharacter() == character.NULL);

					curDude = party[dude];
					reset = true;
					mainOpts[0].text = "Character Select:\n<" + curDude.getName() + ">";

                    // Play select sound effect
                    mySound.GetComponent<AudioSource>().clip = select;
                    mySound.GetComponent<AudioSource>().Play();
                }
				else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.UP])) {
					curOpt += mainOpts.Length - 1;
					curOpt %= mainOpts.Length;

                    // Play select sound effect
                    mySound.GetComponent<AudioSource>().clip = select;
                    mySound.GetComponent<AudioSource>().Play();
                }
				else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.DOWN])) {
					curOpt += 1;
					curOpt %= mainOpts.Length;

                    // Play select sound effect
                    mySound.GetComponent<AudioSource>().clip = select;
                    mySound.GetComponent<AudioSource>().Play();
                }
				else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2])){
					b3time = 0;
					if (reset)
						ResetFighters();
					
					// Hide menu
					textDisplay(mainOpts, false);
					menuScreen.GetComponent<SpriteRenderer>().enabled = false;

                    // Play select cancel effect
                    mySound.GetComponent<AudioSource>().clip = cancel;
                    mySound.GetComponent<AudioSource>().Play();
                }
				if (Input.anyKeyDown) 
					updateSelection(mainOpts, curOpt);
				break;

			case menu.COMMAND:

				if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.UP])) {
					curOpt += skillList.Count - 1;
					curOpt %= skillList.Count;

                    // Play select sound effect
                    mySound.GetComponent<AudioSource>().clip = select;
                    mySound.GetComponent<AudioSource>().Play();
                }
				else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.DOWN])) {
					curOpt += 1;
					curOpt %= skillList.Count;

                    // Play select sound effect
                    mySound.GetComponent<AudioSource>().clip = select;
                    mySound.GetComponent<AudioSource>().Play();
                }
				else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2])) {
					curMenu = menu.MAIN;
					curOpt = 1;
					updateSelection(mainOpts, curOpt);

					menuScreen.GetComponent<SpriteRenderer>().sprite = screensForMenu[0];
					textDisplay(mainOpts, true);
					description.enabled = false;
					textDisplay(skills, false);
					textDisplay(skillInfo, false);

                    // Play select cancel effect
                    mySound.GetComponent<AudioSource>().clip = cancel;
                    mySound.GetComponent<AudioSource>().Play();
                }
				if (Input.anyKeyDown) {
					updateSelection(skills, curOpt % skills.Length);
					updateSkillInfo(curOpt);
				}

				break;

			case menu.HELP:

				if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.UP])) {
					curOpt += tips.Length - 1;
					curOpt %= tips.Length;

                    // Play select sound effect
                    mySound.GetComponent<AudioSource>().clip = select;
                    mySound.GetComponent<AudioSource>().Play();
                }
				else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.DOWN])) {
					curOpt += 1;
					curOpt %= tips.Length;

                    // Play select sound effect
                    mySound.GetComponent<AudioSource>().clip = select;
                    mySound.GetComponent<AudioSource>().Play();
                }
				else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2])) {
					curMenu = menu.MAIN;
					curOpt = 2;
					updateSelection(mainOpts, curOpt);

					menuScreen.GetComponent<SpriteRenderer>().sprite = screensForMenu[0];
					textDisplay(mainOpts, true);
					textDisplay(tips, false);
					description.enabled = false;
					answer.enabled = false;

                    // Play select cancel effect
                    mySound.GetComponent<AudioSource>().clip = cancel;
                    mySound.GetComponent<AudioSource>().Play();
                }
				if (Input.anyKeyDown) {
					updateSelection(tips, curOpt);
					answer.text = theAnswers[curOpt];
				}

				break;

			default:
				break;
		}

	}

	// Update skill info
	private void updateSkillInfo(int skill){
		skillInfo[0].text = skillList[skill].getName();
		skillInfo[1].text = "Input:\n";
		KeyCode[] theInput = skillList[skill].getInput();
		for (int i = 0; i < theInput.Length; i++) {
			if (theInput[i] == DataManager.savedOptions.controls[(int)key.LEFT])
				skillInfo[1].text += "BACK";
			else if (theInput[i] == DataManager.savedOptions.controls[(int)key.RIGHT])
				skillInfo[1].text += "FOWARD";
			else {
				for (int j = 0; j < DataManager.savedOptions.controls.Length; j++) {
					if (theInput[i] == DataManager.savedOptions.controls[j])
						skillInfo[1].text += (key)j;
				}
			}
			if (i != theInput.Length - 1)
				skillInfo[1].text += " ";
		}
		skillInfo[2].text = "Power: " + skillList[skill].getBasePow().ToString();
		skillInfo[3].text = "Jump Cancel: " + skillList[skill].isJumpCancelable().ToString();
		skillInfo[4].text = "Special Cancel: " + skillList[skill].isSpecialCancelable().ToString();
		if (skillList[skill].getHitFrames().Length > 0) {
			skillInfo[5].text = "Startup: " + (skillList[skill].getHitFrames()[0] - 1).ToString();
			if (500 < skillList[skill].getHitFrames()[skillList[skill].getHitFrames().Length - 1]) {
				skillInfo[6].text = "Active: until grounded";
				skillInfo[7].text = "Recovery: until grounded";
			}
			else {
				skillInfo[6].text = "Active: " + skillList[skill].getHitFrames().Length.ToString();
				skillInfo[7].text = "Recovery: " + (skillList[skill].firstActive()
				- skillList[skill].getHitFrames()[skillList[skill].getHitFrames().Length - 1]).ToString();
			}
		}
		else {
			skillInfo[5].text = "Startup: 0";
			skillInfo[6].text = "Active: no hitboxes";
			skillInfo[7].text = "Recovery: " + skillList[skill].firstActive().ToString();
		}

		// Update skill list display based on current input
		int section = skill / skills.Length;
		for(int i = section; i < skills.Length + section; i++){
			if (section + i < skillList.Count)
				skills[i - section].text = skillList[section + i].getName();
			else
				skills[i - section].text = "----------------------------";
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
}
