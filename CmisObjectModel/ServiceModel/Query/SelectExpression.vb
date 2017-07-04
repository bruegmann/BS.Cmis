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
   Public Class SelectExpression
      Inherits FieldContainerExpression

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer)
         MyBase.New(match, groupName, rank, index)
      End Sub
#End Region

#Region "Helper classes"
      Private Delegate Sub FieldSetter(ByRef field As Expression, value As Expression, condition As Boolean)
#End Region

      Public Overrides Function CanSetValue() As Boolean
         Return False
      End Function

      Private _from As FromExpression
      Public Property [From] As FromExpression
         Get
            Return _from
         End Get
         Set(value As FromExpression)
            If _from IsNot Nothing Then _children.Remove(_from)
            _from = value
            If value IsNot Nothing Then
               SetParent(value, Me)
               _children.Add(value)
            End If
         End Set
      End Property

      ''' <summary>
      ''' Returns True if classField is set to value, otherwise
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function GenericFieldSetter(Of T As Expression)(ByRef classField As T, value As Expression, expressions As List(Of Expression),
                                                              Optional condition As Boolean = True) As Boolean
         If classField Is Nothing AndAlso value.Parent Is Nothing AndAlso condition Then
            classField = CType(value, T)
            _children.Add(value)
            SetParent(value, Me)
            _sealResult = value.Seal(expressions)
         Else
            _sealResult = value.Match.Index
         End If

         Return Not _sealResult.HasValue
      End Function

      Protected Overrides Function GetValue(executingType As System.Type) As String
         If Me.GetType().IsAssignableFrom(executingType) Then
            Dim sb As New st.StringBuilder(MyBase.GetValue(GetType(Expression)))

            sb.Append(" ")
            sb.Append(String.Join(", ", (From field As FieldExpression In _fields
                                         Let fieldExpression As String = field.Value
                                         Select fieldExpression).ToArray()))
            If _from IsNot Nothing Then
               sb.Append(" ")
               sb.Append(_from.Value)
               If _where IsNot Nothing Then
                  sb.Append(" ")
                  sb.Append(_where.Value)
               End If
               If _orderBy IsNot Nothing Then
                  sb.Append(" ")
                  sb.Append(_orderBy.Value)
               End If
            End If

            Return sb.ToString()
         Else
            Return MyBase.GetValue(executingType)
         End If
      End Function

      Private _orderBy As OrderByExpression
      Public Property OrderBy As OrderByExpression
         Get
            Return _orderBy
         End Get
         Set(value As OrderByExpression)
            If _orderBy IsNot Nothing Then _children.Remove(_orderBy)
            _orderBy = value
            If value IsNot Nothing Then
               SetParent(value, Me)
               _children.Add(value)
            End If
         End Set
      End Property

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
                  If TypeOf rightExpression Is FromExpression Then
                     If Not GenericFieldSetter(_from, rightExpression, expressions) Then Exit While
                  ElseIf TypeOf rightExpression Is WhereExpression Then
                     If Not GenericFieldSetter(_where, rightExpression, expressions, _from IsNot Nothing AndAlso _orderBy Is Nothing) Then Exit While
                  ElseIf TypeOf rightExpression Is OrderByExpression Then
                     If Not GenericFieldSetter(_orderBy, rightExpression, expressions, _from IsNot Nothing) Then Exit While
                  ElseIf _from Is Nothing Then
                     If _separatorExpected <> (rightExpression.GroupName = "Separator") Then
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
                     ElseIf Not AddField(rightExpression, expressions) Then
                        Exit While
                     End If
                  Else
                     'other expression types will not belong to this select
                     Exit While
                  End If
                  _separatorExpected = Not _separatorExpected
                  rightExpression = rightExpression.GetRightExpression(expressions)
               End While
               'check, if there is at least one field and a from expression defined
               If Not _sealResult.HasValue Then
                  If _fields.Count = 0 Then
                     _sealResult = Me.Match.Index + Me.Match.Length
                  ElseIf _from Is Nothing Then
                     With _fields(_fields.Count - 1).Match
                        _sealResult = .Index + .Length
                     End With
                  End If
               End If
            End If
         End If

         Return _sealResult
      End Function

      Private _where As WhereExpression
      Public Property [Where] As WhereExpression
         Get
            Return _where
         End Get
         Set(value As WhereExpression)
            If _where IsNot Nothing Then _children.Remove(_where)
            _where = value
            If value IsNot Nothing Then
               SetParent(value, Me)
               _children.Add(value)
            End If
         End Set
      End Property

   End Class
End Namespace