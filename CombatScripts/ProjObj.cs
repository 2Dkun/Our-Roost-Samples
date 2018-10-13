using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjObj : MonoBehaviour {

    // Position variables
    public float xShift, yShift;
    public float xVel, yVel;
    public HitBox[] hb;
    // Damage variables
    public int basePow, hitstun;
    public float xPow, yPow;
    // Animation variables
    public Sprite[] projAnim;
    public float fps;
    
    // Update is called once per frame
    void Update () {
        transform.Translate(new Vector2(xVel * getDir() * Time.deltaTime, yVel * Time.deltaTime));
        UpdateSprite(fps);
    }

    // Return 1 for left, -1 for right
    private int getDir(){
        return (int) this.transform.localScale.x;
    }

    // Check if the hitboxes of the attack has made contact with a specified target
    public bool checkHit (GameObject target, HitBox[] hurtboxes){
        float targetX = target.transform.localPosition.x;
        float targetY = target.transform.localPosition.y;
        int targetFlip = (int)target.transform.localScale.x;

        for (int i = 0; i < this.hb.Length; i++) {
            this.hb[i].shiftBox(this.transform.position.x, this.transform.position.y);
            this.hb[i].flipBox(getDir());

            for (int j = 0; j < hurtboxes.Length; j++) {
                hurtboxes[j].shiftBox(targetX, targetY);
                hurtboxes[j].flipBox(targetFlip);
                hurtboxes[j].flipBox(getDir());

                if (this.hb[i].checkHit(hurtboxes[j])) {
                    return true;
                }
            }
        }

        return false;
    }

    // Update sprite of projectile object
    private void UpdateSprite(float fps) {
        int index = (int)(Time.timeSinceLevelLoad * fps);
        index %= projAnim.Length;
        GetComponent<SpriteRenderer>().sprite = projAnim[index];
    }
}
