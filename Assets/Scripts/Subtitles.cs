using System.Collections;
using System.Collections.Generic;
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
    public Sprite natalieAsking;
    public Sprite natalieBusy;
    public Sprite natalieBusyNotices;

    public AudioSource voiceOverSource;

    public AudioClip[] voiceOverClips = { };

    public Sprite Portrait {
        set => portrait.sprite = value;
    }
    public string SpeakerName {
        set => speakerNameText.text = value;
    }
    public string Text => speechText.text;

    public bool Visible {
        set {
            root.SetActive(value);
            speechText.text = "";
        }
    }

    [Command]
    public void Play() {
        Visible = true;
        StopAllCoroutines();
        StartCoroutine(Coroutine());
    }

    public IEnumerator Coroutine() {

        PlayerThemeAudio.ToneDownMusic = true;

        var voiceOverClips = new VoiceOverClipSequence{ voiceOverClips = this.voiceOverClips };
        
        SpeakerName = _("Natalie");

        Portrait = natalieBusy;
        yield return Pause(1);
        Portrait = natalieBusyNotices;
        yield return Pause(.75f);

        Portrait = natalieWelcoming;
        yield return Say(_("Commander, welcome back! [...]"), voiceOverClips.Next);
        yield return Say(_("[...] I hope you had a great time on your vacation! [...]"), voiceOverClips.Next);
        yield return Say(_("[...] Having all those fancy cocktails didn't make you to forget how to command your troops right? *giggles*"), voiceOverClips.Next);

        Portrait = natalieExplaining;
        yield return Say(_("Anyway, returning to the business... Not much has changed since you left."), voiceOverClips.Next);

        Portrait = nataliePointing;
        yield return Say(_("Our troops are still holding their positions, so do the enemy troops. The overall situation on our direction is stable."), voiceOverClips.Next);
        yield return Say(_("The most action is happening in the central sector under the command of general staff. The enemy is trying to break through our defenses there. Villages and cities are being captured and recaptured almost every day."), voiceOverClips.Next);

        Portrait = natalieExplaining;
        yield return Say(_("The upper command wants us to continue holding out current positions and wait for further orders. [...]"), voiceOverClips.Next);
        yield return Say(_("[...] So we should not expect any major changes in the near future."), voiceOverClips.Next);

        Portrait = natalieWelcoming;
        yield return Say(_("Not a bad time to get back into the swing of things, right?"), voiceOverClips.Next);

        Portrait = nataliePointing;
        yield return Say(_("Here we are at the south. Our troops are holding the line over here. The situation was quite calm for the last few days."), voiceOverClips.Next);
        yield return Say(_("The enemy positions are located in the north over here."), voiceOverClips.Next);

        Portrait = natalieAsking;
        yield return Say(_("For the first move I would suggest to send a scout to the north to gather some information about the enemy positions."), voiceOverClips.Next);

        Portrait = natalieWelcoming;
        yield return Say(_("So... Let's get us started, shall we?"), voiceOverClips.Next);

        Visible = false;
        
        PlayerThemeAudio.ToneDownMusic = false;
    }

    public class VoiceOverClipSequence {
        public int index;
        public AudioClip[] voiceOverClips;
        public AudioClip Next => index >= voiceOverClips.Length ? null : voiceOverClips[index++];
    }

    public YieldInstruction Pause(float duration = 2) {
        return new WaitForSeconds(duration);
    }
    public IEnumerator WaitSpace() {
        pressSpaceText.enabled = true;
        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;
        pressSpaceText.enabled = false;
        voiceOverSource.Stop();
    }

    public IEnumerator Say(string message, AudioClip voiceOverClip = null) {
        var append = false;
        var oldText = speechText.text;
        if (message.StartsWith("[...]")) {
            while (voiceOverSource.isPlaying) {
                if (Input.GetKeyDown(KeyCode.Space)) {
                    voiceOverSource.Stop();
                    break;
                }
                yield return null;
            }
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

        voiceOverSource.Stop();
        if (voiceOverClip)
            voiceOverSource.PlayOneShot(voiceOverClip);

        for (var timeLeft = duration; timeLeft > 0; timeLeft -= Time.deltaTime) {
            speechText.text = (append ? oldText : "") + message.Substring(0, Mathf.Clamp(length - Mathf.RoundToInt(timeLeft * speed), 0, length));
            yield return null;
        }
        speechText.text = (append ? oldText : "") + message;
        if (waitAnyKeyDown)
            yield return WaitSpace();
    }
}