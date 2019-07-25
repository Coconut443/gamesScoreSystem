grammar Test;

/*
µ½µ×ÔõÃ´Ğ´Ñ½£¨
	*/

init	: '{' value (',' value)* '}';
value	: init 
		| intvalue
		;
intvalue	: INT;

INT	: [0-9]+;
WS	: [ \t\r\n]+ -> skip ;