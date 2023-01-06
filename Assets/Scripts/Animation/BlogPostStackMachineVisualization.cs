using Butjok.CommandLine;
using DG.Tweening;
using UnityEngine;

public class BlogPostStackMachineVisualization : MonoBehaviour {
    
    public RectTransform bottomWp, bottomWp1, bottomWp2, middleWp, leftWp, rightWp;
    public float speed = 200;
    public Ease verticalEase = Ease.InOutExpo;
    public Ease horizontalEase = Ease.InOutExpo;
    public float fadeDuration = .5f;
    public Ease fadeEase = Ease.InOutSine;
    
    public BlogPostStackMachineVisualizationStackValue item;
    public BlogPostStackMachineVisualizationStackValue item1;
    public BlogPostStackMachineVisualizationStackValue item2;
    public BlogPostStackMachineVisualizationStackValue item3;
    public BlogPostStackMachineVisualizationStackValue infix;
    public BlogPostStackMachineVisualizationStackValue randomRange;
    
    [Command]
    public void TestMove() {
        var sequence = DOTween.Sequence();
        item.Text = "1";
        item1.Text = "-90";
        item2.Text = "90";
        sequence.Append(item.Fade(1, fadeDuration, fadeEase));
        sequence.Append(item.Move(middleWp, bottomWp, speed, verticalEase));
        sequence.Append(item1.Fade(1, fadeDuration, fadeEase));
        sequence.Append(item1.Move(middleWp, bottomWp1, speed, verticalEase));
        sequence.Append(item2.Fade(1, fadeDuration, fadeEase));
        sequence.Append(item2.Move(middleWp, bottomWp2, speed, verticalEase));
        
        sequence.Append(item2.Move(bottomWp2, middleWp, speed, verticalEase));
        sequence.Append(item2.Move(middleWp, rightWp, speed, horizontalEase));
        
        // sequence.Append(item1.Move(bottomWp1, middleWp, speed, verticalEase));
        sequence.Append(item1.Move(bottomWp1, middleWp, speed, horizontalEase));

        // infix.Text = "+";
        // sequence.Append(infix.Fade(1, .15f, fadeEase));

        // var ss = DOTween.Sequence();
        // ss.Append(item2.Move(leftWp, middleWp, speed, horizontalEase));
        // ss.Join(item2.Fade(0, fadeDuration, fadeEase).SetDelay(.15f));
        //
        // var ss1 = DOTween.Sequence();
        // ss1.Append(item1.Move(rightWp, middleWp, speed, horizontalEase));
        // ss1.Join(item1.Fade(0, fadeDuration, fadeEase).SetDelay(.15f));
        //
        // sequence.Append(ss);
        // sequence.Join(ss1);
        // sequence.Join(infix.Fade(0, fadeDuration, fadeEase));
        //
        // item3.Text = "0";
        // sequence.Join(item3.Fade(1, fadeDuration, fadeEase));

        sequence.Append(randomRange.Fade(1, fadeDuration, fadeEase));
    }
}