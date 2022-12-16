grammar Lisp;

value: ;

LeftParenthesis:    '(';
RightParenthesis:   ')';

Whitespace: [ \r\n\t]+ -> skip;
COMMENT: ';' [^\r\n]* -> skip;