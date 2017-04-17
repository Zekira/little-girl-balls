﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {

    private Transform left, right;
    private Text leftText, rightText;
    private Color speakingColor = new Color(1f, 1f, 1f, 1f);
    private Color silentColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    private List<DialogueEntry> dialogue = new List<DialogueEntry>();
    public string path;

    private int currentLine = 0;
    private DialogueEntry currentDialogue;

    void Awake() {
        left = GameObject.FindWithTag("Dialogue").transform.FindChild("Left").transform;
        right = GameObject.FindWithTag("Dialogue").transform.FindChild("Right").transform;
        leftText = left.FindChild("DialogueBox").FindChild("Text").GetComponent<Text>();
        rightText = right.FindChild("DialogueBox").FindChild("Text").GetComponent<Text>();
        Hide();
    }

    public void Hide() {
        left.gameObject.SetActive(false);
        right.gameObject.SetActive(false);
    }

    public void startDialogue(string p) {
        GlobalHelper.dialogue = true;
        path = p;
        dialogue = DialogueEntry.ParseFile(path);
        currentLine = 0;
        currentDialogue = dialogue[0];
        if (currentDialogue.leftSpeaking) {
            leftSays(currentDialogue, currentDialogue.showOther);
        } else {
            rightSays(currentDialogue, currentDialogue.showOther);
        }
    }

    public void advanceDialogue() {
        currentLine++;
        if (currentLine >= dialogue.Count) {
            Hide();
            GlobalHelper.dialogue = false;
            return;
        }
        currentDialogue = dialogue[currentLine];
        if (currentDialogue.leftSpeaking) {
            leftSays(currentDialogue, currentDialogue.showOther);
        } else {
            rightSays(currentDialogue, currentDialogue.showOther);
        }
    }

    public void leftSays(DialogueEntry dialogue, bool showRight) {
        left.gameObject.SetActive(true);
        StartCoroutine(moveTowards(new Vector3(0f, 0f, 0f), 0.2f, left.GetComponent<RectTransform>()));
        left.FindChild("DialogueBox").gameObject.SetActive(true);
        Texture2D charTexture = Resources.Load("Graphics/Characters/" + dialogue.currentCharacer.ToString() + "_" + dialogue.currentEmotion.ToString()) as Texture2D;
        left.FindChild("Character").GetComponent<Image>().sprite = Sprite.Create(charTexture, new Rect(0f, 0f, charTexture.width, charTexture.height), 0.5f*Vector2.one);
        left.FindChild("Character").GetComponent<Image>().color = speakingColor;
        leftText.text = dialogue.text;
        if (showRight) {
            right.gameObject.SetActive(true);
            StartCoroutine(moveTowards(new Vector3(40f, -40f, 0f), 0.2f, right.GetComponent<RectTransform>()));
            right.FindChild("DialogueBox").gameObject.SetActive(false);
            right.FindChild("Character").GetComponent<Image>().color = silentColor;
        } else {
            right.gameObject.SetActive(false);
        }
    }

    public void rightSays(DialogueEntry dialogue, bool showLeft) {
        right.gameObject.SetActive(true);
        StartCoroutine(moveTowards(new Vector3(0f, 0f, 0f), 0.2f, right.GetComponent<RectTransform>()));
        right.FindChild("DialogueBox").gameObject.SetActive(true);
        Texture2D charTexture = Resources.Load("Graphics/Characters/" + dialogue.currentCharacer.ToString() + "_" + dialogue.currentEmotion.ToString()) as Texture2D;
        right.FindChild("Character").GetComponent<Image>().sprite = Sprite.Create(charTexture, new Rect(0f, 0f, charTexture.width, charTexture.height), 0.5f * Vector2.one);
        right.FindChild("Character").GetComponent<Image>().color = speakingColor;
        rightText.text = dialogue.text;
        if (showLeft) {
            left.gameObject.SetActive(true);
            StartCoroutine(moveTowards(new Vector3(-40f, -40f, 0f), 0.2f, left.GetComponent<RectTransform>()));
            left.FindChild("DialogueBox").gameObject.SetActive(false);
            left.FindChild("Character").GetComponent<Image>().color = silentColor;
        } else {
            left.gameObject.SetActive(false);
        }
    }

    IEnumerator moveTowards(Vector2 to, float speed, RectTransform transform) {
        Vector2 from = transform.anchoredPosition;
        float progress = 0f;
        if (from == to) {
            progress = 1f;
        }
        Vector2 delta = to - from;
        while (progress < 1f) {
            transform.anchoredPosition = from + progress * delta;
            progress += speed;
            yield return null;
        }
        transform.anchoredPosition = to;
    }
}
