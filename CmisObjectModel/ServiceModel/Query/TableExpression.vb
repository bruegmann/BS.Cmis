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
   Public Class TableExpression
      Inherits DatabaseObjectExpression

#Region "Constructors"
      Public Sub New(table As Expression)
         MyBase.New(table)
      End Sub
#End Region

      Protected Overrides ReadOnly Property AllowAlias As Boolean
         Get
            Return True
         End Get
      End Property

      Protected Overrides Function GetValue(executingType As System.Type) As String
         Dim myBaseResult As String = MyBase.GetValue(executingType)

         If Me.GetType().IsAssignableFrom(executingType) Then
            Dim sb As New st.StringBuilder(myBaseResult)

            If _join IsNot Nothing Then
               sb.Append(" ")
               sb.Append(_join.Value)
            End If

            Return sb.ToString()
         Else
            Return myBaseResult
         End If
      End Function

      Private _join As JoinExpression
      Public Property Join As JoinExpression
         Get
            Return _join
         End Get
         Set(value As JoinExpression)
            If _join IsNot Nothing Then _children.Remove(_join)
            _join = value
            If value IsNot Nothing Then
               _children.Add(value)
               SetParent(value, Me)
            End If
         End Set
      End Property

      ''' <summary>
      ''' Returns null if the expression is accepted in the parsed query, otherwise the position of the match
      ''' </summary>
      ''' <param name="expressions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function Seal(expressions As System.Collections.Generic.List(Of Expression)) As Integer?
         If Not _sealed Then
            _sealResult = MyBase.Seal(expressions)
            If Not _sealResult.HasValue Then
               Me.Join = TryCast(GetRightExpression(expressions), JoinExpression)
               If _join IsNot Nothing Then _sealResult = _join.Seal(expressions)
            End If
         End If

         Return _sealResult
      End Function

   End Class
End Namespace