using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum direction {UP, LEFT, RIGHT, DOWN, NULL};

public class PlayerManager : MonoBehaviour {

    [System.Serializable]
    public class PlayerDuck {
        public character duck;
        public Sprite[] idle = new Sprite[4];
        public Sprite[] up = new Sprite[4];
        public Sprite[] left = new Sprite[6];
        public Sprite[] right = new Sprite[6];
        public Sprite[] down = new Sprite[4];
        public Sprite[] swimUp = new Sprite[2];
        public Sprite[] swimLeft = new Sprite[2];
        public Sprite[] swimRight = new Sprite[2];
        public Sprite[] swimDown = new Sprite[2];
    }
    public PlayerDuck[] myDucks;
    private PlayerDuck curPlayer;

    public static GameObject curDuck;
    private static bool isLoaded;

    public GameObject mapPlayer;
    public float mapScale, mapShiftX, mapShiftY;

    private direction lastDir;
    private direction curDir;

    // Use this for initialization
    void Start () {
        lastDir = TitleManager.curFile.getDir();
        curDir = direction.NULL;

        curPlayer = myDucks[0];
        for (int i = 0; i < myDucks.Length; i++) {
            if (TitleManager.curFile.getParty()[0].getCharacter() == myDucks[i].duck) {
                curPlayer = myDucks[i];
                break;
            }
        }

        GetComponent<SpriteRenderer>().sprite = curPlayer.idle[((int)lastDir) % curPlayer.idle.Length];
        if(lastDir == direction.LEFT)
            transform.localScale = new Vector2(-1, 1);
    }
    
    // Update is called once per frame
    void Update () {
        // If the gamestate is currently in Overworld mode
        if (DungeonHandler.curState == gameState.OVERWORLD && !DungeonHandler.stopMoving) {
            // Determine distance to move each frame
            float speed = 2.5f * Time.deltaTime;
            // Don't move more than certain distance
            if (speed > 0.15f)
                speed = 0.15f;

            if (Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
                RaycastHit2D detector = new RaycastHit2D();
                // Check up for any surrounding objects
                switch(lastDir){
                    case direction.DOWN:
                        detector = Physics2D.Raycast(transform.localPosition, Vector2.down, 0.15f);
                        break;
                    case direction.UP:
                        detector = Physics2D.Raycast(transform.localPosition, Vector2.up, 0.15f);
                        break;
                    case direction.LEFT:
                        detector = Physics2D.Raycast(transform.localPosition, Vector2.left, 0.3f);
                        break;
                    case direction.RIGHT:
                        detector = Physics2D.Raycast(transform.localPosition, Vector2.right, 0.3f);
                        break;
                }

                if (detector) {
                    if (detector.collider.tag == "NPC") {
                        NPC person = detector.transform.gameObject.GetComponent<NPC>();
                        if (person) {
                            switch(lastDir){
                                case direction.DOWN:
                                    person.faceUp();
                                    break;
                                case direction.UP:
                                    person.faceDown();
                                    break;
                                case direction.LEFT:
                                    person.faceRight();
                                    break;
                                case direction.RIGHT:
                                    person.faceLeft();
                                    break;
                                default:
                                    person.spinDude();
                                    break;
                            }
                        }
                        GetComponent<SpriteRenderer>().sprite = curPlayer.idle[((int)lastDir) % curPlayer.idle.Length];
                        curDir = direction.NULL;
                        curDuck = detector.transform.gameObject;
                        DungeonHandler.curState = gameState.DIALOUGE;
                    }
                }
            }

            if (Input.GetKey(DataManager.savedOptions.controls[(int)key.UP])) {
                curDir = direction.UP;
                transform.localScale = new Vector2(1, 1);
            }
            else if (Input.GetKey(DataManager.savedOptions.controls[(int)key.DOWN])) {
                curDir = direction.DOWN;
                transform.localScale = new Vector2(1, 1);
            }
            else if (Input.GetKey(DataManager.savedOptions.controls[(int)key.LEFT])) {
                curDir = direction.LEFT;
                transform.localScale = new Vector2(-1, 1);
            }
            else if (Input.GetKey(DataManager.savedOptions.controls[(int)key.RIGHT])) {
                curDir = direction.RIGHT;
                transform.localScale = new Vector2(1, 1);
            }

            if(curDir == direction.UP) {
                lastDir = direction.UP;
                TitleManager.curFile.setDirection(lastDir);

                // Allows player to move into the current direction
                bool canWalk = true;
                bool inWater = false;

                // Check up for any surrounding objects
                RaycastHit2D detector = Physics2D.Raycast(transform.localPosition, Vector2.up, 0.15f);
                if (detector) {
                    if (detector.collider.tag == "Wall" || detector.collider.tag == "NPC") {
                        canWalk = false;
                    }

                    if (detector.collider.tag == "water") {
                        inWater = true;
                    }
                        
                    if (detector.collider.tag == "Exit") {
                        curDuck = detector.transform.gameObject;
                        DungeonHandler.curState = gameState.DIALOUGE;
                    }
                }
                    
                // No point in checking left if there's already a wall
                if (canWalk || inWater) {
                    // Check left
                    Vector2 left = new Vector2(-0.3f, 0.15f);
                    RaycastHit2D checkLeft = Physics2D.Raycast(transform.localPosition, left, 0.2f);

                    if (checkLeft) {
                        if (canWalk && (checkLeft.collider.tag == "Wall" || checkLeft.collider.tag == "NPC"))
                            canWalk = false;
                        if (checkLeft.collider.tag != "water")
                            inWater = false;
                            
                    }
                }

                // No point in checking right if there's already a wall
                if (canWalk || inWater) {
                    // Check right
                    Vector2 right = new Vector2(0.3f, 0.15f);
                    RaycastHit2D checkRight = Physics2D.Raycast(transform.localPosition, right, 0.2f);

                    if (checkRight) {
                        if (canWalk && (checkRight.collider.tag == "Wall" || checkRight.collider.tag == "NPC"))
                            canWalk = false;
                        if (checkRight.collider.tag != "water")
                            inWater = false;
                    }
                }
                    
                // Walk if there was nothing detected
                if(canWalk) {
                    transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y + speed);
                }

                // Animate player
                if (inWater)    { UpdateSprite(curPlayer.swimUp, 10); }
                else            { UpdateSprite(curPlayer.up, 10); }
            }
            else if (curDir == direction.LEFT) {
                lastDir = direction.LEFT;
                TitleManager.curFile.setDirection(lastDir);

                bool inWater = false;
                // Check for any surrounding objects
                RaycastHit2D detector = Physics2D.Raycast(transform.localPosition, Vector2.left, 0.3f);
                if (detector) {
                    // Keep walking if there's no wall
                    if (detector.collider.tag != "Wall" && detector.collider.tag != "NPC") {
                        transform.localPosition = new Vector2(transform.localPosition.x - speed, transform.localPosition.y);
                    }
                    if (detector.collider.tag == "water") {
                        inWater = true;
                    }

                    if (detector.collider.tag == "Exit") {
                        curDuck = detector.transform.gameObject;
                        DungeonHandler.curState = gameState.DIALOUGE;
                    }
                }
                // Walk if there was nothing detected
                else {
                    transform.localPosition = new Vector2(transform.localPosition.x - speed, transform.localPosition.y);
                }

                // Animate player
                if (inWater)    { UpdateSprite(curPlayer.swimLeft, 10); }
                else            { UpdateSprite(curPlayer.left, 10); }
            }
            else if (curDir == direction.RIGHT) {
                lastDir = direction.RIGHT;
                TitleManager.curFile.setDirection(lastDir);

                bool inWater = false;
                // Check for any surrounding objects
                RaycastHit2D detector = Physics2D.Raycast(transform.localPosition, Vector2.right, 0.3f);
                if (detector) {
                    // Keep walking if there's no wall
                    if (detector.collider.tag != "Wall" && detector.collider.tag != "NPC") {
                        transform.localPosition = new Vector2(transform.localPosition.x + speed, transform.localPosition.y);
                    }
                    if (detector.collider.tag == "water") {
                        inWater = true;
                    }

                    if (detector.collider.tag == "Exit") {
                        curDuck = detector.transform.gameObject;
                        DungeonHandler.curState = gameState.DIALOUGE;
                    }
                }
                // Walk if there was nothing detected
                else {
                    transform.localPosition = new Vector2(transform.localPosition.x + speed, transform.localPosition.y);
                }

                // Animate player
                if (inWater)    { UpdateSprite(curPlayer.swimRight, 10); }
                else            { UpdateSprite(curPlayer.right, 10); }
            }
            else if (curDir == direction.DOWN) {
                lastDir = direction.DOWN;
                TitleManager.curFile.setDirection(lastDir);

                // Allows player to move into the current direction
                bool canWalk = true; 
                bool inWater = false;

                // Check up for any surrounding objects
                RaycastHit2D detector = Physics2D.Raycast(transform.localPosition, Vector2.down, 0.15f);
                if (detector) {
                    if (detector.collider.tag == "Wall" || detector.collider.tag == "NPC") 
                        canWalk = false;
                    if (detector.collider.tag == "water")
                        inWater = true;

                    if (detector.collider.tag == "Exit") {
                        curDuck = detector.transform.gameObject;
                        DungeonHandler.curState = gameState.DIALOUGE;
                    }
                }

                // No point in checking left if there's already a wall
                if (canWalk || inWater) {
                    // Check left
                    Vector2 left = new Vector2(-0.3f, -0.15f);
                    RaycastHit2D checkLeft = Physics2D.Raycast(transform.localPosition, left, 0.2f);

                    if (checkLeft) {
                        if (canWalk && (checkLeft.collider.tag == "Wall" || checkLeft.collider.tag == "NPC"))
                            canWalk = false;
                        if (checkLeft.collider.tag != "water")
                            inWater = false;
                    }
                }

                // No point in checking right if there's already a wall
                if (canWalk || inWater) {
                    // Check right
                    Vector2 right = new Vector2(0.3f, -0.15f);
                    RaycastHit2D checkRight = Physics2D.Raycast(transform.localPosition, right, 0.2f);

                    if (checkRight) {
                        if (canWalk && (checkRight.collider.tag == "Wall" || checkRight.collider.tag == "NPC"))
                            canWalk = false;
                        if (checkRight.collider.tag != "water")
                            inWater = false;
                    }
                }

                // Walk if there was nothing detected
                if(canWalk) {
                    transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y - speed);
                }
                    
                // Animate player
                if (inWater)    { UpdateSprite(curPlayer.swimDown, 10); }
                else            { UpdateSprite(curPlayer.down, 10); }
            }

            if (Input.GetKeyUp(DataManager.savedOptions.controls[(int)key.UP]) && curDir == direction.UP) {
                curDir = direction.NULL;
            
                bool inWater = false;
                RaycastHit2D detector = Physics2D.Raycast(transform.localPosition, Vector2.up, 0.15f);
                if (detector) {
                    if (detector.collider.tag == "water")
                        inWater = true;
                }

                // Animate player
                if (inWater)    { UpdateSprite(curPlayer.swimUp, 10); }
                else            { GetComponent<SpriteRenderer>().sprite = curPlayer.idle[0]; }
            }
            else if (Input.GetKeyUp(DataManager.savedOptions.controls[(int)key.LEFT]) && curDir == direction.LEFT) {
                curDir = direction.NULL;

                bool inWater = false;
                RaycastHit2D detector = Physics2D.Raycast(transform.localPosition, Vector2.left, 0.3f);
                if (detector) {
                    if (detector.collider.tag == "water")
                        inWater = true;
                }

                // Animate player
                if (inWater)    { UpdateSprite(curPlayer.swimLeft, 10); }
                else            { GetComponent<SpriteRenderer>().sprite = curPlayer.idle[1]; }
            }
            else if (Input.GetKeyUp(DataManager.savedOptions.controls[(int)key.RIGHT]) && curDir == direction.RIGHT) {
                curDir = direction.NULL;

                bool inWater = false;
                RaycastHit2D detector = Physics2D.Raycast(transform.localPosition, Vector2.right, 0.3f);
                if (detector) {
                    if (detector.collider.tag == "water")
                        inWater = true;
                }

                // Animate player
                if (inWater)    { UpdateSprite(curPlayer.swimRight, 10); }
                else            { GetComponent<SpriteRenderer>().sprite = curPlayer.idle[2]; }
            }
            else if (Input.GetKeyUp(DataManager.savedOptions.controls[(int)key.DOWN]) && curDir == direction.DOWN) {
                curDir = direction.NULL;

                bool inWater = false;
                RaycastHit2D detector = Physics2D.Raycast(transform.localPosition, Vector2.down, 0.15f);
                if (detector) {
                    if (detector.collider.tag == "water")
                        inWater = true;
                }

                // Animate player
                if (inWater)    { UpdateSprite(curPlayer.swimDown, 10); }
                else            { GetComponent<SpriteRenderer>().sprite = curPlayer.idle[3]; }
            }
        }

        mapPlayer.transform.localPosition = new Vector2((transform.localPosition.x - mapShiftX) * mapScale,(transform.localPosition.y - mapShiftY) * mapScale);
    }

    void UpdateSprite(Sprite[] animation, float fps) {
        int index = (int)(Time.timeSinceLevelLoad * fps);
        index %= animation.Length;
        GetComponent<SpriteRenderer> ().sprite = animation[index];
        return;
    }
}
