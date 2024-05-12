using System.Collections;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public static class LevelEditorFacade {

    public struct Objects {

        public TileMapCreator tileMapCreator;
        public ForestCreator forestCreator;
        public TerrainMapper terrainMapper;
        public RoadCreator roadCreator;
        public PropPlacement propPlacement;
        public LevelEditorSessionState levelEditorSessionState;
        public HeightMapBaker heightMapBaker;
        public GameObject[] staticPropsRoots;
        public GameObject[] allStaticPropsRoots;

        public static Objects Find(string saveName) {
            var game = Object.FindObjectOfType<Game>();
            Assert.IsTrue(game);
            var levelEditorSessionState = game.stateMachine.Find<LevelEditorSessionState>();

            var result = new Objects() {
                tileMapCreator = Object.FindObjectOfType<TileMapCreator>(),
                forestCreator = Object.FindObjectOfType<ForestCreator>(),
                terrainMapper = Object.FindObjectOfType<TerrainMapper>(),
                roadCreator = Object.FindObjectOfType<RoadCreator>(),
                propPlacement = Object.FindObjectOfType<PropPlacement>(),
                levelEditorSessionState = levelEditorSessionState,
                heightMapBaker = Object.FindObjectOfType<HeightMapBaker>(),
                allStaticPropsRoots = Object.FindObjectsOfType<GameObject>(true).Where(go => go.name.EndsWith("-StaticProps")).ToArray(),
                staticPropsRoots = Object.FindObjectsOfType<GameObject>(true).Where(go => go.name == saveName + "-StaticProps").ToArray(),
            };
            Assert.IsTrue(result.tileMapCreator);
            Assert.IsTrue(result.forestCreator);
            Assert.IsTrue(result.terrainMapper);
            Assert.IsTrue(result.roadCreator);
            Assert.IsTrue(result.propPlacement);

            return result;
        }
    }

    public static string Path(string saveName) => System.IO.Path.Combine(Application.streamingAssetsPath, saveName + ".save");

    [Command]
    public static void Save(string saveName) {
        var objects = Objects.Find(saveName);
        objects.tileMapCreator.Save(saveName + "-Tiles");
        objects.forestCreator.Save(saveName + "-Forests");
        objects.terrainMapper.SaveBushes(saveName + "-Bushes");
        objects.roadCreator.Save(saveName + "-Roads");
        objects.propPlacement.Save(saveName + "-Props");
        var levelWriter = new LevelWriter(new StringWriter());
        levelWriter.WriteLevel(objects.levelEditorSessionState.level);
        //LevelEditorFileSystem.Save(saveName + "-Level", levelWriter.tw.ToString());

        var commands = SaveGame.CommandEmitter.Emit(objects.levelEditorSessionState.level);
        var stringWriter = new StringWriter();
        SaveGame.TextFormatter.Format(stringWriter, commands);
        var text = stringWriter.ToString();
        LevelEditorFileSystem.Save(saveName + "-Level", text);
    }

    [Command]
    public static bool TryLoad(string saveName) {
        var objects = Objects.Find(saveName);
        if (!objects.tileMapCreator.TryLoad(saveName + "-Tiles"))
            return false;
        if (!objects.forestCreator.TryLoad(saveName + "-Forests"))
            return false;
        if (!objects.terrainMapper.TryLoadBushes(saveName + "-Bushes"))
            return false;
        if (!objects.roadCreator.TryLoad(saveName + "-Roads"))
            return false;
        if (!objects.propPlacement.TryLoad(saveName + "-Props"))
            return false;
        var levelText = LevelEditorFileSystem.TryReadLatest(saveName + "-Level");
        if (levelText == null)
            return false;

        LevelEditorSessionState.Load(saveName + "-Level");

        if (objects.heightMapBaker)
            objects.heightMapBaker.Bake();

        foreach (var staticPropsRoot in objects.allStaticPropsRoots)
            staticPropsRoot.SetActive(false);
        foreach (var staticPropsRoot in objects.staticPropsRoots)
            staticPropsRoot.SetActive(true);

        return true;
    }
}