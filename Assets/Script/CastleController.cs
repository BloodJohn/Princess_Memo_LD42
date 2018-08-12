﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CastleController : MonoBehaviour
{
    private const int CardMax = 15;

    public Text descriptionText;
    public GridLayoutGroup cardTable;
    public GameObject prefab;
    public Sprite FogSprite;
    public Sprite DeleteSprite;
    public Sprite[] CardSpriteList;
    public string[] storyList;
    public string[] oldStoryList;
    private int cardIndex;

    private readonly List<CardController> cardList = new List<CardController>(CardMax);
    private int turnCount;

    void Awake()
    {
        if (null == GameController.Instance)
        {
            GameController.LoadScene();
            return;
        }

        for (var i = 0; i < CardMax; i++)
        {
            var obj = Instantiate(prefab, cardTable.transform);
            cardList.Add(obj.GetComponent<CardController>());

            cardList[i].Init(this);
        }

        descriptionText.text = "Tap on picture to open next memory\n" +
                               "Tap on damaged picture to refresh them\n";
        turnCount = 0;
        AddCard();

        /*foreach (var story in storyList)
            Debug.AssertFormat(story.Length <= 216, "{0}:{1}", story.Length, story);

        foreach (var story in oldStoryList)
            Debug.AssertFormat(story.Length <= 216, "{0}:{1}", story.Length, story);*/
    }

    public static void LoadScene()
    {
        Debug.LogFormat("CastleScene");
        SceneManager.LoadSceneAsync("CastleScene");
    }

    public void OnTurn(int index, bool isFog)
    {
        turnCount++;
        descriptionText.text = isFog ? oldStoryList[index] : storyList[index];

        for (var i = 0; i < 2; i++)
            BlurCard();

        if (turnCount < 6) AddCard();
        else if (0 == turnCount % 3) AddCard();
    }

    private void BlurCard()
    {
        var rnd = (int)((cardList.Count - 1) * Random.value);
        cardList[rnd].BlurMemory();
    }

    private void AddCard()
    {
        var rnd = (int)((cardList.Count - 1) * Random.value);

        if (cardIndex > CardSpriteList.Length) cardIndex = CardSpriteList.Length - 1;
        var index = cardIndex;
        cardIndex++;

        for (var i = rnd; i < cardList.Count; i++)
            if (cardList[i].AddMemory(index))
                return;

        for (var i = 0; i < rnd; i++)
            if (cardList[i].AddMemory(index))
                return;

        if (!cardList
            .OrderBy(item => item.Index)
            .Any(item => item.AddFog()))
        {
            GameController.Instance.saveMemoCount = cardList.Count(item => item.Status != MemoStatus.Deleted);

            LoseController.LoadScene();
        }
    }
}
