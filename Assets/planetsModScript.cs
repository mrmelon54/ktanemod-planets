using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

using Random = UnityEngine.Random;

public class planetsModScript : MonoBehaviour {
    public KMAudio BombAudio;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMSelectable[] ModuleButtons;
    public KMSelectable ModuleSelect;
    public TextMesh screenText;
    public GameObject[] planetModels;
    public GameObject planetIcon;
    public GameObject[] stripLights;

    int planetShown;
    string myText = "";
    string answerText = "";
    int[] stripColours = new int[5];
    readonly int[,] stripColourChangeTableOne = {
        { 0, -3, -5, 2, -9, -8, -6, 1, -4 },
        { 5, 6, 2, 6, -7, -4, 3, -8, 3 },
        { 6, -2, -8, -5, 4, 8, 4, 2, -1 },
        { -6, 7, -4, -5, -4, -4, -5, -3, 8 },
        { 7, -5, 3, -7, 6, 1, -4, 4, -9 },
        { -5, -9, -2, -1, 3, -9, -7, -5, -9 },
        { -2, -1, 9, -9, -2, 5, 5, -8, 0 },
        { -1, 8, 3, 8, 6, -2, 4, 4, 8 },
        { -2, 9, -3, -6, -4, 2, 4, -3, -1 }
    };

    readonly int[] stripColourChangeTableTwo = { 89, 30, 41, 97, 49, 63, 60, 3, 74 };
    int solvedModules;

    bool moduleSolved;

    static int moduleIdCounter = 1;
    int moduleId;

    void Start() {
        moduleId = moduleIdCounter++;
        planetShown=Random.Range(0,planetModels.Length-2);
        if(DateTime.Now.Month==4&&DateTime.Now.Day==1){
          planetShown=Random.Range(8,10);
        }
        Debug.LogFormat("[Planets #{0}] Planet showing: {1}", moduleId, planetModels[planetShown].name);

        for (int i = 0; i < planetModels.Length; i++) {
            planetModels[i].SetActive(false);
        }

        planetModels[planetShown].SetActive(true);

        for (int i = 0; i < stripColours.Length; i++) {
            stripColours[i] = Random.Range(0, 9);
        }

        var stripCol = new[] { "Aqua", "Blue", "Green", "Lime", "Orange", "Red", "Yellow", "White", "Black" };
        Debug.LogFormat("[Planets #{0}] Strip Lights showing: {1}", moduleId, stripColours.Select(x => stripCol[x]).Join(", "));

        for (int i = 0; i < stripLights.Length; i++) {
            for (int j = 0; j < stripLights[i].transform.childCount; j++) {
                stripLights[i].transform.GetChild(j).gameObject.SetActive((stripColours[i] == j));
            }
        }

        CalculateCorrectAnswer();

        for (int i = 0; i < ModuleButtons.Length; i++) {
            int j = i;

            ModuleButtons[i].OnInteract += delegate() {
                PressButton(j);

                return false;
            };
        }
    }

    void Update() {
        var newSolvedModules = BombInfo.GetSolvedModuleNames().Count;

        if (newSolvedModules != solvedModules) {
            solvedModules = newSolvedModules;
            CalculateCorrectAnswer();
        }
        if(planetShown==2) {
          var dayEarth=planetModels[2].transform.GetChild(0).gameObject;
          var nightEarth=planetModels[2].transform.GetChild(2).gameObject;
          if(DateTime.Now.Hour>5&&DateTime.Now.Hour<19) {
            dayEarth.SetActive(true);
            nightEarth.SetActive(false);
          } else {
            dayEarth.SetActive(false);
            nightEarth.SetActive(true);
          }
        }
    }

    int IntProduct(int[] a) {
        var o = a[0] + 1;

        for (int i = 1; i < a.Length; i++) {
            o = o * (a[i] + 1);
        }

        return o;
    }

    void CalculateCorrectAnswer() {
        var planetNumber = planetShown==9?9:planetShown-1;
        var numA = (planetNumber + 1) * 123 + solvedModules * 10;
        var numB = BombInfo.GetBatteryCount() * 5 + BombInfo.GetOnIndicators().Count() * 6;
        var numC = numA + numB + 4 * BombInfo.GetPortCount() + 462;
        var numD = (IntProduct(stripColours) + stripColourChangeTableOne[stripColours[0], stripColours[3]]) * stripColourChangeTableTwo[stripColours[2]] * ((stripColours[4] > 6) ? 5 : 1);
        answerText = (Math.Abs(numC * numD) % 1000000).ToString().PadLeft(6, '0');
        Debug.LogFormat("[Planets #{0}] Correct code: {1}", moduleId, answerText);
    }

    void PressButton(int buttonId) {
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        ModuleSelect.AddInteractionPunch();

        if (moduleSolved) {
            return;
        }

        if (buttonId == 11) {
            myText = myText.PadLeft(6, '0');

            if (!myText.Equals("") && answerText == myText) {
                moduleSolved = true;
                myText = "Solved";
                RenderScreen();
                Debug.LogFormat("[Planets #{0}] Module solved.", moduleId);
                BombModule.HandlePass();
            } else {
                Debug.LogFormat("[Planets #{0}] Strike! The PIN {1} is incorrect.", moduleId, myText);
                myText = "";
                BombModule.HandleStrike();
            }
        } else if (buttonId == 10) {
            if (myText.Length > 0) {
                myText = myText.Remove(myText.Length - 1);
            }
        } else {
            myText += buttonId;
        }

        RenderScreen();
    }

    void RenderScreen() {
        if (myText.Length > 6 && myText != "Help Me" && myText != "Solved") {
            myText = myText.Remove(myText.Length - 1);
        }

        screenText.text = myText;

        if (myText.Length == 0) {
            screenText.text = "Help Me";
        }
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Submit your answer with “!{0} press 1234 delete space”.";
    #pragma warning restore 414

    KMSelectable[] ProcessTwitchCommand(string command) {
        command = command.ToLowerInvariant().Trim();

        if (Regex.IsMatch(command, @"^press +[0-9a-z^, |&]+$")) {
            command = command.Substring(6).Trim();
            var presses = command.Split(new[] { ',', ' ', '|', '&' }, StringSplitOptions.RemoveEmptyEntries);
            var pressList = new List<KMSelectable>();

            for (int i = 0; i < presses.Length; i++) {
                if (Regex.IsMatch(presses[i], @"^(delete|space)$")) {
                    pressList.Add(ModuleButtons[(presses[i].Equals("delete")) ? 10 : 11]);
                } else {
                    String numpadPresses=presses[i];
                    for(int j=0;j<numpadPresses.Length;j++) {
                        if(Regex.IsMatch(numpadPresses[j].ToString(),@"^[0-9]$")) {
                            pressList.Add(ModuleButtons[int.Parse(numpadPresses[j].ToString())]);
                        }
                    }
                }
            }

            return (pressList.Count > 0) ? pressList.ToArray() : null;
        }

        return null;
    }
}
