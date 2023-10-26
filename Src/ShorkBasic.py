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

class NotImplementedError(ShorkError):
    def __init__(self, startPosition: Position, endPosition: Position, details: str) -> None:
        super().__init__(startPosition, endPosition, "Not Implemented", details)
    def __repr__(self) -> str:
        return f'Not Implemented Error: <{self.details}> is not implemented.'

class RuntimeError(ShorkError):
    def __init__(self, startPosition: Position, endPosition: Position, details: str) -> None:
        super().__init__(startPosition, endPosition, "Runtime Error", details)

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

TokenType = Enum('TokenType', [
    'INT',
    'FLOAT',

    'PLUS',
    'MINUS',
    'MULTIPLY',
    'DIVIDE',
    'POWER',

    'LPAREN',
    'RPAREN',

    'EOF'
])

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
            elif self.currentChar == '^':
                tokens.append(Token(TokenType.POWER, startPosition=self.position))
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
    def __init__(self, startPosition:Position, endPosition:Position) -> None:
        self.startPosition = startPosition
        self.endPosition = endPosition

class NumberNode(NodeBase):
    def __init__(self, numberToken:Token) -> None:
        super().__init__(numberToken.startPosition, numberToken.endPosition)
        self.numberToken = numberToken
    
    def __repr__(self) -> str:
        return f'{self.numberToken}'

class BinOpNode(NodeBase):
    def __init__(self, leftNode:NodeBase,  opToken:Token, rightNode:NodeBase) -> None:
        super().__init__(leftNode.startPosition, rightNode.endPosition)
        self.leftNode = leftNode
        self.opToken = opToken
        self.rightNode = rightNode
    
    def __repr__(self) -> str:
        return f'({self.leftNode}, {self.opToken}, {self.rightNode})'

class UnaryOpNode(NodeBase):
    def __init__(self, opToken:Token, node:NodeBase) -> None:
        super().__init__(opToken.startPosition, node.endPosition)
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
    def Parse(tokens: list[Token]) -> NodeBase:
        return Parser(tokens).DoParse().node


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
    
    
    def ParseAtom(self):
        result = ParseResult()
        token:Token = self.currentToken
        
        if token.tokenType in (TokenType.INT, TokenType.FLOAT):
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
        
        result.Failure(InvalidSyntaxError(self.currentToken.startPosition, self.currentToken.endPosition, "Expected int, float, '+', '-' or ')'"))

    def ParsePower(self):
        return self.ParseBinOp(self.ParseAtom, (TokenType.POWER, ), self.ParseFactor)

    def ParseFactor(self) -> ParseResult:
        result = ParseResult()
        token:Token = self.currentToken

        if token.tokenType in (TokenType.PLUS, TokenType.MINUS):
            result.Register(self.Advance())
            factor = result.Register(self.ParseFactor())
            return result.Success(UnaryOpNode(token, factor))
        
        return self.ParsePower()
    
    def ParseTerm(self) -> ParseResult:
        return self.ParseBinOp(self.ParseFactor, (TokenType.MULTIPLY, TokenType.DIVIDE))
    
    def ParseExpression(self) -> ParseResult:
        return self.ParseBinOp(self.ParseTerm, (TokenType.PLUS, TokenType.MINUS))


    def ParseBinOp(self, leftFunc, ops, rightFunc = None) -> ParseResult:
        if rightFunc == None:
            rightFunc = leftFunc

        result = ParseResult()
        left = result.Register(leftFunc())

        while self.currentToken.tokenType in ops:
            opToken = self.currentToken
            result.Register(self.Advance())
            right = result.Register(rightFunc())
            
            left = BinOpNode(left, opToken, right)
        
        return result.Success(left)

######################
### RUNTIME RESULT ###
######################

class RuntimeResult:
    def __init__(self) -> None:
        self.value:Object = None
    
    def Register(self, result:RuntimeResult) -> Object:
        return result.value
    
    def Success(self, value:Object) -> RuntimeResult:
        self.value = value
        return self
    
    def Failure(self, error:ShorkError) -> None:
        raise error

##############
### VALUES ###
##############

class Object:
    def __init__(self, value:any) -> None:
        self.value = value

    def __repr__(self) -> str:
        return f'{self.value}'

    def SetPosition(self, startPosition, endPosition:Position = None) -> Object:
        self.startPosition = startPosition
        self.endPosition = endPosition
        return self
    
    def SetContext(self, context:Context) -> Object:
        self.context = context
        return self
    
    def AddTo(self, other:Object) -> Object:
        raise NotImplementedError(None, None, f"{type(self).__name__}.AddTo")
    
    def SubFrom(self, other:Object) -> Object:
        raise NotImplementedError(None, None, f"{type(self).__name__}.SubFrom")
    
    def MultiplyBy(self, other:Object) -> Object:
        raise NotImplementedError(None, None, f"{type(self).__name__}.MultiplyBy")
    
    def DivideBy(self, other:Object) -> Object:
        raise NotImplementedError(None, None, f"{type(self).__name__}.DivideBy")
    
    def ToPowerOf(self, other:Object) -> Object:
        raise NotImplementedError(None, None, f"{type(self).__name__}.ToPowerOf")
    
class Number(Object):
    def __init__(self, value: int|float) -> None:
        super().__init__(value)
    
    def AddTo(self, other: Object) -> Object:
        match other:
            case Number():
                return Number(self.value + other.value).SetContext(self.context)
            case _:
                raise RuntimeError(self.startPosition, self.endPosition, f"Cannot use the '+' operator on objects of type 'Number' and '{type(other).__name__}'")
    
    def SubFrom(self, other: Object) -> Object:
        match other:
            case Number():
                return Number(self.value - other.value).SetContext(self.context)
            case _:
                raise RuntimeError(self.startPosition, self.endPosition, f"Cannot use the '-' operator on objects of type 'Number' and '{type(other).__name__}'")
    
    def MultiplyBy(self, other: Object) -> Object:
        match other:
            case Number():
                return Number(self.value * other.value).SetContext(self.context)
            case _:
                raise RuntimeError(self.startPosition, self.endPosition, f"Cannot use the '*' operator on objects of type 'Number' and '{type(other).__name__}'")
    
    def DivideBy(self, other: Object) -> Object:
        match other:
            case Number():
                if other.value == 0:
                    raise RuntimeError(other.startPosition, other.endPosition, "Cannot divide by zero")
                return Number(self.value * other.value).SetContext(self.context)
            case _:
                raise RuntimeError(self.startPosition, self.endPosition, f"Cannot use the '/' operator on objects of type 'Number' and '{type(other).__name__}'")
    
    def ToPowerOf(self, other: Object) -> Object:
        match other:
            case Number():
                return Number(self.value ** other.value).SetContext(self.context)
            case _:
                raise RuntimeError(self.startPosition, self.endPosition, f"Cannot use the '^' operator on objects of type 'Number' and '{type(other).__name__}'")

###############
### CONTEXT ###
###############

class Context:
	def __init__(self, displayName, parent=None, parentEntryPosition=None):
		self.displayName = displayName
		self.parent = parent
		self.parentEntryPosition = parentEntryPosition

###################
### INTERPRETER ###
###################

class Interpreter:
    @staticmethod
    def Interpret(rootNode:NodeBase, context:Context) -> Object:
        return Interpreter().Visit(rootNode, context).value

    def Visit(self, node:NodeBase, context:Context):
        methodName = f'Visit{type(node).__name__}'
        method = getattr(self, methodName, self.NoVisit)
        return method(node, context)
    
    def NoVisit(self, node:NodeBase, context:Context):
        raise NotImplementedError(f'Interpreter.Visit{type(node).__name__}')
    
    def VisitNumberNode(self, node:NumberNode, context:Context):
        return RuntimeResult().Success(Number(node.numberToken.value).SetContext(context).SetPosition(node.startPosition, node.endPosition))

    def VisitBinOpNode(self, node:BinOpNode, context:Context):
        result = RuntimeResult()
        left:Object = result.Register(self.Visit(node.leftNode, context))
        right:Object = result.Register(self.Visit(node.rightNode, context))

        match node.opToken.tokenType:
            case TokenType.PLUS:
                return result.Success(left.AddTo(right).SetPosition(node.startPosition, node.endPosition))
            case TokenType.MINUS:
                return result.Success(left.SubFrom(right).SetPosition(node.startPosition, node.endPosition))
            case TokenType.MULTIPLY:
                return result.Success(left.MultiplyBy(right).SetPosition(node.startPosition, node.endPosition))
            case TokenType.DIVIDE:
                return result.Success(left.DivideBy(right).SetPosition(node.startPosition, node.endPosition))
            case TokenType.POWER:
                return result.Success(left.ToPowerOf(right).SetPosition(node.startPosition, node.endPosition))
    
    def VisitUnaryOpNode(self, node:UnaryOpNode, context:Context):
        result = RuntimeResult()
        object:Object = result.Register(self.Visit(node.node, context))

        match object:
            case Number():
                if node.opToken.tokenType == TokenType.MINUS:
                    return result.Success(object.MultiplyBy(Number(-1)).SetPosition(node.startPosition, node.endPosition))
            case _:
                raise RuntimeError(node.startPosition, node.endPosition,
                                   f"Unary operation '{node.opToken.tokenType}' not defined for objects of type {type(object).__name__}")

###########
### RUN ###
###########

def Run(text:str, filename:str) -> None:
    try:
        tokens = Lexer.Lex(text, filename)
        nodes = Parser.Parse(tokens)
        result = Interpreter.Interpret(nodes, Context("<program>"))
        print(result)
    except ShorkError as e:
        print(e.__repr__())

def __SignalHandler(sig, frame):
    sys.exit(0)

if __name__ == "__main__":
    signal.signal(signal.SIGINT, __SignalHandler)
    while True:
        text = input("🦈> ")
        Run(text, "<STDIN>")