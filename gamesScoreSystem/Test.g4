grammar Test;

/*
������ôдѽ��
	*/

init	: '{' value (',' value)* '}';
value	: init 
		| intvalue
		;
intvalue	: INT;

INT	: [0-9]+;
WS	: [ \t\r\n]+ -> skip ;