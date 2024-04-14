using static Gettext;

public static partial class Missions {
    
    public class Tutorial : Mission {
        public override string SceneName => "MarchingSquares";
        public override string HumanFriendlyName => _("Tutorial");
        public override string Description => _("A introduction into the game.");
        public override string StartInput => "tutorial";
    }
    
    public class FirstMission : Mission {
        public override string SceneName => "MarchingSquares";
        public override bool IsAvailable => campaign.tutorial.isCompleted;
        public override string HumanFriendlyName => _("First mission");
        public override string Description => _("A challenger appears!");
        public override string StartInput => "tutorial";
    }
    
    public class SecondMission : Mission {
        public override string SceneName => "MarchingSquares";
        public override bool IsAvailable => campaign.firstMission.isCompleted;
        public override string HumanFriendlyName => _("Second mission");
        public override string Description => _("Final push!");
        public override string StartInput => "tutorial";
    }
}