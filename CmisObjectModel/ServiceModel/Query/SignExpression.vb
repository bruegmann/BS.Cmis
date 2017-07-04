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
   Public Class SignExpression
      Inherits OperatorExpression

#Region "Constructors"
      Public Sub New(match As str.Match, rank As Integer, index As Integer)
         MyBase.New(match, "Sign", rank, index, False)
      End Sub
#End Region

      Private Shared _allowedRights As New HashSet(Of String) From {
         "OpenParenthesis", "Constant", "Identifier", "Method", "Sign"}
      ''' <summary>
      ''' Returns the valid group names for the right side of the operator
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overrides ReadOnly Property AllowedRights As System.Collections.Generic.HashSet(Of String)
         Get
            Return _allowedRights
         End Get
      End Property

      Public Overrides Function CanSetValue() As Boolean
         Return False
      End Function

      Protected Overrides Function GetValue(executingType As System.Type) As String
         If Me.GetType().IsAssignableFrom(executingType) Then
            Dim sign As SignExpression = Me
            Dim isNegative As Boolean = (MyBase.GetValue(GetType(Expression)) = "-")

            While TypeOf sign._right Is SignExpression
               sign = CType(sign._right, SignExpression)
               isNegative = (isNegative <> (sign.Value = "-"))
            End While

            Return If(isNegative, "-", Nothing) & If(sign._right Is Nothing, Nothing, sign._right.Value)
         Else
            Return MyBase.GetValue(executingType)
         End If
      End Function

   End Class
End Namespace