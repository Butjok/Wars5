grammar Serializer;

int2: '('Integer ',' Integer')';

color: 'Red' | 'Blue' | 'Green';
savedGame: (tile | building)* player+ ('turn' Integer)? EOF;

tile: type=('Plain' | 'Road' | 'Sea') 'at' int2+;

unit: type=('Infantry' | 'AntiTank' | 'Recon' | 'Apc') unitProperty*;
unitProperty
    : Integer 'hp'          #unitPropertyHp
    | 'at' int2             #unitPropertyPosition
    | (Integer | Real) 'deg' #unitPropertyRotation
    | 'moved' #unitPropertyMoved
    | Integer 'ammo' #unitPropertyAmmo
    | Integer 'fuel' #unitPropertyFuel
    | '[' unit* ']' #unitCargo
    ;

player: color playerProperty* (unit | building)*;
playerProperty
    : Integer 'cr'                             #playerPropertyCredits
    | difficulty=('Easy' | 'Normal' | 'Hard') 'Ai'  #playerPropertyAi
    | 'Human'                                       #playerPropertyHuman
    | ('Natalie' | 'Vladan')                        #playerPropertyCo
    ;

building: type=('Hq' | 'City' | 'Factory') buildingProperty*;
buildingProperty
    : 'at' int2
    | Integer 'cp'
    ;

Whitespace: [ \r\n\t]+ -> skip;
BlockComment: '/*' .*? '*/' -> skip;
LineComment: '//' ~[\r\n]* -> skip;

Identifier: [a-zA-Z_][a-zA-Z_0-9]*;
Integer: '-'? INT;
Real: '-'? (INT '.' INT | '.' INT | INT '.'); 
String: '"' ('\\' ["\\bfnrt]  | ~ ["\\\u0000-\u001F])* '"';

fragment INT: [0-9]+;
