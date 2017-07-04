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
   Public Class cmisObjectInFolderListType
      Inherits Messaging.cmisObjectInFolderListType
      Implements Contracts.IServiceModelObjectEnumerable

#Region "IServiceModelObjectEnumerable"
      Protected Overrides Function IEnumerable_GetEnumerator() As System.Collections.IEnumerator
         Return If(_objects, New cmisObjectInFolderType() {}).GetEnumerator()
      End Function

      Public ReadOnly Property ContainsObjects As Boolean Implements Contracts.IServiceModelObjectEnumerable.ContainsObjects
         Get
            Return _objects IsNot Nothing
         End Get
      End Property

      Private ReadOnly Property IServiceModelObjectEnumerable_HasMoreItems As Boolean Implements Contracts.IServiceModelObjectEnumerable.HasMoreItems
         Get
            Return _hasMoreItems
         End Get
      End Property

      Private ReadOnly Property IServiceModelObjectEnumerable_NumItems As xs_Integer? Implements Contracts.IServiceModelObjectEnumerable.NumItems
         Get
            Return _numItems
         End Get
      End Property
#End Region

      Public Shadows Property Objects As cmisObjectInFolderType()
         Get
            If MyBase._objects Is Nothing Then
               Return Nothing
            Else
               Return (From cmisObject As Messaging.cmisObjectInFolderType In _objects
                       Where cmisObject Is Nothing OrElse TypeOf cmisObject Is cmisObjectInFolderType
                       Select CType(cmisObject, cmisObjectInFolderType)).ToArray()
            End If
         End Get
         Set(value As cmisObjectInFolderType())
            If value Is Nothing Then
               _objects = Nothing
            Else
               MyBase.Objects = (From item As cmisObjectInFolderType In value
                                 Select CType(item, Messaging.cmisObjectInFolderType)).ToArray()
            End If
         End Set
      End Property

      Public Overloads Shared Widening Operator CType(value As cmisObjectInFolderListType) As cmisObjectListType
         If value Is Nothing Then
            Return Nothing
         Else
            Return New cmisObjectListType() With {.HasMoreItems = value._hasMoreItems, .NumItems = value._numItems,
                                                  .Objects = If(value._objects Is Nothing, Nothing,
                                                                (From cmisObjectInFolder As Messaging.cmisObjectInFolderType In value._objects
                                                                 Let cmisObject As cmisObjectType = If(cmisObjectInFolder Is Nothing OrElse Not TypeOf cmisObjectInFolder Is cmisObjectInFolderType,
                                                                                                       Nothing, CType(cmisObjectInFolder, cmisObjectInFolderType).Object)
                                                                 Where cmisObject IsNot Nothing OrElse cmisObjectInFolder Is Nothing
                                                                 Select cmisObject).ToArray())}
         End If
      End Operator
   End Class
End Namespace