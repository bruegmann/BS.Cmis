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
   Public Class ParenthesisExpression
      Inherits CompositeExpression

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer)
         MyBase.New(match, groupName, rank, index, ", ", Nothing)
      End Sub
#End Region

      Private _separatorExpected As Boolean = False
      Private Function Add(expression As Expression) As Boolean
         Dim retVal As Boolean = _termination Is Nothing AndAlso (_separatorExpected = (expression.GroupName = "Separator"))

         If retVal Then
            SetParent(expression, Me)
            _separatorExpected = Not _separatorExpected
            If _separatorExpected Then _children.Add(expression)
         End If

         Return retVal
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
            _sealResult = MyBase.Seal(expressions)
            If Not _sealResult.HasValue Then
               If Match.Value = ")" Then
                  If _parent Is Nothing Then _sealResult = Match.Index
               Else
                  Dim rightExpression = GetRightExpression(expressions)

                  While rightExpression IsNot Nothing
                     If TypeOf rightExpression Is ParenthesisExpression AndAlso
                        rightExpression.Match.Value = ")" Then
                        _termination = rightExpression
                        SetParent(rightExpression, Me)
                        Exit While
                     ElseIf _separatorExpected <> (rightExpression.GroupName = "Separator") Then
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
                     Else
                        rightExpression = rightExpression.Root
                        _children.Add(rightExpression)
                        SetParent(rightExpression, Me)
                        _sealResult = rightExpression.Seal(expressions)
                        If _sealResult.HasValue Then Exit While
                     End If
                     _separatorExpected = Not _separatorExpected
                     rightExpression = rightExpression.GetRightExpression(expressions)
                  End While
                  'check if the open parenthesis is followed by a close parenthesis
                  If _termination Is Nothing Then _sealResult = Match.Index
               End If
            End If
         End If

         Return _sealResult
      End Function

   End Class
End Namespace