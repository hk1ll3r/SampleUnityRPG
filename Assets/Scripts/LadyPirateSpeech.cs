using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LadyPirateSpeech : SpeechSource {
    public static readonly string LogTag = typeof(LadyPirateSpeech).Name;

    public static Logger logger = new Logger(Debug.unityLogger.logHandler);

    public override DialogSet dialogs {
        get {
            Dictionary<string, UIProp<object>> ladyPirateProps = new Dictionary<string, UIProp<object>>() {
                ["avatar"] = new UIProp<object>("lady-pirate"),
                ["name"] = new UIProp<object>("Lady Pirate"),
            };
            Dictionary<string, UIProp<object>> devilProps = new Dictionary<string, UIProp<object>>() {
                ["avatar"] = new UIProp<object>("devil"),
                ["name"] = new UIProp<object>("Devil"),
            };
            DialogSet ds = new DialogSet("ladypirate", states: new Dictionary<string, DialogBase>() {
                ["intro"] = new DialogSet("intro", states: new Dictionary<string, DialogBase>() {
                    ["1"] = new DialogState("1", "Hi! Whats up!", uiProps: devilProps),
                    ["2"] = new DialogState("2", "You look devil-ish... I'm going to call you Devil!", uiProps: ladyPirateProps),
                    ["3"] = new DialogState("3", "Welcome to my town!", uiProps: ladyPirateProps)
                }),
                ["low-hp"] = new DialogSet("low-hp", states: new Dictionary<string, DialogBase>() {
                    ["offer"] = new DialogState("offer", "Oh you are low on HP. Do you want some food to recover?", choices: new List<DialogChoice> {
                        new DialogChoice("A", "Yes! (go to full hp)", "accept"),
                        new DialogChoice("B", "No! I like to live on the edge."),
                    }, uiProps: ladyPirateProps),
                    ["accept"] = new DialogState("accept", "Here you go. You look much better already.", uiProps: ladyPirateProps),
                    ["3"] = new DialogState("3", "Thanks!", uiProps: devilProps),
                }, transitions: new Dictionary<string, string>() {
                    ["accept"] = "3",
                }, startStateName: "offer"),
                ["cat-quest-start"] = new DialogSet("cat-quest-start", states: new Dictionary<string, DialogBase>() {
                    ["propose"] = new DialogState("propose", "I have lost my Kitty cat. Can you find him?", choices: new List<DialogChoice> {
                        new DialogChoice("A", "Yes, my pleasure! (starts quest)", "accept"),
                        new DialogChoice("B", "No, your problem!"),
                    }, uiProps: ladyPirateProps),
                    ["accept"] = new DialogState("accept", "Awesome! I'll reward you generously.", uiProps: ladyPirateProps)
                }, transitions: new Dictionary<string, string>(), startStateName: "propose"),
                ["cat-quest-middle"] = new DialogSet("cat-quest-middle", states: new Dictionary<string, DialogBase>() {
                    ["1"] = new DialogState("1", "Any idea where I could find your kitty?", uiProps: devilProps),
                    ["2"] = new DialogState("2", "He likes to play on the nearby hills.", uiProps: ladyPirateProps),
                }),
                ["cat-quest-end"] = new DialogSet("cat-quest-end", states: new Dictionary<string, DialogBase>() {
                    ["1"] = new DialogState("1", "Amazing! Thanks for finding kitty.", uiProps: ladyPirateProps),
                    ["2"] = new DialogState("2", "Here is a boost to your weapon as my token of gratitude.", uiProps: ladyPirateProps),
                }),
                ["dragon-quest-start"] = new DialogSet("dragon-quest-start", states: new Dictionary<string, DialogBase>() {
                    ["1"] = new DialogState("1", "The Micro Dragons have nested near my town.", uiProps: ladyPirateProps),
                    ["2"] = new DialogState("2", "They scare my people and kitty.", uiProps: ladyPirateProps),
                    ["3"] = new DialogState("3", "Can you get rid of them for me?", choices: new List<DialogChoice> {
                        new DialogChoice("A", "Sure! (starts quest)", "4"),
                        new DialogChoice("B", "No! Micro Dragons are near extinct. They should be protected!"),
                    }, uiProps: ladyPirateProps),
                    ["4"] = new DialogState("4", "Perfect! Beware they reproduce quickly. You should kill their Queen, the large orange one.", uiProps: ladyPirateProps)
                }, transitions: new Dictionary<string, string>() {
                    ["1"] = "2",
                    ["2"] = "3"
                }),
                ["dragon-quest-middle"] = new DialogSet("dragon-quest-middle", states: new Dictionary<string, DialogBase>() {
                    ["1"] = new DialogState("1", "Kill the Queen dragon, the giant orange one.", uiProps: ladyPirateProps),
                }),
                ["dragon-quest-end"] = new DialogSet("dragon-quest-end", states: new Dictionary<string, DialogBase>() {
                    ["1"] = new DialogState("1", "You are a hero!", uiProps: ladyPirateProps),
                }),
                ["outro"] = new DialogState("outro", "My village is in peace thanks to you Devil.", uiProps: ladyPirateProps),
            }, transitions: new Dictionary<string, string>() {
                ["intro"] = "cat-quest-start",
                ["dragon-quest-end"] = "outro",
            });

            ds.OnStarted = (p) => {
                if (PersistentVariable.Get("ladypirate.greet", 0f) == 0f) {
                    PersistentVariable.Set("ladypirate.greet", 1f);
                    ds.defaultOnStarted("intro");
                } else if (PersistentVariable.Get("quests.cat.state", 0f) == 0f) {
                    ds.defaultOnStarted("cat-quest-start");
                } else if (PersistentVariable.Get("quests.cat.state", 0f) == 1f && PersistentVariable.Get("cat.follow", 0f) == 0f) {
                    ds.defaultOnStarted("cat-quest-middle");
                } else if (PersistentVariable.Get("quests.cat.state", 0f) == 1f && PersistentVariable.Get("cat.follow", 0f) == 1f) {
                    ds.defaultOnStarted("cat-quest-end");
                } else if (PersistentVariable.Get("quests.dragon.state", 0f) == 0f) {
                    ds.defaultOnStarted("dragon-quest-start");
                } else if (PersistentVariable.Get("quests.dragon.state", 0f) == 1f && PersistentVariable.Get("dragon.dead", 0f) == 0f) {
                    ds.defaultOnStarted("dragon-quest-middle");
                } else if (PersistentVariable.Get("quests.dragon.state", 0f) == 1f && PersistentVariable.Get("dragon.dead", 0f) == 1f) {
                    ds.defaultOnStarted("dragon-quest-end");
                } else {
                    ds.defaultOnStarted("outro");
                }
            };

            ds.OnChildSkipped = (s) => {
                if (!ds.transitions.ContainsKey(ds.CurrentStateName)
                    && ds.CurrentStateName != "low-hp"
                    && GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().Health < 50f) {
                    ds.TransitionToState("low-hp");
                } else {
                    ds.defaultOnChildSkipped(s);
                }
            };

            DialogState dsCatAccept = (DialogState)ds.GetDialogBaseByPath("cat-quest-start/accept");
            dsCatAccept.OnStarted = (p) => {
                PersistentVariable.Set("quests.cat.state", 1f);
                Events.gInstance.RaiseEvent("quests.cat.state", 1f);
                dsCatAccept.defaultOnStarted(p);
            };

            DialogState dsHPOffer = (DialogState)ds.GetDialogBaseByPath("low-hp/offer");
            dsHPOffer.OnChoice = (c) => {
                if (c.name == "A") { // accept
                    Events.gInstance.RaiseEvent("player.fullhp");
                }
                dsHPOffer.defaultOnChoice(c);
            };

            DialogSet dsCatEnd = (DialogSet)ds.GetDialogBaseByPath("cat-quest-end");
            dsCatEnd.OnStarted = (p) => {
                PersistentVariable.Set("quests.cat.state", 2f);
                Events.gInstance.RaiseEvent("quests.cat.state", 2f);
                dsCatEnd.defaultOnStarted(p);
            };

            DialogState dsCatEnd2 = (DialogState)dsCatEnd.GetDialogBaseByPath("2");
            dsCatEnd2.OnStarted = (p) => {
                Events.gInstance.RaiseEvent("player.updamage");
                dsCatEnd2.defaultOnStarted(p);
            };

            DialogState dsDragonAccept = (DialogState)ds.GetDialogBaseByPath("dragon-quest-start/4");
            dsDragonAccept.OnStarted = (p) => {
                PersistentVariable.Set("quests.dragon.state", 1f);
                Events.gInstance.RaiseEvent("quests.dragon.state", 1f);
                dsDragonAccept.defaultOnStarted(p);
            };

            DialogSet dsDragonEnd = (DialogSet)ds.GetDialogBaseByPath("dragon-quest-end");
            dsDragonEnd.OnStarted = (p) => {
                PersistentVariable.Set("quests.dragon.state", 2f);
                Events.gInstance.RaiseEvent("quests.dragon.state", 2f);
                dsDragonEnd.defaultOnStarted(p);
            };
            

            return ds;
        }
    }
}
