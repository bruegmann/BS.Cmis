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
   Public Class WhereExpression
      Inherits SqlPartExpression

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer)
         MyBase.New(match, groupName, rank, index, Nothing, " ")
      End Sub
#End Region

      Public Overrides Function CanSetValue() As Boolean
         Return False
      End Function

      Private _condition As Expression
      Public Property Condition As Expression
         Get
            Return _condition
         End Get
         Set(value As Expression)
            If _condition IsNot Nothing Then _children.Remove(_condition)
            _condition = value
            If value IsNot Nothing Then
               SetParent(value, Me)
               _children.Add(value)
            End If
         End Set
      End Property

      Private Shared _allowedConditions As New HashSet(Of String) From {
         "CompareOperator", "LogicalOperator", "OpenParenthesis", "Method", "Negation"}
      Public Overrides Function Seal(expressions As System.Collections.Generic.List(Of Expression)) As Integer?
         If Not _sealed Then
            _sealResult = MyBase.Seal(expressions)
            If Not _sealResult.HasValue Then
               Dim rightExpression = GetRightExpression(expressions)

               If rightExpression Is Nothing Then
                  _sealResult = Match.Index
               Else
                  rightExpression = rightExpression.Root
                  If Not _allowedConditions.Contains(rightExpression.GroupName) Then
                     _sealResult = Match.Index + Match.Length
                  Else
                     Condition = rightExpression
                     _sealResult = rightExpression.Seal(expressions)
                  End If
               End If
            End If
         End If

         Return _sealResult
      End Function

   End Class
End Namespace