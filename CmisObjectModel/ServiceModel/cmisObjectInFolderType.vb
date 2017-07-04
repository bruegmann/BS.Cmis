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
Namespace CmisObjectModel.ServiceModel
   Public Class cmisObjectInFolderType
      Inherits Messaging.cmisObjectInFolderType
      Implements Contracts.IServiceModelObject


      Public Shadows Property [Object] As cmisObjectType
         Get
            Return TryCast(MyBase.Object, cmisObjectType)
         End Get
         Set(value As cmisObjectType)
            MyBase.Object = value
         End Set
      End Property

#Region "IServiceModelObject"
      Private ReadOnly Property IServiceModelObject_Object As cmisObjectType Implements Contracts.IServiceModelObject.Object
         Get
            Return Me.Object
         End Get
      End Property

      Private ReadOnly Property IServiceModelObject_PathSegment As String Implements Contracts.IServiceModelObject.PathSegment
         Get
            Return _pathSegment
         End Get
      End Property

      Public ReadOnly Property RelativePathSegment As String Implements Contracts.IServiceModelObject.RelativePathSegment
         Get
            Return Nothing
         End Get
      End Property

      Public ReadOnly Property ServiceModel As cmisObjectType.ServiceModelExtension Implements Contracts.IServiceModelObject.ServiceModel
         Get
            Dim cmisObject As cmisObjectType = Me.Object
            Return If(If(cmisObject Is Nothing, Nothing, cmisObject.ServiceModel), New cmisObjectType.ServiceModelExtension(cmisObject))
         End Get
      End Property
#End Region

      Public Overloads Shared Widening Operator CType(value As cmisObjectInFolderType) As cmisObjectType
         Return If(value Is Nothing, Nothing, value.Object)
      End Operator

   End Class
End Namespace