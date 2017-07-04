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
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If
Namespace CmisObjectModel.ServiceModel
   Public Class getContentChanges
      Inherits Messaging.Responses.getContentChangesResponse
      Implements Contracts.IServiceModelObjectEnumerable

#Region "IServiceModelObjectEnumerable"
      Protected Overrides Function IEnumerable_GetEnumerator() As System.Collections.IEnumerator
         Return If(Objects, New cmisObjectListType()).GetEnumerator()
      End Function

      Public ReadOnly Property ContainsObjects As Boolean Implements Contracts.IServiceModelObjectEnumerable.ContainsObjects
         Get
            Dim objects As cmisObjectListType = Me.Objects
            Return objects IsNot Nothing AndAlso objects.ContainsObjects
         End Get
      End Property

      Public ReadOnly Property HasMoreItems As Boolean Implements Contracts.IServiceModelObjectEnumerable.HasMoreItems
         Get
            Dim objects As cmisObjectListType = Me.Objects
            Return objects IsNot Nothing AndAlso objects.HasMoreItems
         End Get
      End Property

      Public ReadOnly Property NumItems As xs_Integer? Implements Contracts.IServiceModelObjectEnumerable.NumItems
         Get
            Dim objects As cmisObjectListType = Me.Objects
            If objects Is Nothing Then
               Return Nothing
            Else
               Return objects.NumItems
            End If
         End Get
      End Property
#End Region

      Public Shadows Property Objects As cmisObjectListType
         Get
            Return TryCast(_objects, cmisObjectListType)
         End Get
         Set(value As cmisObjectListType)
            MyBase.Objects = value
         End Set
      End Property
   End Class
End Namespace