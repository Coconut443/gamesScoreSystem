grammar Interpreter;

/*
 * Parser Rules
 */

//程序
prog	: (stat? NEWLINE)* EOF;

//一个完整的执行语句
stat	: expr;

//表达式
expr	: (subject ('.' function)*);

//函数
function : ID '(' params ')';

//主体（有时也匹配到函数）
subject	: ID ('(' params ')')?;

//参数
param	: ID | NUM | STRING | (ID '.' ID) | expr;

//参数列表
params	: (param (',' param)*)?;

/*
 * Lexer Rules
 */

//ID，可能是数据库名、字段名、特殊意义的标识符等
ID	: LETTER (LETTER | DIGIT | '_')*;

//一串数字
NUM	: DIGIT+;

//字符串
STRING	: ('"' (ESC | .)*? '"') | ('\'' (ESC | .)*? '\'');

//标识一个新行
NEWLINE : '\r'? '\n';

//跳过注释
COMMENT : '//' .*? '\r'? '\n' -> skip;

//跳过空白符号
WS		: (' '|'\t')+ -> skip;

//字母
fragment LETTER : [a-zA-Z];

//数字
fragment DIGIT	: [0-9];

//转义符号
fragment ESC	: '\\' [btnr"\\];