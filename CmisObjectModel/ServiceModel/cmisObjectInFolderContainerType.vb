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
   Public Class cmisObjectInFolderContainerType
      Inherits Messaging.cmisObjectInFolderContainerType
      Implements Contracts.IServiceModelObjectEnumerable, Contracts.IServiceModelObject

#Region "IServiceModelObjectEnumerable"
      Protected Overrides Function IEnumerable_GetEnumerator() As System.Collections.IEnumerator
         Return If(_children, New cmisObjectInFolderContainerType() {}).GetEnumerator()
      End Function

      Public ReadOnly Property ContainsObjects As Boolean Implements Contracts.IServiceModelObjectEnumerable.ContainsObjects
         Get
            Return _children IsNot Nothing
         End Get
      End Property

      Private ReadOnly Property IServiceModelObjectEnumerable_HasMoreItems As Boolean Implements Contracts.IServiceModelObjectEnumerable.HasMoreItems
         Get
            Return False
         End Get
      End Property

      Private ReadOnly Property IServiceModelObjectEnumerable_NumItems As xs_Integer? Implements Contracts.IServiceModelObjectEnumerable.NumItems
         Get
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
            Return If(_children is Nothing, 0, _children.Length)
#Else
            Return If(_children Is Nothing, 0, _children.LongLength)
#End If
         End Get
      End Property
#End Region

#Region "IServiceModelObject"
      Public ReadOnly Property [Object] As cmisObjectType Implements Contracts.IServiceModelObject.Object
         Get
            Return ObjectInFolder
         End Get
      End Property

      Public ReadOnly Property PathSegment As String Implements Contracts.IServiceModelObject.PathSegment
         Get
            Dim objectInFolder As cmisObjectInFolderType = Me.ObjectInFolder
            Return If(objectInFolder Is Nothing, Nothing, objectInFolder.PathSegment)
         End Get
      End Property

      Public ReadOnly Property RelativePathSegment As String Implements Contracts.IServiceModelObject.RelativePathSegment
         Get
            Dim objectInFolder As cmisObjectInFolderType = Me.ObjectInFolder
            Return If(objectInFolder Is Nothing, Nothing, objectInFolder.RelativePathSegment)
         End Get
      End Property

      Public ReadOnly Property ServiceModel As cmisObjectType.ServiceModelExtension Implements Contracts.IServiceModelObject.ServiceModel
         Get
            Dim cmisObject As cmisObjectType = Me.Object
            Return If(If(cmisObject Is Nothing, Nothing, cmisObject.ServiceModel), New cmisObjectType.ServiceModelExtension(cmisObject))
         End Get
      End Property
#End Region

      Public Shadows Property Children As cmisObjectInFolderContainerType()
         Get
            If _children Is Nothing Then
               Return Nothing
            Else
               Return (From container As Messaging.cmisObjectInFolderContainerType In _children
                       Where container Is Nothing OrElse TypeOf container Is cmisObjectInFolderContainerType
                       Select CType(container, cmisObjectInFolderContainerType)).ToArray()
            End If
         End Get
         Set(value As cmisObjectInFolderContainerType())
            If value Is Nothing Then
               _children = Nothing
            Else
               MyBase.Children = (From container As cmisObjectInFolderContainerType In value
                                  Select CType(container, Messaging.cmisObjectInFolderContainerType)).ToArray()
            End If
         End Set
      End Property

      Public Shadows Property ObjectInFolder As cmisObjectInFolderType
         Get
            Return TryCast(_objectInFolder, cmisObjectInFolderType)
         End Get
         Set(value As cmisObjectInFolderType)
            MyBase.ObjectInFolder = value
         End Set
      End Property

   End Class
End Namespace