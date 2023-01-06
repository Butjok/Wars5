using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerCollection_ : IEnumerable<Player> {

    private readonly List<Player> list = new();
    private readonly Dictionary<Color32, Player> lookup = new();

    public int Count => list.Count;

    public void Add(Player player) {
        list.Add(player);
        lookup.Add(player.color, player);
    }
    public Player this[int index] {
        get {
            Assert.IsTrue(list.Count > 0, index.ToString());
            return list[index.PositiveModulo(list.Count)];
        }
    }
    public bool TryGet(Color32 color, out Player result) {
        return lookup.TryGetValue(color, out result);
    }
    public Player this[Color32 color] {
        get {
            var found = TryGet(color, out var player);
            Assert.IsTrue(found);
            return player;
        }
    }

    public IEnumerator<Player> GetEnumerator() => list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)list).GetEnumerator();
}