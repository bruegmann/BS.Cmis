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
   Public Class OrderByExpression
      Inherits FieldContainerExpression

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer)
         MyBase.New(match, groupName, rank, index)
      End Sub
#End Region

      Public Overrides Function CanSetValue() As Boolean
         Return False
      End Function

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
                  If _separatorExpected <> (rightExpression.GroupName = "Separator") Then
                     'end of order-by expression
                     Exit While
                  ElseIf _separatorExpected Then
                     'the next separator MUST NOT have a parent at this time
                     If rightExpression.Parent Is Nothing Then
                        SetParent(rightExpression, Me)
                     Else
                        _sealResult = rightExpression.Match.Index
                        Exit While
                     End If
                  ElseIf Not AddField(rightExpression, expressions) Then
                     'expected field not present
                     _sealResult = rightExpression.Match.Index
                     Exit While
                  End If
                  _separatorExpected = Not _separatorExpected
                  rightExpression = rightExpression.GetRightExpression(expressions)
               End While
               'check, if there is at least one field
               If Not _sealResult.HasValue AndAlso _fields.Count = 0 Then
                  _sealResult = Me.Match.Index + Me.Match.Length
               End If
            End If
         End If

         Return _sealResult
      End Function

   End Class
End Namespace