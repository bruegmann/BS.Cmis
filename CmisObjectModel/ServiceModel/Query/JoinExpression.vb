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
   Public Class JoinExpression
      Inherits CompositeExpression

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer)
         MyBase.New(match, groupName, rank, index, Nothing, " ")
      End Sub
#End Region

      Public Overrides Function CanSetValue() As Boolean
         Return False
      End Function

      Protected Overrides Function GetValue(executingType As System.Type) As String
         If Me.GetType().IsAssignableFrom(executingType) Then
            Dim sb As New st.StringBuilder(MyBase.GetValue(GetType(Expression)))

            If _table IsNot Nothing Then
               sb.Append(" ")
               sb.Append(_table.Value)
               If _on IsNot Nothing Then
                  sb.Append(" ")
                  sb.Append(_on.Value)
               End If
            End If

            Return sb.ToString()
         Else
            Return MyBase.GetValue(executingType)
         End If
      End Function

      Private _on As WhereExpression
      Public Property [On] As WhereExpression
         Get
            Return _on
         End Get
         Set(value As WhereExpression)
            If _on IsNot Nothing Then _children.Remove(_on)
            _on = value
            If value IsNot Nothing Then
               SetParent(value, Me)
               _children.Add(value)
            End If
         End Set
      End Property

      Private Shared _allowedTables As New HashSet(Of String) From {
         "Identifier", "OpenParenthesis"}
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
               Dim rightExpression = Me.GetRightExpression(expressions)

               If rightExpression Is Nothing Then
                  _sealResult = Me.Match.Index + Me.Match.Length
               ElseIf rightExpression.Parent Is Nothing AndAlso _allowedTables.Contains(rightExpression.GroupName) Then
                  Me.Table = New TableExpression(rightExpression)
                  _sealResult = Me.Table.Seal(expressions)
                  If Not _sealResult.HasValue Then
                     rightExpression = rightExpression.GetRightExpression(expressions)
                     If TypeOf rightExpression Is WhereExpression AndAlso rightExpression.Parent Is Nothing AndAlso
                        String.Compare(rightExpression.Value, "On", True) = 0 Then
                        Me.On = CType(rightExpression, WhereExpression)
                        _sealResult = Me.On.Seal(expressions)
                        If Not _sealResult.HasValue Then
                           _sealResult = _table.Seal(expressions)
                        End If
                     Else
                        _sealResult = rightExpression.Match.Index
                     End If
                  End If
               Else
                  _sealResult = rightExpression.Match.Index
               End If
            End If
         End If

         Return _sealResult
      End Function

      Private _table As TableExpression
      Public Property Table As TableExpression
         Get
            Return _table
         End Get
         Set(value As TableExpression)
            If _table IsNot Nothing Then _children.Remove(_table)
            _table = value
            If value IsNot Nothing Then
               SetParent(value, Me)
               _children.Add(value)
            End If
         End Set
      End Property

   End Class
End Namespace