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
Imports st = System.Text
Imports str = System.Text.RegularExpressions

Namespace CmisObjectModel.ServiceModel.Query
   Public Class BetweenExpression
      Inherits OperatorExpression

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer)
         MyBase.New(match, groupName, rank, index, True)
      End Sub
#End Region

      ''' <summary>
      ''' Searches for the And expression that belongs to this between
      ''' </summary>
      ''' <param name="expressions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function FindAndExpression(expressions As System.Collections.Generic.List(Of Expression)) As OperatorExpression
         Dim rightExpression As Expression = Me.GetRightExpression(expressions)

         'skip method followed by open parenthesis
         If rightExpression IsNot Nothing AndAlso rightExpression.GroupName = "Method" Then rightExpression = rightExpression.GetRightExpression(expressions)
         'skip parenthesis-block
         If rightExpression IsNot Nothing AndAlso rightExpression.GroupName = "OpenParenthesis" Then
            Dim openCounter As Integer = 1
            Dim offsets As New Dictionary(Of String, Integer) From {{"OpenParenthesis", 1}, {"CloseParenthesis", -1}}
            For index As Integer = rightExpression.Index + 1 To expressions.Count - 1
               If offsets.ContainsKey(expressions(index).GroupName) Then
                  openCounter += offsets(expressions(index).GroupName)
                  If openCounter = 0 Then
                     rightExpression = expressions(index)
                     Exit For
                  End If
               End If
            Next
            'at least one open parenthesis is not closed
            If openCounter > 0 Then Return Nothing
         End If
         'the next expression MUST be the and expression
         If rightExpression IsNot Nothing Then rightExpression = rightExpression.GetRightExpression(expressions)
         Return If(rightExpression Is Nothing OrElse String.Compare(rightExpression.Match.Value, "And", True) = 0,
                   CType(rightExpression, OperatorExpression), Nothing)
      End Function

      ''' <summary>
      ''' Returns null if the expression is accepted in the parsed query, otherwise the position of the match
      ''' that invalidates the query
      ''' </summary>
      ''' <param name="expressions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function Seal(expressions As System.Collections.Generic.List(Of Expression)) As Integer?
         If Not _sealed Then
            _sealed = True
            If Me.Index = 0 OrElse Me.Index + 3 > expressions.Count - 1 Then
               _sealResult = Me.Match.Index
            Else
               Dim leftExpression As Expression = expressions(Index - 1)

               If _allowedLefts.Contains(leftExpression.GroupName) Then
                  leftExpression = leftExpression.Root
                  If TypeOf leftExpression Is ParenthesisExpression AndAlso leftExpression.Match.Value = ")" Then
                     _left = leftExpression
                     AddHandler leftExpression.ParentChanged, AddressOf _left_ParentChanged
                  Else
                     Me.Left = leftExpression
                  End If

                  Dim andExpression As OperatorExpression = FindAndExpression(expressions)
                  If andExpression Is Nothing Then
                     _sealResult = Me.Match.Index + Me.Match.Length
                  Else
                     _sealResult = andExpression.Seal(expressions)
                     Me.Right = andExpression
                  End If
               Else
                  _sealResult = leftExpression.Match.Index
               End If
            End If
         End If

         Return _sealResult
      End Function

   End Class
End Namespace