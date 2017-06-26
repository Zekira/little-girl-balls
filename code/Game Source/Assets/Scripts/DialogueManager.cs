using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Class made to manage a conversation.
/// </summary>
public class DialogueManager : MonoBehaviour {

    private Transform left, right, leftTitle, rightTitle;
    private Text leftText, rightText;
    private Color speakingColor = new Color(1f, 1f, 1f, 1f);
    private Color silentColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    private List<DialogueEntry> dialogue = new List<DialogueEntry>();
    private int waitTime = 0; //Whether the current active dialogue entry is timed and cannot be skipped.
    public string path;

    private int currentLine = 0;
    private DialogueEntry currentDialogue;

    void Awake() {
        left = GameObject.FindWithTag("Dialogue").transform.FindChild("Left").transform;
        right = GameObject.FindWithTag("Dialogue").transform.FindChild("Right").transform;
        leftText = left.FindChild("DialogueBox").FindChild("Text").GetComponent<Text>();
        rightText = right.FindChild("DialogueBox").FindChild("Text").GetComponent<Text>();
        leftTitle = left.FindChild("Title").transform;
        rightTitle = right.FindChild("Title").transform;
        Hide();
    }
    
    /// <summary>
    /// Hides both sides.
    /// </summary>
    public void Hide() {
        left.gameObject.SetActive(false);
        right.gameObject.SetActive(false);
    }

    /// <summary>
    /// Initialises dialogue.
    /// </summary>
    public void StartDialogue(string p) {
        GlobalHelper.dialogue = true;
        path = p;
        dialogue = DialogueEntry.ParseFile(path);
        currentLine = 0;
        currentDialogue = dialogue[0];
        if (currentDialogue.leftSpeaking) {
            LeftSays(currentDialogue, currentDialogue.showOther);
        } else {
            RightSays(currentDialogue, currentDialogue.showOther);
        }
    }

    /// <summary>
    /// Continues the dialogue. Why am I writing these obvious comments on obvious functions?
    /// </summary>
    public void AdvanceDialogue() {
        if (waitTime>0) { //Prevent advancement if it's timed dialogue that's not yet to be continued
            return;
        }

        currentLine++;
        if (currentLine >= dialogue.Count) {
            Hide();
            GlobalHelper.dialogue = false;
            return;
        }
        currentDialogue = dialogue[currentLine];
        if (currentDialogue.leftSpeaking) {
            LeftSays(currentDialogue, currentDialogue.showOther);
        } else {
            RightSays(currentDialogue, currentDialogue.showOther);
        }
    }

    public void LeftSays(DialogueEntry dialogue, bool showRight) {
        left.gameObject.SetActive(true);
        StartCoroutine(MoveTowards(new Vector3(0f, 0f, 0f), 0.2f, left.GetComponent<RectTransform>()));
        left.FindChild("DialogueBox").gameObject.SetActive(true);
        left.FindChild("Character").GetComponent<Image>().sprite = GlobalHelper.characterPortraits.GetSprite(dialogue.currentCharacer, dialogue.currentEmotion);
        left.FindChild("Character").GetComponent<Image>().color = speakingColor;
        leftText.text = dialogue.text;
        if (showRight) {
            right.gameObject.SetActive(true);
            StartCoroutine(MoveTowards(new Vector3(40f, -40f, 0f), 0.2f, right.GetComponent<RectTransform>()));
            right.FindChild("DialogueBox").gameObject.SetActive(false);
            right.FindChild("Character").GetComponent<Image>().color = silentColor;
        } else {
            right.gameObject.SetActive(false);
        }
        ParseSpecial(dialogue.special);
    }

    public void RightSays(DialogueEntry dialogue, bool showLeft) {
        right.gameObject.SetActive(true);
        StartCoroutine(MoveTowards(new Vector3(0f, 0f, 0f), 0.2f, right.GetComponent<RectTransform>()));
        right.FindChild("DialogueBox").gameObject.SetActive(true);
        right.FindChild("Character").GetComponent<Image>().sprite = GlobalHelper.characterPortraits.GetSprite(dialogue.currentCharacer, dialogue.currentEmotion);
        right.FindChild("Character").GetComponent<Image>().color = speakingColor;
        rightText.text = dialogue.text;
        if (showLeft) {
            left.gameObject.SetActive(true);
            StartCoroutine(MoveTowards(new Vector3(-40f, -40f, 0f), 0.2f, left.GetComponent<RectTransform>()));
            left.FindChild("DialogueBox").gameObject.SetActive(false);
            left.FindChild("Character").GetComponent<Image>().color = silentColor;
        } else {
            left.gameObject.SetActive(false);
        }
        ParseSpecial(dialogue.special);
    }

    /// <summary>
    /// Uses any data set after the text in the dialogue.
    /// Things it can add: 
    ///   "timelinetick": allows all timelines to tick for a single frame.
    ///   "timed|<amount>": the dialogue is unskippable, but timed and continues after <amount> ticks.
    /// </summary>
    private void ParseSpecial(string[] text) {
        for (int i = 0; i < text.Length; i++) {
            switch (text[i].ToLower()) {
                case "timelinetick":
                    GlobalHelper.TickInterprenters();
                    continue;
                case "timed":
                    int waitTime = 0;
                    int.TryParse(text[i + 1], out waitTime);
                    StartCoroutine(DialogueWait(waitTime));
                    continue;
                case "introduction":
                    Transform transform;
                    if (currentDialogue.leftSpeaking) {
                        transform = leftTitle;
                        transform.gameObject.SetActive(true);
                        StartCoroutine(MoveSmooth(transform.GetComponent<RectTransform>(), new Vector2(-380, -227), new Vector2(-280, -227)));
                    } else {
                        transform = rightTitle;
                        transform.gameObject.SetActive(true);
                        StartCoroutine(MoveSmooth(transform.GetComponent<RectTransform>(), new Vector2(70, -227), new Vector2(170, -227)));
                    }
                    transform.FindChild("Name").GetComponent<Text>().text = text[i + 1];
                    transform.FindChild("Title").GetComponent<Text>().text = text[i + 2];
                    continue;
                default:
                    continue;
            }
        }
    }

    /// <summary>
    /// Moves "transform" from the current position to "to" with "speed".
    /// </summary>
    private IEnumerator MoveTowards(Vector2 to, float speed, RectTransform transform) {
        Vector2 from = transform.anchoredPosition;
        float progress = 0f;
        if (from == to) {
            progress = 1f;
        }
        Vector2 delta = to - from;
        while (progress < 1f) {
            if (!GlobalHelper.paused) {
                transform.anchoredPosition = from + progress * delta;
                progress += speed;
            }
            yield return null;
        }
        transform.anchoredPosition = to;
    }

    /// <summary>
    /// Stalls the dialogue for "time" ticks.
    /// </summary>
    private IEnumerator DialogueWait(int waitTime) {
        this.waitTime = waitTime;
        while (this.waitTime >= 0) {
            if (!GlobalHelper.paused) {
                this.waitTime--;
            }
            yield return null;
        }
        AdvanceDialogue();
    }

    private IEnumerator MoveSmooth(RectTransform transform, Vector2 startPos, Vector2 endPos) {
        float linearProgress = 0f;
        float progress = 0f;;
        transform.anchoredPosition = new Vector3(startPos.x, startPos.y);
        Color nameColor, nameOutline, titleColor, titleOutline;
        nameColor = transform.FindChild("Name").GetComponent<Text>().color;
        titleColor = transform.FindChild("Title").GetComponent<Text>().color;
        nameOutline = transform.FindChild("Name").GetComponent<Outline>().effectColor;
        titleOutline = transform.FindChild("Title").GetComponent<Outline>().effectColor;
        float alpha;
        //how much it should move when linprogress = [0,1] is defined by f(x)=5(2x-1)^4
        while (linearProgress < 1.1f) { //Overshooting 1 because the titles shooting away looks nice.
            if (!GlobalHelper.paused) {
                progress = 0.05f * f(linearProgress);
                transform.anchoredPosition += progress * (endPos - startPos);
                alpha = Mathf.Max(1 - f(linearProgress), 0);
                nameColor.a = alpha; titleColor.a = alpha; nameOutline.a = alpha; titleOutline.a = alpha;
                transform.FindChild("Name").GetComponent<Text>().color = nameColor;
                transform.FindChild("Title").GetComponent<Text>().color = titleColor;
                transform.FindChild("Name").GetComponent<Outline>().effectColor = nameOutline;
                transform.FindChild("Title").GetComponent<Outline>().effectColor = titleOutline;

                linearProgress += 0.0075f;
            }
            yield return null;
        }
        transform.gameObject.SetActive(false);
    }

    private float f(float x) {
        return (2 * x - 1) * (2 * x - 1) * (2 * x - 1) * (2 * x - 1);
    }
}
