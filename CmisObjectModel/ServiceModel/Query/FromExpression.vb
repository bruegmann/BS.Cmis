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
Imports str = System.Text.RegularExpressions

Namespace CmisObjectModel.ServiceModel.Query
   Public Class FromExpression
      Inherits SqlPartExpression

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer)
         MyBase.New(match, groupName, rank, index, ", ", " ")
      End Sub
#End Region

      Public Overrides Function CanSetValue() As Boolean
         Return False
      End Function

      Private Shared _allowedTables As New HashSet(Of String) From {
         "Identifier", "OpenParenthesis"}
      Private _separatorExpected As Boolean = False
      ''' <summary>
      ''' Returns null if the expression is accepted in the parsed query, otherwise the position of the match
      ''' that invalidates the query
      ''' </summary>
      ''' <param name="expressions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function Seal(expressions As System.Collections.Generic.List(Of Expression)) As Integer?
         If Not _sealed Then
            _sealResult = MyBase.Seal(expressions)

            If Not _sealResult.HasValue Then
               Dim rightExpression As Expression = Me.GetRightExpression(expressions)

               While rightExpression IsNot Nothing
                  Dim isSeparator As Boolean = (rightExpression.GroupName = "Separator")
                  Dim isTable As Boolean = _allowedTables.Contains(rightExpression.GroupName)

                  If Not (isSeparator OrElse isTable) Then
                     Exit While
                  ElseIf _separatorExpected <> isSeparator Then
                     'expected expression missed
                     _sealResult = rightExpression.Match.Index
                     Exit While
                  ElseIf _separatorExpected Then
                     'the next separator MUST NOT have a parent at this time
                     If rightExpression.Parent Is Nothing Then
                        SetParent(rightExpression, Me)
                     Else
                        _sealResult = rightExpression.Match.Index
                        Exit While
                     End If
                  ElseIf isTable Then
                     'the next expression MUST be a table expression without a parent
                     If rightExpression.Parent Is Nothing Then
                        Dim tableExpression As New TableExpression(rightExpression)
                        _children.Add(tableExpression)
                        SetParent(tableExpression, Me)
                        _sealResult = tableExpression.Seal(expressions)
                        If _sealResult.HasValue Then Exit While
                     Else
                        _sealResult = rightExpression.Match.Index
                        Exit While
                     End If
                  Else
                     'the next expression is neither a table expression nor a separator
                     Exit While
                  End If
                  'get the next expression
                  rightExpression = rightExpression.GetRightExpression(expressions)
                  _separatorExpected = Not _separatorExpected
               End While
               'check, if there is at least one table defined and the last expression MUST NOT be a separator
               If Not (_sealResult.HasValue OrElse _separatorExpected) Then
                  Dim tables = Me.Tables
                  Dim length As Integer = tables.Length

                  With If(length = 0, CType(Me, Expression), CType(tables(length - 1), Expression)).Match
                     _sealResult = .Index + .Length
                  End With
               End If
            End If
         End If

         Return _sealResult
      End Function

      ''' <summary>
      ''' Returns the children of type TableExpression
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks>Normally it should be the complete list of _children</remarks>
      Public ReadOnly Property Tables As TableExpression()
         Get
            Return (From child As Expression In _children
                    Let table As TableExpression = TryCast(child, TableExpression)
                    Where table IsNot Nothing
                    Select table).ToArray()
         End Get
      End Property

   End Class
End Namespace