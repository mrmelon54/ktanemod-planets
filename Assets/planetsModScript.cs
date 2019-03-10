using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class planetsModScript : MonoBehaviour {
	public KMAudio audio;
	public KMBombInfo bomb;

	public KMSelectable button1;
	public KMSelectable button2;
	public KMSelectable button3;
	public KMSelectable button4;
	public KMSelectable button5;
	public KMSelectable button6;
	public KMSelectable button7;
	public KMSelectable button8;
	public KMSelectable button9;
	public KMSelectable button0;
	public KMSelectable buttonBackspace;
	public KMSelectable buttonToSpace;
	public TextMesh screenText;
	private String myText="";
	private int planetShown;
	private String answerText="";
	public GameObject[] planetModels;
	public GameObject planetIcon;
	public GameObject[] stripLights;
	public int[] stripColours={0,0,0,0,0};

	private void renderStripLights() {
		for(int i=0;i<stripLights.Count();i++) {
			for(int j=0;j<stripLights[i].transform.childCount;j++) {
				if(stripColours[i]==j+1) {
					stripLights[i].transform.GetChild(j).gameObject.SetActive(true);
				} else {
					stripLights[i].transform.GetChild(j).gameObject.SetActive(false);
				}
			}
		}
	}

	static int[] scctoOne={0,-3,-5,2,-9,-8,-6,1,-4};
	static int[] scctoTwo={5,6,2,6,-7,-4,3,-8,3};
	static int[] scctoThree={6,-2,-8,-5,4,8,4,2,-1};
	static int[] scctoFour={-6,7,-4,-5,-4,-4,-5,-3,8};
	static int[] scctoFive={7,-5,3,-7,6,1,-4,4,-9};
	static int[] scctoSix={-5,-9,-2,-1,3,-9,-7,-5,-9};
	static int[] scctoSeven={-2,-1,9,-9,-2,5,5,-8,0};
	static int[] scctoEight={-1,8,3,8,6,-2,4,4,8};
	static int[] scctoNine={-2,9,-3,-6,-4,2,4,-3,-1};
	int[][] stripColourChangeTableOne={
		scctoOne,scctoTwo,scctoThree,scctoFour,
		scctoFive,scctoSix,scctoSeven,scctoEight,
		scctoNine
	};
	int[] stripColourChangeTableTwo={89,30,41,97,49,63,60,3,74};
	int[] stripColourChangeTableThree={1,1,1,1,1,1,1,5,5};

	private int numOfSolvedModules=0;

	//logging
	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	void Awake() {
		moduleId = moduleIdCounter++;
		/*/foreach (KMSelectable object in keypad) {
			KMSelectable pressedObject = object;
			object.OnInteract += delegate() {keypadPress(pressedObject);return false;};
		}/*/
		button1.OnInteract+=delegate(){PressButton(1,button1);return false;};
		button2.OnInteract+=delegate(){PressButton(2,button2);return false;};
		button3.OnInteract+=delegate(){PressButton(3,button3);return false;};
		button4.OnInteract+=delegate(){PressButton(4,button4);return false;};
		button5.OnInteract+=delegate(){PressButton(5,button5);return false;};
		button6.OnInteract+=delegate(){PressButton(6,button6);return false;};
		button7.OnInteract+=delegate(){PressButton(7,button7);return false;};
		button8.OnInteract+=delegate(){PressButton(8,button8);return false;};
		button9.OnInteract+=delegate(){PressButton(9,button9);return false;};
		button0.OnInteract+=delegate(){PressButton(0,button0);return false;};
		buttonBackspace.OnInteract+=delegate(){PressButton(-1,buttonBackspace);return false;};
		buttonToSpace.OnInteract+=delegate(){PressButton(-2,buttonToSpace);return false;};
	}

	void Start () {
		chooseAPlanet();
		changePlanet(planetShown-1);
		chooseStripLights();
		renderStripLights();
		calculateCorrectAnswer();
	}

	void Update () {
		int newSolvedModules=bomb.GetSolvedModuleNames().Count();
		if(newSolvedModules!=numOfSolvedModules) {
			numOfSolvedModules=newSolvedModules;
			calculateCorrectAnswer();
		}
	}

	void chooseAPlanet() {
		planetShown=UnityEngine.Random.Range(1,9);
		Debug.LogFormat("[Planets #{0}] Planet Showing: {1}",moduleId,planetModels[planetShown-1].name);
	}

	void changePlanet(int n) {
		for(int i=0;i<planetModels.Count();i++) {
			planetModels[i].SetActive(false);
		}
		planetModels[n].SetActive(true);
	}

	void chooseStripLights() {
		stripColours[0]=UnityEngine.Random.Range(1,10);
		stripColours[1]=UnityEngine.Random.Range(1,10);
		stripColours[2]=UnityEngine.Random.Range(1,10);
		stripColours[3]=UnityEngine.Random.Range(1,10);
		stripColours[4]=UnityEngine.Random.Range(1,10);
		Debug.LogFormat("[Planets #{0}] Strip Lights Showing: {1}, {2}, {3}, {4} and {5}",
			moduleId,
			stripNumToCol(stripColours[0]),
			stripNumToCol(stripColours[1]),
			stripNumToCol(stripColours[2]),
			stripNumToCol(stripColours[3]),
			stripNumToCol(stripColours[4])
		);
	}
	String stripNumToCol(int n) {
		if(n==1) return "Aqua";
		if(n==2) return "Blue";
		if(n==3) return "Green";
		if(n==4) return "Lime";
		if(n==5) return "Orange";
		if(n==6) return "Red";
		if(n==7) return "Yellow";
		if(n==8) return "White";
		if(n==9) return "Black";
		return "";
	}

	int intProduct(int[] a) {
		int o=a[0];
		for(int i=1;i<a.Count();i++) {
			o=o*a[i];
		}
		return o;
	}

	int IntParseFast(String value) {
		int result=0;
		for(int i=0;i<value.Length;i++) {
			char letter=value[i];
			result=10*result+(letter-48);
		}
		return result;
	}

	void calculateCorrectAnswer() {
		int numOfBatteries=bomb.GetBatteryCount();
		int numOfLitIndicators=bomb.GetOnIndicators().Count();
		int numOfPorts=bomb.GetPortCount();
		int answer=0;

		int numA=planetShown*123+numOfSolvedModules*10;
		int numB=numOfBatteries*5+numOfLitIndicators*6;
		int numC=numA+numB+4*numOfPorts+21*22;
		int numD=intProduct(stripColours);
		int numE=stripColourChangeTableOne[stripColours[0]-1][stripColours[3]-1];
		int numF=stripColourChangeTableTwo[stripColours[2]-1];
		int numG=numD+numE;
		int numH=numG*numF;
		int numI=stripColourChangeTableThree[stripColours[4]-1];
		int numJ=numH*numI;
		int numK=numJ*numC;
		answer=Math.Abs(numK);

		answer=answer%1000000;
		answerText=answer.ToString();
		while(answerText.Length<5) {
			answerText="0"+answerText;
		}
		Debug.LogFormat("[Planets #{0}] Correct code: {1}",moduleId,answerText);
	}

	void PressButton(int buttonId,KMSelectable b) {
		if(moduleSolved==true) {
			return;
		} else {
			b.AddInteractionPunch();
			if(buttonId==-2) {
				if(myText=="") {
					Strike();
				} else {
					while(myText.Length<5) {
						myText="0"+myText;
					}
					if(answerText==myText) {
						Pass();
					} else {
						Strike();
					}
				}
			} else if(buttonId==-1) {
				myText=myText.Remove(myText.Length-1);
				renderScreen();
			} else if(buttonId==0) {
				myText+='0';
				renderScreen();
			} else if(buttonId==1) {
				myText+='1';
				renderScreen();
			} else if(buttonId==2) {
				myText+='2';
				renderScreen();
			} else if(buttonId==3) {
				myText+='3';
				renderScreen();
			} else if(buttonId==4) {
				myText+='4';
				renderScreen();
			} else if(buttonId==5) {
				myText+='5';
				renderScreen();
			} else if(buttonId==6) {
				myText+='6';
				renderScreen();
			} else if(buttonId==7) {
				myText+='7';
				renderScreen();
			} else if(buttonId==8) {
				myText+='8';
				renderScreen();
			} else if(buttonId==9) {
				myText+='9';
				renderScreen();
			}
		}
	}

	void renderScreen() {
		if(myText.Length>6&&myText!="Help Me"&&myText!="Solved") myText=myText.Remove(myText.Length-1);
		screenText.text=myText;
		if(myText.Length==0) screenText.text="Help Me";
	}

	private void Pass() {
		moduleSolved=true;
		myText="Solved";
		renderScreen();
		Debug.LogFormat("[Planets #{0}] Module solved.",moduleId);
		GetComponent<KMBombModule>().HandlePass();
	}

	private void Strike() {
		Debug.LogFormat("[Planets #{0}] Strike! The PIN {1} is incorrect",moduleId,myText);
		myText="";
		renderScreen();
		GetComponent<KMBombModule>().HandleStrike();
	}



	private string TwitchHelpMessage = @"Submit your answer with “!{0} press 1 2 3 4 delete space”.";
  private IEnumerator ProcessTwitchCommand(string command) {
    var pieces = command.ToLowerInvariant().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
    if (pieces.Length < 2 || (pieces[0] != "submit" && pieces[0] != "press"))
      yield break;
		var buttons=[];
		for(int i=1;i<pieces.Length;i++) {
			if(pieces[i]=="0") buttons.add(button0);
			if(pieces[i]=="1") buttons.add(button1);
			if(pieces[i]=="2") buttons.add(button2);
			if(pieces[i]=="3") buttons.add(button3);
			if(pieces[i]=="4") buttons.add(button4);
			if(pieces[i]=="5") buttons.add(button5);
			if(pieces[i]=="6") buttons.add(button6);
			if(pieces[i]=="7") buttons.add(button7);
			if(pieces[i]=="8") buttons.add(button8);
			if(pieces[i]=="9") buttons.add(button9);
			if(pieces[i]=="delete") buttons.add(buttonBackspace);
			if(pieces[i]=="space") buttons.add(buttonToSpace);
		}
    foreach (int ix in buttonIndexes) {
      buttons[ix].OnInteract();
      yield return new WaitForSeconds(.1f);
    }
  }
}
