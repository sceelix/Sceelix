grammar SceelixGrammar;

options
{
    language=CSharp3;
    output=AST;
// ANTLR can handle literally any tree node type.
// For convenience, specify the Java type
    ASTLabelType=CommonTree; // type of $stat.tree ref etc...
}

tokens { 
TYPE; 
} // imaginary token

@lexer::namespace {Sceelix.Designer.Graphs.ExpressionParsing}

@parser::namespace {Sceelix.Designer.Graphs.ExpressionParsing}

@members
{
	public override void ReportError(RecognitionException e)
    {
        throw e;
    }
}

public expr 	
	: logicalExpression EOF!;
	
	

logicalExpression
    :    booleanAndExpression ( OR^ booleanAndExpression )*
    ;
    
booleanAndExpression
    :    equalityExpression ( AND^ equalityExpression )*
    ;
    
equalityExpression
    :    relationalExpression ( 
    		(EQUALS^ 
    		| NOTEQUALS^) relationalExpression)*
    ;
    
relationalExpression
    :    additiveExpression ( (LT^ 
    				| LTEQ^ 
    				| GT^ 
    				| GTEQ^) additiveExpression)*
    ;

additiveExpression
    :    multiplicativeExpression 
    		( (PLUS^ 
    		| MINUS^ ) 
    		multiplicativeExpression )*
    ;

multiplicativeExpression
    :    unaryExpression (( MULT^ | DIV^ | MOD^ ) unaryExpression)*
    ;

unaryExpression
    :    (NOT^|MINUS^)? arrayExpression;
    
arrayExpression 
	:	primaryExpression (SQBRACKETOPEN^ logicalExpression SQBRACKETCLOSE)*;
	
/*memberExpression
	:  primaryExpression  (DOT propertyOrFunction);*/

primaryExpression
    :    
    '(' logicalExpression ')' -> ^(TYPE["()"] logicalExpression)
    |   value    
    |	parameter
    |   attribute
    | internalattribute
    //|   attributeLocal
    |   ID ('(' args? ')')	-> ^(TYPE["DirectFunction"] ID args?)
    |   array;
    //    |	classParameterOrAttribute (DOT propertyOrFunction)* -> ^(TYPE["ObjectMember"] classParameterOrAttribute propertyOrFunction*)
        
//lets take a look at this later    
array 	: SQBRACKETOPEN arraykeyValues? SQBRACKETCLOSE	-> ^(TYPE["Array"] arraykeyValues?);

	
arraykeyValues
	:	 keyvalue (',' keyvalue )* -> ^(TYPE["ArrayKeyValue"] keyvalue*);

	
keyvalue 
	:	logicalExpression (COLON^ logicalExpression)? ;
	
    //| ID (DOT^ secondPartMember)? ^(TYPE["StaticFunction"] ID secondPartMember)
    //|   parameter
    //|	portAttribute
    //|   isolatedFunctionCall
    //|	ID DOT member -> ^(TYPE["StaticFunction"] ID member)
/*classParameterOrAttribute 
	:	 ID DOT propertyOrFunction -> ^(TYPE["StaticMember"] ID propertyOrFunction)
		| parameter
		| attribute		
		| specialData;*/
	
className 
	:	ID	-> ^(TYPE["ClassName"] ID);

propertyOrFunction	:	  ID 			-> ^(TYPE["Property"] ID) 
				| ID ('(' args? ')')	-> ^(TYPE["Function"] ID args?) 
				;
   
args 	:   logicalExpression (',' logicalExpression )* -> ^(TYPE["Args"] logicalExpression*);

/*objectAccess
	:	classParameterOrAttribute (DOT^ secondPartMember);
		
secondPartMember 
	:	 propertyOrFunction (DOT^ secondPartMember)?;*/
	
	

/*member
	: functionOrMember (DOT^ functionOrMember)* ;*/

/*functionOrMember
	:	ID ('(' args? ')')?;*/
    
/*isolatedFunctionCall 
	: ID DOT functionCall -> ^(TYPE["StaticFunction"] ID functionCall);
	
functionCall:	
	ID '(' args? ')' (memberAccess)? -> ^(ID ^(TYPE["Args"] args)? memberAccess?);

memberAccess 
	:	DOT 
	( ID	memberAccess? -> ^(TYPE["Member"] ID memberAccess?)
	| functionCall -> ^(TYPE["MemberFunctionCall"] functionCall))
	;*/
	
		
value    : 
   INT -> ^(TYPE["Int"] INT )
    |    FLOAT -> ^(TYPE["Float"] FLOAT )
    |    STRING -> ^(TYPE["String"] STRING )
    |    BOOLEAN  -> ^(TYPE["Boolean"] BOOLEAN )
    | 	DOUBLE  -> ^(TYPE["Double"] DOUBLE )
    | 	CHAR  -> ^(TYPE["Char"] CHAR )
    ;

//globalparameter 
//	:	 SPECIALID  ^(TYPE["Parameter"] SPECIALID);
	

//FOR FUTURE DEVELOPERS:
//I first tried to use the ATTRID to setup this local attribute thingy
//however ANTLR throws NotRecognizedExceptions when I try to use normal
//attributes for some reason.
attribute 
	:	 '@' ID CHECK? -> ^(TYPE["Attribute"] ID CHECK?);
	
internalattribute 
	:	 '@@' ID CHECK? -> ^(TYPE["InternalAttribute"] ID CHECK?);
	
//attributeLocal 
//	:	 '@local:' ID  -> ^(TYPE["LocalAttribute"] ID);

//globalattribute 
//	:	 '$' ID  -> ^(TYPE["GlobalAttribute"] ID);
		
parameter 
	:	 ID -> ^(TYPE["Parameter"] ID);
	
specialData 
	:	'$' ID '[' logicalExpression ']' -> ^(TYPE["SpecialData"] ID logicalExpression);
	
/*
portAttribute
	:	 '$' SPECIALID SPECIALID -> ^(TYPE["PortAttribute"] SPECIALID SPECIALID); 	//(memberAccess)?-> ^(TYPE["PortAttribute"] SPECIALID SPECIALID (memberAccess)?;
	
objectData
	:	 '$' SPECIALID -> ^(TYPE["ObjectData"] SPECIALID);*/
	
CHECK 	:	 '?';

COLON 	:	 
		':';	

SQBRACKETOPEN 
	:	 '[';

SQBRACKETCLOSE 
	:	 ']';	
	
SPECIALID 
	:	 '{' ~('{'|'}')* '}';
	
DOT 	:	 '.';

OR    :     '||';

AND   :     '&&';

EQUALS      :    '==';
NOTEQUALS   :    '!=';

GTEQ  :    '>=';
LTEQ  :    '<=';

LT    :    '<';
GT    :    '>';

NOT	:  '!';

PLUS  :    '+';
MINUS :    '-';
MULT  :    '*';
DIV   :    '/';
MOD   :    '%';

BOOLEAN
    :    'true'
    |    'false'
    ;


ID  :	('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_')*
    ;
 
ATTRID 	: LOCAL? ID
	;

LOCAL 	: 'local:';
	

INT :	'0'..'9'+
    ;

FLOAT
    :   ('0'..'9')+ '.' ('0'..'9')+ EXPONENT? 'f'
    |   '.' ('0'..'9')+ EXPONENT? 'f'
    |   ('0'..'9')+ EXPONENT? 'f'
    ;
    
DOUBLE
    :   ('0'..'9')+ '.' ('0'..'9')+ EXPONENT?
    |   '.' ('0'..'9')+ EXPONENT? 
    |   ('0'..'9')+ EXPONENT?
    ;
  
COMMENT
    :   '//' ~('\n'|'\r')* '\r'? '\n' {$channel=Hidden;}
    |   '/*' ( options {greedy=false;} : . )* '*/' {$channel=Hidden;}
    ;

WS  :   ( ' '
        | '\t'
        | '\r'
        | '\n'
        ) {$channel=Hidden;}
    ;
  
/*STRING
     :    '\'' (~ '\'' )* '\''
     ;*/
     
STRING
    :  '"' ( ESC_SEQ | ~('\\'|'"') )* '"'
    ;

CHAR:  '\'' ( ESC_SEQ | ~('\''|'\\') ) '\''
    ;

fragment
EXPONENT : ('e'|'E') ('+'|'-')? ('0'..'9')+ ;

fragment
HEX_DIGIT : ('0'..'9'|'a'..'f'|'A'..'F') ;


fragment
ESC_SEQ
    :   '\\' ('b'|'t'|'n'|'f'|'r'|'\"'|'\''|'\\')
    |   UNICODE_ESC
    |   OCTAL_ESC
    ;

fragment
OCTAL_ESC
    :   '\\' ('0'..'3') ('0'..'7') ('0'..'7')
    |   '\\' ('0'..'7') ('0'..'7')
    |   '\\' ('0'..'7')
    ;

fragment
UNICODE_ESC
    :   '\\' 'u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
    ;


