using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatLiteSpeech : SpeechSource {
    public static readonly string LogTag = typeof(CatLiteSpeech).Name;

    public static Logger logger = new Logger(Debug.unityLogger.logHandler);

    public override DialogSet dialogs {
        get {
            Dictionary<string, UIProp<object>> catLiteProps = new Dictionary<string, UIProp<object>>() {
                ["avatar"] = new UIProp<object>("cat-lite"),
                ["name"] = new UIProp<object>("Kitty"),
            };
            Dictionary<string, UIProp<object>> devilProps = new Dictionary<string, UIProp<object>>() {
                ["avatar"] = new UIProp<object>("devil"),
                ["name"] = new UIProp<object>("Devil"),
            };
            DialogSet ds = new DialogSet("catlite", states: new Dictionary<string, DialogBase>() {
                ["intro"] = new DialogSet("intro", states: new Dictionary<string, DialogBase>() {
                    ["1"] = new DialogState("1", "meow!", uiProps: catLiteProps, choices: new List<DialogChoice> {
                        new DialogChoice("A", "meow!", "../meow"),
                        new DialogChoice("B", "shoo!", "../shoo"),
                    }),
                }),
                ["meow"] = new DialogState("meow", "meeeeeeooooowwwwwww!!!", uiProps: catLiteProps),
                ["shoo"] = new DialogSet("shoo", states: new Dictionary<string, DialogBase>() {
                    ["1"] = new DialogState("1", "errrrrrrrrrr!!!", uiProps: catLiteProps),
                }),
            }, startStateName: "intro");

            DialogState dsMeow = (DialogState)ds.GetDialogBaseByPath("meow");
            dsMeow.OnSkipped = (p) => {
                PersistentVariable.Set("cat.follow", 1f);
                Events.gInstance.RaiseEvent("cat.follow");
                dsMeow.defaultOnSkipped(p);
            };

            return ds;
        }
    }
}
