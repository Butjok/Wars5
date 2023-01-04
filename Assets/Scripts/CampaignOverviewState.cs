using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public static class CampaignOverviewState {
    public static IEnumerator New() {

        var view = Object.FindObjectOfType<CampaignView>();
        Assert.IsTrue(view);

        var persistentData = PersistentData.Read();
        var campaign = persistentData.campaign;

        view.Show();
        view.Actualize(campaign);

        while (true) {
            yield return null;
            
        }

        view.Hide();
    }
}