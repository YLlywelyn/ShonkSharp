###############
### IMPORTS ###
###############

from __future__ import annotations
import sys, signal
from enum import Enum

#################
### CONSTANTS ###
#################

DIGITS = '1234567890'

##############
### ERRORS ###
##############

class ShorkError(Exception):
    def __init__(self, startPosition:Position, endPosition:Position, errorName:str, details:str) -> None:
        self.startPosition = startPosition
        self.endPosition = endPosition
        self.errorName = errorName
        self.details = details
    
    def __repr__(self) -> str:
        return f"""{self.errorName}: {self.details}
File: {self.startPosition.filename}, Line {self.startPosition.line}"""

class IllegalCharacterError(ShorkError):
    def __init__(self, startPosition:Position, endPosition:Position, details: str) -> None:
        super().__init__(startPosition, endPosition, "Illegal Character", details)

class InvalidSyntaxError(ShorkError):
    def __init__(self, startPosition: Position, endPosition: Position, details: str) -> None:
        super().__init__(startPosition, endPosition, "Invalid Syntax", details)

################
### POSITION ###
################

class Position:
    def __init__(self, index:int, line:int, column:int, filename:str, filetext:str) -> None:
        self.index = index
        self.line = line
        self.column = column
        self.filename = filename
        self.filetext = filetext
    
    def Advance(self, currentChar=None) -> Position:
        self.index += 1
        self.column += 1

        if currentChar == '\n':
            self.line += 1
            self.column = 0
        
        return self
    
    def Copy(self) -> Position:
        return Position(self.index, self.line, self.column, self.filename, self.filetext)

##############
### TOKENS ###
##############

class TokenType(Enum):
    INT         = 1
    FLOAT       = 2

    PLUS        = 4
    MINUS       = 8
    MULTIPLY    = 16
    DIVIDE      = 32

    LPAREN      = 64
    RPAREN      = 128

    EOF         = 256

class Token:
    def __init__(self, tokenType:TokenType, value:any = None, startPosition:Position = None, endPosition:Position = None) -> None:
        self.tokenType = tokenType
        self.value = value

        if startPosition:
            self.startPosition = startPosition.Copy()
            if not endPosition:
                self.endPosition = startPosition.Copy().Advance()
        
        if endPosition:
            self.endPosition = endPosition.Copy()
    
    def __repr__(self) -> str:
        if self.value: return f'{self.tokenType}:{self.value}'
        else: return f'{self.tokenType}'

#############
### LEXER ###
#############

class Lexer:
    @staticmethod
    def  Lex(text:str, filename:str) -> list[Token]:
        return Lexer(text, filename).MakeTokens()

    def __init__(self, text: str, filename:str) -> None:
        self.text:str = text
        self.position:Position = Position(-1, 0, -1, filename, text)
        self.currentChar:str = None
        self.Advance()
    
    def Advance(self) -> None:
        self.position.Advance(self.currentChar)
        self.currentChar = self.text[self.position.index] if self.position.index < len(self.text) else None
    
    def MakeTokens(self) -> list[Token]:
        tokens = []

        while self.currentChar != None:
            if self.currentChar in ' \t':
                self.Advance()
            
            elif self.currentChar in DIGITS:
                tokens.append(self.MakeNumber())
            
            elif self.currentChar == '+':
                tokens.append(Token(TokenType.PLUS, startPosition=self.position))
                self.Advance()
            elif self.currentChar == '-':
                tokens.append(Token(TokenType.MINUS, startPosition=self.position))
                self.Advance()
            elif self.currentChar == '*':
                tokens.append(Token(TokenType.MULTIPLY, startPosition=self.position))
                self.Advance()
            elif self.currentChar == '/':
                tokens.append(Token(TokenType.DIVIDE, startPosition=self.position))
                self.Advance()
            elif self.currentChar == '(':
                tokens.append(Token(TokenType.LPAREN, startPosition=self.position))
                self.Advance()
            elif self.currentChar == ')':
                tokens.append(Token(TokenType.RPAREN, startPosition=self.position))
                self.Advance()
            
            else:
                char = self.currentChar
                startPosition = self.position.Copy()
                self.Advance()
                raise IllegalCharacterError(startPosition, self.position, f"'{char}'")

        tokens.append(Token(TokenType.EOF))
        return tokens
    
    def MakeNumber(self) -> Token:
        numString = ''
        dotCount = 0
        startPosition = self.position.Copy()

        while self.currentChar != None and self.currentChar in DIGITS+'.':
            if self.currentChar == '.':
                if dotCount == 1: break
                dotCount += 1
                numString += '.'
            else:
                numString += self.currentChar
            self.Advance()

        if dotCount == 0:
            return Token(TokenType.INT, int(numString), startPosition, self.position)
        else:
            return Token(TokenType.FLOAT, float(numString), startPosition, self.position)

#############
### NODES ###
#############

class NodeBase:
    pass

class NumberNode(NodeBase):
    def __init__(self, numberToken:Token) -> None:
        self.numberToken = numberToken
    
    def __repr__(self) -> str:
        return f'{self.numberToken}'

class BinOpNode(NodeBase):
    def __init__(self, leftNode:NodeBase,  opToken:Token, rightNode:NodeBase) -> None:
        self.leftNode = leftNode
        self.opToken = opToken
        self.rightNode = rightNode
    
    def __repr__(self) -> str:
        return f'({self.leftNode}, {self.opToken}, {self.rightNode})'

class UnaryOpNode(NodeBase):
    def __init__(self, opToken:Token, node:NodeBase) -> None:
        self.opToken = opToken
        self.node = node
    
    def __repr__(self) -> str:
        return f'({self.opToken}, {self.node})'

####################
### PARSE RESULT ###
####################

class ParseResult:
    def __init__(self) -> None:
        self.node = None
    
    def Register(self, result:ParseResult) -> NodeBase:
        if isinstance(result, ParseResult):
            return result.node
        return result
    
    def Success(self, node:NodeBase) -> ParseResult:
        self.node = node
        return self
    
    def Failure(self, error:ShorkError) -> ParseResult:
        raise error

##############
### PARSER ###
##############

class Parser:
    @staticmethod
    def Parse(tokens: list[Token]) -> ParseResult:
        return Parser(tokens).DoParse()


    def __init__(self, tokens: list[Token]) -> None:
        self.tokens = tokens
        self.tokenIndex = -1
        self.Advance()
    
    def Advance(self) -> Token:
        self.tokenIndex += 1
        if self.tokenIndex < len(self.tokens):
            self.currentToken:Token = self.tokens[self.tokenIndex]
        return self.currentToken
    
    def DoParse(self) -> ParseResult:
        result = self.ParseExpression()
        if self.currentToken.tokenType != TokenType.EOF:
            result.Failure(InvalidSyntaxError(self.currentToken.startPosition, self.currentToken.endPosition, "Expected '+', '-', '*' or '/'"))
        return result
    

    def ParseFactor(self) -> ParseResult:
        result = ParseResult()
        token:Token = self.currentToken

        if token.tokenType in (TokenType.PLUS, TokenType.MINUS):
            result.Register(self.Advance())
            factor = result.Register(self.ParseFactor())
            return result.Success(UnaryOpNode(token, factor))
        
        elif token.tokenType in (TokenType.INT, TokenType.FLOAT):
            result.Register(self.Advance())
            return result.Success(NumberNode(token))
        
        elif token.tokenType == TokenType.LPAREN:
            result.Register(self.Advance())
            expression = result.Register(self.ParseExpression())
            if self.currentToken.tokenType == TokenType.RPAREN:
                result.Register(self.Advance())
                return result.Success(expression)
            else:
                result.Failure(InvalidSyntaxError(self.currentToken.startPosition, self.currentToken.endPosition, "Expected ')'"))
        
        result.Failure(InvalidSyntaxError(token.startPosition, token.endPosition, "Expected int or float"))
    
    def ParseTerm(self) -> ParseResult:
        return self.ParseBinOp(self.ParseFactor, (TokenType.MULTIPLY, TokenType.DIVIDE))
    
    def ParseExpression(self) -> ParseResult:
        return self.ParseBinOp(self.ParseTerm, (TokenType.PLUS, TokenType.MINUS))


    def ParseBinOp(self, func, ops) -> ParseResult:
        result = ParseResult()
        left = result.Register(func())

        while self.currentToken.tokenType in ops:
            opToken = self.currentToken
            result.Register(self.Advance())
            right = result.Register(func())
            
            left = BinOpNode(left, opToken, right)
        
        return result.Success(left)

###################
### INTERPRETER ###
###################

class Interpreter:
    def Visit(self, node:NodeBase):
        methodName = f'Visit{type(node).__name__}'
        method = getattr(self, methodName, self.NoVisit)
        return method(node)

###########
### RUN ###
###########

def Run(text:str, filename:str) -> None:
    try:
        tokens: list[Token] = Lexer.Lex(text, filename)
        rootNode: NodeBase = Parser.Parse(tokens).node

        print(rootNode)
    except ShorkError as e:
        print(f'{e}')

def __SignalHandler(sig, frame):
    sys.exit(0)

if __name__ == "__main__":
    signal.signal(signal.SIGINT, __SignalHandler)
    while True:
        text = input("🦈> ")
        Run(text, "<STDIN>")