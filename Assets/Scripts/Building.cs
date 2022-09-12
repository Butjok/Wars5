using UnityEngine;
using UnityEngine.Assertions;

public class Building {

	public TileType type;
	public Game2 game;
	public Vector2Int position;
	public ChangeTracker<Player> player;
	public ChangeTracker<int> cp ;

	public Building(Game2 game, Vector2Int position, TileType type = TileType.City, Player player = null) {

		this.player = new ChangeTracker<Player>(_ => { });
		this.cp = new ChangeTracker<int>(_ => { });
		
		this.type = type;
		this.game = game;
		this.position = position;
		this.player.v = player;
		Assert.IsFalse(game.buildings.ContainsKey(position));

		Assert.IsTrue(!game.buildings.ContainsKey(position) || game.buildings[position] == null);
		game.buildings[position] = this;
		game.tiles[position] = type;
	}

	public static implicit operator TileType(Building building) {
		return building.type;
	}
	
	public override string ToString() {
		return $"{type}{position} {player.v}";
	}
}