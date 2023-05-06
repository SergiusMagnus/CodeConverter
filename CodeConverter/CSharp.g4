grammar CSharp;

/*
 * Parser Rules
 */

start               : method+ EOF ;
statements          : statement+ ;
statement           : (declaration | assignment | expression | returnExpr) ';' ;
declaration         : TYPE VAR_NAME ;
assignment          : (declaration | VAR_NAME) ASSIGNMENT_OPERATOR expression ;
expression          : (operator? (primaryLiteral | funcCall))+ ;
returnExpr          : 'return' expression ;
operator            : OPERATORS;
primaryLiteral      : NUMBER | VAR_NAME | STRING ;
controlConstructs   : controlConstruct+ ;
controlConstruct    : conditional | cycle ;
conditional         : 'if' '(' expression ')' (codeBlock | statement) (conditionalElif | conditionalElse)? ;
codeBlock           : '{' (statements | controlConstructs | method)+ '}' ;
conditionalElif     : 'else' 'if' '(' expression ')' (codeBlock | statement) (conditionalElif | conditionalElse)? ;
conditionalElse     : 'else' (codeBlock | statement) ;
cycle               : whileCycle ;
whileCycle          : 'while' '(' expression ')' (codeBlock | statement) ;
method              : TYPE VAR_NAME '('args')' codeBlock ;
args                : '(' (declaration ','?)* ')' ;
funcCall            : VAR_NAME argsCall ;
argsCall            : '(' (primaryLiteral ','?)* ')' ;

/*
 * Lexer Rules
 */

fragment LOWERCASE  : [a-z] ;
fragment UPPERCASE  : [A-Z] ;
fragment DIGITS     : [0-9] ;

fragment FIRST_ALLOW_NAME_SYMBOL : LOWERCASE | UPPERCASE | '_' ;
fragment ALLOW_NAME_SYMBOL : LOWERCASE | UPPERCASE | '_' | DIGITS ;

TYPE                : 'bool' | 'byte' | 'sbyte' | 'short' | 'ushort' | 'int' | 'uint' | 'long' | 'ulong' | 
                      'float' | 'double' | 'decimal' | 'char' | 'string' | 'object' | 'void' ;
ASSIGNMENT_OPERATOR : '=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '|=' | '^=' | '<<=' ;
OPERATORS           : '+' | '-' | '*' | '/' | '%' | '>>' | '<<' | '!' | '<' | '>' | '<=' | '>=' | '==' | '!=' | 
                      '&' | '^' | '|' | '&&' | '||' | '~' | '++' | '--' ;
NUMBER              : DIGITS+([.]DIGITS*)? ;
VAR_NAME            : FIRST_ALLOW_NAME_SYMBOL ALLOW_NAME_SYMBOL* ;
STRING              : '"' .* '"' ;