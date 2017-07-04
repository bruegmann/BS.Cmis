'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2014, Brügmann Software GmbH, Papenburg, All rights reserved
'*
'* Contact: opensource<at>patorg.de
'* 
'* CmisObjectModelLibrary is a VB.NET implementation of the Content Management Interoperability Services (CMIS) standard
'*
'* This file is part of CmisObjectModelLibrary.
'* 
'* This library is free software; you can redistribute it and/or
'* modify it under the terms of the GNU Lesser General Public
'* License as published by the Free Software Foundation; either
'* version 3.0 of the License, or (at your option) any later version.
'*
'* This library is distributed in the hope that it will be useful,
'* but WITHOUT ANY WARRANTY; without even the implied warranty of
'* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
'* Lesser General Public License for more details.
'*
'* You should have received a copy of the GNU Lesser General Public
'* License along with this library (lgpl.txt).
'* If not, see <http://www.gnu.org/licenses/lgpl.txt>.
'***********************************************************************************************************************
Imports ccg = CmisObjectModel.Common.Generic
Imports cm = CmisObjectModel.Messaging
Imports str = System.Text.RegularExpressions

Namespace CmisObjectModel.ServiceModel.Query
   ''' <summary>
   ''' Simple parser to evaluate a query expression
   ''' </summary>
   ''' <remarks>see chapter 2.1.14.2.1 BNF Grammar of the cmis documentation
   ''' Chapter 2.1.14 Query of the cmis documentation defines the grammar of sql supported in cmis.
   ''' The defined grammar is a subset of sql grammar supported by this parser. The parser detects
   ''' functions in general, not only CONTAINS(), SCORE(), IN_FOLDER() and IN_TREE(). It is the job
   ''' of the server implementation to determine where an extension of the cmis standard is useful.
   ''' String expressions within the CONTAINS() function MAY contain \' instead of '' to define a
   ''' single quote. Outside the CONTAINS() function a single quote within a string expression MUST
   ''' defined through ''.
   ''' Supported literals are: strings, numerics, datetimes and booleans. Boolean expressions are
   ''' complete case insensitive (for example tRUe is a valid notation). DateTime expressions MAY
   ''' define only the date without any time information. The separator between year, month and
   ''' day can be choosen free with the only limitation that it has to be exact one sign. If a time
   ''' is defined the separator between hour, minute and second must be :. The definition of second
   ''' and millisecond is optional (see const _dateTimeLiteral). Note: if the optional offset isn't
   ''' defined the given time is interpreted as local time.
   ''' </remarks>
   Public Class QueryParser

#Region "Constants"
      Private Const _dateTimeLiteral As String = "<datetime literal> ::= TIMESTAMP[<space>]<quote>YYYY<char>MM<char>DD[Thh:mm[:ss[.sss]][{Z|{+|-}hh:mm}]]<quote>"

      'processing rank
      Private Const _signRank As Integer = 1 << 30
      Private Const _mathOperatorRank As Integer = _signRank >> 1
      Private Const _stringOperatorRank As Integer = _mathOperatorRank >> 1
      Private Const _compareOperatorRank As Integer = _stringOperatorRank >> 1
      Private Const _betweenOperatorRank As Integer = _compareOperatorRank >> 1
      Private Const _negationRank As Integer = _betweenOperatorRank >> 1
      Private Const _logicalOperatorRank As Integer = _negationRank >> 1
      Private Const _methodRank As Integer = _logicalOperatorRank >> 1
      Private Const _parenthesisRank As Integer = _methodRank >> 1
      Private Const _selectRank As Integer = _parenthesisRank >> 1
      Private Const _joinRank As Integer = _selectRank >> 1
      Private Const _conditionRank As Integer = _joinRank >> 1
      Private Const _orderByRank As Integer = _conditionRank >> 1

      Private Const shortAliasPattern As String = "(\)\s*|\s+)(?<Alias>(((?<AliasName>""(""""|[^""])+""))|(?<AliasName>" & forbiddenIdentifierCharPattern & "+)))"
      Private Const blankOrCloseParenthesisPrefixPattern As String = "(?<=[\s\)])"
      Private Const blankOrOpenParenthesisSuffixPattern As String = "(?=(\z|[\s\(]))"
      Private Const identifierStartCharPattern As String = "[A-Z_]"
      Private Const forbiddenIdentifierCharPattern As String = "[^\s\.,\<\>=\(\)\-\+]"
      Private Const dateTimeLiteralPattern As String = "(?<!\w)TIMESTAMP\s*'(?<DateConstant>(?<Year>\d{4}4{5}).(?<Month>\d{4}1,2{5}).(?<Day>\d{4}1,2{5})(T(?<Hour>\d{4}1,2{5}):(?<Minute>\d{4}1,2{5})(:(?<Second>\d{4}1,2{5})(\.(?<Millisecond>\d{4}1,3{5})\d*)?)?(?<Offset>((?<OffsetUtc>Z)|(?<OffsetSign>[\+\-])(?<OffsetHour>\d{4}2{5}):(?<OffsetMinute>\d{4}2{5})))?)?)\s*'"
      Public Shared sqlPattern As String =
         String.Format("(" &
                           "(?<=(\A|[\s,\(]))(?<Select>Select){2}" &
                        "|" &
                           "(?<=[\s\)""])(?<SqlMainPart>(From|Where)){2}" &
                        "|" &
                           "(?<=([+\-*\/\(\<\>=]\s*|\s+(and|in|is|like|on|or|select|where)\s+|\Aselect\s+))(?<Sign>[+\-])" &
                        "|" &
                           "(?<=(select\s+|,\s*))(?<Identifier>((?<Prefix>{0}{1}*)\.)*\*)" &
                        "|" &
                           "(?<=[\s,\(\<\>=]({0}{1}*\.)*{0}{1}*\s*\(\s*)(?<Identifier>\*)(?=\s*\))" &
                        "|" &
                           "(?<MathOperator>([+\-*\/]|\<\<|\>\>))" &
                        "|" &
                           "((?<CompareOperator>([\<\>]=|\<\>|[\=\<\>]))|(?<=\s)((?<Negation>not)\s+)?(?<CompareOperator>(like|in)){2}|(?<=\s)(?<CompareOperator>is)(\s+(?<Negation>not))?(?=(\z|\s)))" &
                        "|" &
                           "(?<=\s)((?<Negation>not)\s+)?(?<BetweenOperator>between){2}" &
                        "|" &
                           "{3}(?<LogicalOperator>(or|and)){2}" &
                        "|" &
                           "(?<=[\s\(])(?<Negation>not){2}" &
                        "|" &
                           "{3}(?<Alias>As((\s*(?<AliasName>""(""""|[^""])+""))|\s+(?<AliasName>{1}+)))" &
                        "|" &
                           "(?<Separator>,)" &
                        "|" &
                           "(?<StringOperator>\|\|)" &
                        "|" &
                           "(?<CloseParenthesis>[\)])" &
                        "|" &
                           "(?<OpenParenthesis>[\(])" &
                        "|" &
                           "(?<=[\s,\(\<\>=](?<IsContainsOperation>)contains\s*\(\s*)(?<Constant>'(?<StringConstant>(\\\\|''|\\'|[^'])*)')" &
                        "|" &
                           "(?<Constant>('(?<StringConstant>(''|[^'])*)'|(?<NumberConstant>\d+(\.\d+)?)|(?<=\s)null(?=(\s|\z)))|" & dateTimeLiteralPattern & "|(?<!\w)(?<BooleanConstant>(true|false))(?!\w))" &
                        "|" &
                           "{3}(?<Join>((inner|(left|right|full)(\s+outer)?)\s+)?join){2}" &
                        "|" &
                           "{3}(?<Condition>(on|having|start\s+with|connect\s+by)){2}" &
                        "|" &
                           "{3}(?<OrderBy>order(\s+siblings)?\s+by){2}" &
                        "|" &
                           "{3}(?<OrderDirection>((?<Direction>(Asc|Desc)(ending)?)(\s+(?<Nulls>Nulls\s+(First|Last)))?|(?<Nulls>Nulls\s+(First|Last))))(?=(\z|[\s,]))" &
                        "|" &
                           "(?<=[\s,\(\<\>=])(?<Method>((?<Prefix>{0}{1}*)\.)*(?<MethodName>{0}{1}*))(?=\s*\()" &
                        "|" &
                           "(?<=[\s,\(\<\>=])((?<Any>any)\s+)?(?<Identifier>((?<Prefix>(""(""""|[^""])*""|{0}{1}*))\.)*(""(""""|[^""])*""|{0}{1}*))" &
                       ")", identifierStartCharPattern, forbiddenIdentifierCharPattern,
                            blankOrOpenParenthesisSuffixPattern, blankOrCloseParenthesisPrefixPattern, "{", "}")
      Private Shared _expressionFactories As New Dictionary(Of String, Func(Of str.Match, Integer, CmisObjectModel.ServiceModel.Query.Expression)) From {
         {"Alias", AddressOf CreateAliasExpression},
         {"AliasName", Nothing},
         {"BetweenOperator", AddressOf CreateBetweenOperatorExpression},
         {"CloseParenthesis", AddressOf CreateParenthesisExpression},
         {"CompareOperator", AddressOf CreateCompareOperatorExpression},
         {"Condition", AddressOf CreateConditionExpression},
         {"Constant", AddressOf CreateConstantExpression},
         {"Direction", Nothing},
         {"Identifier", AddressOf CreateIdentifierExpression},
         {"IsContainsOperation", Nothing},
         {"Join", AddressOf CreateJoinExpression},
         {"LogicalOperator", AddressOf CreateLogicalOperatorExpression},
         {"MathOperator", AddressOf CreateMathOperatorExpression},
         {"Method", AddressOf CreateMethodExpression},
         {"Negation", AddressOf CreateNegationExpression},
         {"Nulls", Nothing},
         {"NumberConstant", Nothing},
         {"OpenParenthesis", AddressOf CreateParenthesisExpression},
         {"OrderBy", AddressOf CreateOrderByExpression},
         {"OrderDirection", AddressOf CreateOrderDirectionExpression},
         {"Prefix", Nothing},
         {"Select", AddressOf CreateSelectExpression},
         {"Separator", AddressOf CreateSeparatorExpression},
         {"Sign", AddressOf CreateSignExpression},
         {"SqlMainPart", AddressOf CreateSqlMainPartExpression},
         {"StringConstant", Nothing},
         {"StringOperator", AddressOf CreateStringOperatorExpression}}
#End Region

#Region "Constructors"
      Public Shared Function CreateInstance(q As String) As ccg.Result(Of SelectExpression)
         If String.IsNullOrEmpty(q) Then
            Return cm.cmisFaultType.CreateInvalidArgumentException("q")
         Else
            'parse the query expression
            Dim expressions As List(Of Expression) = GetExpressions(q)

            'failure
            If expressions Is Nothing Then Return cm.cmisFaultType.CreateInvalidArgumentException("q")

            'create expression tree
            Dim groups = (From expression As Expression In expressions
                          Where expression.Rank > 0
                          Order By expression.Index
                          Group By rank = expression.Rank
                          Into newGroup = Group
                          Order By rank Descending
                          Select newGroup).ToArray()
            For Each group As IEnumerable(Of Expression) In groups
               Dim groupExpressions As Expression() = If(TypeOf group Is Expression(), CType(group, Expression()), group.ToArray())

               For groupExpressionsIndex As Integer = groupExpressions.Length - 1 To 0 Step -1
                  groupExpressions(groupExpressionsIndex).Seal(expressions)
               Next
            Next

            'search for result
            Dim retVal As SelectExpression = GetSelectExpression(expressions)

            'search for the first error if any
            If retVal Is Nothing Then
               'query doesn't start with '[\(]*Select'-Pattern
               Dim httpStatusCode As System.Net.HttpStatusCode = Common.ToHttpStatusCode(cm.enumServiceException.invalidArgument)
               Dim fault As New cm.cmisFaultType(httpStatusCode, cm.enumServiceException.invalidArgument,
                                                 "Unexpected expression in parameter 'q'")
               Return fault.ToFaultException
            Else
               For expressionIndex As Integer = 0 To expressions.Count - 1
                  Dim expression = expressions(expressionIndex)
                  Dim failureIndex As Integer? = If(expression.Seal(expressions),
                                                 If(expression Is retVal OrElse expression.Parent IsNot Nothing,
                                                    Nothing, CType(expression.Match.Index, Integer?)))
                  If failureIndex.HasValue Then
                     Dim httpStatusCode As System.Net.HttpStatusCode = Common.ToHttpStatusCode(cm.enumServiceException.invalidArgument)
                     Dim fault As New cm.cmisFaultType(httpStatusCode, cm.enumServiceException.invalidArgument,
                                                       "Unexpected expression in parameter 'q' at position " & failureIndex.Value)
                     Return fault.ToFaultException
                  End If
               Next
            End If

            Return retVal
         End If
      End Function

      ''' <summary>
      ''' Splits the query term in expressions
      ''' </summary>
      ''' <param name="q"></param>
      ''' <returns>A list of expressions or nothing if a found match cannot be converted into an expression-instance</returns>
      ''' <remarks></remarks>
      Friend Shared Function GetExpressions(q As String) As List(Of Expression)
         Dim regEx As New str.Regex(sqlPattern,
                                       Text.RegularExpressions.RegexOptions.ExplicitCapture Or
                                       Text.RegularExpressions.RegexOptions.IgnoreCase Or
                                       Text.RegularExpressions.RegexOptions.Multiline)
         Dim shortAliasRegEx As New str.Regex(shortAliasPattern,
                                    Text.RegularExpressions.RegexOptions.ExplicitCapture Or
                                    Text.RegularExpressions.RegexOptions.IgnoreCase Or
                                    Text.RegularExpressions.RegexOptions.Multiline)
         Dim retVal As New List(Of Expression)
         Dim index As Integer = 0
         Dim expression As Expression = Nothing

         'parse the query expression
         For Each match As str.Match In regEx.Matches(q)
            expression = CreateExpression(match, index, expression, q, shortAliasRegEx)

            If expression Is Nothing Then
               Return Nothing
            Else
               retVal.Add(expression)
               index += 1
            End If
         Next

         Return retVal
      End Function
#End Region

#Region "Expression-factories"
      Private Shared Function CreateExpression(match As str.Match, index As Integer, lastExpression As Expression, q As String, shortAliasRegEx As str.Regex) As Expression
         For Each de As KeyValuePair(Of String, Func(Of str.Match, Integer, Expression)) In _expressionFactories
            If de.Value IsNot Nothing Then
               Dim group As str.Group = match.Groups(de.Key)
               If group IsNot Nothing AndAlso group.Success Then
                  If String.Compare(de.Key, "Identifier") = 0 AndAlso
                     (TypeOf lastExpression Is IdentifierExpression OrElse
                      (TypeOf lastExpression Is ParenthesisExpression AndAlso lastExpression.Match.Value = ")") OrElse
                      TypeOf lastExpression Is ConstantExpression) Then
                     Return CreateExpression(shortAliasRegEx.Match(q, match.Index - 1), index, lastExpression, q, shortAliasRegEx)
                  Else
                     Return de.Value(match, index)
                  End If
               End If
            End If
         Next

         Return Nothing
      End Function

      Private Shared Function CreateAliasExpression(match As str.Match, index As Integer) As Expression
         Return New AliasExpression(match, "Alias", 0, index)
      End Function

      Private Shared Function CreateBetweenOperatorExpression(match As str.Match, index As Integer) As Expression
         Return New BetweenExpression(match, "BetweenOperator", _betweenOperatorRank, index)
      End Function

      Private Shared Function CreateCompareOperatorExpression(match As str.Match, index As Integer) As Expression
         Return New OperatorExpression(match, "CompareOperator", _compareOperatorRank, index, True)
      End Function

      Private Shared Function CreateConditionExpression(match As str.Match, index As Integer) As Expression
         Return New WhereExpression(match, "Condition", _conditionRank, index)
      End Function

      Private Shared Function CreateConstantExpression(match As str.Match, index As Integer) As Expression
         Return New ConstantExpression(match, "Constant", 0, index)
      End Function

      Private Shared Function CreateIdentifierExpression(match As str.Match, index As Integer) As Expression
         Return New IdentifierExpression(match, "Identifier", 0, index)
      End Function

      Private Shared Function CreateJoinExpression(match As str.Match, index As Integer) As Expression
         Return New JoinExpression(match, "Join", _joinRank, index)
      End Function

      Private Shared Function CreateLogicalOperatorExpression(match As str.Match, index As Integer) As Expression
         Return New OperatorExpression(match, "LogicalOperator", _logicalOperatorRank + If(String.Compare(match.Value, "or", True) = 0, 0, 1), index, True)
      End Function

      Private Shared Function CreateMathOperatorExpression(match As str.Match, index As Integer) As Expression
         Dim offset As Integer
         Select Case match.Value
            Case "-", "+"
               offset = 0
            Case "*", "/"
               offset = 1
            Case Else
               offset = 2
         End Select
         Return New OperatorExpression(match, "MathOperator", (_mathOperatorRank) + offset, index, True)
      End Function

      Private Shared Function CreateMethodExpression(match As str.Match, index As Integer) As Expression
         Return New MethodExpression(match, "Method", _methodRank, index)
      End Function

      Private Shared Function CreateNegationExpression(match As str.Match, index As Integer) As Expression
         Return New OperatorExpression(match, "Negation", _negationRank, index, False)
      End Function

      Private Shared Function CreateOrderByExpression(match As str.Match, index As Integer) As Expression
         Return New OrderByExpression(match, "OrderBy", _orderByRank, index)
      End Function

      Private Shared Function CreateOrderDirectionExpression(match As str.Match, index As Integer) As Expression
         Return New OrderDirectionExpression(match, "OrderDirection", 0, index)
      End Function

      Private Shared Function CreateParenthesisExpression(match As str.Match, index As Integer) As Expression
         Dim rank = If(match.Value = "(", _parenthesisRank, 0)
         Return New ParenthesisExpression(match, If(rank = 0, "Close", "Open") & "Parenthesis", rank, index)
      End Function

      Private Shared Function CreateSelectExpression(match As str.Match, index As Integer) As Expression
         Return New SelectExpression(match, "Select", _selectRank, index)
      End Function

      Private Shared Function CreateSeparatorExpression(match As str.Match, index As Integer) As Expression
         Return New Expression(match, "Separator", 0, index)
      End Function

      Private Shared Function CreateSignExpression(match As str.Match, index As Integer) As Expression
         Return New SignExpression(match, _signRank, index)
      End Function

      Private Shared Function CreateSqlMainPartExpression(match As str.Match, index As Integer) As Expression
         Select Case match.Value.ToLowerInvariant()
            Case "from"
               Return New FromExpression(match, "SqlMainPart", 0, index)
            Case "where"
               Return New WhereExpression(match, "SqlMainPart", 0, index)
            Case Else
               Return Nothing
         End Select
      End Function

      Private Shared Function CreateStringOperatorExpression(match As str.Match, index As Integer) As Expression
         Return New OperatorExpression(match, "StringOperator", _stringOperatorRank, index, True)
      End Function
#End Region

      ''' <summary>
      ''' Returns the main select expression of the query or null
      ''' </summary>
      ''' <param name="expressions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Shared Function GetSelectExpression(expressions As List(Of Expression)) As SelectExpression
         If expressions.Count = 0 Then
            Return Nothing
         Else
            Dim root = expressions(0).Root

            'skip embedding parenthesis
            While TypeOf root Is ParenthesisExpression
               With CType(root, ParenthesisExpression)
                  Dim children = .GetChildren(Function(expression) True)
                  root = If(children Is Nothing OrElse children.Length <> 1, Nothing, children(0))
               End With
            End While
            Return TryCast(root, SelectExpression)
         End If
      End Function

   End Class
End Namespace