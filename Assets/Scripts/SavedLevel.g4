grammar SavedLevel;

input: statement* EOF;
statement: push | tiles | player | unit | building | bridge;
pop: 'pop';
push: 'push' expression;
tiles: 'tiles' Identifier '{' int2* '}';
int2: '(' Integer ',' Integer ')';
player: 'player'


Identifier: [a-zA-Z_][a-zA-Z_0-9]* ('.' [a-zA-Z_][a-zA-Z_0-9]*)*;
Whitespace: [ \r\n\t]+ -> skip;
BlockComment: '/*' .*? '*/' -> skip;
LineComment: '//' ~[\r\n]* -> skip;
Integer: '-'? INT;
Real: '-'? (INT '.' INT | '.' INT | INT '.'); 
fragment INT: [0-9]+;