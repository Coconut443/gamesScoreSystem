grammar Interpreter;

/*
 * Parser Rules
 */

//����
prog	: (stat? NEWLINE)* EOF;

//һ��������ִ�����
stat	: expr;

//���ʽ
expr	: (subject ('.' function)*);

//����
function : ID '(' params ')';

//���壨��ʱҲƥ�䵽������
subject	: ID ('(' params ')')?;

//����
param	: ID | NUM | STRING | (ID '.' ID) | expr;

//�����б�
params	: (param (',' param)*)?;

/*
 * Lexer Rules
 */

//ID�����������ݿ������ֶ�������������ı�ʶ����
ID	: LETTER (LETTER | DIGIT | '_')*;

//һ������
NUM	: DIGIT+;

//�ַ���
STRING	: ('"' (ESC | .)*? '"') | ('\'' (ESC | .)*? '\'');

//��ʶһ������
NEWLINE : '\r'? '\n';

//����ע��
COMMENT : '//' .*? '\r'? '\n' -> skip;

//�����հ׷���
WS		: (' '|'\t')+ -> skip;

//��ĸ
fragment LETTER : [a-zA-Z];

//����
fragment DIGIT	: [0-9];

//ת�����
fragment ESC	: '\\' [btnr"\\];