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
   Public Class cmisObjectListType
      Inherits Messaging.cmisObjectListType
      Implements Contracts.IServiceModelObjectEnumerable

#Region "IServiceModelObjectEnumerable"
      Protected Overrides Function IEnumerable_GetEnumerator() As System.Collections.IEnumerator
         Return If(_objects, New cmisObjectType() {}).GetEnumerator()
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

      Public Shadows Property Objects As cmisObjectType()
         Get
            If _objects Is Nothing Then
               Return Nothing
            Else
               Return (From cmisObject As Core.cmisObjectType In _objects
                       Where cmisObject Is Nothing OrElse TypeOf cmisObject Is cmisObjectType
                       Select CType(cmisObject, cmisObjectType)).ToArray()
            End If
         End Get
         Set(value As cmisObjectType())
            If value Is Nothing Then
               _objects = Nothing
            Else
               MyBase.Objects = (From cmisObject As cmisObjectType In value
                                 Select CType(cmisObject, Core.cmisObjectType)).ToArray()
            End If
         End Set
      End Property

   End Class
End Namespace