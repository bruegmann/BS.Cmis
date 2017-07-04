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
   Public Class MethodExpression
      Inherits CompositeExpression

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer)
         MyBase.New(match, groupName, rank, index, Nothing, Nothing)
         Me.MethodName = match.Groups("MethodName").Value
      End Sub
#End Region

      Private _parenthesis As ParenthesisExpression
      Public Property Parenthesis As ParenthesisExpression
         Get
            Return _parenthesis
         End Get
         Set(value As ParenthesisExpression)
            If _parenthesis IsNot Nothing Then _children.Remove(_parenthesis)
            _parenthesis = value
            If value IsNot Nothing Then
               SetParent(value, Me)
               _children.Add(value)
            End If
         End Set
      End Property

      Public ReadOnly MethodName As String

      ''' <summary>
      ''' Returns the complete prefix of the identifier
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

               If Not TypeOf rightExpression Is ParenthesisExpression OrElse rightExpression.Parent IsNot Nothing Then
                  _sealResult = Me.Match.Index + Me.Match.Length
               Else
                  Me.Parenthesis = CType(rightExpression, ParenthesisExpression)
               End If
            End If
         End If

         Return _sealResult
      End Function

   End Class
End Namespace