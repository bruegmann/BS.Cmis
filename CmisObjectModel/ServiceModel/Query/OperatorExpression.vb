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
   Public Class OperatorExpression
      Inherits CompositeExpression

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer,
                     leftOperandSupported As Boolean)
         MyBase.New(match, groupName, rank, index, Nothing, Nothing)

         Dim grNegation As System.Text.RegularExpressions.Group = match.Groups("Negation")

         Me.LeftOperandSupported = leftOperandSupported
         Me.HasNegation = grNegation IsNot Nothing AndAlso grNegation.Success AndAlso String.Compare(groupName, "Negation") <> 0
         Me.Operator = MyBase.GetValue(GetType(Expression))
      End Sub
#End Region

      Protected Shared _allowedLefts As New HashSet(Of String) From {
         "CloseParenthesis", "Constant", "Identifier"}
      Private Shared _allowedRights As New HashSet(Of String) From {
         "OpenParenthesis", "Constant", "Identifier", "Method", "Negation"}
      ''' <summary>
      ''' Returns the valid group names for the right side of the operator
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable ReadOnly Property AllowedRights As HashSet(Of String)
         Get
            Return _allowedRights
         End Get
      End Property

      Public Overrides Function CanSetValue() As Boolean
         Return False
      End Function

      Protected Overrides Function GetValue(executingType As System.Type) As String
         If Me.GetType().IsAssignableFrom(executingType) Then
            Dim sb As New st.StringBuilder

            If _left IsNot Nothing AndAlso LeftOperandSupported Then
               sb.Append(_left.Value)
               sb.Append(" ")
            End If
            If Not HasNegation Then
               sb.Append(Me.Operator)
            ElseIf String.Compare("is", Me.Operator, True) = 0 Then
               sb.Append(Me.Operator)
               sb.Append(" Not")
            Else
               sb.Append("Not ")
               sb.Append(Me.Operator)
            End If
            If _right IsNot Nothing Then
               sb.Append(" ")
               sb.Append(_right.Value)
            End If

            Return sb.ToString()
         Else
            Return MyBase.GetValue(executingType)
         End If
      End Function

      Protected _left As Expression
      Public Property Left As Expression
         Get
            Return _left
         End Get
         Set(value As Expression)
            If _left IsNot Nothing Then _children.Remove(_left)
            _left = value
            If value IsNot Nothing Then
               SetParent(value, Me)
               _children.Add(value)
            End If
         End Set
      End Property
      Protected Sub _left_ParentChanged(sender As Object, e As EventArgs)
         Dim newParent = _left.Root

         RemoveHandler _left.ParentChanged, AddressOf _left_ParentChanged
         _left = Nothing
         Left = newParent
      End Sub

      Public ReadOnly HasNegation As Boolean
      Public ReadOnly [Operator] As String
      Public ReadOnly LeftOperandSupported As Boolean

      Public Overrides Function ReplaceChild(oldChild As Expression, newChild As Expression) As Boolean
         If MyBase.ReplaceChild(oldChild, newChild) Then
            If oldChild Is _left Then
               _left = newChild
            ElseIf oldChild Is _right Then
               _right = newChild
            End If
            Return True
         Else
            Return False
         End If
      End Function

      Protected _right As Expression
      Public Property Right As Expression
         Get
            Return _right
         End Get
         Set(value As Expression)
            If _right IsNot Nothing Then _children.Remove(_right)
            _right = value
            If value IsNot Nothing Then
               SetParent(value, Me)
               _children.Add(value)
            End If
         End Set
      End Property

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
               If Me.LeftOperandSupported Then
                  If Index = 0 Then
                     _sealResult = Match.Index
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
                     Else
                        _sealResult = leftExpression.Match.Index
                     End If
                  End If
               End If
               If Not _sealResult.HasValue Then
                  Dim rightIndex As Integer = Index + 1

                  If expressions.Count <= rightIndex Then
                     _sealResult = Match.Index + Match.Length
                  Else
                     Dim rightExpression As Expression = expressions(rightIndex)

                     If Me.AllowedRights.Contains(rightExpression.GroupName) Then
                        Me.Right = rightExpression.Root
                     Else
                        _sealResult = rightExpression.Match.Index
                     End If
                  End If
               End If
            End If
         End If

         Return _sealResult
      End Function

   End Class
End Namespace