using System.Collections;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PortraitStack : ImageStack {

    public Image TryFindImage(PersonName personName) {
        return images.SingleOrDefault(i => i.name == personName.ToString());
    }

    [Command]
    public (Image image, IEnumerator coroutine) AddPerson(PersonName personName, Mood mood = default) {
        Assert.IsTrue(!TryFindImage(personName));
        var (image, coroutine) = AddImage(personName.ToString());
        image.sprite = Persons.TryGetPortrait(personName, mood);
        return (image, coroutine);
    }

    [Command]
    public IEnumerator RemovePerson(PersonName personName) {
        var image = TryFindImage(personName);
        Assert.IsTrue(image);
        return RemoveImage(image);
    }

    [Command]
    public void SetMood(PersonName personName, Mood mood) {
        var image = TryFindImage(personName);
        Assert.IsTrue(image);
        image.sprite = Persons.TryGetPortrait(personName, mood);
    }
}