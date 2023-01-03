using System;
using UnityEngine;

public class CampaignViewTest : MonoBehaviour {
    private void Start() {
        StartCoroutine(CampaignOverviewState.New());
    }
}