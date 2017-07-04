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
   Public MustInherit Class FieldContainerExpression
      Inherits SqlPartExpression

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer)
         MyBase.New(match, groupName, rank, index, ", ", " ")
      End Sub
#End Region

      Protected _fields As New List(Of FieldExpression)
      Public ReadOnly Property Fields As FieldExpression()
         Get
            Return _fields.ToArray()
         End Get
      End Property

      Private Shared _allowedFields As New HashSet(Of String) From {
         "Constant", "Identifier", "MathOperator", "Method", "OpenParenthesis", "Sign", "StringOperator"}
      Protected Function AddField(field As Expression, expressions As List(Of Expression)) As Boolean
         Dim root As Expression = field.Root

         If _allowedFields.Contains(root.GroupName) Then
            Dim newField As New FieldExpression(root)

            _children.Add(newField)
            SetParent(newField, Me)
            _fields.Add(newField)
            _sealResult = newField.Seal(expressions)
         Else
            _sealResult = field.Match.Index
         End If

         Return Not _sealResult.HasValue
      End Function

   End Class
End Namespace