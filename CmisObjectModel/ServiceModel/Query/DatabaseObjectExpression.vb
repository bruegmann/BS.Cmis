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
   ''' <summary>
   ''' Base class for column and table expressions
   ''' </summary>
   ''' <remarks></remarks>
   Public MustInherit Class DatabaseObjectExpression
      Inherits CompositeExpression

#Region "Constructors"
      Public Sub New(innerExpression As Expression)
         MyBase.New(innerExpression.Match, innerExpression.GroupName, innerExpression.Rank, innerExpression.Index, Nothing, Nothing)
         _innerExpression = innerExpression
         _children.Add(innerExpression)
         SetParent(innerExpression, Me)
      End Sub
#End Region

      Protected _alias As AliasExpression
      Public ReadOnly Property [Alias] As AliasExpression
         Get
            Return _alias
         End Get
      End Property
      Public Sub SetAlias(value As String)
         If AllowAlias Then
            'keyword 'as' is optional, therefore an expression is built to ensure detection of alias name
            Dim expressions = QueryParser.GetExpressions("Select fieldName " & value)

            If expressions IsNot Nothing Then
               For Each expression As Expression In expressions
                  If TypeOf expression Is AliasExpression Then
                     _alias = CType(expression, AliasExpression)
                     Exit For
                  End If
               Next
            End If
         End If
      End Sub
      Public Sub SetAlias(value As AliasExpression)
         If AllowAlias Then _alias = value
      End Sub

      Protected MustOverride ReadOnly Property AllowAlias As Boolean

      Protected Overrides Function GetValue(executingType As System.Type) As String
         If Me.GetType().IsAssignableFrom(executingType) Then
            Dim sb As New st.StringBuilder(_innerExpression.Value)

            If Not (_alias Is Nothing OrElse String.IsNullOrEmpty(_alias.Value)) Then
               sb.Append(" ")
               sb.Append(_alias.Value)
            End If

            Return sb.ToString()
         Else
            Return MyBase.GetValue(executingType)
         End If
      End Function

      Protected _innerExpression As Expression
      Public ReadOnly Property InnerExpression As Expression
         Get
            Return _innerExpression
         End Get
      End Property

      ''' <summary>
      ''' Returns the complete prefix of the database object (column or table)
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property Prefix As String
         Get
            Return String.Join(".", PrefixParts)
         End Get
      End Property

      ''' <summary>
      ''' Returns the parts of the prefix; parts are separated by . from each other
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property PrefixParts As String()
         Get
            Dim prefix As str.Group = Match.Groups("Prefix")

            If prefix Is Nothing OrElse Not prefix.Success Then
               Return New String() {}
            Else
               Return (From capture As Object In prefix.Captures
                       Select CType(capture, str.Capture).Value).ToArray()
            End If
         End Get
      End Property

      ''' <summary>
      ''' Returns null if the expression is accepted in the parsed query, otherwise the position of the match
      ''' </summary>
      ''' <param name="expressions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function Seal(expressions As System.Collections.Generic.List(Of Expression)) As Integer?
         If Not _sealed Then
            _sealed = True
            _sealResult = _innerExpression.Seal(expressions)

            If Not _sealResult.HasValue AndAlso AllowAlias Then
               _alias = TryCast(GetRightExpression(expressions), AliasExpression)
               If _alias IsNot Nothing Then
                  _children.Add(_alias)
                  SetParent(_alias, Me)
                  _sealResult = _alias.Seal(expressions)
               End If
            End If
         End If

         Return _sealResult
      End Function
   End Class
End Namespace