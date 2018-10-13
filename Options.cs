using System.Collections;
using UnityEngine;

public enum key {UP, DOWN, LEFT, RIGHT, B1, B2, B3};

[System.Serializable]
public class Options {
	public int[] volume = new int[3]; // Stores volume preferences
	public KeyCode[] controls = new KeyCode[7]; // Stores input preferences
	public int resolution = 2; // Stores resolution preference

	public Options (){
        volume[0] = 100;
        volume[1] = 25;
        volume[2] = 50;

		controls[0] = KeyCode.W;
		controls[1] = KeyCode.S;
		controls[2] = KeyCode.A;
		controls[3] = KeyCode.D;
		controls[4] = KeyCode.J;
		controls[5] = KeyCode.K;
		controls[6] = KeyCode.L;
		resolution = 0;
	}
}
