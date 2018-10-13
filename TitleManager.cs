using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour {

    // Public display objects
    public Sprite[] menu = new Sprite[2]; // Allows for change in size of menu
    public GameObject menuObj; // Object that will present the current menu
    public Text[] main = new Text[4]; // Displays main options
    public Text[] options = new Text[3]; // Displays settings
    public Text[] controlTxt = new Text[8]; // Displays controls
    public GameObject controlDisplay; // Object that will present the controls
    public Text[] resolutionTxt = new Text[5]; // Displays resolutions
    public Text[] sounds = new Text[4]; // Displays sounds
    public Text[] soundNums = new Text[3]; // Displays sound numbers
    public GameObject[] sliders = new GameObject[6]; // Displays sliders
    public GameObject instructions; // Displays controls to newer players
    public Text[] saveFiles = new Text[6]; // Displays save data
    public GameObject[] saveSlots = new GameObject[3]; // Displays save slots
    public Text[] fileOpts = new Text[3]; // Displays file options
    public Text[] deletePrompt = new Text[3]; // Displays prompt to delete save
    public static int curSave = 1; // Stores the current save file
    public static GameProgress curFile = DataManager.file1; // Points to loaded file(set as file1 by default)

    // Menu Navigation
    private enum menus {INSTRUCTIONS, MAIN, FILESELECT, FILEOPT, DELETE, VSFILE, OPTIONS, CONTROLS, INPUT, RESOLUTION, SOUNDS}; // Label each menu
    private int curOpt, maxOpt, curMenu; // Used to determine current menu and selection
    // Volume Adjustment
    private int[] newVolume = new int[3];
    private int sliderDelay = 0; // Used to delay fast slider movement
    // Input Adjustment
    private KeyCode[] tempControls = new KeyCode[7]; // Used to remember the previous controls
    // Resolution Adjustment
    private int newResolution;

    public GameObject mainSFX;
    public AudioClip encMusic, select, cancel;
    private bool startGame;
    public GameObject blackScreen;

    // Called when the script is opened
    void Awake() {
        Application.targetFrameRate = 60;
        DataManager.LoadOptions(); // Load options
        DataManager.Load(); // Load save files
    }

    // Use this for initialization
    void Start () {
        startGame = false;
        sliderDelay = 0; // Delay not being used at this point
        menuObj.GetComponent<SpriteRenderer> ().sprite = menu[1]; // Start with smaller menu
        UpdateRes(DataManager.savedOptions.resolution); // Apply saved resolution

        // If there's been save progress, don't show the instructions
        if (DataManager.file1.getTime() > 0.0f || DataManager.file2.getTime() > 0.0f || DataManager.file3.getTime() > 0.0f) {
            curMenu = (int)menus.MAIN;
            curOpt = 0;
            maxOpt = 4;
            UpdateSelection(main);
            TextDisplay(main, true);
            instructions.GetComponent<SpriteRenderer> ().enabled = false; // Hide instructions
        }
        else {
            curOpt = 0;
            maxOpt = 1;
            TextDisplay(main, false);
            instructions.GetComponent<SpriteRenderer> ().enabled = true; // Show instructions
        }

        // Hide everything else
        TextDisplay(options, false);
        TextDisplay(soundNums, false);
        TextDisplay(saveFiles, false);
        TextDisplay(controlTxt, false);
        TextDisplay(resolutionTxt, false);
        TextDisplay(sounds, false);
        TextDisplay(fileOpts, false);
        TextDisplay(deletePrompt, false);
        ImageDisplay(saveSlots, false);
        ImageDisplay(sliders, false);
        controlDisplay.GetComponent<SpriteRenderer> ().enabled = false;

        // Update from saved data
        for (int i = 0; i < 3; i++) {
            newVolume[i] = DataManager.savedOptions.volume[i]; // Initialize a new volume variable
            soundNums[i].text = DataManager.savedOptions.volume[i].ToString(); // Update volume amount
            sliders[i+3].transform.localPosition = new Vector2((DataManager.savedOptions.volume[i] * 0.019f) - 0.95f, 0);
        }
        for (int i = 0; i < 7; i++) {
            tempControls[i] = DataManager.savedOptions.controls[i]; // Update controls
            controlTxt[i].GetComponent<Text>().text = DataManager.savedOptions.controls[i].ToString();
        }
        newResolution = DataManager.savedOptions.resolution; // Update resolution
        saveFiles[0].GetComponent<Text> ().text = "File 1"; // Update save files
        saveFiles[3].GetComponent<Text> ().text = ToTime(DataManager.file1.getTime());
        saveFiles[4].GetComponent<Text> ().text = ToTime(DataManager.file2.getTime());
        saveFiles[5].GetComponent<Text> ().text = ToTime(DataManager.file3.getTime());
    }
        
    // Update is called once per frame
    void Update () {
        if(startGame) {
            if(blackScreen.GetComponent<SpriteRenderer>().color.a == 0 && curFile.getTime() <= 0) {
                // Play skill sound effect
                mainSFX.GetComponent<AudioSource>().clip = encMusic;
                mainSFX.GetComponent<AudioSource>().Play();
            }

            // Fade to black
            float curAlpha = blackScreen.GetComponent<SpriteRenderer>().color.a + Time.deltaTime;
            if(curAlpha > 1) {
                curAlpha = 1;
            }

            saveFiles[0].GetComponent<Text>().color = new Color(1 - curAlpha,1 - curAlpha,1 - curAlpha);
            saveFiles[3].GetComponent<Text>().color = new Color(1 - curAlpha,1 - curAlpha,1 - curAlpha);
            fileOpts[1].color = new Color(1 - curAlpha,1 - curAlpha,1 - curAlpha);
            fileOpts[2].color = new Color(1 - curAlpha,1 - curAlpha,1 - curAlpha);

            blackScreen.GetComponent<SpriteRenderer>().color = new Color(0,0,0,curAlpha);
            if(curAlpha == 1) {
                DungeonHandler.curState = gameState.OVERWORLD;
                DungeonHandler.preState = gameState.MENU;
                SceneManager.LoadScene(curFile.getScene(),LoadSceneMode.Single);
            }
        } else {

            if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.DOWN])) {
                if(curMenu != (int)menus.INPUT) { // Don't move selection if changing input
                    curOpt = (curOpt + 1) % maxOpt;
                }

                // Play sound effect
                mainSFX.GetComponent<AudioSource>().clip = select;
                mainSFX.GetComponent<AudioSource>().Play();
            } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.UP])) {
                if(curMenu != (int)menus.INPUT) { // Don't move selection if changing input
                    curOpt = (curOpt - 1 + maxOpt) % maxOpt;
                }

                // Play sound effect
                mainSFX.GetComponent<AudioSource>().clip = select;
                mainSFX.GetComponent<AudioSource>().Play();
            } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
                switch(curMenu) {
                    case (int)menus.INSTRUCTIONS:
                        curMenu = (int)menus.MAIN;
                        curOpt = 0;
                        maxOpt = 4;

                        // Change displays
                        instructions.GetComponent<SpriteRenderer>().enabled = false;
                        TextDisplay(main,true);
                        break;
                    case (int)menus.MAIN:
                        // START GAME
                        if(curOpt == 0) {
                            // Update menu
                            curMenu = (int)menus.FILESELECT;
                            menuObj.GetComponent<SpriteRenderer>().sprite = menu[0]; // Use bigger menu display 
                            maxOpt = 3;

                            // Change displays
                            TextDisplay(main,false);
                            TextDisplay(saveFiles,true);
                            ImageDisplay(saveSlots,true);
                        }
                        // VS MODE
                        else if(curOpt == 1) {
                            curMenu = (int)menus.VSFILE;
                            menuObj.GetComponent<SpriteRenderer>().sprite = menu[0]; // Use bigger menu display 
                            maxOpt = 3;
                            curOpt = 0;

                            // Change displays
                            TextDisplay(main,false);
                            TextDisplay(saveFiles,true);
                            ImageDisplay(saveSlots,true);
                        }
                        // OPTIONS
                        else if(curOpt == 2) {
                            // Update menu
                            curMenu = (int)menus.OPTIONS;
                            curOpt = 0; // Reset the current option back to the top
                            maxOpt = 3;

                            // Change displays
                            TextDisplay(main,false);
                            TextDisplay(options,true);
                        }
                        // EXIT GAME
                        else {
                            Application.Quit();
                        }
                        break;
                    case (int)menus.FILESELECT:
                        // Bring selected file to the top
                        if(curOpt == 1) {
                            curSave = 2;
                            saveFiles[0].GetComponent<Text>().text = "File 2";
                            saveFiles[3].GetComponent<Text>().text = ToTime(DataManager.file2.getTime());
                        } else if(curOpt == 2) {
                            curSave = 3;
                            saveFiles[0].GetComponent<Text>().text = "File 3";
                            saveFiles[3].GetComponent<Text>().text = ToTime(DataManager.file3.getTime());
                        } else {
                            curSave = 1;
                        }

                        // Update menu
                        curMenu = (int)menus.FILEOPT;
                        curOpt = 0; // Reset the current option back to the top
                        maxOpt = 3;

                        // Change displays
                        TextDisplay(saveFiles,false);
                        ImageDisplay(saveSlots,false);
                        saveFiles[0].GetComponent<Text>().enabled = true;
                        saveFiles[0].GetComponent<Text>().color = Color.white;
                        saveFiles[3].GetComponent<Text>().enabled = true;
                        saveSlots[0].GetComponent<SpriteRenderer>().enabled = true;
                        TextDisplay(fileOpts,true);
                        break;
                    case (int)menus.FILEOPT:
                        // START GAME
                        if(curOpt == 0) {
                            // Load location based on save
                            if(curSave == 1) {
                                curFile = DataManager.file1;
                            } else if(curSave == 2) {
                                curFile = DataManager.file2;
                            } else {
                                curFile = DataManager.file3;
                            }

                            if(curFile.getTime() <= 0) {
                                curFile = new GameProgress();
                                curFile.makeFile();
                            } else {
                                curFile.loadFile();
                            }
                            startGame = true;
                        }
                        // DELETE SAVE
                        else if(curOpt == 2) {
                            // Update menu
                            curMenu = (int)menus.DELETE;
                            curOpt = 0; // Reset the current option back to the top
                            maxOpt = 2;

                            // Update display
                            TextDisplay(fileOpts,false);
                            TextDisplay(deletePrompt,true);
                        }
                        break;
                    case (int)menus.DELETE:
                        // YES
                        if(curOpt == 0) {
                            // Delete file 1
                            if(curSave == 1) {
                                DataManager.file1 = new GameProgress();
                                saveFiles[3].GetComponent<Text>().text = ToTime(DataManager.file1.getTime());
                                DataManager.Save(1);
                            }
                            // Delete file 2
                            else if(curSave == 2) {
                                DataManager.file2 = new GameProgress();
                                saveFiles[3].GetComponent<Text>().text = ToTime(DataManager.file2.getTime());
                                saveFiles[4].GetComponent<Text>().text = ToTime(DataManager.file2.getTime());
                                DataManager.Save(2);
                            }
                            // Delete file 3
                            else {
                                DataManager.file3 = new GameProgress();
                                saveFiles[3].GetComponent<Text>().text = ToTime(DataManager.file3.getTime());
                                saveFiles[5].GetComponent<Text>().text = ToTime(DataManager.file3.getTime());
                                DataManager.Save(3);
                            }
                        }

                        // Go back to files anyways
                        curMenu = (int)menus.FILEOPT;
                        curOpt = 2; // Selection on delete save
                        maxOpt = 3;

                        // Change display
                        TextDisplay(deletePrompt,false);
                        TextDisplay(fileOpts,true);
                        break;
                    case (int)menus.VSFILE:
                        // Load save file
                        if(curOpt == 0) { curFile = DataManager.file1; } else if(curOpt == 1) { curFile = DataManager.file2; } else { curFile = DataManager.file3; }

                        if(curFile.getTime() <= 0) {
                            curFile = new GameProgress();
                            curFile.makeFile();
                            SceneManager.LoadScene("VSDemo",LoadSceneMode.Single);
                        } else {
                            curFile.loadFile();
                            if(curFile.getFlag(-30)) {
                                SceneManager.LoadScene("VSTrainingMode",LoadSceneMode.Single);
                            } else {
                                curFile.setFlag(-30);
                                // Save file 1
                                if(curOpt == 0) {
                                    DataManager.file1 = TitleManager.curFile;
                                    DataManager.Save(1);
                                }
                                // Save file 2
                                else if(curOpt == 1) {
                                    DataManager.file2 = TitleManager.curFile;
                                    DataManager.Save(2);
                                }
                                // Save file 3
                                else {
                                    DataManager.file3 = TitleManager.curFile;
                                    DataManager.Save(3);
                                }

                                SceneManager.LoadScene("VSDemo",LoadSceneMode.Single);
                            }
                        }
                        break;
                    case (int)menus.OPTIONS:
                        // Hide options menu
                        TextDisplay(options,false);
                        menuObj.GetComponent<SpriteRenderer>().sprite = menu[0]; // Use bigger menu display 

                        // CONTROLS
                        if(curOpt == 0) {
                            // Update menu
                            curMenu = (int)menus.CONTROLS;
                            maxOpt = 8;

                            // Show controls
                            TextDisplay(controlTxt,true);
                            controlDisplay.GetComponent<SpriteRenderer>().enabled = true;
                        }
                        // RESOLUTION
                        else if(curOpt == 1) {
                            // Update menu
                            curMenu = (int)menus.RESOLUTION;
                            maxOpt = 5;

                            // Show resolution
                            TextDisplay(resolutionTxt,true);
                        }
                        // SOUNDS
                        else if(curOpt == 2) {
                            // Update menu
                            curMenu = (int)menus.SOUNDS;
                            maxOpt = 4;

                            // Show sounds
                            TextDisplay(sounds,true);
                            TextDisplay(soundNums,true);
                            ImageDisplay(sliders,true);
                        }

                        // Reset the current option back to the top
                        curOpt = 0;
                        break;
                    case (int)menus.CONTROLS:
                        // APPLY
                        if(curOpt == 7) {
                            // Update menu
                            curMenu = (int)menus.OPTIONS;
                            menuObj.GetComponent<SpriteRenderer>().sprite = menu[1]; // Use smaller menu display 
                            curOpt = 0; // Selection on controls
                            maxOpt = 3;

                            // Change displays
                            TextDisplay(options,true);
                            TextDisplay(controlTxt,false);
                            controlDisplay.GetComponent<SpriteRenderer>().enabled = false;

                            // Update controls
                            for(int i = 0; i < 7; i++) {
                                tempControls[i] = DataManager.savedOptions.controls[i];
                            }
                            DataManager.SaveOptions(); // Save updates
                        } else {
                            // Update menu
                            curMenu = (int)menus.INPUT;
                            controlTxt[curOpt].color = Color.red; // Indicates "awaiting input"
                        }
                        break;
                    case (int)menus.RESOLUTION:
                        if(curOpt < 4) {
                            UpdateRes(curOpt); // Change to selected resolution
                            newResolution = curOpt; // Keep track of changed resolution
                        }
                        // APPLY
                        else {
                            // Update menu
                            curMenu = (int)menus.OPTIONS;
                            menuObj.GetComponent<SpriteRenderer>().sprite = menu[1]; // Use smaller menu display 
                            curOpt = 1; // Selection on resolution
                            maxOpt = 3;

                            // Change displays
                            TextDisplay(options,true);
                            TextDisplay(resolutionTxt,false);

                            // Save new resolution
                            DataManager.savedOptions.resolution = newResolution;
                            DataManager.SaveOptions();
                        }
                        break;
                    case (int)menus.SOUNDS:
                        // APPLY
                        if(curOpt == 3) {
                            // Update menu
                            curMenu = (int)menus.OPTIONS;
                            menuObj.GetComponent<SpriteRenderer>().sprite = menu[1]; // Use smaller menu display 
                            curOpt = 2; // Selection on sounds
                            maxOpt = 3;

                            // Change displays
                            TextDisplay(options,true);
                            TextDisplay(sounds,false);
                            TextDisplay(soundNums,false);
                            ImageDisplay(sliders,false);

                            // Save the changes made to the volume
                            for(int i = 0; i < 3; i++) {
                                DataManager.savedOptions.volume[i] = newVolume[i];
                            }
                            DataManager.SaveOptions();

                            // Update volumes
                            this.GetComponent<SoundManager>().UpdateVolumes();
                        }
                        break;
                    default:
                        break;
                }

                // Play sound effect
                mainSFX.GetComponent<AudioSource>().clip = select;
                mainSFX.GetComponent<AudioSource>().Play();
            } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2])) {
                switch(curMenu) {
                    case (int)menus.FILESELECT:
                        // Update menu
                        curMenu = (int)menus.MAIN;
                        menuObj.GetComponent<SpriteRenderer>().sprite = menu[1]; // Use smaller menu display 
                        curOpt = 0; // Selection on game start
                        maxOpt = 4;

                        // Change displays
                        TextDisplay(main,true);
                        TextDisplay(saveFiles,false);
                        ImageDisplay(saveSlots,false);
                        break;
                    case (int)menus.FILEOPT: // go to file select
                                             // Update menu
                        curMenu = (int)menus.FILESELECT;
                        // Determine which file was selected and place selection on it
                        if(curSave == 1)
                            curOpt = 0;
                        else if(curSave == 2)
                            curOpt = 1;
                        else
                            curOpt = 2;
                        maxOpt = 3;

                        // Revert changes made to the first slot
                        saveFiles[0].GetComponent<Text>().text = "File 1";
                        saveFiles[3].GetComponent<Text>().text = ToTime(DataManager.file1.getTime());

                        // Change displays
                        TextDisplay(fileOpts,false);
                        TextDisplay(saveFiles,true);
                        ImageDisplay(saveSlots,true);
                        break;
                    case (int)menus.DELETE:
                        // Update menu
                        curMenu = (int)menus.FILEOPT;
                        curOpt = 2; // Selection on delete save
                        maxOpt = 3;

                        // Change display
                        TextDisplay(deletePrompt,false);
                        TextDisplay(fileOpts,true);
                        break;
                    case (int)menus.VSFILE:
                        // Update menu
                        curMenu = (int)menus.MAIN;
                        menuObj.GetComponent<SpriteRenderer>().sprite = menu[1]; // Use smaller menu display 
                        curOpt = 1; // Selection on game start
                        maxOpt = 4;

                        // Change displays
                        TextDisplay(main,true);
                        TextDisplay(saveFiles,false);
                        ImageDisplay(saveSlots,false);
                        break;
                    case (int)menus.OPTIONS:
                        // Update menu
                        curMenu = (int)menus.MAIN;
                        curOpt = 2; // Selection on options
                        maxOpt = 4;

                        // Change displays
                        TextDisplay(main,true);
                        TextDisplay(options,false);
                        break;
                    case (int)menus.CONTROLS:
                        // Update menu
                        curMenu = (int)menus.OPTIONS;
                        menuObj.GetComponent<SpriteRenderer>().sprite = menu[1]; // Use smaller menu display 
                        curOpt = 0; // Selection on controls
                        maxOpt = 3;

                        // Change displays
                        TextDisplay(options,true);
                        TextDisplay(controlTxt,false);
                        controlDisplay.GetComponent<SpriteRenderer>().enabled = false;

                        // Revert to previous controls
                        for(int i = 0; i < 7; i++) {
                            DataManager.savedOptions.controls[i] = tempControls[i];
                            controlTxt[i].GetComponent<Text>().text = DataManager.savedOptions.controls[i].ToString();
                        }
                        break;
                    case (int)menus.RESOLUTION:
                        // Update menu
                        curMenu = (int)menus.OPTIONS;
                        menuObj.GetComponent<SpriteRenderer>().sprite = menu[1]; // Use smaller menu display
                        curOpt = 1; // Selection on resolution
                        maxOpt = 3;

                        // Change displays
                        TextDisplay(options,true);
                        TextDisplay(resolutionTxt,false);

                        // Revert to old resolution
                        UpdateRes(DataManager.savedOptions.resolution);
                        newResolution = DataManager.savedOptions.resolution; // There is no new resolution
                        break;
                    case (int)menus.SOUNDS:
                        // Update menu
                        curMenu = (int)menus.OPTIONS;
                        curOpt = 2; // Selection on sounds
                        maxOpt = 3;
                        menuObj.GetComponent<SpriteRenderer>().sprite = menu[1]; // Use smaller menu display 

                        // Change displays
                        TextDisplay(options,true);
                        TextDisplay(sounds,false);
                        TextDisplay(soundNums,false);
                        ImageDisplay(sliders,false);

                        // Reset volume to previous settings
                        for(int i = 0; i < 3; i++) {
                            soundNums[i].text = DataManager.savedOptions.volume[i].ToString();
                            newVolume[i] = DataManager.savedOptions.volume[i]; // There is no new volume
                            sliders[i + 3].transform.localPosition = new Vector2((DataManager.savedOptions.volume[i] * 0.019f) - 0.95f,0);
                        }
                        break;
                    default:
                        break;
                }

                // Play sound effect
                mainSFX.GetComponent<AudioSource>().clip = cancel;
                mainSFX.GetComponent<AudioSource>().Play();
            }
              // Horizonatal input only needed for sound menu
              else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.LEFT]) || (Input.GetKey(DataManager.savedOptions.controls[(int)key.LEFT]) && sliderDelay > 15)) {
                if(curMenu == (int)menus.SOUNDS && curOpt < 4 && newVolume[curOpt] > 0) {
                    newVolume[curOpt] -= 1; // Decrease volume amount
                    soundNums[curOpt].text = newVolume[curOpt].ToString(); // Update volume amount
                    sliders[curOpt + 3].transform.localPosition = new Vector2(sliders[curOpt + 3].transform.localPosition.x - 0.019f,0);
                }
                sliderDelay += 1;

                // Play sound effect
                if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.LEFT])) {
                    mainSFX.GetComponent<AudioSource>().clip = select;
                    mainSFX.GetComponent<AudioSource>().Play();
                }
            } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.RIGHT]) || (Input.GetKey(DataManager.savedOptions.controls[(int)key.RIGHT]) && sliderDelay > 15)) {
                if(curMenu == (int)menus.SOUNDS && curOpt < 4 && newVolume[curOpt] < 100) {
                    newVolume[curOpt] += 1; // Increase volume amount
                    soundNums[curOpt].text = newVolume[curOpt].ToString(); // Update volume amount
                    sliders[curOpt + 3].transform.localPosition = new Vector2(sliders[curOpt + 3].transform.localPosition.x + 0.019f,0);
                }
                sliderDelay += 1;

                // Play sound effect
                if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.RIGHT])) {
                    mainSFX.GetComponent<AudioSource>().clip = select;
                    mainSFX.GetComponent<AudioSource>().Play();
                }
            }

            // Begin the slider delay process if left or right were held down
            if(sliderDelay > 0 && sliderDelay <= 15) {
                sliderDelay += 1;
            }
            // Reset slider delay if there is no horizontal input
            if(!Input.GetKey(DataManager.savedOptions.controls[(int)key.LEFT])) {
                if(!Input.GetKey(DataManager.savedOptions.controls[(int)key.RIGHT])) {
                    sliderDelay = 0;
                }
            }

            // Update display on current menu if a key was pressed
            if(Input.anyKeyDown) {
                switch(curMenu) {
                    case (int)menus.MAIN:
                        UpdateSelection(main);
                        break;
                    case (int)menus.FILESELECT:
                        UpdateSelection(saveFiles);
                        break;
                    case (int)menus.FILEOPT:
                        UpdateSelection(fileOpts);
                        break;
                    case (int)menus.DELETE:
                        UpdateSelection(deletePrompt);
                        break;
                    case (int)menus.VSFILE:
                        UpdateSelection(saveFiles);
                        break;
                    case (int)menus.OPTIONS:
                        UpdateSelection(options);
                        break;
                    case (int)menus.CONTROLS:
                        UpdateSelection(controlTxt);
                        // Always change text in case inputs were reset
                        for(int i = 0; i < 7; i++) {
                            controlTxt[i].GetComponent<Text>().text = DataManager.savedOptions.controls[i].ToString();
                        }
                        break;
                    case (int)menus.INPUT:
                        // Make sure this input doesn't already exist
                        bool newKey = true;
                        KeyCode curInput = DetermineKey();
                        for(int i = 0; i < 7; i++) {
                            // Must be unique key and not escape since it's used to reset inputs
                            if(curInput == DataManager.savedOptions.controls[i] || curInput == KeyCode.None || curInput == KeyCode.Escape) {
                                newKey = false;
                                break;
                            }
                        }
                        // If it is a new key
                        if(newKey) {
                            curMenu = (int)menus.CONTROLS;
                            DataManager.savedOptions.controls[curOpt] = curInput;
                            controlTxt[curOpt].GetComponent<Text>().text = curInput.ToString();
                            controlTxt[curOpt].color = Color.black;
                        }
                        break;
                    case (int)menus.RESOLUTION:
                        UpdateSelection(resolutionTxt);
                        if(newResolution != curOpt) {
                            resolutionTxt[newResolution].color = Color.blue;
                        }
                        break;
                    case (int)menus.SOUNDS:
                        UpdateSelection(sounds);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    // Updates the display on the current selection
    void UpdateSelection(Text[] t){
        for (int i = 0; i < maxOpt; i++) {
            if (i == curOpt) {
                t[i].color = Color.black;
            } else {
                t[i].color = Color.white;
            }
        }
    }

    // Changes display of an array of text
    void TextDisplay(Text[] t, bool display){
        for (int i = 0; i < t.Length; i++) {
            t[i].GetComponent<Text>().enabled = display;
        }
    }

    // Changes display of an array of gameObjects
    void ImageDisplay(GameObject[] g, bool display){
        for (int i = 0; i < g.Length; i++) {
            g[i].GetComponent<SpriteRenderer>().enabled = display;
        }
    }

    // Looks through all of the possible KeyCodes and determines if that's the one being pressed
    KeyCode DetermineKey() {
        int maxKey = System.Enum.GetNames(typeof(KeyCode)).Length;
        for (int i = 0; i < maxKey; i++) {
            if (Input.GetKey((KeyCode)i)) {
                return (KeyCode)i;
            }
        }
        return KeyCode.None;
    }

    // Changes the resolution according to a dedicated value
    void UpdateRes(int res){
        switch (res) {
            case 0:
                Screen.SetResolution(1600, 960, true);
                break;
            case 1:
                Screen.SetResolution(400, 240, false);
                break;
            case 2:
                Screen.SetResolution(800, 480, false);
                break;
            case 3:
                Screen.SetResolution(1600, 960, false);
                break;
            default:
                break;
        }
    }

    // Returns a formatted timer based on a given float
    public string ToTime(float timer){
        string theTime = "";
        int hours = ((int)timer) / 3600;
        int minutes = (((int)timer) % 3600) / 60;
        int seconds = (((int)timer) % 3600) % 60;

        // Hours
        if (hours < 10)
            theTime = "0" + hours.ToString();
        else
            theTime = hours.ToString();
        // Minutes
        if (minutes < 10)
            theTime += ":0" + minutes.ToString();
        else
            theTime += ":" + minutes.ToString();
        // Seconds
        if (seconds < 10)
            theTime += ":0" + seconds.ToString();
        else
            theTime += ":" + seconds.ToString();

        return theTime;
    }
}
