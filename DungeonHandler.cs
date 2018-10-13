using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;

public enum gameState {OVERWORLD, BATTLE, MENU, DIALOUGE, SHOP};

public enum npcType {TALK, SHOP, CHEST, EVENT_TRIGGER, ENCOUNTER};

public class DungeonHandler : MonoBehaviour {

	private const float textSpd = 25;

	public static gameState curState;
	public static gameState preState;
   	public static bool stopMoving;
	public Camera main; // Main camera for overworld
	public GameObject player;
	public string foeSetDataPath;
	public GameObject textbox;
	public GameObject duckSprite;
	public TextMeshProUGUI speech;
	public bool isTown;
	public Sprite background;

	[System.Serializable]
	public class NPCs {
		public npcType type;
		// TALK
		public string name;
		public string[] dialouge;
		public Sprite[] image;
        	public float pitch;
		// SHOP
		public int[] itemIds;
		public string shopBye;
		// EVENT SCENE
		public string scene;
		public Vector2 newPos;
		public string eventScene;
		public int eventID; 
		// CHEST ITEMS
		[System.Serializable]
		public class itemGet {
			public int itemID, itemAmt;
		}
		public itemGet[] loot;
		public int chestID;
        // ENCOUNTER
        public string enemySet;
        public Sprite background;
        public AudioClip battleMusic, encMusic;

        public string getName(){
			return this.name;
		}
	}
	public NPCs[] ducks;
	public GameObject[] entity;
	private int curEntity;
	private List<Item> itemList;

	// Variables for shop
	public GameObject[] shopDisplay;
	public GameObject itemIcon;
	public GameObject statDisp;
	public TextMeshProUGUI itemName;
	public TextMeshProUGUI itemDes;
	public TextMeshProUGUI itemAmt;
	public TextMeshProUGUI balance;
	public TextMeshProUGUI[] itemStat;
	public TextMeshProUGUI[] itemCost;
	public TextMeshProUGUI curBal;
	public TextMeshProUGUI[] itemDisp;
	private int curItem, purchaseAmt;
	private bool checkout;

	// Holds how many items you've gotten from the chest
	private int curLoot;

	private string curName;
	private Dialouge[] curSpeech;
	private string shopText;
	private string shopExitTxt;
	private bool exitShop;

	private List<Foe>[] foeList;
	private List<int> encounterOdds;

	private float curSteps = 0; // Keeps track of how long player has been walking
	private float encounter; // Random number for an encounter to happen

    public GameObject blackScreen, mainSFX;
    public static GameObject mainBGM;
    public AudioClip battleMusic, encMusic, keyPress, cancel;
    public AudioClip[] talking;

    public GameObject map;

	void Start () {
		// Move player to last spot
		player.transform.localPosition = new Vector2(TitleManager.curFile.getXPos(), TitleManager.curFile.getYPos());

		// Hide dialouge stuff
		textbox.GetComponent<SpriteRenderer>().enabled = false;
		duckSprite.GetComponent<SpriteRenderer>().enabled = false;
		speech.enabled = false;
		curEntity = -1;
		purchaseAmt = 1;

		// Hide shop stuff
		DisplaySprite(shopDisplay, false);
		itemIcon.GetComponent<SpriteRenderer>().enabled = false;
		statDisp.GetComponent<SpriteRenderer>().enabled = false;
		itemName.enabled = false;
		itemDes.enabled = false;
		itemAmt.enabled = false;
		balance.enabled = false;
		curBal.enabled = false;
		DisplayText(itemStat, false);
		DisplayText(itemCost, false);
		DisplayText(itemDisp, false);

		// Load in all text and sprite as dialouge
		this.curSpeech = new Dialouge[ducks.Length];
		for (int i = 0; i < curSpeech.Length; i++) {
			curSpeech[i] = new Dialouge(ducks[i].dialouge, ducks[i].image, ducks[i].pitch);
		}

		// Use the main camera at this point
		main.enabled = true;

		// Set encounter variables
		curSteps = 0;
		encounter = Random.Range(100, 700);

		// Read in set of foes
		TextAsset set = Resources.Load<TextAsset>(@foeSetDataPath);
		GameProgress.jankFile setTxt = new GameProgress.jankFile(set);
		readFoes(setTxt);

		// Open any opened chests
		for(int i = 0; i < ducks.Length; i++){
			if (ducks[i].type == npcType.CHEST) {
				if (TitleManager.curFile.getFlag(ducks[i].chestID)) {
					if(entity[i].GetComponent<NPC>() != null)
						entity[i].GetComponent<NPC>().openChest();
				}
			}
		}

        stopMoving = false;
	}
	
	void Update () {
		TitleManager.curFile.updateTime();

        if(curState == gameState.DIALOUGE){
			if (curEntity < 0) {
                map.SetActive(false);
				for (int i = 0; i < entity.Length; i++) {
					if (entity[i] == PlayerManager.curDuck) {
						curEntity = i;
						break;
					}
				}
				if (ducks[curEntity].type == npcType.EVENT_TRIGGER && ducks[curEntity].dialouge.Length <= 0) {
					preState = gameState.OVERWORLD;
					curState = gameState.OVERWORLD;
                    stopMoving = true;
                    if (TitleManager.curFile.getFlag(ducks[curEntity].eventID) || ducks[curEntity].eventID == 0) {
						TitleManager.curFile.setLocation(ducks[curEntity].newPos.x, ducks[curEntity].newPos.y);
                        Destroy(mainBGM);
                        SceneManager.LoadScene(ducks[curEntity].scene, LoadSceneMode.Single);
					}
					else {
						TitleManager.curFile.setFlag(ducks[curEntity].eventID);
						TitleManager.curFile.setLocation(ducks[curEntity].newPos.x, ducks[curEntity].newPos.y);
                        Destroy(mainBGM);
                        SceneManager.LoadScene(ducks[curEntity].eventScene, LoadSceneMode.Single);
					}
				}
				else if (ducks[curEntity].type == npcType.CHEST) {
                    if(!TitleManager.curFile.getFlag(ducks[curEntity].chestID)) {
                        TitleManager.curFile.setFlag(ducks[curEntity].chestID);
                        entity[curEntity].GetComponent<NPC>().openChest();
                        textbox.GetComponent<SpriteRenderer>().enabled = true;
                        speech.enabled = true;
                        // Add first item
                        Item theDrop = (Item)TitleManager.curFile.getItemList()[ducks[curEntity].loot[0].itemID];
                        Item drop = theDrop.copy(ducks[curEntity].loot[0].itemAmt);
                        if(drop.getName() == "Coin") {
                            speech.text = "Obtained $" + drop.getAmt().ToString() + "!";
                            TitleManager.curFile.adjustBalance(drop.getAmt());
                        } else {
                            speech.text = "Obtained " + drop.getName() + " x" + drop.getAmt().ToString() + ".";
                            TitleManager.curFile.addToInventory(drop.copy());
                        }
                        // Play sound effect
                        mainSFX.GetComponent<AudioSource>().clip = keyPress;
                        mainSFX.GetComponent<AudioSource>().Play();

                        curLoot += 1;
                    }
                    else {
                        curState = gameState.OVERWORLD;
                        textbox.GetComponent<SpriteRenderer>().enabled = false;
                        speech.enabled = false;
                        curEntity = -1;
                        map.SetActive(true);
                    }
				}
				else {
					curName = ducks[curEntity].getName();
					textbox.GetComponent<SpriteRenderer>().enabled = true;
					duckSprite.GetComponent<SpriteRenderer>().sprite = curSpeech[curEntity].getSprite();
					duckSprite.GetComponent<SpriteRenderer>().enabled = true;
					speech.enabled = true;
					speech.text = curName + ": " + curSpeech[curEntity].getReadingLine(curSpeech[curEntity].getLine(), textSpd);

					itemList = new List<Item>();
					Hashtable itemHash = TitleManager.curFile.getItemList();
					for (int i = 0; i < ducks[curEntity].itemIds.Length; i++) {
						Item thing = (Item)itemHash[ducks[curEntity].itemIds[i]];
						itemList.Add(thing.copy());
					}
				}
			}

			else if (ducks[curEntity].type == npcType.CHEST) {
				if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
                    if(curLoot >= ducks[curEntity].loot.Length) {
                        curState = gameState.OVERWORLD;
                        textbox.GetComponent<SpriteRenderer>().enabled = false;
                        speech.enabled = false;
                        curEntity = -1;
                        map.SetActive(true);
                        curLoot = 0;
                    } 
                    else {
                        Item theDrop = (Item)TitleManager.curFile.getItemList()[ducks[curEntity].loot[curLoot].itemID];
                        Item drop = theDrop.copy(ducks[curEntity].loot[curLoot].itemAmt);
                        if(drop.getName() == "Coin") {
                            speech.text = "Obtained $" + drop.getAmt().ToString() + "!";
                            TitleManager.curFile.adjustBalance(drop.getAmt());
                        } else {
                            speech.text = "Obatined " + drop.getName() + " x" + drop.getAmt().ToString() + ".";
                            TitleManager.curFile.addToInventory(drop.copy());
                        }

                        // Play sound effect
                        mainSFX.GetComponent<AudioSource>().clip = keyPress;
                        mainSFX.GetComponent<AudioSource>().Play();
                        curLoot += 1;
                    }
				}
			}

			else {
				if (curSpeech[curEntity].isDone()) {
                    mainSFX.GetComponent<AudioSource>().pitch = 1;

                    if(ducks[curEntity].type == npcType.SHOP) {
                        curState = gameState.SHOP;
                        shopText = speech.text;
                        shopExitTxt = ducks[curEntity].shopBye;
                        exitShop = false;
                        curItem = 0;
                        DisplaySprite(shopDisplay,true);
                        itemIcon.GetComponent<SpriteRenderer>().enabled = true;
                        statDisp.GetComponent<SpriteRenderer>().enabled = true;
                        itemName.enabled = true;
                        itemDes.enabled = true;
                        itemAmt.enabled = true;
                        balance.enabled = true;
                        curBal.enabled = true;
                        DisplayText(itemStat,true);
                        DisplayText(itemCost,true);
                        DisplayText(itemDisp,true);

                        // Update display
                        if(itemList[curItem].getMain() == itemType.ARMOR || itemList[curItem].getMain() == itemType.WEAPON ||
                            itemList[curItem].getSub() == itemType.ARMOR || itemList[curItem].getSub() == itemType.WEAPON) {
                            DisplayText(itemStat,true);
                            statDisp.GetComponent<SpriteRenderer>().enabled = true;
                            for(int i = 0; i < itemStat.Length; i++) {
                                int value = itemList[curItem].getStats()[i + 2];
                                if(value > 0)
                                    itemStat[i].text = "+";
                                else
                                    itemStat[i].text = "";
                                itemStat[i].text += value.ToString();
                            }
                        } else {
                            DisplayText(itemStat,false);
                            statDisp.GetComponent<SpriteRenderer>().enabled = false;
                        }

                        itemIcon.GetComponent<SpriteRenderer>().sprite = itemList[curItem].getIcon();
                        itemName.text = itemList[curItem].getName();
                        itemDes.text = itemList[curItem].getDes();
                        itemCost[1].text = "$" + itemList[curItem].getPrice().ToString();
                        UpdateItemDisp(curItem);
                        balance.text = "$" + TitleManager.curFile.getBalance().ToString();

                        List<Item> inventory = TitleManager.curFile.getInventory();
                        if(inventory.Contains(itemList[curItem])) {
                            int index = inventory.IndexOf(itemList[curItem]);
                            Item thing = inventory[index];
                            itemAmt.text = thing.getAmt().ToString();
                        } else {
                            itemAmt.text = "0";
                        }
                    } else if(ducks[curEntity].type == npcType.EVENT_TRIGGER) {
                        preState = gameState.OVERWORLD;
                        curState = gameState.OVERWORLD;
                        stopMoving = true;
                        if(TitleManager.curFile.getFlag(ducks[curEntity].eventID)) {
                            Destroy(mainBGM);
                            SceneManager.LoadScene(ducks[curEntity].scene,LoadSceneMode.Single);
                        } else {
                            TitleManager.curFile.setFlag(ducks[curEntity].eventID);
                            Destroy(mainBGM);
                            SceneManager.LoadScene(ducks[curEntity].eventScene,LoadSceneMode.Single);
                        }
                    } 
                    else if(ducks[curEntity].type == npcType.ENCOUNTER) {
                        // Play encounter music
                        if(blackScreen.GetComponent<SpriteRenderer>().color.a == 0) {
                            mainSFX.GetComponent<AudioSource>().clip = encMusic;
                            mainSFX.GetComponent<AudioSource>().Play();
                        }

                        // Fade to black
                        float theAlpha = blackScreen.GetComponent<SpriteRenderer>().color.a + Time.deltaTime;
                        if(theAlpha > 1) {
                            theAlpha = 1;
                        }
                        // Fade text
                        speech.color = new Color(1 - theAlpha,1 - theAlpha,1 - theAlpha);

                        blackScreen.GetComponent<SpriteRenderer>().color = new Color(0,0,0,theAlpha);
                        if(theAlpha == 1) {
                            TitleManager.curFile.setScene(SceneManager.GetActiveScene().name);
                            TitleManager.curFile.setLocation(player.transform.localPosition.x,player.transform.localPosition.y);

                            string foeSetDataPath = ducks[curEntity].enemySet;
                            TextAsset set = Resources.Load<TextAsset>(@foeSetDataPath);
                            GameProgress.jankFile setTxt = new GameProgress.jankFile(set);
                            readFoes(setTxt);
                            TitleManager.curFile.setEnemySet(foeList[0]);

                            TitleManager.curFile.setBackground(ducks[curEntity].background);
                            TitleManager.curFile.setSound(ducks[curEntity].battleMusic);
                            Destroy(mainBGM);
                            SceneManager.LoadScene("BattleScene",LoadSceneMode.Single);
                        }
                    } 
                    else {
                        curState = gameState.OVERWORLD;
                        curSpeech[curEntity].resetDia();
                        textbox.GetComponent<SpriteRenderer>().enabled = false;
                        duckSprite.GetComponent<SpriteRenderer>().enabled = false;
                        speech.enabled = false;
                        curEntity = -1;
                        map.SetActive(true);
                    }
				}
				else {
					if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
						if (curSpeech[curEntity].getReadingLine(curSpeech[curEntity].getLine(), textSpd) == curSpeech[curEntity].getLine()) {
							curSpeech[curEntity].nextLine();
							if (!curSpeech[curEntity].isDone())
								duckSprite.GetComponent<SpriteRenderer>().sprite = curSpeech[curEntity].getSprite();
						}
						else {
							speech.text = curName + ": " + curSpeech[curEntity].getLine();
							curSpeech[curEntity].endLineRead();
						}
					}
					else {
						speech.text = curName + ": " + curSpeech[curEntity].getReadingLine(curSpeech[curEntity].getLine(), textSpd);
                        // Play talking sound effect
                        if(curSpeech[curEntity].IsNewChar()) {
                            mainSFX.GetComponent<AudioSource>().clip = talking[(int)Random.Range(0,talking.Length - 1)];
                            mainSFX.GetComponent<AudioSource>().pitch = curSpeech[curEntity].getPitch();
                            mainSFX.GetComponent<AudioSource>().Play();
                        }
                    }
				}
			}
		}



		else if (curState == gameState.SHOP) {

			if (!checkout && Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.UP])) {
				curItem = curItem - 1 + itemList.Count;
				curItem %= itemList.Count;
            }
			else if (!checkout && Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.LEFT])) {
				curItem = curItem - (itemDisp.Length / 2) + (itemDisp.Length * itemList.Count);
				curItem %= itemList.Count;
            }
			else if (!checkout && Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.RIGHT])) {
				curItem += itemDisp.Length / 2;
				curItem %= itemList.Count;
            }
			else if (!checkout && Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.DOWN])) {
				curItem += 1;
				curItem %= itemList.Count;
            }
			else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
				if (exitShop) {
					curState = gameState.OVERWORLD;
					curSpeech[curEntity].resetDia();
					textbox.GetComponent<SpriteRenderer>().enabled = false;
					duckSprite.GetComponent<SpriteRenderer>().enabled = false;
					speech.enabled = false;
					curEntity = -1;
                    map.SetActive(true);

                    DisplaySprite(shopDisplay, false);
					itemIcon.GetComponent<SpriteRenderer>().enabled = false;
					statDisp.GetComponent<SpriteRenderer>().enabled = false;
					itemName.enabled = false;
					itemDes.enabled = false;
					itemAmt.enabled = false;
					balance.enabled = false;
					curBal.enabled = false;
					DisplayText(itemStat, false);
					DisplayText(itemCost, false);
					DisplayText(itemDisp, false);
				}
				else if (itemList[curItem].getPrice() > TitleManager.curFile.getBalance()) {
					speech.text = curName + ": Sorry. It doesn't seem like you have enough money for that.";
				}
				else if (!checkout) {
					speech.text = curName + ": How many would you like to purchase? <" + purchaseAmt.ToString() + ">";
					checkout = true;
				}
				else {
					speech.text = curName + ": Thanks for the purchase!";
					TitleManager.curFile.adjustBalance(-1 * purchaseAmt *  itemList[curItem].getPrice());
                    Item theGoods = itemList[curItem].copy(purchaseAmt);
                    TitleManager.curFile.addToInventory(theGoods);
					purchaseAmt = 1;
					//Update balance after each purchase
					balance.text = "$" + TitleManager.curFile.getBalance().ToString();
					checkout = false;
				}
			}
			else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2])) {
				if (exitShop) {
					curState = gameState.OVERWORLD;
					curSpeech[curEntity].resetDia();
					textbox.GetComponent<SpriteRenderer>().enabled = false;
					duckSprite.GetComponent<SpriteRenderer>().enabled = false;
					speech.enabled = false;
					curEntity = -1;
                    map.SetActive(true);

                    DisplaySprite(shopDisplay, false);
					itemIcon.GetComponent<SpriteRenderer>().enabled = false;
					statDisp.GetComponent<SpriteRenderer>().enabled = false;
					itemName.enabled = false;
					itemDes.enabled = false;
					itemAmt.enabled = false;
					balance.enabled = false;
					curBal.enabled = false;
					DisplayText(itemStat, false);
					DisplayText(itemCost, false);
					DisplayText(itemDisp, false);
				}
				else if(checkout){
					checkout = false;
					speech.text = shopText;
				}
				else {
					speech.text = curName + ": " + shopExitTxt;
					exitShop = true;
				}

                // Play sound effect
                mainSFX.GetComponent<AudioSource>().clip = cancel;
                mainSFX.GetComponent<AudioSource>().Play();
            }
			else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.LEFT])) {
				int maxAmt = 1;
				while (maxAmt * itemList[curItem].getPrice() <= TitleManager.curFile.getBalance()) {
					maxAmt += 1;
				}
				purchaseAmt += maxAmt - 1;
				purchaseAmt %= maxAmt;
				if (purchaseAmt == 0)
					purchaseAmt = 1;
				speech.text = curName + ": How many would you like to purchase? <" + purchaseAmt.ToString() + ">";
			}
			else if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.RIGHT])){
				int maxAmt = 1;
				while (maxAmt * itemList[curItem].getPrice() <= TitleManager.curFile.getBalance()) {
					maxAmt += 1;
				}
				purchaseAmt += 1;
				purchaseAmt %= maxAmt;
				if (purchaseAmt == 0)
					purchaseAmt = 1;
				speech.text = curName + ": How many would you like to purchase? <" + purchaseAmt.ToString() + ">";
			}

			// Update current item image
			if (Input.anyKeyDown && !Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2]) && !exitShop) {
				if (!Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1]) && !checkout)
					speech.text = shopText;
				if (itemList[curItem].getMain() == itemType.ARMOR || itemList[curItem].getMain() == itemType.WEAPON ||
					itemList[curItem].getSub() == itemType.ARMOR || itemList[curItem].getSub() == itemType.WEAPON) {
					DisplayText(itemStat, true);
					statDisp.GetComponent<SpriteRenderer>().enabled = true;
					for (int i = 0; i < itemStat.Length; i++) {
						int value = itemList[curItem].getStats()[i + 2];
                        if(value > 0)
                            itemStat[i].text = "+";
                        else
                            itemStat[i].text = "";
                        itemStat[i].text += value.ToString();
					}
				}
				else {
					DisplayText(itemStat, false);
					statDisp.GetComponent<SpriteRenderer>().enabled = false;
				}

				itemIcon.GetComponent<SpriteRenderer>().sprite = itemList[curItem].getIcon();
				itemName.text = itemList[curItem].getName();
				itemDes.text = itemList[curItem].getDes();
				itemCost[1].text = "$" + itemList[curItem].getPrice().ToString();
				UpdateItemDisp(curItem);

				List<Item> inventory = TitleManager.curFile.getInventory();
				if (inventory.Contains(itemList[curItem])) {
					int index = inventory.IndexOf(itemList[curItem]);
					Item thing = inventory[index];
					itemAmt.text = thing.getAmt().ToString();
				}
				else {
					itemAmt.text = "0";
				}

                // Play sound effect
                mainSFX.GetComponent<AudioSource>().clip = keyPress;
                mainSFX.GetComponent<AudioSource>().Play();
            }
		}



		else if (curState == gameState.OVERWORLD || curState == gameState.BATTLE) {
			// Check if pause
			if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B3]) && curState == gameState.OVERWORLD) {
				curState = gameState.MENU;
				TitleManager.curFile.setScene(SceneManager.GetActiveScene().name);
				TitleManager.curFile.setLocation(player.transform.localPosition.x, player.transform.localPosition.y);
				SceneManager.LoadScene("PauseMenu", LoadSceneMode.Single);
			}
			else {
				// Increase the enounter meter
				if (Input.GetKey(DataManager.savedOptions.controls[(int)key.UP]) || Input.GetKey(DataManager.savedOptions.controls[(int)key.LEFT]) ||
				   Input.GetKey(DataManager.savedOptions.controls[(int)key.RIGHT]) || Input.GetKey(DataManager.savedOptions.controls[(int)key.DOWN])) {
					curSteps += 1;
				}

				// Encounter time
				if (curSteps >= encounter && !isTown) {
                    // Play encounter music
                    if(blackScreen.GetComponent<SpriteRenderer>().color.a == 0) {
                        curState = gameState.BATTLE;
                        mainSFX.GetComponent<AudioSource>().clip = encMusic;
                        mainSFX.GetComponent<AudioSource>().Play();
                    }

                    // Fade to black
                    float curAlpha = blackScreen.GetComponent<SpriteRenderer>().color.a + Time.deltaTime;
                    if(curAlpha > 1) {
                        curAlpha = 1;
                    }

                    blackScreen.GetComponent<SpriteRenderer>().color = new Color(0,0,0,curAlpha);
                    if(curAlpha == 1) {
                        // Reset encounter variables
                        curSteps = 0;
                        encounter = Random.Range(50,500);

                        // Decide a set of enemies to make appear out of random
                        int roll = (int)Random.Range(1,101);
                        int tempSum = 0;
                        List<Foe> curSet = new List<Foe>();
                        for(int i = 0; i < encounterOdds.Count; i++) {
                            if(encounterOdds[i] + tempSum >= roll) {
                                curSet.Clear();
                                for(int j = 0; j < foeList[i].Count; j++) {
                                    curSet.Add(foeList[i][j].copy());
                                }
                                break;
                            } else {
                                tempSum += encounterOdds[i];
                            }
                        }

                        // Store the enemies into a global variable for battle handler to read
                        TitleManager.curFile.setEnemySet(curSet);

                        // Change to Battle Mode
                        TitleManager.curFile.setBackground(background);
                        TitleManager.curFile.setScene(SceneManager.GetActiveScene().name);
                        TitleManager.curFile.setLocation(player.transform.localPosition.x,player.transform.localPosition.y);
                        TitleManager.curFile.setSound(battleMusic);
                        Destroy(mainBGM);
                        SceneManager.LoadScene("BattleScene",LoadSceneMode.Single);
                    }
				}
			}
		}
	}

	private void readFoes(GameProgress.jankFile input){
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
		foeList = new List<Foe>[setAmt];
		encounterOdds = new List<int>();
		for (int j = 0; j < setAmt; j++) {
			foeList[j] = new List<Foe>();
			input.ReadLine();
			// Read in rate
			int rate;
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out rate);
			encounterOdds.Add(rate);
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
					foeList[j].Add(community[foeNum]);
				}
				else {
					foeList[j].Add(empty);
				}
			}
		}
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

	private void DisplayText(TextMeshProUGUI[] t, bool disp){
		for(int i = 0; i < t.Length; i++)
			t[i].enabled = disp;
	}

	private void DisplaySprite(GameObject[] g, bool disp){
		for (int i = 0; i < g.Length; i++)
			g[i].GetComponent<SpriteRenderer>().enabled = disp;
	}


	private void UpdateItemDisp(int curItem){
		int section = (curItem / itemDisp.Length) * itemDisp.Length;
		for (int i = 0; i < itemDisp.Length; i++) {
			if (i+section < itemList.Count) 
				itemDisp[i].text = itemList[i+section].getName();		
			else 
				itemDisp[i].text = "----------";

			if (i == curItem % itemDisp.Length)
				itemDisp[i].color = Color.black;
			else
				itemDisp[i].color = Color.white;
		}
	}
}
