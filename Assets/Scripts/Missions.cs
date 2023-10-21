using static Gettext;

public static partial class Missions {
    
    public partial class Tutorial : Mission {
        public override string SceneName => "LevelEditor";
        public override string Name => _("Tutorial");
        public override string Description => _("A introduction into the game.");
    }
    
    public partial class FirstMission : Mission {
        public override string SceneName => "LevelEditor";
        public override bool IsAvailable => campaign.tutorial.isCompleted;
        public override string Name => _("First mission");
        public override string Description => _("A challenger appears!");
    }
    
    public partial class SecondMission : Mission {
        public override string SceneName => "LevelEditor";
        public override bool IsAvailable => campaign.firstMission.isCompleted;
        public override string Name => _("Second mission");
        public override string Description => _("Final push!");
    }
}