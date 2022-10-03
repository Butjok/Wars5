import time
from watchdog.observers import Observer
from watchdog.events import PatternMatchingEventHandler
from pathlib import Path
import json
from collections import OrderedDict


input_path = '/Users/butjok/Documents/GitHub/Wars5/Output.json'
output_path = '/Users/butjok/Documents/GitHub/Wars5/Input.txt'

known_colors = {
    (255, 87, 17): 'red',
    (134, 255, 0): 'green',
    (2, 81, 255): 'blue',
}
for (color, name) in list(known_colors.items()):
    known_colors[name] = color


def parse(event):

    if event.src_path != input_path:
        return

    text = Path(input_path).read_text()
    data = json.loads(text)

    tiles = {(entry['position']['x'], entry['position']['y']): entry['type'] for entry in data['tiles']}
    turn = data['turn']

    players = []
    lookup_player = dict()
    for player in data['players']:
        color = (player['color']['r'], player['color']['g'], player['color']['b'])
        if color in known_colors:
            color = known_colors[color]
        player['color'] = color
        lookup_player[player['id']] = player
        lookup_player[player['color']] = player
        players.append(player)

    current_player = players[data['turn'] % len(players)]
    credits = current_player['credits']

    units = []
    lookup_unit = dict()
    for unit in data['units']:
        unit['position'] = None if unit['position'] is None else (unit['position']['x'], unit['position']['y'])
        unit['player'] = lookup_player[unit['playerId']] if unit['playerId'] in lookup_player else None
        units.append(unit)
        lookup_unit[unit['id']] = unit
        if unit['position'] is not None:
            lookup_unit[unit['position']] = unit

    buildings = []
    lookup_building = dict()
    for building in data['buildings']:
        building['position'] = (building['position']['x'], building['position']['y'])
        building['player'] = lookup_player[building['playerId']] if building['playerId'] in lookup_player else None
        buildings.append(building)
        lookup_building[building['id']] = building
        lookup_building[building['position']] = building

    unmoved_units = list(filter(lambda unit: unit['player'] == current_player and not unit['moved'], units))
    accessible_buildings = list(filter(lambda building: building['player'] == current_player and building['position'] not in lookup_unit, buildings))
    factories = list(filter(lambda building: building['type'] == 'Factory', accessible_buildings))
    airports = list(filter(lambda building: building['type'] == 'Airport', accessible_buildings))
    seaports = list(filter(lambda building: building['type'] == 'Seaport', accessible_buildings))

    Path(output_path).write_text('''
        InputCommandsListener.Select 2 1;
        InputCommandsListener.ReconstructPath 1 0;
        InputCommandsListener.Move;
    ''')


    pass


if __name__ == "__main__":

    patterns = ["*"]
    ignore_patterns = None
    ignore_directories = False
    case_sensitive = True
    my_event_handler = PatternMatchingEventHandler(patterns, ignore_patterns, ignore_directories, case_sensitive)

    my_event_handler.on_modified = parse

    go_recursively = True
    my_observer = Observer()
    my_observer.schedule(my_event_handler, input_path, recursive=go_recursively)

    my_observer.start()
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        my_observer.stop()
        my_observer.join()
