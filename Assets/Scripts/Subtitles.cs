using System.Collections;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Gettext;

public class Subtitles : MonoBehaviour {

    public GameObject root;
    public Image portrait;
    public TMP_Text speechText;
    public TMP_Text speakerNameText;
    public TMP_Text pressSpaceText;
    public float speed = 50;

    public Sprite natalieWelcoming;
    public Sprite natalieExplaining;
    public Sprite nataliePointing;

    public Sprite Portrait {
        set => portrait.sprite = value;
    }
    public string SpeakerName {
        set => speakerNameText.text = value;
    }
    public string Text => speechText.text;

    [Command]
    public void Play() {
        root.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Coroutine());
    }

    public IEnumerator Coroutine() {
        SpeakerName = _("Natalie");

        Portrait = natalieWelcoming;
        yield return Say(_("Welcome back, Commander! [...]"));
        yield return Say(_("[...] I hope you had a great time on your vacation. Having all those fancy cocktails didn't make you to forget how to command your troops right? *giggles*"));

        Portrait = natalieExplaining;
        yield return Say(_("Anyway, returning to the business... Not much has changed since you left."));

        Portrait = nataliePointing;
        yield return Say(_("Our troops are still holding their positions, so do the enemy troops. The overall situation on out direction is stable."));
        yield return Say(_("The most action is happening in the central sector. The enemy is trying to break through our defenses there. Villages and cities are being captured and recaptured almost every day."));

        Portrait = natalieExplaining;
        yield return Say(_("The upper command wants us to continue holding out current positions and wait for further orders. [...]"));
        yield return Say(_("[...] So we should not expect any major changes in the near future. [...]"));

        Portrait = natalieWelcoming;
        yield return Say(_("[...] Not a bad time to get back into the swing of things, right?"));

        Portrait = nataliePointing;
        yield return Say(_("Here we are at "));
        
        root.SetActive(false);
    }

    public YieldInstruction Pause(float duration = 1) {
        return new WaitForSeconds(duration);
    }
    public IEnumerator WaitSpace() {
        pressSpaceText.enabled = true;
        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;
        pressSpaceText.enabled = false;
    }

    public IEnumerator Say(string message) {
        var append = false;
        var oldText = speechText.text;
        if (message.StartsWith("[...]")) {
            yield return Pause();
            message = message.Substring(5).TrimStart();
            message = ' ' + message;
            append = true;
        }

        bool waitAnyKeyDown;
        if (message.EndsWith("[...]")) {
            message = message.Substring(0, message.Length - 5).TrimEnd();
            waitAnyKeyDown = false;
        }
        else
            waitAnyKeyDown = true;

        var length = message.Length;
        var duration = length / speed;

        for (var timeLeft = duration; timeLeft > 0; timeLeft -= Time.deltaTime) {
            speechText.text = (append ? oldText : "") + message.Substring(0, Mathf.Clamp(length - Mathf.RoundToInt(timeLeft * speed), 0, length));
            yield return null;
        }
        speechText.text = (append ? oldText : "") + message;
        if (waitAnyKeyDown)
            yield return WaitSpace();
    }
}