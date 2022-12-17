grammar AiScript;

program: expression* EOF;
expression
    : Integer #integer 
    | Symbol  #symbol
    | String  #string
    | (True | False) #boolean
    | '(' expression* ')' #list 
    | '\'' expression     #quote;

Whitespace: [ \r\n\t] -> skip;

Comment: ';' [ \r\n]* -> skip;
True: '#t';
False: '#f';
Integer: '-'? [0-9]+;
// no dot (.) for now
Symbol: [A-Za-z*/!><=+-]+;
// from CommandLine
String: '"' ('\\' ["\\bfnrt]  | ~ ["\\\u0000-\u001F])* '"';