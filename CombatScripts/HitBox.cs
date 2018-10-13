using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitBox {

	// ---Fields---
	public float xPos1, yPos1;
	public float xPos2, yPos2;
	int flipConstant = 1;
	float xShift, yShift;

	// ---Constructor---
	public HitBox(float xPos1, float yPos1, float xPos2, float yPos2){
		if (xPos1 >= xPos2) 	{ this.xPos1 = xPos1; this.xPos2 = xPos2; }
		else 					{ this.xPos1 = xPos2; this.xPos2 = xPos1; }

		if (yPos1 >= yPos2) 	{ this.yPos1 = yPos1; this.yPos2 = yPos2; }
		else 					{ this.yPos1 = yPos2; this.yPos2 = yPos1; }
		return;
	}

	// ---Access Functions---
	public float getX1(){ 
		return getMax((this.xPos1 * this.flipConstant) + this.xShift, (this.xPos2 * this.flipConstant) + this.xShift);
	}
	public float getY1(){ 
		return getMax(this.yPos1 + this.yShift, this.yPos2 + this.yShift); 
	}
	public float getX2(){ 
		return getMin((this.xPos1 * this.flipConstant) + this.xShift, (this.xPos2 * this.flipConstant) + this.xShift);
	}
	public float getY2(){ 
		return getMin(this.yPos1 + this.yShift, this.yPos2 + this.yShift); 
	}

	public float getMax(float first, float second){
		if (first >= second)
			return first;
		else
			return second;
	}
	public float getMin(float first, float second){
		if (first >= second)
			return second;
		else
			return first;
	}

	// ---Manipulation Procedures---
	public void flipBox(int flip){
		this.flipConstant = flip;
		return;
	}

	public void shiftBox(float xShift, float yShift){
		this.xShift = xShift;
		this.yShift = yShift;
		return;
	}

	public void resetBox(){
		this.flipConstant = 1;
		this.xShift = 0;
		this.yShift = 0;
	}

	// ---Other Functions---
	public bool checkHit(HitBox h){
		if (flipConstant > 0) {
			Debug.DrawLine(new Vector2(getX1(), getY1()), new Vector2(getX2(), getY1()), Color.red);
			Debug.DrawLine(new Vector2(getX1(), getY2()), new Vector2(getX2(), getY2()), Color.red);
			Debug.DrawLine(new Vector2(getX1(), getY1()), new Vector2(getX1(), getY2()), Color.red);
			Debug.DrawLine(new Vector2(getX2(), getY1()), new Vector2(getX2(), getY2()), Color.red);

			Debug.DrawLine(new Vector2(h.getX1(), h.getY1()), new Vector2(h.getX2(), h.getY1()), Color.green);
			Debug.DrawLine(new Vector2(h.getX1(), h.getY2()), new Vector2(h.getX2(), h.getY2()), Color.green);
			Debug.DrawLine(new Vector2(h.getX1(), h.getY1()), new Vector2(h.getX1(), h.getY2()), Color.green);
			Debug.DrawLine(new Vector2(h.getX2(), h.getY1()), new Vector2(h.getX2(), h.getY2()), Color.green);
		}
		else {
			Debug.DrawLine(new Vector2(getX1(), getY1()), new Vector2(getX2(), getY1()), Color.red);
			Debug.DrawLine(new Vector2(getX1(), getY2()), new Vector2(getX2(), getY2()), Color.red);
			Debug.DrawLine(new Vector2(getX1(), getY1()), new Vector2(getX1(), getY2()), Color.red);
			Debug.DrawLine(new Vector2(getX2(), getY1()), new Vector2(getX2(), getY2()), Color.red);

			Debug.DrawLine(new Vector2(h.getX1(), h.getY1()), new Vector2(h.getX2(), h.getY1()), Color.green);
			Debug.DrawLine(new Vector2(h.getX1(), h.getY2()), new Vector2(h.getX2(), h.getY2()), Color.green);
			Debug.DrawLine(new Vector2(h.getX1(), h.getY1()), new Vector2(h.getX1(), h.getY2()), Color.green);
			Debug.DrawLine(new Vector2(h.getX2(), h.getY1()), new Vector2(h.getX2(), h.getY2()), Color.green);
		}

		if (getY1() < h.getY2() || getY2() > h.getY1()) {
			return false;
		}
		if (getX1() < h.getX2() || getX2() > h.getX1()) {
			return false;
		}

		return true;
	}

	public HitBox copy(){
		HitBox clone = new HitBox(this.xPos1, this.yPos1, this.xPos2, this.yPos2);
		return clone;
	}

}
