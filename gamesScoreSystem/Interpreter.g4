grammar Interpreter;

/*
 * Parser Rules
 */

prog	: (stat? NEWLINE)* EOF;
stat	: expr;
expr	: (subject ('.' function)*);
function : ID '(' params ')';
subject	: ID ('(' params ')')?;
param	: ID | NUM | STRING | (ID '.' ID) | expr;
params	: (param (',' param)*)?;

/*
 * Lexer Rules
 */

ID	: LETTER (LETTER | DIGIT | '_')*;
NUM	: DIGIT+;
STRING	: ('"' (ESC | .)*? '"') | ('\'' (ESC | .)*? '\'');

NEWLINE : '\r'? '\n';
COMMENT : '//' .*? '\r'? '\n' -> skip;
WS		: (' '|'\t')+ -> skip;

fragment LETTER : [a-zA-Z];
fragment DIGIT	: [0-9];
fragment ESC	: '\\' [btnr"\\];