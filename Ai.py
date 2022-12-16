#!/usr/local/bin/python3
import json

input_file_name = 'Output.json'

with open(input_file_name) as file:
	input = json.load(file)

if __name__ == '__main__':
	print('Hello!')