import json
from pathlib import Path

red = (255,44,0)
green = (134,255,0)
blue = (36,96,255)

def parse(path):

    text = Path(path).read_text()
    data = json.loads(text)

    data['tiles'] = {(entry['position']['x'], entry['position']['y']): entry['type'] for entry in data['tiles']}

    players = dict()
    for player in data['players']:
        player['color'] = (player['color']['r'], player['color']['g'], player['color']['b'])
        players[player['color']] = players[player['id']] = player
    data['players'] = players

    units = dict()
    for unit in data['units']:
        unit['position'] = None if unit['position'] is None else (unit['position']['x'], unit['position']['y'])
        units[unit['id']] = unit
        if unit['position'] is not None:
            units[unit['position']] = unit
        unit['player'] = players[unit['playerId']] if unit['playerId'] in players else None
    data['units'] = units

    buildings = dict()
    for building in data['buildings']:
        building['position'] = (building['position']['x'], building['position']['y'])
        building['player'] = players[building['playerId']] if building['playerId'] in players else None
        buildings[building['id']] = buildings[building['position']] = building
    data['buildings'] = buildings

    return data

input = '/Users/butjok/Documents/GitHub/Wars5/Assets/Out.json'
output = '/Users/butjok/Documents/GitHub/Wars5/Assets/File.json'

if __name__ == '__main__':
    level = parse(input)
    points = {unit['position'] for unit in level['units'].values() if unit['player']['color'] == green}
    out = {
        'points': [{'x': point[0], 'y': point[1]} for point in points]
    }
    Path(output).write_text(json.dumps(out))