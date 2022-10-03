import time
from watchdog.observers import Observer
from watchdog.events import PatternMatchingEventHandler
from pathlib import Path
import json


path = '/Users/butjok/Documents/GitHub/Wars5/Output.json'


def parse():

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


def on_modified(event):

    if event.src_path != path:
        return

    data = parse()

    pass


if __name__ == "__main__":

    patterns = ["*"]
    ignore_patterns = None
    ignore_directories = False
    case_sensitive = True
    my_event_handler = PatternMatchingEventHandler(patterns, ignore_patterns, ignore_directories, case_sensitive)

    my_event_handler.on_modified = on_modified

    go_recursively = True
    my_observer = Observer()
    my_observer.schedule(my_event_handler, path, recursive=go_recursively)

    my_observer.start()
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        my_observer.stop()
        my_observer.join()
