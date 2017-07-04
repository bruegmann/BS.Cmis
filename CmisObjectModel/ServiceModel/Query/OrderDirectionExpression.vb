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
   Public Class OrderDirectionExpression
      Inherits Expression

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer)
         MyBase.New(match, groupName, rank, index)
      End Sub
#End Region

      Public ReadOnly Property Direction As ccg.Nullable(Of String)
         Get
            Dim group = Match.Groups("Direction")

            If group Is Nothing OrElse Not group.Success Then
               Return Nothing
            Else
               Return group.Value
            End If
         End Get
      End Property

      Public ReadOnly Property Nulls As ccg.Nullable(Of String)
         Get
            Dim group = Match.Groups("Nulls")

            If group Is Nothing OrElse Not group.Success Then
               Return Nothing
            Else
               Return group.Value
            End If
         End Get
      End Property

      ''' <summary>
      ''' Test if the instance is bound to a FieldExpression. If not the parsed query expression is not valid.
      ''' </summary>
      ''' <param name="expressions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function Seal(expressions As System.Collections.Generic.List(Of Expression)) As Integer?
         If Not _sealed Then
            _sealResult = MyBase.Seal(expressions)

            If Not _sealResult.HasValue AndAlso Not TypeOf _parent Is FieldExpression Then _sealResult = Match.Index
         End If

         Return _sealResult
      End Function

   End Class
End Namespace