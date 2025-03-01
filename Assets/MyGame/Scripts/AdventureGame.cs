﻿using UnityEngine;
using System;
using UnityEngine.UI;

public class AdventureGame : MonoBehaviour
{
    private const string stateInfoAlarm = "Info.Alarm";
    private const string stateDo = "Knit.Do";
    private const string doCollect = "Collect.Do";
    private const string doneInfo = "Info.Done";
    private const string collectInfo = "Collect.Info";
    private const int maxDehydration = 20;

    //private static readonly System.Random getrandom = new System.Random(123);

    [SerializeField] Text textIntroComponent;
    [SerializeField] Text textStoryComponent;
    [SerializeField] Text textComponentChoices;
    [SerializeField] State startingState;
    public Image introBG;
    public Image storyBG;
    public Image storyMenueBG;
    public Image humanStateBG;
    public Image woolStateBG;
    public Text woolStateTxt;
    public Text humanStateTxt;
    private int passedStatesCount;
    private int collectedWoolCount;
    private double dehydrationCount;
    private bool wait, overrideTextComponent;
    private bool infoOn;
    private string overrideText;
    private int statesUntilRescue;


    State actualState;

    private void SetupIntroUI()
    {
        introBG.enabled = textIntroComponent.enabled = true;
        storyMenueBG.enabled = textComponentChoices.enabled = true;
        storyBG.enabled = textStoryComponent.enabled = false;
        humanStateBG.enabled = humanStateTxt.enabled = false;
        woolStateBG.enabled = woolStateTxt.enabled = false;
        infoOn = false;
    }

    private void SetupInfoUI()
    {
        introBG.enabled = textIntroComponent.enabled = false;
        storyMenueBG.enabled = textComponentChoices.enabled = true;
        storyBG.enabled = textStoryComponent.enabled = true;
        humanStateBG.enabled = humanStateTxt.enabled = true;
        woolStateBG.enabled = woolStateTxt.enabled = true;
        infoOn = true;
    }

    // Use this for initialization
    void Start()
    {
        actualState = startingState;
        textIntroComponent.text = actualState.GetStateStory();
        textComponentChoices.text = actualState.GetStateStoryMenue();
        passedStatesCount = 0;
        collectedWoolCount = 0;
        dehydrationCount = 0;
        statesUntilRescue = 30;
        wait = false;
        Debug.Log("Enter");

        SetupIntroUI();
    }

    // Update is called once per frame
    void Update()
    {
        ManageState();
    }

    private void ResetCounters()
    {
        passedStatesCount = 0;
        collectedWoolCount = 0;
        dehydrationCount = 0;
    }

    private State doTransition(State currentState, State nextState)
    {

        passedStatesCount += 1;
        dehydrationCount = (dehydrationCount < maxDehydration) ? dehydrationCount += 0.5 : dehydrationCount = maxDehydration;

        if (passedStatesCount == statesUntilRescue)
        {
            overrideTextComponent = wait = false;
            Debug.Log("reached passed state counts: " + passedStatesCount);
            var rescue = Resources.Load<State>("States/Rescue");
            return rescue;
        }

        if (dehydrationCount == maxDehydration)
        {
            Debug.Log("Exit Dehydration " + dehydrationCount);
            overrideTextComponent = wait = false;
            dehydrationCount = 100;

            //return (State)AssetDatabase.LoadAssetAtPath("Assets/MyGame/States/Dead.Dehydration.asset", typeof(State));
            var deadDehyd = Resources.Load<State>("States/Dead.Dehydration");
            return deadDehyd;

        }

        if (nextState.name == stateInfoAlarm)
        {
            ResetCounters();
            Debug.Log("Counters Reseted + " + passedStatesCount + " " + collectedWoolCount + " " + dehydrationCount);
        }

        if (currentState.name != nextState.name)
        {
            wait = false;
            overrideText = "reset";
        }

        if (currentState.name == nextState.name)
        {
            if (nextState.name == stateDo || nextState.name == "Fight.Attack" || nextState.name == doCollect)
            {
                wait = false;
                overrideText = "reset in do|attack";
            }
            else
            {
                wait = true;
                overrideText = "Yes, waiting is the best option";
            }

        }

        if (nextState.name == doneInfo || nextState.name == collectInfo)
        {
            SetupInfoUI();
            overrideTextComponent = false;
        }

        if (currentState.name == "Info.Human" && nextState.name == doneInfo)
        {
            overrideTextComponent = true;
            overrideText = "Notification: Crime scene investigation revealed that robots destroyed all water inventories and water sponge warehouses. " + "\n \n" +
                           "Notification: All proper working service robots have to ensure that their godhumans stay alive and do not dry out." + "\n \n" +
                           "Notification: Collect wool and knit water sponges which are able to make water out of air. ";
        }

        if (currentState.name == "Info.Accident" && nextState.name == doneInfo)
        {
            overrideTextComponent = true;
            overrideText = "Magda is a 21 year old woman. She loves salty food and is doing a lot of sports." + "\n" +
                           "Good news, Magda is alive and at this moment she isn't dehydrated." + "\n" +
                           "For knitting wool you visit her in her house. Collect wool and knit enough sponges so that she will " +
                           "survive until rescue is approaching.";
        }

        if (currentState.name == doneInfo && nextState.name == "Collect")
        {
            overrideTextComponent = false;
        }

        if ((currentState.name == collectInfo || currentState.name == doCollect) && nextState.name == doCollect)
        {
            int nbrWool = RandomState.getrandom.Next(1, 3);
            collectedWoolCount += nbrWool;
            collectedWoolCount = Clamp(collectedWoolCount, 0, 5);
            Debug.Log("Collected " + nbrWool + "kg wool: current wool count: " + collectedWoolCount);
            return nextState;
        }


        if ((currentState.name == "Knit.Info" || nextState.name == stateDo) && nextState.name == stateDo)
        {
            if (collectedWoolCount >= 2)
            {
                collectedWoolCount -= 2;
                dehydrationCount -= 1.5;
                Debug.Log("Wool Knitted -2kg + 1L water for magda, current dehydration" + dehydrationCount);
            }
            else
            {
                overrideText = "Sorry, not enough wool for knitting. collect wool";
                overrideTextComponent = true;
                //nextState.SetKnitNotification(/*"Sorry, not enough wool for knitting. collect wool*/");
            }

            Debug.Log("Wolle -2, Wasser +1");
            return nextState;
        }

        if (currentState.name == stateDo && currentState.name == collectInfo)
        {
            overrideTextComponent = false;
        }

        if (currentState.name == "Fight.Do" && (nextState.name == collectInfo || nextState.name == "Fight.Do"))
        {
            Debug.Log("wool before Fight in kg: " + collectedWoolCount);
            collectedWoolCount += RandomState.getrandom.Next(0, 3);
            collectedWoolCount = Clamp(collectedWoolCount, 0, 5);
            Debug.Log("wool after Fight in kg: " + collectedWoolCount);
        }
        return nextState;
    }

    private int Clamp(int value, int cmin, int cmax)
    {
        return Math.Max(Math.Min(value, cmax), cmin);
    }

    private void ManageState()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            State[] nextStates = actualState.GetNextStates();
            Debug.Log("States size 1");
            if (nextStates.Length < 1)
            {
                return;
            }
            State nextState = nextStates[0];
            actualState = doTransition(actualState, nextState);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            State[] nextStates = actualState.GetNextStates();
            Debug.Log("States size 2");
            if (nextStates.Length < 2)
            {
                return;
            }
            State nextState = nextStates[1];
            actualState = doTransition(actualState, nextState);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            State[] nextStates = actualState.GetNextStates();
            Debug.Log("States size 3");
            if (nextStates.Length < 3)
            {
                return;
            }
            State nextState = nextStates[2];
            actualState = doTransition(actualState, nextState);
        }

        if (wait || overrideTextComponent)
        {
            Debug.Log("in wait " + infoOn);
            if (infoOn)
            {
                textStoryComponent.text = overrideText;
            }
            else
            {
                textIntroComponent.text = overrideText;
            }
        }
        else
        {
            Debug.Log("in wait else" + infoOn);
            if (infoOn)
            {
                textStoryComponent.text = actualState.GetStateStory();
            }
            else
            {
                textIntroComponent.text = actualState.GetStateStory();
            }
        }


        textComponentChoices.text = actualState.GetStateStoryMenue();
        humanStateTxt.text = "Human \n" +
                             "Name: Magda \n" +
                             "Age: 21 \n" +
                             "Dehydration: \n" +
                             dehydrationCount + " %";
        woolStateTxt.text = "Wool collected (kg): " + collectedWoolCount;
    }
}
